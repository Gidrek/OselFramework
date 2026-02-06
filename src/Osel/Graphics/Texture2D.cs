using Osel.Core;
using SDL;

namespace Osel.Graphics;

public unsafe class Texture2D : IDisposable
{
    internal SDL_GPUTexture* Handle { get; private set; }
    public int Width { get; }
    public int Height { get; }

    private readonly SDL_GPUDevice* _device;

    private readonly GraphicsDevice _graphicsDevice;

    internal Texture2D(GraphicsDevice graphicsDevice, int width, int height, byte[] rgbaPixels)
    {
        _graphicsDevice = graphicsDevice;
        _device = graphicsDevice.DeviceHandle;
        Width = width;
        Height = height;

        // Create GPU texture
        var textureInfo = new SDL_GPUTextureCreateInfo
        {
            type = SDL_GPUTextureType.SDL_GPU_TEXTURETYPE_2D,
            format = SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM,
            usage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER,
            width = (uint)width,
            height = (uint)height,
            layer_count_or_depth = 1,
            num_levels = 1,
            sample_count = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1,
        };

        Handle = SDL3.SDL_CreateGPUTexture(_device, &textureInfo);
        if (Handle == null)
            throw new OselException($"SDL_CreateGPUTexture failed: {SDL3.SDL_GetError()}");

        UploadPixelData(graphicsDevice, rgbaPixels);
    }

    private void UploadPixelData(GraphicsDevice graphicsDevice, byte[] rgbaPixels)
    {
        uint dataSize = (uint)(Width * Height * 4);

        // Create transfer buffer
        var transferInfo = new SDL_GPUTransferBufferCreateInfo
        {
            usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
            size = dataSize,
        };

        var transferBuffer = SDL3.SDL_CreateGPUTransferBuffer(_device, &transferInfo);
        if (transferBuffer == null)
            throw new OselException($"SDL_CreateGPUTransferBuffer failed: {SDL3.SDL_GetError()}");

        // Map, copy, unmap
        var mapped = SDL3.SDL_MapGPUTransferBuffer(_device, transferBuffer, false);
        fixed (byte* src = rgbaPixels)
        {
            Buffer.MemoryCopy(src, (void*)mapped, dataSize, dataSize);
        }
        SDL3.SDL_UnmapGPUTransferBuffer(_device, transferBuffer);

        // Upload via copy pass
        var cmdBuf = SDL3.SDL_AcquireGPUCommandBuffer(_device);
        var copyPass = SDL3.SDL_BeginGPUCopyPass(cmdBuf);

        var srcRegion = new SDL_GPUTextureTransferInfo
        {
            transfer_buffer = transferBuffer,
            offset = 0,
        };

        var dstRegion = new SDL_GPUTextureRegion
        {
            texture = Handle,
            w = (uint)Width,
            h = (uint)Height,
            d = 1,
        };

        SDL3.SDL_UploadToGPUTexture(copyPass, &srcRegion, &dstRegion, false);
        SDL3.SDL_EndGPUCopyPass(copyPass);
        SDL3.SDL_SubmitGPUCommandBuffer(cmdBuf);

        // Cleanup transfer buffer
        SDL3.SDL_ReleaseGPUTransferBuffer(_device, transferBuffer);
    }

    /// <summary>
    /// Re-uploads pixel data to the existing GPU texture. Dimensions must match.
    /// Used by the hot-reload system.
    /// </summary>
    internal bool ReloadPixelData(byte[] rgba, int width, int height)
    {
        if (width != Width || height != Height)
        {
            Console.WriteLine($"[HotReload] Texture size changed ({Width}x{Height} -> {width}x{height}), skipping reload.");
            return false;
        }
        UploadPixelData(_graphicsDevice, rgba);
        return true;
    }

    public void Dispose()
    {
        if (Handle != null)
        {
            SDL3.SDL_ReleaseGPUTexture(_device, Handle);
            Handle = null;
        }
        GC.SuppressFinalize(this);
    }
}
