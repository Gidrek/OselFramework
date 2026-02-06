# Content

Asset loading with caching, automatic file extension resolution, and development hot-reload.

## ContentManager

```csharp
public class ContentManager : IDisposable
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `RootDirectory` | `string` | `"Content"` | Base directory for asset loading (relative to output directory) |

### Constructor

```csharp
public ContentManager(GraphicsDevice graphicsDevice, string rootDirectory = "Content")
```

Created automatically by `Game` — access it via `Content`:

```csharp
protected override void LoadContent()
{
    var texture = Content.Load<Texture2D>("player");
}
```

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Load<T>(string assetName)` | `T` | Load and cache an asset |
| `LoadFont(string assetName, float fontSize = 32f)` | `SpriteFont` | Load a TTF font at a specific size |
| `EnableHotReload()` | `void` | Start watching for file changes |
| `DisableHotReload()` | `void` | Stop watching for file changes |
| `ProcessPendingReloads()` | `void` | Apply pending hot-reload changes (called automatically) |
| `Unload()` | `void` | Dispose all cached assets |
| `Dispose()` | `void` | Unload and clean up |

### Supported Types

| Type | Method | Extensions | Description |
|------|--------|-----------|-------------|
| `Texture2D` | `Load<Texture2D>("name")` | `.png`, `.jpg`, `.bmp` | Image → GPU texture |
| `SoundEffect` | `Load<SoundEffect>("name")` | `.wav` | WAV audio clip |
| `Music` | `Load<Music>("name")` | `.ogg` | OGG streaming audio |
| `SpriteFont` | `LoadFont("name", size)` | `.ttf` | TTF → glyph atlas |
| `TileMap` | `Load<TileMap>("name")` | `.json` | JSON tile map + tileset |

### Asset Name Resolution

Pass the asset name without extension. The `ContentManager` searches for files in `RootDirectory` and auto-detects the extension:

```csharp
Content.Load<Texture2D>("player");         // → Content/player.png (or .jpg, .bmp)
Content.Load<SoundEffect>("sounds/jump");  // → Content/sounds/jump.wav
Content.Load<Music>("music/theme");        // → Content/music/theme.ogg
Content.LoadFont("fonts/arial", 24f);      // → Content/fonts/arial.ttf
Content.Load<TileMap>("maps/level1");      // → Content/maps/level1.json
```

### Caching

Assets are cached by name. Calling `Load<T>("player")` multiple times returns the same instance. Call `Unload()` to dispose all cached assets.

### Project Setup

Ensure content files are copied to the output directory:

```xml
<!-- In your .csproj -->
<ItemGroup>
  <None Update="Content\**\*" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

### Example Directory

```
Content/
├── player.png
├── tiles.png
├── fonts/
│   ├── arial.ttf
│   └── debug.ttf
├── sounds/
│   └── jump.wav
├── music/
│   └── theme.ogg
└── maps/
    └── demo.json
```

## Hot-Reload

During development, enable hot-reload to automatically update textures when files change on disk:

```csharp
protected override void LoadContent()
{
    Content.EnableHotReload();
    // Edit player.png in your image editor — changes appear immediately
}
```

Hot-reload uses `FileSystemWatcher` with debouncing. Currently supports `Texture2D` reloading. Disable it for release builds:

```csharp
Content.DisableHotReload();
```
