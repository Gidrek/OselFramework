using System.Runtime.InteropServices;
using Osel.Core;
using Osel.Graphics.Shaders;
using Osel.Math;
using SDL;

namespace Osel.Graphics;

public unsafe class SpriteBatch : IDisposable
{
    private const int MaxSprites = 8192;
    private const int SpriteDataSize = 80; // sizeof(SpriteData) — 5 × 16 bytes, Metal-aligned
    private const int MaxVertices = MaxSprites * 6;

    private readonly GraphicsDevice _device;

    // GPU resources
    private SDL_GPUGraphicsPipeline* _pipeline;
    private SDL_GPUBuffer* _storageBuffer;
    private SDL_GPUTransferBuffer* _transferBuffer;
    private SDL_GPUSampler* _sampler;
    private SDL_GPUShader* _vertexShader;
    private SDL_GPUShader* _fragmentShader;

    // CPU-side sprite buffer
    private readonly SpriteData[] _spriteBuffer = new SpriteData[MaxSprites];
    private int _spriteCount;

    // Batching state
    private SDL_GPUTexture* _currentTexture;
    private int _currentTextureWidth;
    private int _currentTextureHeight;
    private bool _isBatching;
    private Matrix4x4? _transformMatrix;

    // Cumulative counters for debug overlay (never auto-reset)
    internal int TotalDrawCalls { get; private set; }
    internal int TotalSpriteCount { get; private set; }

    [StructLayout(LayoutKind.Sequential)]
    private struct SpriteData
    {
        public System.Numerics.Vector2 Position;   // 8 bytes
        public System.Numerics.Vector2 Size;        // 8 bytes
        public System.Numerics.Vector4 SourceRect;  // 16 bytes (x,y = UV origin; z,w = UV size)
        public System.Numerics.Vector4 Color;       // 16 bytes
        public System.Numerics.Vector2 Origin;      // 8 bytes
        public System.Numerics.Vector2 Scale;       // 8 bytes
        public float Rotation;                       // 4 bytes
        public float _pad0;                          // 4 bytes
        public float _pad1;                          // 4 bytes
        public float _pad2;                          // 4 bytes
        // Total: 80 bytes (5 × 16, aligned for Metal)
    }

    public SpriteBatch(GraphicsDevice device)
    {
        _device = device;
        CreateResources();
    }

    private void CreateResources()
    {
        var format = BuiltInShaders.DetectFormat(_device.SupportedFormats);

        // Create shaders
        _vertexShader = CreateShader(
            BuiltInShaders.GetVertexShaderCode(format),
            BuiltInShaders.GetVertexEntryPoint(format),
            format,
            SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX,
            numSamplers: 0, numStorageTextures: 0, numStorageBuffers: 1, numUniformBuffers: 1);

        _fragmentShader = CreateShader(
            BuiltInShaders.GetFragmentShaderCode(format),
            BuiltInShaders.GetFragmentEntryPoint(format),
            format,
            SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT,
            numSamplers: 1, numStorageTextures: 0, numStorageBuffers: 0, numUniformBuffers: 0);

        // Create graphics pipeline
        _pipeline = CreatePipeline();

        // Release shader objects (no longer needed after pipeline creation)
        SDL3.SDL_ReleaseGPUShader(_device.DeviceHandle, _vertexShader);
        SDL3.SDL_ReleaseGPUShader(_device.DeviceHandle, _fragmentShader);
        _vertexShader = null;
        _fragmentShader = null;

        // Create storage buffer for sprite data
        var bufferInfo = new SDL_GPUBufferCreateInfo
        {
            usage = SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_GRAPHICS_STORAGE_READ,
            size = (uint)(MaxSprites * SpriteDataSize),
        };
        _storageBuffer = SDL3.SDL_CreateGPUBuffer(_device.DeviceHandle, &bufferInfo);
        if (_storageBuffer == null)
            throw new OselException($"Failed to create storage buffer: {SDL3.SDL_GetError()}");

        // Create transfer buffer for uploading sprite data
        var transferInfo = new SDL_GPUTransferBufferCreateInfo
        {
            usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
            size = (uint)(MaxSprites * SpriteDataSize),
        };
        _transferBuffer = SDL3.SDL_CreateGPUTransferBuffer(_device.DeviceHandle, &transferInfo);
        if (_transferBuffer == null)
            throw new OselException($"Failed to create transfer buffer: {SDL3.SDL_GetError()}");

        // Create sampler (point filtering for pixel art)
        var samplerInfo = new SDL_GPUSamplerCreateInfo
        {
            min_filter = SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
            mag_filter = SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
            mipmap_mode = SDL_GPUSamplerMipmapMode.SDL_GPU_SAMPLERMIPMAPMODE_NEAREST,
            address_mode_u = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
            address_mode_v = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
            address_mode_w = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
        };
        _sampler = SDL3.SDL_CreateGPUSampler(_device.DeviceHandle, &samplerInfo);
        if (_sampler == null)
            throw new OselException($"Failed to create sampler: {SDL3.SDL_GetError()}");
    }

