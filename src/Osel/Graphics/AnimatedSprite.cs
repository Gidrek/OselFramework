using Osel.Core;
using Osel.Math;

namespace Osel.Graphics;

/// <summary>
/// Manages playback state for sprite animations on a spritesheet texture.
/// </summary>
public class AnimatedSprite
{
    public Texture2D Texture { get; }

    private readonly Dictionary<string, SpriteAnimation> _animations = new();

    public string? CurrentAnimation { get; private set; }
    public int CurrentFrameIndex { get; private set; }
    public bool IsPlaying { get; private set; }

    private float _timer;

    public AnimatedSprite(Texture2D texture)
    {
        Texture = texture;
    }

    public void AddAnimation(SpriteAnimation animation)
    {
        _animations[animation.Name] = animation;
    }

    /// <summary>
    /// The source rectangle for the current frame. Returns the full texture if no animation is playing.
    /// </summary>
    public Rectangle CurrentFrame
    {
        get
        {
            if (CurrentAnimation != null && _animations.TryGetValue(CurrentAnimation, out var anim))
                return anim.GetFrame(CurrentFrameIndex);
            return new Rectangle(0, 0, Texture.Width, Texture.Height);
        }
    }

    /// <summary>
    /// Plays the named animation. Resets timer if switching to a different animation.
    /// </summary>
    public void Play(string name)
    {
        if (!_animations.ContainsKey(name))
            throw new OselException($"Animation '{name}' not found.");

        if (CurrentAnimation == name && IsPlaying)
            return;

        CurrentAnimation = name;
        CurrentFrameIndex = 0;
        _timer = 0;
        IsPlaying = true;
    }

    /// <summary>Pauses playback at the current frame.</summary>
    public void Stop()
    {
        IsPlaying = false;
    }

    /// <summary>
    /// Advances the animation timer. Call once per frame with delta time in seconds.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!IsPlaying || CurrentAnimation == null)
            return;

        if (!_animations.TryGetValue(CurrentAnimation, out var anim))
            return;

        _timer += deltaTime;

        while (_timer >= anim.FrameDuration)
        {
            _timer -= anim.FrameDuration;
            CurrentFrameIndex++;

            if (CurrentFrameIndex >= anim.FrameCount)
            {
                if (anim.Loop)
                {
                    CurrentFrameIndex = 0;
                }
                else
                {
                    CurrentFrameIndex = anim.FrameCount - 1;
                    IsPlaying = false;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Convenience method to draw the current frame.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color,
        float rotation = 0f, Vector2? origin = null, float scale = 1f,
        SpriteEffects effects = SpriteEffects.None)
    {
        spriteBatch.Draw(Texture, position, CurrentFrame, color,
            rotation, origin ?? Vector2.Zero, scale, effects);
    }
}
