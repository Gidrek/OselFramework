using Osel.Audio;
using Osel.Input;
using SDL;

namespace Osel.Platform;

internal unsafe class PlatformBackend : IDisposable
{
    public SDLWindow Window { get; }
    public SDLGpuDevice GpuDevice { get; }

    public PlatformBackend(string title, int width, int height)
    {
        SDLPlatform.Initialize();
        Window = new SDLWindow(title, width, height);
        GpuDevice = new SDLGpuDevice(Window);
        AudioManager.Initialize();
    }

    /// <summary>
    /// Polls SDL events. Returns false if the application should quit.
    /// </summary>
    public bool PollEvents()
    {
        InputManager.BeginFrame();

        SDL_Event evt;
        while (SDL3.SDL_PollEvent(&evt))
        {
            if (evt.type == (uint)SDL_EventType.SDL_EVENT_QUIT)
                return false;

            InputManager.ProcessEvent(ref evt);
        }

        return true;
    }

    public void UpdateAudio() => AudioManager.UpdateMusic();

    public void Dispose()
    {
        AudioManager.Shutdown();
        InputManager.Shutdown();
        GpuDevice.ReleaseWindow(Window);
        GpuDevice.Dispose();
        Window.Dispose();
        SDLPlatform.Shutdown();
    }
}
