# Getting Started

Set up a new Osel project and draw your first sprite in under 5 minutes.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Project Setup

1. Create a new console project:

```bash
dotnet new console -n MyGame
cd MyGame
```

2. Add a project reference to Osel (or a NuGet package when available):

```xml
<!-- MyGame.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Osel\Osel.csproj" />
  </ItemGroup>

  <!-- Copy content files to output -->
  <ItemGroup>
    <None Update="Content\**\*" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

3. Create a `Content/` folder and add a PNG image (e.g. `Content/player.png`).

## Your First Game

```csharp
// MyGame.cs
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

## Run It

```bash
dotnet run
```

## Content Directory

Place assets under `Content/` in your project root. The `ContentManager` resolves files relative to the output directory and auto-detects extensions:

```
Content/
├── player.png
├── fonts/
│   └── arial.ttf
├── sounds/
│   └── jump.wav
├── music/
│   └── theme.ogg
└── maps/
    └── level1.json
```

Supported formats:

| Type | Extensions | Loader |
|------|-----------|--------|
| Texture2D | `.png`, `.jpg`, `.bmp` | StbImageSharp |
| SoundEffect | `.wav` | SDL3 `SDL_LoadWAV` |
| Music | `.ogg` | NVorbis streaming |
| SpriteFont | `.ttf` | StbTrueTypeSharp |
| TileMap | `.json` | Built-in JSON parser |

## Next Steps

- [Core](core.md) — Game loop, window, colors
- [Graphics](graphics.md) — SpriteBatch drawing, textures, fonts, shaders
- [Input](input.md) — Keyboard, mouse, gamepad
- [Audio](audio.md) — Sound effects and music
