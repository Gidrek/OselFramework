using Osel.Core;
using Osel.Graphics.Shaders;
using SDL;

namespace Osel.Graphics;

/// <summary>
/// Wraps a vertex + fragment shader pair into a GPU graphics pipeline.
/// Can be used with SpriteBatch.Begin() to apply custom shaders.
/// </summary>
public unsafe class Effect : IDisposable
{
    internal SDL_GPUGraphicsPipeline* Pipeline { get; private set; }

    private readonly SDL_GPUDevice* _device;

    private Effect(SDL_GPUDevice* device, SDL_GPUGraphicsPipeline* pipeline)
    {
        _device = device;
        Pipeline = pipeline;
    }

    /// <summary>
    /// Creates an Effect from vertex and fragment shader bytecode.
    /// The pipeline is configured to match SpriteBatch defaults
    /// (no vertex input, alpha blend, cull none, triangle list).
    /// </summary>
    public static Effect Create(GraphicsDevice graphicsDevice,
        byte[] vertexCode, string vertexEntry, SDL_GPUShaderFormat vertexFormat,
        uint vertexStorageBuffers, uint vertexUniformBuffers,
        byte[] fragmentCode, string fragmentEntry, SDL_GPUShaderFormat fragmentFormat,
        uint fragmentSamplers)
    {
        var device = graphicsDevice.DeviceHandle;

        // Create vertex shader
        SDL_GPUShader* vertexShader;
        fixed (byte* codePtr = vertexCode)
        {
            var entryBytes = System.Text.Encoding.UTF8.GetBytes(vertexEntry + '\0');
            fixed (byte* entryPtr = entryBytes)
            {
                var info = new SDL_GPUShaderCreateInfo
                {
                    code = codePtr,
                    code_size = (nuint)vertexCode.Length,
                    entrypoint = entryPtr,
                    format = vertexFormat,
                    stage = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX,
                    num_samplers = 0,
                    num_storage_textures = 0,
                    num_storage_buffers = vertexStorageBuffers,
                    num_uniform_buffers = vertexUniformBuffers,
                };
                vertexShader = SDL3.SDL_CreateGPUShader(device, &info);
                if (vertexShader == null)
                    throw new OselException($"Effect vertex shader creation failed: {SDL3.SDL_GetError()}");
            }
        }

        // Create fragment shader
        SDL_GPUShader* fragmentShader;
        fixed (byte* codePtr = fragmentCode)
        {
            var entryBytes = System.Text.Encoding.UTF8.GetBytes(fragmentEntry + '\0');
            fixed (byte* entryPtr = entryBytes)
            {
                var info = new SDL_GPUShaderCreateInfo
                {
                    code = codePtr,
                    code_size = (nuint)fragmentCode.Length,
                    entrypoint = entryPtr,
                    format = fragmentFormat,
                    stage = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT,
                    num_samplers = fragmentSamplers,
                    num_storage_textures = 0,
                    num_storage_buffers = 0,
                    num_uniform_buffers = 0,
                };
                fragmentShader = SDL3.SDL_CreateGPUShader(device, &info);
                if (fragmentShader == null)
                {
                    SDL3.SDL_ReleaseGPUShader(device, vertexShader);
                    throw new OselException($"Effect fragment shader creation failed: {SDL3.SDL_GetError()}");
                }
            }
        }

        // Create pipeline matching SpriteBatch defaults
        var colorTarget = new SDL_GPUColorTargetDescription
        {
            format = graphicsDevice.SwapchainFormat,
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

        var pipelineInfo = new SDL_GPUGraphicsPipelineCreateInfo
        {
            vertex_shader = vertexShader,
            fragment_shader = fragmentShader,
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
            vertex_input_state = default,
        };

        var pipeline = SDL3.SDL_CreateGPUGraphicsPipeline(device, &pipelineInfo);

        SDL3.SDL_ReleaseGPUShader(device, vertexShader);
        SDL3.SDL_ReleaseGPUShader(device, fragmentShader);

        if (pipeline == null)
            throw new OselException($"Effect pipeline creation failed: {SDL3.SDL_GetError()}");

        return new Effect(device, pipeline);
    }

    /// <summary>
    /// Creates a built-in engine effect using the default SpriteBatch shaders.
    /// </summary>
    internal static Effect CreateBuiltIn(GraphicsDevice graphicsDevice)
    {
        var format = BuiltInShaders.DetectFormat(graphicsDevice.SupportedFormats);

        return Create(graphicsDevice,
            BuiltInShaders.GetVertexShaderCode(format),
            BuiltInShaders.GetVertexEntryPoint(format),
            format,
            vertexStorageBuffers: 1,
            vertexUniformBuffers: 1,
            BuiltInShaders.GetFragmentShaderCode(format),
            BuiltInShaders.GetFragmentEntryPoint(format),
            format,
            fragmentSamplers: 1);
    }

    public void Dispose()
    {
        if (Pipeline != null)
        {
            SDL3.SDL_ReleaseGPUGraphicsPipeline(_device, Pipeline);
            Pipeline = null;
        }
        GC.SuppressFinalize(this);
    }
}
