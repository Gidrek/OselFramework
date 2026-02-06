# Osel Engine

Osel is a 2D game framework for C# inspired by XNA/MonoGame, built on .NET 10 and SDL3 GPU API. It provides a modern, minimal API for making 2D games with pixel-art or any other style.

## Features

- **Modern graphics backend** — SDL3 GPU API (Vulkan, Metal, D3D12)
- **Simple game loop** — `Initialize`, `LoadContent`, `Update`, `Draw`
- **SpriteBatch rendering** — Sprites, text, rotation, scale, origin, flip
- **Input** — Keyboard, mouse, and gamepad (up to 4 controllers)
- **Audio** — WAV and OGG playback via SDL3 audio streams + NVorbis
- **Fonts** — Load any TTF file directly, no pipeline tools needed
- **Camera** — 2D camera with follow, zoom, rotation, and bounds clamping
- **Tilemaps** — JSON-based tile maps with collision detection
- **Animations** — Spritesheet-based sprite animation system
- **Shaders** — Custom GLSL shaders compiled to SPIR-V
- **Hot-reload** — Asset hot-reload during development
- **AOT ready** — Native AOT compilation support for all platforms
- **Console ready** — Platform abstraction layer designed for future console ports

## Quick Start

```csharp
using Osel.Core;
using Osel.Graphics;
using Osel.Input;

public class MyGame : Game
{
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _texture = null!;

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _texture = Content.Load<Texture2D>("player");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.IsKeyDown(Keys.Escape)) Exit();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_texture, new Osel.Math.Vector2(100, 100), Color.White);
        _spriteBatch.End();
    }
}
```

```csharp
// Program.cs
using var game = new MyGame();
game.Run();
```

## Documentation

| Page | Description |
|------|-------------|
| [Getting Started](getting-started.md) | Installation, project setup, first game |
| [Core](core.md) | Game, GameTime, GameWindow, Color, DebugOverlay |
| [Graphics](graphics.md) | SpriteBatch, Texture2D, RenderTarget2D, SpriteFont, Effect |
| [Input](input.md) | Keyboard, Mouse, Gamepad |
| [Audio](audio.md) | SoundEffect, Music |
| [Math](math.md) | Vector2, Vector3, Vector4, Rectangle, Matrix4x4, MathHelper |
| [Camera](camera.md) | Camera2D |
| [Animation](animation.md) | SpriteAnimation, AnimatedSprite |
| [Tilemap](tilemap.md) | TileMap, TileLayer, Collision |
| [Content](content.md) | ContentManager, hot-reload, supported formats |
| [Platform](platform.md) | AOT compilation, platform targets, debug overlay |

## Requirements

- .NET 10 SDK
- SDL3 native libraries (provided by `ppy.SDL3-CS`)

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| ppy.SDL3-CS | latest | SDL3 bindings (windowing, input, audio, GPU) |
| StbImageSharp | 2.30.15 | PNG/JPG/BMP image loading |
| StbTrueTypeSharp | 1.26.12 | TTF font rasterization |
| NVorbis | 0.10.5 | OGG Vorbis audio decoding |
