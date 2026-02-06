# Graphics

Sprite rendering, textures, render targets, fonts, and custom shaders.

## SpriteBatch

Batches 2D sprite draw calls for efficient GPU rendering. Uses a pull-model architecture with storage buffers.

```csharp
public unsafe class SpriteBatch : IDisposable
```

### Constructor

```csharp
public SpriteBatch(GraphicsDevice device)
```

### Methods

| Method | Description |
|--------|-------------|
| `void Begin(Effect? effect = null, Matrix4x4? transformMatrix = null)` | Start a batch (optional shader and transform) |
| `void End()` | Flush and render all queued sprites |
| `void Dispose()` | Release GPU resources |

### Draw Overloads

```csharp
// Simple position + color
void Draw(Texture2D texture, Vector2 position, Color color)

// With source rectangle (sprite sheet region)
void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color)

// Full control: rotation, origin, uniform scale, flip
void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color,
    float rotation, Vector2 origin, float scale,
    SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)

// Full control: rotation, origin, non-uniform scale, flip
void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color,
    float rotation, Vector2 origin, Vector2 scale,
    SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)

// Destination rectangle (stretch to fit)
void Draw(Texture2D texture, Rectangle destinationRect, Rectangle? sourceRect, Color color)
```

### DrawString Overloads

```csharp
// Simple text
void DrawString(SpriteFont font, string text, Vector2 position, Color color)

// With rotation, origin, uniform scale
void DrawString(SpriteFont font, string text, Vector2 position, Color color,
    float rotation, Vector2 origin, float scale,
    SpriteEffects effects = SpriteEffects.None)

// With rotation, origin, non-uniform scale
void DrawString(SpriteFont font, string text, Vector2 position, Color color,
    float rotation, Vector2 origin, Vector2 scale,
    SpriteEffects effects = SpriteEffects.None)
```

### Example

```csharp
_spriteBatch.Begin();

// Simple sprite
_spriteBatch.Draw(_texture, new Vector2(100, 100), Color.White);

// Rotated, scaled, centered origin
var origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
_spriteBatch.Draw(_texture, new Vector2(200, 200), null, Color.White,
    rotation: 0.5f, origin: origin, scale: 2f, effects: SpriteEffects.FlipHorizontally);

// Text
_spriteBatch.DrawString(_font, "Score: 100", new Vector2(10, 10), Color.Yellow);

_spriteBatch.End();
```

### Using with Camera

Pass a view matrix to `Begin()` to apply camera transforms:

```csharp
var viewMatrix = _camera.GetViewMatrix(GraphicsDevice.BackbufferWidth, GraphicsDevice.BackbufferHeight);
_spriteBatch.Begin(transformMatrix: viewMatrix);
// All draws are now in world coordinates
_spriteBatch.End();
```

## SpriteEffects

```csharp
[Flags]
public enum SpriteEffects
{
    None = 0,
    FlipHorizontally = 1,
    FlipVertically = 2
}
```

## Texture2D

A GPU texture loaded from an image file.

```csharp
public unsafe class Texture2D : IDisposable
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Width` | `int` | Texture width in pixels |
| `Height` | `int` | Texture height in pixels |

### Loading

```csharp
var texture = Content.Load<Texture2D>("player");  // loads Content/player.png
```

## RenderTarget2D

An off-screen texture you can render to.

```csharp
public unsafe class RenderTarget2D : IDisposable
```

### Constructor

```csharp
public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Width` | `int` | Target width in pixels |
| `Height` | `int` | Target height in pixels |

### Example

```csharp
var target = new RenderTarget2D(GraphicsDevice, 320, 240);

// Render to the target
GraphicsDevice.SetRenderTarget(target);
GraphicsDevice.Clear(Color.TransparentBlack);
_spriteBatch.Begin();
_spriteBatch.Draw(_texture, Vector2.Zero, Color.White);
_spriteBatch.End();

// Switch back to screen
GraphicsDevice.SetRenderTarget(null);

// Draw the render target as a texture (e.g. scaled up for pixel art)
_spriteBatch.Begin();
_spriteBatch.Draw(target, new Rectangle(0, 0, 1280, 960), null, Color.White);
_spriteBatch.End();
```

## SpriteFont

TTF font rasterized to a texture atlas. Supports ASCII (32-126) and extended Latin (192-255) characters.

```csharp
public class SpriteFont : IDisposable
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Atlas` | `Texture2D` | The glyph atlas texture |
| `Glyphs` | `Dictionary<char, GlyphData>` | Per-character glyph metrics |
| `LineHeight` | `float` | Line height in pixels |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `MeasureString(string text)` | `Vector2` | Measure text dimensions in pixels |
| `GetKerning(char left, char right)` | `float` | Kerning offset between two characters |

### GlyphData

```csharp
public readonly record struct GlyphData(Rectangle SourceRect, Vector2 Offset, float Advance)
```

### Loading

```csharp
var font = Content.LoadFont("fonts/arial", 24f);  // loads fonts/arial.ttf at 24px
```

### Example

```csharp
_spriteBatch.Begin();
_spriteBatch.DrawString(_font, "Hello, Osel!", new Vector2(10, 10), Color.White);
_spriteBatch.End();

// Measure text for centering
var size = _font.MeasureString("Hello, Osel!");
var centered = new Vector2(
    (GraphicsDevice.BackbufferWidth - size.X) / 2,
    (GraphicsDevice.BackbufferHeight - size.Y) / 2);
```

## Effect

Custom shader loaded from compiled SPIR-V bytecode.

```csharp
public unsafe class Effect : IDisposable
```

### Static Factory

```csharp
public static Effect Create(
    GraphicsDevice graphicsDevice,
    byte[] vertexCode, string vertexEntry, SDL_GPUShaderFormat vertexFormat,
    uint vertexStorageBuffers, uint vertexUniformBuffers,
    byte[] fragmentCode, string fragmentEntry, SDL_GPUShaderFormat fragmentFormat,
    uint fragmentSamplers)
```

### Usage with SpriteBatch

```csharp
var effect = Effect.Create(GraphicsDevice, vertSpv, "main", format, 1, 1, fragSpv, "main", format, 1);

_spriteBatch.Begin(effect: effect);
_spriteBatch.Draw(_texture, position, Color.White);
_spriteBatch.End();
```

### GLSL Shader Pipeline

Write GLSL 450 shaders, compile with `glslc` or `glslangValidator` to SPIR-V:

```bash
glslc -fshader-stage=vert shader.vert -o shader.vert.spv
glslc -fshader-stage=frag shader.frag -o shader.frag.spv
```

SDL3 GPU API cross-compiles SPIR-V to the native backend (Vulkan, Metal, D3D12) at runtime.
