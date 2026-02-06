# Camera

2D camera with position, zoom, rotation, smooth follow, and optional bounds clamping.

## Camera2D

```csharp
public class Camera2D
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Position` | `Vector2` | (0, 0) | Camera center in world coordinates |
| `Zoom` | `float` | 1.0 | Zoom level (>1 = zoomed in) |
| `Rotation` | `float` | 0.0 | Rotation in radians |
| `Bounds` | `Rectangle?` | null | World bounds to clamp the camera within |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetViewMatrix(int viewportWidth, int viewportHeight)` | `Matrix4x4` | View matrix for SpriteBatch |
| `ScreenToWorld(Vector2 screenPos, int viewportWidth, int viewportHeight)` | `Vector2` | Convert screen coordinates to world coordinates |
| `WorldToScreen(Vector2 worldPos, int viewportWidth, int viewportHeight)` | `Vector2` | Convert world coordinates to screen coordinates |
| `GetVisibleArea(int viewportWidth, int viewportHeight)` | `Rectangle` | Get the world-space rectangle currently visible |
| `Follow(Vector2 target, float lerp = 1f)` | `void` | Move camera toward target (lerp=1 for instant, <1 for smooth) |

### Basic Example

```csharp
private Camera2D _camera = null!;

protected override void LoadContent()
{
    _camera = new Camera2D
    {
        Position = new Vector2(400, 300),
        Zoom = 2f
    };
}

protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.Black);

    var viewMatrix = _camera.GetViewMatrix(
        GraphicsDevice.BackbufferWidth,
        GraphicsDevice.BackbufferHeight);

    _spriteBatch.Begin(transformMatrix: viewMatrix);
    // All draws use world coordinates
    _spriteBatch.Draw(_texture, new Vector2(100, 100), Color.White);
    _spriteBatch.End();
}
```

### Smooth Follow with Bounds

```csharp
protected override void LoadContent()
{
    _camera = new Camera2D
    {
        Zoom = 3f,
        Bounds = new Rectangle(0, 0, mapWidth, mapHeight)
    };
}

protected override void Update(GameTime gameTime)
{
    // Smooth follow the player (lerp < 1 for lag)
    _camera.Follow(_playerPosition, 0.1f);

    // Zoom with keyboard
    if (Keyboard.IsKeyDown(Keys.E))
        _camera.Zoom = MathF.Min(_camera.Zoom + 2f * gameTime.DeltaTime, 8f);
    if (Keyboard.IsKeyDown(Keys.Q))
        _camera.Zoom = MathF.Max(_camera.Zoom - 2f * gameTime.DeltaTime, 1f);
}
```

### Screen-to-World Conversion

Useful for mouse picking in zoomed/rotated views:

```csharp
var worldPos = _camera.ScreenToWorld(
    Mouse.Position,
    GraphicsDevice.BackbufferWidth,
    GraphicsDevice.BackbufferHeight);
```

### View Matrix Composition

The view matrix is built as: `Translation(-position) * RotationZ * Scale(zoom) * Translation(viewportCenter)`. When `Bounds` is set, the position is clamped so the visible area stays within bounds (accounting for zoom).
