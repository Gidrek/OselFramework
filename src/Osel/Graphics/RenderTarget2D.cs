using Osel.Core;
using SDL;

namespace Osel.Graphics;

public unsafe class RenderTarget2D : IDisposable
{
    internal SDL_GPUTexture* Handle { get; private set; }
    public int Width { get; }
    public int Height { get; }

    private readonly SDL_GPUDevice* _device;

    public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
    {
        _device = graphicsDevice.DeviceHandle;
        Width = width;
        Height = height;

        var textureInfo = new SDL_GPUTextureCreateInfo
        {
            type = SDL_GPUTextureType.SDL_GPU_TEXTURETYPE_2D,
            format = graphicsDevice.SwapchainFormat,
            usage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_COLOR_TARGET
                  | SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER,
            width = (uint)width,
            height = (uint)height,
            layer_count_or_depth = 1,
            num_levels = 1,
            sample_count = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1,
        };

        Handle = SDL3.SDL_CreateGPUTexture(_device, &textureInfo);
        if (Handle == null)
            throw new OselException($"SDL_CreateGPUTexture (RenderTarget2D) failed: {SDL3.SDL_GetError()}");
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
