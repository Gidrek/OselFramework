# Animation

Spritesheet-based animation system with immutable animation definitions and stateful playback.

## SpriteAnimation

Immutable definition of an animation sequence. Create one per animation type and share across entities.

```csharp
public record SpriteAnimation(string Name, Rectangle[] Frames, float FrameDuration, bool Loop = true)
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Animation identifier (e.g. "walk", "idle") |
| `Frames` | `Rectangle[]` | Source rectangles for each frame |
| `FrameDuration` | `float` | Seconds per frame |
| `Loop` | `bool` | Whether to loop (default: true) |
| `FrameCount` | `int` | Number of frames |
| `TotalDuration` | `float` | Total animation duration in seconds |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetFrame(int index)` | `Rectangle` | Get the source rectangle for a specific frame |

### Factory: FromGrid

Create an animation from a uniform spritesheet grid:

```csharp
public static SpriteAnimation FromGrid(
    string name,
    int frameWidth, int frameHeight,
    int startFrame, int frameCount,
    int columns,
    float frameDuration,
    bool loop = true)
```

| Parameter | Description |
|-----------|-------------|
| `frameWidth` | Width of each frame in pixels |
| `frameHeight` | Height of each frame in pixels |
| `startFrame` | 0-based index of the first frame in the grid |
| `frameCount` | Number of frames in the animation |
| `columns` | Number of columns in the spritesheet |
| `frameDuration` | Seconds per frame |

### Example

```csharp
// From a 4-column spritesheet, 16x16 frames
var walkRight = SpriteAnimation.FromGrid("walk_right",
    frameWidth: 16, frameHeight: 16,
    startFrame: 0, frameCount: 4,
    columns: 4, frameDuration: 0.15f);

var idle = SpriteAnimation.FromGrid("idle",
    frameWidth: 16, frameHeight: 16,
    startFrame: 4, frameCount: 2,
    columns: 4, frameDuration: 0.5f);
```

## AnimatedSprite

Stateful playback controller for sprite animations. Manages current frame, timing, and rendering.

```csharp
public class AnimatedSprite
```

### Constructor

```csharp
public AnimatedSprite(Texture2D texture)
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Texture` | `Texture2D` | The spritesheet texture |
| `CurrentAnimation` | `string?` | Name of the playing animation |
| `CurrentFrameIndex` | `int` | Current frame index |
| `IsPlaying` | `bool` | Whether an animation is playing |
| `CurrentFrame` | `Rectangle` | Source rectangle of the current frame |

### Methods

| Method | Description |
|--------|-------------|
| `void AddAnimation(SpriteAnimation animation)` | Register an animation by name |
| `void Play(string name)` | Play an animation (restarts if different from current) |
| `void Stop()` | Stop the current animation |
| `void Update(float deltaTime)` | Advance the animation timer |
| `void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation = 0f, Vector2? origin = null, float scale = 1f, SpriteEffects effects = SpriteEffects.None)` | Draw the current frame |

### Example

```csharp
private AnimatedSprite _player = null!;

protected override void LoadContent()
{
    var sheet = Content.Load<Texture2D>("player_sheet");
    _player = new AnimatedSprite(sheet);

    _player.AddAnimation(SpriteAnimation.FromGrid("idle", 16, 16, 0, 2, 4, 0.5f));
    _player.AddAnimation(SpriteAnimation.FromGrid("walk", 16, 16, 4, 4, 4, 0.15f));

    _player.Play("idle");
}

protected override void Update(GameTime gameTime)
{
    bool moving = Keyboard.IsKeyDown(Keys.A) || Keyboard.IsKeyDown(Keys.D);
    _player.Play(moving ? "walk" : "idle");
    _player.Update(gameTime.DeltaTime);
}

protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.Black);
    _spriteBatch.Begin();
    _player.Draw(_spriteBatch, _position, Color.White);
    _spriteBatch.End();
}
```
