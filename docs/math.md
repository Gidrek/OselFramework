# Math

Lightweight math types implemented as `readonly record struct` with implicit conversions to/from `System.Numerics`.

## Vector2

2D vector for positions, velocities, and directions.

```csharp
public readonly record struct Vector2(float X, float Y)
```

### Static Fields

| Field | Value |
|-------|-------|
| `Vector2.Zero` | (0, 0) |
| `Vector2.One` | (1, 1) |
| `Vector2.UnitX` | (1, 0) |
| `Vector2.UnitY` | (0, 1) |

### Instance Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Length()` | `float` | Vector magnitude |
| `LengthSquared()` | `float` | Squared magnitude (faster, avoids sqrt) |
| `Normalized()` | `Vector2` | Unit vector (returns Zero if length is 0) |

### Static Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Dot(a, b)` | `float` | Dot product |
| `Distance(a, b)` | `float` | Distance between two points |
| `DistanceSquared(a, b)` | `float` | Squared distance (avoids sqrt) |
| `Lerp(a, b, t)` | `Vector2` | Linear interpolation |
| `Min(a, b)` | `Vector2` | Component-wise minimum |
| `Max(a, b)` | `Vector2` | Component-wise maximum |
| `Clamp(value, min, max)` | `Vector2` | Component-wise clamp |

### Operators

`+`, `-`, `*` (scalar), `/` (scalar), unary `-`, implicit conversion to/from `System.Numerics.Vector2`

### Example

```csharp
var position = new Vector2(100, 200);
var velocity = new Vector2(1, 0).Normalized() * 150f;
position += velocity * gameTime.DeltaTime;

float dist = Vector2.Distance(player, enemy);
var smoothed = Vector2.Lerp(current, target, 0.1f);
```

## Vector3

3D vector.

```csharp
public readonly record struct Vector3(float X, float Y, float Z)
```

### Static Fields

`Zero`, `One`, `UnitX`, `UnitY`, `UnitZ`

### Instance Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Length()` | `float` | Vector magnitude |
| `LengthSquared()` | `float` | Squared magnitude |
| `Normalized()` | `Vector3` | Unit vector |

### Static Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Dot(a, b)` | `float` | Dot product |
| `Cross(a, b)` | `Vector3` | Cross product |
| `Lerp(a, b, t)` | `Vector3` | Linear interpolation |

### Operators

`+`, `-`, `*` (scalar), unary `-`, implicit conversion to/from `System.Numerics.Vector3`

## Vector4

4D vector, also used for RGBA color representation.

```csharp
public readonly record struct Vector4(float X, float Y, float Z, float W)
```

### Static Fields

`Zero`, `One`

### Instance Methods

`Length()`, `LengthSquared()`

### Static Methods

`Lerp(a, b, t)`

### Operators

`+`, `-`, `*` (scalar), implicit conversion to/from `System.Numerics.Vector4`

## Rectangle

Axis-aligned integer rectangle for sprite regions, collision bounds, and UI layout.

```csharp
public readonly record struct Rectangle(int X, int Y, int Width, int Height)
```

### Static Fields

| Field | Value |
|-------|-------|
| `Rectangle.Empty` | (0, 0, 0, 0) |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Left` | `int` | X |
| `Top` | `int` | Y |
| `Right` | `int` | X + Width |
| `Bottom` | `int` | Y + Height |
| `Center` | `Vector2` | Center point |
| `Location` | `Vector2` | Top-left corner |
| `Size` | `Vector2` | Width and height |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Contains(int x, int y)` | `bool` | Point containment test |
| `Contains(Vector2 point)` | `bool` | Point containment test |
| `Intersects(Rectangle other)` | `bool` | AABB overlap test |
| `Offset(Vector2 amount)` | `Rectangle` | Move by offset |
| `Offset(int dx, int dy)` | `Rectangle` | Move by offset |

### Static Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Intersect(a, b)` | `Rectangle` | Overlapping region |
| `Union(a, b)` | `Rectangle` | Bounding rectangle of both |

### Example

```csharp
var bounds = new Rectangle(10, 20, 32, 32);
bool hit = bounds.Contains(Mouse.Position);
bool collides = bounds.Intersects(enemyBounds);
var moved = bounds.Offset(new Vector2(5, 0));
```

## Matrix4x4

4x4 transformation matrix for 2D camera transforms and orthographic projection.

```csharp
public readonly record struct Matrix4x4(
    float M11, float M12, float M13, float M14,
    float M21, float M22, float M23, float M24,
    float M31, float M32, float M33, float M34,
    float M41, float M42, float M43, float M44)
```

### Static Fields

| Field | Description |
|-------|-------------|
| `Matrix4x4.Identity` | Identity matrix |

### Static Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar)` | `Matrix4x4` | Orthographic projection |
| `CreateTranslation(float x, float y)` | `Matrix4x4` | 2D translation |
| `CreateScale(float scale)` | `Matrix4x4` | Uniform scale |
| `CreateScale(float scaleX, float scaleY)` | `Matrix4x4` | Non-uniform scale |
| `CreateRotationZ(float radians)` | `Matrix4x4` | Rotation around Z axis |
| `Invert(Matrix4x4 matrix)` | `Matrix4x4?` | Matrix inverse (null if singular) |

### Operators

`*` (matrix multiplication), implicit conversion to/from `System.Numerics.Matrix4x4`

## MathHelper

Common math utilities.

```csharp
public static class MathHelper
```

### Constants

| Constant | Value | Description |
|----------|-------|-------------|
| `Pi` | 3.14159... | Pi |
| `TwoPi` | 6.28318... | 2 * Pi (full rotation) |
| `HalfPi` | 1.57079... | Pi / 2 (quarter turn) |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Clamp(float value, float min, float max)` | `float` | Clamp value to range |
| `Lerp(float a, float b, float t)` | `float` | Linear interpolation |
| `ToRadians(float degrees)` | `float` | Degrees to radians |
| `ToDegrees(float radians)` | `float` | Radians to degrees |

### Example

```csharp
float angle = MathHelper.ToRadians(45f);
float clamped = MathHelper.Clamp(health, 0, 100);
float smooth = MathHelper.Lerp(current, target, 0.1f);
```
