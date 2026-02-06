# Tilemap

JSON-based tile maps with multiple layers, camera-based culling, and collision detection.

## TileMap

```csharp
public class TileMap
```

### Constructor

```csharp
public TileMap(Texture2D tileset, int tileWidth, int tileHeight, List<TileLayer> layers)
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Tileset` | `Texture2D` | Tileset texture |
| `TileWidth` | `int` | Tile width in pixels |
| `TileHeight` | `int` | Tile height in pixels |
| `TilesetColumns` | `int` | Number of columns in the tileset (computed from texture width) |
| `Layers` | `List<TileLayer>` | All tile layers |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetSourceRect(int tileId)` | `Rectangle?` | Get source rectangle for a tile ID (null for empty) |
| `Draw(SpriteBatch spriteBatch, Camera2D? camera = null, int viewportWidth = 0, int viewportHeight = 0)` | `void` | Draw all visible layers (with camera culling) |
| `GetCollisionLayer()` | `TileLayer?` | Get the first layer marked as collision |
| `IsSolid(int col, int row)` | `bool` | Check if a tile is solid in the collision layer |

### Loading

```csharp
var map = Content.Load<TileMap>("maps/level1");  // loads maps/level1.json
```

### JSON Format

```json
{
    "tileWidth": 16,
    "tileHeight": 16,
    "tileset": "tiles",
    "layers": [
        {
            "name": "ground",
            "width": 20,
            "height": 15,
            "tiles": [1, 2, 1, 3, 0, ...],
            "collision": false
        },
        {
            "name": "walls",
            "width": 20,
            "height": 15,
            "tiles": [0, 0, 5, 5, 0, ...],
            "collision": true
        }
    ]
}
```

- **Tile IDs** are 1-based (0 = empty/no tile)
- The `tileset` field references a texture name loaded via `ContentManager`
- Layers are drawn in order (first = bottom)

### Example

```csharp
private TileMap _map = null!;
private Camera2D _camera = null!;

protected override void LoadContent()
{
    _map = Content.Load<TileMap>("maps/demo");
    _camera = new Camera2D
    {
        Zoom = 3f,
        Bounds = new Rectangle(0, 0,
            _map.Layers[0].Width * _map.TileWidth,
            _map.Layers[0].Height * _map.TileHeight)
    };
}

protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.Black);
    var viewMatrix = _camera.GetViewMatrix(
        GraphicsDevice.BackbufferWidth, GraphicsDevice.BackbufferHeight);

    _spriteBatch.Begin(transformMatrix: viewMatrix);
    _map.Draw(_spriteBatch, _camera,
        GraphicsDevice.BackbufferWidth, GraphicsDevice.BackbufferHeight);
    _spriteBatch.End();
}
```

## TileLayer

A single layer of tile data.

```csharp
public class TileLayer
```

### Constructor

```csharp
public TileLayer(string name, int width, int height, int tileWidth, int tileHeight, int[] tiles, bool isCollision = false)
```

### Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `Name` | `string` | get | Layer name |
| `Width` | `int` | get | Width in tiles |
| `Height` | `int` | get | Height in tiles |
| `TileWidth` | `int` | get | Tile width in pixels |
| `TileHeight` | `int` | get | Tile height in pixels |
| `Visible` | `bool` | get/set | Whether the layer is drawn (default: true) |
| `Opacity` | `float` | get/set | Layer opacity 0.0-1.0 (default: 1.0) |
| `IsCollision` | `bool` | get/set | Whether this layer is used for collision |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetTile(int col, int row)` | `int` | Get tile ID at position (0 = empty) |
| `SetTile(int col, int row, int tileId)` | `void` | Set tile ID at position |
| `IsSolid(int col, int row)` | `bool` | True if tile at position is non-zero |

## Collision

Static utility for AABB collision detection and tile-based movement resolution.

```csharp
public static class Collision
```

*Namespace: `Osel.Math`*

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Intersects(Rectangle a, Rectangle b)` | `bool` | AABB overlap test |
| `GetOverlap(Rectangle a, Rectangle b)` | `Rectangle?` | Overlapping region (null if none) |
| `GetSeparation(Rectangle a, Rectangle b)` | `Vector2` | Minimum translation to separate two rectangles |
| `MoveAndSlide(Rectangle entityBounds, Vector2 velocity, TileMap map)` | `Vector2` | Resolve movement against tile collision (axis-separated) |

### MoveAndSlide

The `MoveAndSlide` method resolves movement against a tilemap's collision layer. It separates horizontal and vertical movement to allow sliding along walls:

```csharp
protected override void Update(GameTime gameTime)
{
    var velocity = movement.Normalized() * speed * gameTime.DeltaTime;

    // Create collision bounds (can be smaller than visual sprite)
    int offset = (16 - 12) / 2;
    var bounds = new Rectangle(
        (int)_position.X + offset,
        (int)_position.Y + offset,
        12, 12);

    // Resolve movement against tilemap collision
    var resolved = Collision.MoveAndSlide(bounds, velocity, _tileMap);
    _position += resolved;
}
```

The method checks each axis independently, so the player slides along walls naturally instead of stopping completely.
