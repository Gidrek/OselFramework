using System.Diagnostics;
using Osel.Content;
using Osel.Graphics;
using Osel.Platform;

namespace Osel.Core;

public abstract class Game : IDisposable
{
    public GraphicsDevice GraphicsDevice { get; private set; } = null!;
    public GameWindow Window { get; private set; } = null!;
    public ContentManager Content { get; private set; } = null!;
    public DebugOverlay? Debug { get; private set; }

    private PlatformBackend _platform = null!;
    private bool _isRunning;
    private readonly Stopwatch _gameTimer = new();

    protected virtual void Initialize() { }
    protected virtual void LoadContent() { }
    protected virtual void UnloadContent() { }
    protected virtual void Update(GameTime gameTime) { }
    protected virtual void Draw(GameTime gameTime) { }

    public void Run()
    {
        InitializePlatform();
        Initialize();
        LoadContent();

        _isRunning = true;
        _gameTimer.Start();
        var previousTime = TimeSpan.Zero;

        while (_isRunning)
        {
            if (!_platform.PollEvents())
            {
                Exit();
                continue;
            }

            Content.ProcessPendingReloads();

            var currentTime = _gameTimer.Elapsed;
            var elapsed = currentTime - previousTime;
            previousTime = currentTime;
            var gameTime = new GameTime(currentTime, elapsed);

            Update(gameTime);
            Debug?.Update(gameTime);
            _platform.UpdateAudio();

            GraphicsDevice.BeginFrame();
            Draw(gameTime);
            GraphicsDevice.EndFrame();
        }

        UnloadContent();
        Dispose();
    }

    public void Exit() => _isRunning = false;

    public void EnableDebug() => Debug ??= new DebugOverlay();

    public void DisableDebug() => Debug = null;

    private void InitializePlatform()
    {
        _platform = new PlatformBackend("Osel Game", 1280, 720);

        Window = new GameWindow(_platform.Window, "Osel Game");
        GraphicsDevice = new GraphicsDevice(_platform.GpuDevice, _platform.Window);
        Content = new ContentManager(GraphicsDevice, "Content");
    }

    public void Dispose()
    {
        Content?.Dispose();
        GraphicsDevice?.Dispose();
        _platform?.Dispose();
        GC.SuppressFinalize(this);
    }
}
