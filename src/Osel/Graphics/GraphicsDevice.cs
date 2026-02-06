using Osel.Core;
using Osel.Platform;
using SDL;

namespace Osel.Graphics;

public unsafe class GraphicsDevice : IDisposable
{
    internal SDL_GPUDevice* DeviceHandle { get; }
    internal SDL_Window* WindowHandle { get; }
    internal SDL_GPUShaderFormat SupportedFormats { get; }
    internal SDL_GPUTextureFormat SwapchainFormat { get; }

    public int BackbufferWidth { get; private set; }
    public int BackbufferHeight { get; private set; }

    // Per-frame state
    internal SDL_GPUCommandBuffer* CommandBuffer { get; private set; }
    internal SDL_GPUTexture* SwapchainTexture { get; private set; }
    internal bool HasSwapchain { get; private set; }

    // Render target state
    private RenderTarget2D? _renderTarget;
    internal bool HasRenderTarget => _renderTarget != null;

    /// <summary>Returns the width of the current render target, or the backbuffer width if none is set.</summary>
    public int RenderWidth => _renderTarget?.Width ?? BackbufferWidth;

    /// <summary>Returns the height of the current render target, or the backbuffer height if none is set.</summary>
    public int RenderHeight => _renderTarget?.Height ?? BackbufferHeight;

    private Color _clearColor = Color.Black;
    private bool _isFirstRenderPass;

    internal GraphicsDevice(SDLGpuDevice gpuDevice, SDLWindow window)
    {
        DeviceHandle = gpuDevice.Handle;
        WindowHandle = window.Handle;
        SupportedFormats = gpuDevice.SupportedShaderFormats;
        SwapchainFormat = SDL3.SDL_GetGPUSwapchainTextureFormat(DeviceHandle, WindowHandle);
        BackbufferWidth = window.Width;
        BackbufferHeight = window.Height;
    }

    public void Clear(Color color)
    {
        _clearColor = color;
    }

    /// <summary>
    /// Sets the current render target. Pass null to render to the backbuffer (swapchain).
    /// </summary>
    public void SetRenderTarget(RenderTarget2D? target)
    {
        _renderTarget = target;
        _isFirstRenderPass = true;
    }

    internal void BeginFrame()
    {
        CommandBuffer = SDL3.SDL_AcquireGPUCommandBuffer(DeviceHandle);
        if (CommandBuffer == null)
            throw new OselException($"SDL_AcquireGPUCommandBuffer failed: {SDL3.SDL_GetError()}");

        SDL_GPUTexture* swapTex;
        uint w, h;
        if (!SDL3.SDL_AcquireGPUSwapchainTexture(CommandBuffer, WindowHandle, &swapTex, &w, &h))
        {
            HasSwapchain = false;
            return;
        }

        SwapchainTexture = swapTex;
        HasSwapchain = SwapchainTexture != null;
        if (HasSwapchain)
        {
            BackbufferWidth = (int)w;
            BackbufferHeight = (int)h;
        }
        _isFirstRenderPass = true;
    }

    internal SDL_GPURenderPass* BeginRenderPass()
    {
        SDL_GPUTexture* targetTexture;

        if (_renderTarget != null)
        {
            targetTexture = _renderTarget.Handle;
        }
        else
        {
            if (!HasSwapchain) return null;
            targetTexture = SwapchainTexture;
        }

        var colorTarget = new SDL_GPUColorTargetInfo
        {
            texture = targetTexture,
            load_op = _isFirstRenderPass ? SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR : SDL_GPULoadOp.SDL_GPU_LOADOP_LOAD,
            store_op = SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE,
            clear_color = new SDL_FColor { r = _clearColor.R, g = _clearColor.G, b = _clearColor.B, a = _clearColor.A },
        };

        _isFirstRenderPass = false;

        return SDL3.SDL_BeginGPURenderPass(CommandBuffer, &colorTarget, 1, null);
    }

    internal void EndRenderPass(SDL_GPURenderPass* renderPass)
    {
        SDL3.SDL_EndGPURenderPass(renderPass);
    }

    internal void EndFrame()
    {
        // If we got a swapchain but never started a render pass, do a clear-only pass
        if (HasSwapchain && _isFirstRenderPass && _renderTarget == null)
        {
            var renderPass = BeginRenderPass();
            if (renderPass != null)
                EndRenderPass(renderPass);
        }

        SDL3.SDL_SubmitGPUCommandBuffer(CommandBuffer);
        CommandBuffer = null;
        SwapchainTexture = null;
        HasSwapchain = false;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
