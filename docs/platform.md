# Platform

AOT compilation, platform abstraction, and deployment targets.

## Platform Architecture

Osel uses a `PlatformBackend` abstraction that coordinates all SDL3 interactions (window creation, event polling, audio, shutdown). The `Game` class and `ContentManager` have no direct SDL dependencies, making the engine ready for future platform backends (e.g. console SDKs).

```
Game (platform-agnostic)
  └── PlatformBackend (SDL3 implementation)
        ├── SDLWindow
        ├── SDLGpuDevice
        ├── InputManager
        └── AudioManager
```

## AOT Compilation

Osel is AOT-compatible out of the box. The engine project has `IsAotCompatible` and `EnableTrimAnalyzer` enabled.

### Publishing with Native AOT

Add `PublishAot` to your game project:

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
</PropertyGroup>
```

Publish for your target platform:

```bash
# macOS (current machine)
dotnet publish -c Release

# macOS ARM64
dotnet publish -c Release -r osx-arm64

# macOS x64
dotnet publish -c Release -r osx-x64

# Windows x64
dotnet publish -c Release -r win-x64

# Linux x64
dotnet publish -c Release -r linux-x64
```

AOT produces a single native executable with no .NET runtime dependency, which is ideal for game distribution and a prerequisite for console platforms.

### Project Configuration

A minimal AOT-ready game project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <PublishAot>true</PublishAot>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Osel\Osel.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update="Content\**\*" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

## Target Platforms

| Platform | Runtime ID | Graphics Backend | Status |
|----------|-----------|-------------------|--------|
| macOS ARM64 | `osx-arm64` | Metal | Supported |
| macOS x64 | `osx-x64` | Metal | Supported |
| Windows x64 | `win-x64` | Vulkan / D3D12 | Supported |
| Linux x64 | `linux-x64` | Vulkan | Supported |
| Consoles | — | — | Architecture ready (requires vendor SDK access) |

SDL3 GPU API automatically selects the best available graphics backend for each platform.

## Debug Overlay

Built-in on-screen debug information. See [Core](core.md#debugoverlay) for the full API.

```csharp
protected override void LoadContent()
{
    _debugFont = Content.LoadFont("fonts/debug", 14f);
    EnableDebug();
}

protected override void Update(GameTime gameTime)
{
    // Toggle with F3
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

The overlay displays:
- **FPS** — Frames per second (smoothed every 0.5s)
- **Frame time** — Milliseconds per frame
- **Draw calls** — SpriteBatch draw calls this frame
- **Sprite count** — Total sprites rendered this frame
- **Memory** — Managed heap usage in MB

## Dependencies

All dependencies are pure C# or have native binaries provided via NuGet:

| Package | Version | Purpose |
|---------|---------|---------|
| [ppy.SDL3-CS](https://www.nuget.org/packages/ppy.SDL3-CS) | latest | SDL3 C# bindings |
| [StbImageSharp](https://www.nuget.org/packages/StbImageSharp) | 2.30.15 | Image loading (PNG, JPG, BMP) |
| [StbTrueTypeSharp](https://www.nuget.org/packages/StbTrueTypeSharp) | 1.26.12 | TTF font rasterization |
| [NVorbis](https://www.nuget.org/packages/NVorbis) | 0.10.5 | OGG Vorbis decoding |
