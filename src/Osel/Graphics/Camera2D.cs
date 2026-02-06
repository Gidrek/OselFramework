using Osel.Math;

namespace Osel.Graphics;

/// <summary>
/// A 2D camera that provides view transformation for SpriteBatch rendering.
/// Supports position, zoom, rotation, smooth following, and optional world bounds clamping.
/// </summary>
public class Camera2D
{
    /// <summary>The camera's position in world space (center of view).</summary>
    public Vector2 Position { get; set; }

    /// <summary>Zoom level. 1 = normal, &gt;1 = zoomed in, &lt;1 = zoomed out.</summary>
    public float Zoom { get; set; } = 1f;

    /// <summary>Rotation in radians.</summary>
    public float Rotation { get; set; }

    /// <summary>Optional world bounds to clamp the camera within.</summary>
    public Rectangle? Bounds { get; set; }

    /// <summary>
    /// Returns the view matrix that transforms world coordinates to screen coordinates.
    /// Pass this to SpriteBatch.Begin(transformMatrix:).
    /// </summary>
    public Matrix4x4 GetViewMatrix(int viewportWidth, int viewportHeight)
    {
        var pos = Position;

        // Clamp to bounds if set
        if (Bounds.HasValue)
            pos = ClampToBounds(pos, viewportWidth, viewportHeight);

        return Matrix4x4.CreateTranslation(-pos.X, -pos.Y)
             * Matrix4x4.CreateRotationZ(Rotation)
             * Matrix4x4.CreateScale(Zoom)
             * Matrix4x4.CreateTranslation(viewportWidth / 2f, viewportHeight / 2f);
    }

    /// <summary>
    /// Converts a screen position to world coordinates.
    /// </summary>
    public Vector2 ScreenToWorld(Vector2 screenPos, int viewportWidth, int viewportHeight)
    {
        var inverse = Matrix4x4.Invert(GetViewMatrix(viewportWidth, viewportHeight));
        if (!inverse.HasValue) return screenPos;

        var m = (System.Numerics.Matrix4x4)inverse.Value;
        var v = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(screenPos.X, screenPos.Y), m);
        return new Vector2(v.X, v.Y);
    }

    /// <summary>
    /// Converts a world position to screen coordinates.
    /// </summary>
    public Vector2 WorldToScreen(Vector2 worldPos, int viewportWidth, int viewportHeight)
    {
        var m = (System.Numerics.Matrix4x4)GetViewMatrix(viewportWidth, viewportHeight);
        var v = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(worldPos.X, worldPos.Y), m);
        return new Vector2(v.X, v.Y);
    }

    /// <summary>
    /// Returns the axis-aligned bounding box of the visible world region (for culling).
    /// </summary>
    public Rectangle GetVisibleArea(int viewportWidth, int viewportHeight)
    {
        // Transform the four screen corners back to world space
        var tl = ScreenToWorld(new Vector2(0, 0), viewportWidth, viewportHeight);
        var tr = ScreenToWorld(new Vector2(viewportWidth, 0), viewportWidth, viewportHeight);
        var bl = ScreenToWorld(new Vector2(0, viewportHeight), viewportWidth, viewportHeight);
        var br = ScreenToWorld(new Vector2(viewportWidth, viewportHeight), viewportWidth, viewportHeight);

        var min = Vector2.Min(Vector2.Min(tl, tr), Vector2.Min(bl, br));
        var max = Vector2.Max(Vector2.Max(tl, tr), Vector2.Max(bl, br));

        return new Rectangle(
            (int)MathF.Floor(min.X),
            (int)MathF.Floor(min.Y),
            (int)MathF.Ceiling(max.X - min.X),
            (int)MathF.Ceiling(max.Y - min.Y));
    }

    /// <summary>
    /// Smoothly moves the camera toward a target position.
    /// lerp = 1 snaps instantly; lerp = 0.05 is very smooth.
    /// </summary>
    public void Follow(Vector2 target, float lerp = 1f)
    {
        Position = Vector2.Lerp(Position, target, MathF.Min(lerp, 1f));
    }

    private Vector2 ClampToBounds(Vector2 pos, int viewportWidth, int viewportHeight)
    {
        var bounds = Bounds!.Value;
        float halfW = viewportWidth / (2f * Zoom);
        float halfH = viewportHeight / (2f * Zoom);

        float minX = bounds.X + halfW;
        float maxX = bounds.Right - halfW;
        float minY = bounds.Y + halfH;
        float maxY = bounds.Bottom - halfH;

        // If the viewport is larger than bounds, center on bounds
        if (minX > maxX) pos = new Vector2(bounds.X + bounds.Width / 2f, pos.Y);
        else pos = new Vector2(System.Math.Clamp(pos.X, minX, maxX), pos.Y);

        if (minY > maxY) pos = new Vector2(pos.X, bounds.Y + bounds.Height / 2f);
        else pos = new Vector2(pos.X, System.Math.Clamp(pos.Y, minY, maxY));

        return pos;
    }
}
