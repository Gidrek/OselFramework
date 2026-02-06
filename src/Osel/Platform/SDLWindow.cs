using Osel.Core;
using SDL;

namespace Osel.Platform;

internal unsafe class SDLWindow : IDisposable
{
    public SDL_Window* Handle { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public SDLWindow(string title, int width, int height)
    {
        Width = width;
        Height = height;

        Handle = SDL3.SDL_CreateWindow(title, width, height, 0);
        if (Handle == null)
            throw new OselException($"SDL_CreateWindow failed: {SDL3.SDL_GetError()}");
    }

    public void SetTitle(string title)
    {
        SDL3.SDL_SetWindowTitle(Handle, title);
    }

    public void GetSize(out int w, out int h)
    {
        int tw, th;
        SDL3.SDL_GetWindowSize(Handle, &tw, &th);
        w = tw;
        h = th;
        Width = w;
        Height = h;
    }

    public void Dispose()
    {
        if (Handle != null)
        {
            SDL3.SDL_DestroyWindow(Handle);
            Handle = null;
        }
    }
}
