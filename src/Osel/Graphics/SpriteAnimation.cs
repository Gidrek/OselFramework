using Osel.Math;

namespace Osel.Graphics;

/// <summary>
/// Immutable definition of a sprite animation: a sequence of source rectangles
/// from a spritesheet with a fixed frame duration.
/// </summary>
public record SpriteAnimation(string Name, Rectangle[] Frames, float FrameDuration, bool Loop = true)
{
    public int FrameCount => Frames.Length;
    public float TotalDuration => FrameCount * FrameDuration;

    public Rectangle GetFrame(int index) => Frames[index];

    /// <summary>
    /// Creates a SpriteAnimation by slicing a spritesheet grid.
    /// </summary>
    /// <param name="name">Animation name.</param>
    /// <param name="frameWidth">Width of each frame in pixels.</param>
    /// <param name="frameHeight">Height of each frame in pixels.</param>
    /// <param name="startFrame">Index of the first frame in the grid (left-to-right, top-to-bottom).</param>
    /// <param name="frameCount">Number of frames in this animation.</param>
    /// <param name="columns">Number of columns in the spritesheet grid.</param>
    /// <param name="frameDuration">Duration of each frame in seconds.</param>
    /// <param name="loop">Whether the animation loops.</param>
    public static SpriteAnimation FromGrid(string name, int frameWidth, int frameHeight,
        int startFrame, int frameCount, int columns, float frameDuration, bool loop = true)
    {
        var frames = new Rectangle[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            int idx = startFrame + i;
            int col = idx % columns;
            int row = idx / columns;
            frames[i] = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
        }
        return new SpriteAnimation(name, frames, frameDuration, loop);
    }
}