    private SDL_GPUShader* CreateShader(byte[] code, string entrypoint, SDL_GPUShaderFormat format,
        SDL_GPUShaderStage stage, uint numSamplers, uint numStorageTextures, uint numStorageBuffers, uint numUniformBuffers)
    {
        fixed (byte* codePtr = code)
        {
            var entrypointBytes = System.Text.Encoding.UTF8.GetBytes(entrypoint + '\0');
            fixed (byte* entrypointPtr = entrypointBytes)
            {
                var createInfo = new SDL_GPUShaderCreateInfo
                {
                    code = codePtr,
                    code_size = (nuint)code.Length,
                    entrypoint = entrypointPtr,
                    format = format,
                    stage = stage,
                    num_samplers = numSamplers,
                    num_storage_textures = numStorageTextures,
                    num_storage_buffers = numStorageBuffers,
                    num_uniform_buffers = numUniformBuffers,
                };

                var shader = SDL3.SDL_CreateGPUShader(_device.DeviceHandle, &createInfo);
                if (shader == null)
                    throw new OselException($"SDL_CreateGPUShader ({stage}) failed: {SDL3.SDL_GetError()}");

                return shader;
            }
        }
    }

    private SDL_GPUGraphicsPipeline* CreatePipeline()
    {
        // Alpha blending: SrcAlpha, OneMinusSrcAlpha
        var colorTarget = new SDL_GPUColorTargetDescription
        {
            format = _device.SwapchainFormat,
            blend_state = new SDL_GPUColorTargetBlendState
            {
                enable_blend = true,
                src_color_blendfactor = SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_SRC_ALPHA,
                dst_color_blendfactor = SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
                color_blend_op = SDL_GPUBlendOp.SDL_GPU_BLENDOP_ADD,
                src_alpha_blendfactor = SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_SRC_ALPHA,
                dst_alpha_blendfactor = SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
                alpha_blend_op = SDL_GPUBlendOp.SDL_GPU_BLENDOP_ADD,
                color_write_mask = SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_R
                                 | SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_G
                                 | SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_B
                                 | SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_A,
            },
        };

        var targetInfo = new SDL_GPUGraphicsPipelineTargetInfo
        {
            num_color_targets = 1,
            color_target_descriptions = &colorTarget,
            has_depth_stencil_target = false,
        };

        var createInfo = new SDL_GPUGraphicsPipelineCreateInfo
        {
            vertex_shader = _vertexShader,
            fragment_shader = _fragmentShader,
            primitive_type = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST,
            target_info = targetInfo,
            rasterizer_state = new SDL_GPURasterizerState
            {
                fill_mode = SDL_GPUFillMode.SDL_GPU_FILLMODE_FILL,
                cull_mode = SDL_GPUCullMode.SDL_GPU_CULLMODE_NONE,
                front_face = SDL_GPUFrontFace.SDL_GPU_FRONTFACE_COUNTER_CLOCKWISE,
            },
            multisample_state = new SDL_GPUMultisampleState
            {
                sample_count = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1,
            },
            // No vertex input (pull model)
            vertex_input_state = default,
        };

        var pipeline = SDL3.SDL_CreateGPUGraphicsPipeline(_device.DeviceHandle, &createInfo);
        if (pipeline == null)
            throw new OselException($"SDL_CreateGPUGraphicsPipeline failed: {SDL3.SDL_GetError()}");

        return pipeline;
    }

    public void Begin(Effect? effect = null, Matrix4x4? transformMatrix = null)
    {
        if (_isBatching)
            throw new InvalidOperationException("Begin was already called.");

        _isBatching = true;
        _spriteCount = 0;
        _currentTexture = null;
        _transformMatrix = transformMatrix;
    }

    // Simple overloads — delegate to full DrawInternal with defaults

