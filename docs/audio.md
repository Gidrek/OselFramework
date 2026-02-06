# Audio

Sound effect and music playback using SDL3 audio streams and NVorbis for OGG decoding.

## SoundEffect

Short audio clips loaded into memory. Best for UI sounds, impacts, and other one-shot effects.

```csharp
public unsafe class SoundEffect : IDisposable
```

### Methods

| Method | Description |
|--------|-------------|
| `void Play(float volume = 1.0f)` | Play the sound effect (volume 0.0 to 1.0) |
| `void Dispose()` | Release audio resources |

### Loading

```csharp
var jumpSound = Content.Load<SoundEffect>("sounds/jump");  // loads sounds/jump.wav
```

### Example

```csharp
private SoundEffect _jumpSound = null!;

protected override void LoadContent()
{
    _jumpSound = Content.Load<SoundEffect>("sounds/jump");
}

protected override void Update(GameTime gameTime)
{
    if (Keyboard.IsKeyPressed(Keys.Space))
        _jumpSound.Play();

    // Play at half volume
    if (Keyboard.IsKeyPressed(Keys.Enter))
        _jumpSound.Play(volume: 0.5f);
}
```

### Supported Formats

- **WAV** — Loaded via SDL3 `SDL_LoadWAV`

## Music

Streaming audio for background music. Only one music track plays at a time.

```csharp
public class Music : IDisposable
```

### Static Methods

| Method | Description |
|--------|-------------|
| `static void Play(Music music, bool loop = true, float volume = 1.0f)` | Play a music track |
| `static void Stop()` | Stop the current music |
| `static void SetVolume(float volume)` | Set music volume (0.0 to 1.0) |

### Loading

```csharp
var bgMusic = Content.Load<Music>("music/theme");  // loads music/theme.ogg
```

### Example

```csharp
private Music _bgMusic = null!;

protected override void LoadContent()
{
    _bgMusic = Content.Load<Music>("music/theme");
    Music.Play(_bgMusic, loop: true);
}

protected override void Update(GameTime gameTime)
{
    // Toggle music
    if (Keyboard.IsKeyPressed(Keys.M))
        Music.Stop();

    // Volume control
    if (Keyboard.IsKeyDown(Keys.Up))
        Music.SetVolume(1.0f);
    if (Keyboard.IsKeyDown(Keys.Down))
        Music.SetVolume(0.3f);
}
```

### Supported Formats

- **OGG** — Decoded via NVorbis (pure C#, streaming)
