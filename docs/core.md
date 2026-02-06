# Core

The core module provides the game loop, window management, colors, and debug tools.

## Game

Abstract base class for your game. Subclass it and override the lifecycle methods.

```csharp
public abstract class Game : IDisposable
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `GraphicsDevice` | `GraphicsDevice` | GPU device for rendering |
| `Window` | `GameWindow` | Application window |
| `Content` | `ContentManager` | Asset loader with caching |
| `Debug` | `DebugOverlay?` | Debug overlay (null until `EnableDebug()`) |

### Methods

| Method | Description |
|--------|-------------|
| `void Run()` | Start the game loop |
| `void Exit()` | Request the game to close |
| `void EnableDebug()` | Create the debug overlay |
| `void DisableDebug()` | Remove the debug overlay |
| `void Dispose()` | Clean up all resources |

### Virtual Methods (Override These)

| Method | Description |
|--------|-------------|
| `void Initialize()` | Called once before content loading |
| `void LoadContent()` | Load textures, fonts, sounds here |
| `void UnloadContent()` | Dispose custom resources |
| `void Update(GameTime gameTime)` | Game logic, called every frame |
| `void Draw(GameTime gameTime)` | Rendering, called every frame |

### Example

```csharp
public class MyGame : Game
{
    protected override void Initialize()
    {
        Window.Title = "My Game";
    }

    protected override void LoadContent()
    {
        // Load assets here
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.IsKeyDown(Keys.Escape)) Exit();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
    }
}
```

## GameTime

Immutable timing data passed to `Update` and `Draw` every frame.

```csharp
public readonly record struct GameTime(TimeSpan TotalGameTime, TimeSpan ElapsedGameTime)
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TotalGameTime` | `TimeSpan` | Time since game started |
| `ElapsedGameTime` | `TimeSpan` | Time since last frame |
| `DeltaTime` | `float` | Elapsed seconds (shortcut for frame-rate-independent movement) |

### Example

```csharp
protected override void Update(GameTime gameTime)
{
    float speed = 200f * gameTime.DeltaTime;
    _position += new Vector2(speed, 0);
}
```

## GameWindow

The application window.

```csharp
public class GameWindow
```

### Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `Title` | `string` | get/set | Window title |
| `Width` | `int` | get | Window width in pixels |
| `Height` | `int` | get | Window height in pixels |

## GraphicsDevice

Manages the GPU and render state.

```csharp
public unsafe class GraphicsDevice : IDisposable
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `BackbufferWidth` | `int` | Backbuffer width in pixels |
| `BackbufferHeight` | `int` | Backbuffer height in pixels |
| `RenderWidth` | `int` | Current render target width (or backbuffer) |
| `RenderHeight` | `int` | Current render target height (or backbuffer) |

### Methods

| Method | Description |
|--------|-------------|
| `void Clear(Color color)` | Clear the screen with a color |
| `void SetRenderTarget(RenderTarget2D? target)` | Redirect rendering to a texture (`null` for screen) |

## Color

RGBA color stored as floats (0.0-1.0).

```csharp
public readonly record struct Color(float R, float G, float B, float A)
```

### Constructors

| Constructor | Description |
|-------------|-------------|
| `Color(float r, float g, float b, float a)` | From floats (0.0-1.0) |
| `Color(byte r, byte g, byte b, byte a = 255)` | From bytes (0-255) |

### Named Colors

`White`, `Black`, `TransparentBlack`, `Red`, `Green`, `Blue`, `Yellow`, `Magenta`, `Cyan`, `CornflowerBlue`, `DarkGray`, `Gray`, `LightGray`

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `ToVector4()` | `Vector4` | Convert to Vector4 |
| `operator *` | `Color` | Scalar multiply (e.g. `Color.White * 0.5f` for 50% opacity) |

## DebugOverlay

On-screen debug info showing FPS, frame time, draw calls, sprite count, and memory usage.

```csharp
public class DebugOverlay
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Enabled` | `bool` | Show/hide the overlay |
| `Fps` | `int` | Frames per second (smoothed) |
| `FrameTimeMs` | `float` | Frame time in milliseconds |
| `DrawCalls` | `int` | Draw calls this frame |
| `SpriteCount` | `int` | Sprites rendered this frame |
| `ManagedMemoryMB` | `long` | Managed heap memory in MB |

### Methods

| Method | Description |
|--------|-------------|
| `void Update(GameTime gameTime)` | Update metrics (called automatically) |
| `void Draw(SpriteBatch spriteBatch, SpriteFont font)` | Render overlay — call after all game drawing |

### Example

```csharp
protected override void LoadContent()
{
    _debugFont = Content.LoadFont("fonts/debug", 14f);
    EnableDebug();
}

protected override void Update(GameTime gameTime)
{
    if (Keyboard.IsKeyPressed(Keys.F3))
        Debug!.Enabled = !Debug.Enabled;
}

protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.Black);
    // ... game drawing ...
    Debug?.Draw(_spriteBatch, _debugFont);
}
```
