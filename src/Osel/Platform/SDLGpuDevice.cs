using Osel.Core;
using SDL;

namespace Osel.Platform;

internal unsafe class SDLGpuDevice : IDisposable
{
    public SDL_GPUDevice* Handle { get; private set; }
    public SDL_GPUShaderFormat SupportedShaderFormats { get; private set; }

    public SDLGpuDevice(SDLWindow window)
    {
        // Request multiple shader formats — SDL will pick the best backend
        var requestedFormats = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV
                             | SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL
                             | SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL;

        Handle = SDL3.SDL_CreateGPUDevice(requestedFormats, true, (byte*)null);
        if (Handle == null)
            throw new OselException($"SDL_CreateGPUDevice failed: {SDL3.SDL_GetError()}");

        if (!SDL3.SDL_ClaimWindowForGPUDevice(Handle, window.Handle))
            throw new OselException($"SDL_ClaimWindowForGPUDevice failed: {SDL3.SDL_GetError()}");

        SupportedShaderFormats = SDL3.SDL_GetGPUShaderFormats(Handle);
    }

    public void ReleaseWindow(SDLWindow window)
    {
        SDL3.SDL_ReleaseWindowFromGPUDevice(Handle, window.Handle);
    }

    public void Dispose()
    {
        if (Handle != null)
        {
            SDL3.SDL_DestroyGPUDevice(Handle);
            Handle = null;
        }
    }
}