    public void Draw(Texture2D texture, Vector2 position, Color color)
    {
        Draw(texture, position, null, color);
    }

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color)
    {
        DrawInternal(
            texture.Handle, texture.Width, texture.Height,
            position, sourceRect, color,
            0f, Vector2.Zero, Vector2.One,
            SpriteEffects.None, 0f);
    }

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color,
        float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
    {
        DrawInternal(
            texture.Handle, texture.Width, texture.Height,
            position, sourceRect, color,
            rotation, origin, new Vector2(scale, scale),
            effects, layerDepth);
    }

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color,
        float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
    {
        DrawInternal(
            texture.Handle, texture.Width, texture.Height,
            position, sourceRect, color,
            rotation, origin, scale,
            effects, layerDepth);
    }

    public void Draw(Texture2D texture, Rectangle destinationRect, Rectangle? sourceRect, Color color)
    {
        // Calculate scale from destination rectangle
        int srcW = sourceRect?.Width ?? texture.Width;
        int srcH = sourceRect?.Height ?? texture.Height;
        var scale = new Vector2(destinationRect.Width / (float)srcW, destinationRect.Height / (float)srcH);

        DrawInternal(
            texture.Handle, texture.Width, texture.Height,
            new Vector2(destinationRect.X, destinationRect.Y), sourceRect, color,
            0f, Vector2.Zero, scale,
            SpriteEffects.None, 0f);
    }

    // Internal Draw for RenderTarget2D (uses raw texture handle + dimensions)
    internal void Draw(SDL_GPUTexture* textureHandle, int textureWidth, int textureHeight,
        Vector2 position, Rectangle? sourceRect, Color color,
        float rotation, Vector2 origin, Vector2 scale,
        SpriteEffects effects, float layerDepth)
    {
        DrawInternal(textureHandle, textureWidth, textureHeight,
            position, sourceRect, color, rotation, origin, scale, effects, layerDepth);
    }

    private void DrawInternal(SDL_GPUTexture* textureHandle, int textureWidth, int textureHeight,
        Vector2 position, Rectangle? sourceRect, Color color,
        float rotation, Vector2 origin, Vector2 scale,
        SpriteEffects effects, float layerDepth)
    {
        if (!_isBatching)
            throw new InvalidOperationException("Begin must be called before Draw.");

        // Flush on texture change
        if (textureHandle != _currentTexture && _spriteCount > 0)
            Flush();

        _currentTexture = textureHandle;
        _currentTextureWidth = textureWidth;
        _currentTextureHeight = textureHeight;

        // Flush if batch is full
        if (_spriteCount >= MaxSprites)
            Flush();

        // Calculate UV coordinates
        float srcX, srcY, srcW, srcH;
        int drawWidth, drawHeight;

        if (sourceRect.HasValue)
        {
            var r = sourceRect.Value;
            srcX = r.X / (float)textureWidth;
            srcY = r.Y / (float)textureHeight;
            srcW = r.Width / (float)textureWidth;
            srcH = r.Height / (float)textureHeight;
            drawWidth = r.Width;
            drawHeight = r.Height;
        }
        else
        {
            srcX = 0; srcY = 0; srcW = 1; srcH = 1;
            drawWidth = textureWidth;
            drawHeight = textureHeight;
        }

        // Apply flip effects by negating scale on CPU side
        var finalScale = scale;
        if (effects.HasFlag(SpriteEffects.FlipHorizontally))
            finalScale = new Vector2(-finalScale.X, finalScale.Y);
        if (effects.HasFlag(SpriteEffects.FlipVertically))
            finalScale = new Vector2(finalScale.X, -finalScale.Y);

        _spriteBuffer[_spriteCount] = new SpriteData
        {
            Position = new System.Numerics.Vector2(position.X, position.Y),
            Size = new System.Numerics.Vector2(drawWidth, drawHeight),
            SourceRect = new System.Numerics.Vector4(srcX, srcY, srcW, srcH),
            Color = new System.Numerics.Vector4(color.R, color.G, color.B, color.A),
            Origin = new System.Numerics.Vector2(origin.X, origin.Y),
            Scale = new System.Numerics.Vector2(finalScale.X, finalScale.Y),
            Rotation = rotation,
        };
        _spriteCount++;
    }

    public void DrawString(SpriteFont font, string text, Vector2 position, Color color)
    {
        DrawString(font, text, position, color, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None);
    }

    public void DrawString(SpriteFont font, string text, Vector2 position, Color color,
        float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None)
    {
        DrawString(font, text, position, color, rotation, origin, new Vector2(scale, scale), effects);
    }

    public void DrawString(SpriteFont font, string text, Vector2 position, Color color,
        float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None)
    {
        if (font == null) throw new ArgumentNullException(nameof(font));
        if (string.IsNullOrEmpty(text)) return;

        var atlas = font.Atlas;
        var cursor = Vector2.Zero;
        char prevChar = '\0';

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '\n')
            {
                cursor = new Vector2(0, cursor.Y + font.LineHeight);
                prevChar = '\0';
                continue;
            }

            if (c == '\r')
            {
                prevChar = '\0';
                continue;
            }

            if (!font.Glyphs.TryGetValue(c, out var glyph))
                continue;

            // Apply kerning
            if (prevChar != '\0')
                cursor += new Vector2(font.GetKerning(prevChar, c), 0);

            var offset = cursor + glyph.Offset;
            var glyphPos = position + new Vector2(offset.X * scale.X, offset.Y * scale.Y);

            DrawInternal(
                atlas.Handle, atlas.Width, atlas.Height,
                glyphPos, glyph.SourceRect, color,
                rotation, origin, scale,
                effects, 0f);

            cursor += new Vector2(glyph.Advance, 0);
            prevChar = c;
        }
    }

    public void End()
    {
        if (!_isBatching)
            throw new InvalidOperationException("Begin must be called before End.");

        if (_spriteCount > 0)
            Flush();

        _isBatching = false;
    }

    private void Flush()
    {
        if (_spriteCount == 0 || (!_device.HasSwapchain && !_device.HasRenderTarget))
            return;

        TotalDrawCalls++;
        TotalSpriteCount += _spriteCount;

        var cmdBuf = _device.CommandBuffer;

        // 1. Upload sprite data via copy pass
        UploadSpriteData(cmdBuf);

        // 2. Begin render pass
        var renderPass = _device.BeginRenderPass();
        if (renderPass == null) return;

        // 3. Bind pipeline
        SDL3.SDL_BindGPUGraphicsPipeline(renderPass, _pipeline);

        // 4. Bind vertex storage buffer (slot 0)
        var storageBufferPtr = _storageBuffer;
        SDL3.SDL_BindGPUVertexStorageBuffers(renderPass, 0, &storageBufferPtr, 1);

        // 5. Bind fragment texture + sampler (slot 0)
        var textureSamplerBinding = new SDL_GPUTextureSamplerBinding
        {
            texture = _currentTexture,
            sampler = _sampler,
        };
        SDL3.SDL_BindGPUFragmentSamplers(renderPass, 0, &textureSamplerBinding, 1);

        // 6. Push projection matrix as uniform data (with optional camera/transform)
        var ortho = System.Numerics.Matrix4x4.CreateOrthographicOffCenter(
            0, _device.RenderWidth, _device.RenderHeight, 0, -1, 1);
        var finalMatrix = _transformMatrix.HasValue
            ? (System.Numerics.Matrix4x4)(_transformMatrix.Value * (Matrix4x4)ortho)
            : ortho;
        SDL3.SDL_PushGPUVertexUniformData(cmdBuf, 0, (nint)(&finalMatrix), (uint)sizeof(System.Numerics.Matrix4x4));

        // 7. Draw
        SDL3.SDL_DrawGPUPrimitives(renderPass, (uint)(_spriteCount * 6), 1, 0, 0);

        // 8. End render pass
        _device.EndRenderPass(renderPass);

        _spriteCount = 0;
    }

    private void UploadSpriteData(SDL_GPUCommandBuffer* cmdBuf)
    {
        uint dataSize = (uint)(_spriteCount * SpriteDataSize);

        // Map transfer buffer and copy sprite data
        var mapped = SDL3.SDL_MapGPUTransferBuffer(_device.DeviceHandle, _transferBuffer, true);
        fixed (SpriteData* src = _spriteBuffer)
        {
            Buffer.MemoryCopy(src, (void*)mapped, dataSize, dataSize);
        }
        SDL3.SDL_UnmapGPUTransferBuffer(_device.DeviceHandle, _transferBuffer);

        // Copy pass: transfer buffer -> storage buffer
        var copyPass = SDL3.SDL_BeginGPUCopyPass(cmdBuf);

        var srcLocation = new SDL_GPUTransferBufferLocation
        {
            transfer_buffer = _transferBuffer,
            offset = 0,
        };

        var dstRegion = new SDL_GPUBufferRegion
        {
            buffer = _storageBuffer,
            offset = 0,
            size = dataSize,
        };

        SDL3.SDL_UploadToGPUBuffer(copyPass, &srcLocation, &dstRegion, true);
        SDL3.SDL_EndGPUCopyPass(copyPass);
    }

    public void Dispose()
    {
        if (_pipeline != null)
        {
            SDL3.SDL_ReleaseGPUGraphicsPipeline(_device.DeviceHandle, _pipeline);
            _pipeline = null;
        }
        if (_storageBuffer != null)
        {
            SDL3.SDL_ReleaseGPUBuffer(_device.DeviceHandle, _storageBuffer);
            _storageBuffer = null;
        }
        if (_transferBuffer != null)
        {
            SDL3.SDL_ReleaseGPUTransferBuffer(_device.DeviceHandle, _transferBuffer);
            _transferBuffer = null;
        }
        if (_sampler != null)
        {
            SDL3.SDL_ReleaseGPUSampler(_device.DeviceHandle, _sampler);
            _sampler = null;
        }
        GC.SuppressFinalize(this);
    }
}
