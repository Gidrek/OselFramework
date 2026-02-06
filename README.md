# Osel Engine

A 2D game framework for C# inspired by XNA/MonoGame, built on .NET 10 and SDL3 GPU API. Modern, minimal, and designed for pixel-art games and beyond.

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

1. Create a new console project and reference Osel:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Osel\Osel.csproj" />
  </ItemGroup>
  <ItemGroup>
    <None Update="Content\**\*" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

2. Add a PNG image to `Content/player.png`, then write your game:

```csharp
using Osel.Core;
using Osel.Graphics;
using Osel.Input;
using Osel.Math;

public class MyGame : Game
{
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _player = null!;
    private Vector2 _position = new(100, 100);

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _player = Content.Load<Texture2D>("player");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.IsKeyDown(Keys.Escape)) Exit();

        float speed = 200f * gameTime.DeltaTime;

        if (Keyboard.IsKeyDown(Keys.W)) _position += new Vector2(0, -speed);
        if (Keyboard.IsKeyDown(Keys.S)) _position += new Vector2(0, speed);
        if (Keyboard.IsKeyDown(Keys.A)) _position += new Vector2(-speed, 0);
        if (Keyboard.IsKeyDown(Keys.D)) _position += new Vector2(speed, 0);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.Draw(_player, _position, Color.White);
        _spriteBatch.End();
    }
}
```

```csharp
// Program.cs
using var game = new MyGame();
game.Run();
```

3. Run it:

```bash
dotnet run
```

## Documentation

Full documentation is available in the [`docs/`](docs/) directory:

| Page | Description |
|------|-------------|
| [Getting Started](docs/getting-started.md) | Installation, project setup, first game |
| [Core](docs/core.md) | Game, GameTime, GameWindow, Color, DebugOverlay |
| [Graphics](docs/graphics.md) | SpriteBatch, Texture2D, RenderTarget2D, SpriteFont, Effect |
| [Input](docs/input.md) | Keyboard, Mouse, Gamepad |
| [Audio](docs/audio.md) | SoundEffect, Music |
| [Math](docs/math.md) | Vector2, Vector3, Vector4, Rectangle, Matrix4x4, MathHelper |
| [Camera](docs/camera.md) | Camera2D |
| [Animation](docs/animation.md) | SpriteAnimation, AnimatedSprite |
| [Tilemap](docs/tilemap.md) | TileMap, TileLayer, Collision |
| [Content](docs/content.md) | ContentManager, hot-reload, supported formats |
| [Platform](docs/platform.md) | AOT compilation, platform targets, debug overlay |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| [ppy.SDL3-CS](https://www.nuget.org/packages/ppy.SDL3-CS) | latest | SDL3 bindings (windowing, input, audio, GPU) |
| [StbImageSharp](https://www.nuget.org/packages/StbImageSharp) | 2.30.15 | PNG/JPG/BMP image loading |
| [StbTrueTypeSharp](https://www.nuget.org/packages/StbTrueTypeSharp) | 1.26.12 | TTF font rasterization |
| [NVorbis](https://www.nuget.org/packages/NVorbis) | 0.10.5 | OGG Vorbis audio decoding |

## License

[MIT](LICENSE)
