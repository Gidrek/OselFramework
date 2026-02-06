using Osel.Platform;

namespace Osel.Core;

public class GameWindow
{
    internal SDLWindow SdlWindow { get; }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            SdlWindow.SetTitle(value);
        }
    }

    public int Width => SdlWindow.Width;
    public int Height => SdlWindow.Height;

    private string _title;

    internal GameWindow(SDLWindow sdlWindow, string title)
    {
        SdlWindow = sdlWindow;
        _title = title;
    }
}
