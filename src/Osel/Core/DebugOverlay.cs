using Osel.Graphics;
using Osel.Math;

namespace Osel.Core;

/// <summary>
/// Opt-in performance metrics overlay. Shows FPS, frame time, draw calls,
/// sprite count, and managed memory usage.
/// </summary>
public class DebugOverlay
{
    public bool Enabled { get; set; }

    // Metrics (read-only)
    public int Fps { get; private set; }
    public float FrameTimeMs { get; private set; }
    public int DrawCalls { get; private set; }
    public int SpriteCount { get; private set; }
    public long ManagedMemoryMB { get; private set; }

    // FPS smoothing (update every 0.5s)
    private int _frameCount;
    private float _fpsTimer;

    // Draw call delta tracking via cumulative SpriteBatch counters
    private int _lastTotalDrawCalls;
    private int _lastTotalSpriteCount;

    public void Update(GameTime gameTime)
    {
        FrameTimeMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        _frameCount++;
        _fpsTimer += FrameTimeMs / 1000f;
        if (_fpsTimer >= 0.5f)
        {
            Fps = (int)(_frameCount / _fpsTimer);
            _frameCount = 0;
            _fpsTimer = 0;
        }

        ManagedMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024);
    }

    /// <summary>
    /// Draws the debug overlay. Call this AFTER all game drawing is complete,
    /// so it can capture the game's draw metrics before rendering its own text.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, SpriteFont font)
    {
        if (!Enabled) return;

        // Capture game's draw metrics BEFORE our own rendering
        DrawCalls = spriteBatch.TotalDrawCalls - _lastTotalDrawCalls;
        SpriteCount = spriteBatch.TotalSpriteCount - _lastTotalSpriteCount;

        var text = $"FPS: {Fps}\nFrame: {FrameTimeMs:F1}ms\nDraws: {DrawCalls}\nSprites: {SpriteCount}\nMem: {ManagedMemoryMB}MB";
        var pos = new Vector2(4, 4);
        var shadowPos = pos + new Vector2(1, 1);

        spriteBatch.Begin();
        spriteBatch.DrawString(font, text, shadowPos, Color.Black);
        spriteBatch.DrawString(font, text, pos, Color.White);
        spriteBatch.End();

        // Update deltas (including our own draw calls)
        _lastTotalDrawCalls = spriteBatch.TotalDrawCalls;
        _lastTotalSpriteCount = spriteBatch.TotalSpriteCount;
    }
}
