using Osel.Graphics;

namespace Osel.Math;

/// <summary>
/// Static utility methods for 2D collision detection and resolution.
/// </summary>
public static class Collision
{
    /// <summary>Returns true if the two rectangles overlap.</summary>
    public static bool Intersects(Rectangle a, Rectangle b)
    {
        return a.Left < b.Right && a.Right > b.Left &&
               a.Top < b.Bottom && a.Bottom > b.Top;
    }

    /// <summary>Returns the overlapping rectangle, or null if no overlap.</summary>
    public static Rectangle? GetOverlap(Rectangle a, Rectangle b)
    {
        var result = Rectangle.Intersect(a, b);
        return result.Width > 0 && result.Height > 0 ? result : null;
    }

    /// <summary>
    /// Returns the minimum translation vector to push rectangle A out of rectangle B.
    /// Pushes along the axis with the smallest overlap.
    /// </summary>
    public static Vector2 GetSeparation(Rectangle a, Rectangle b)
    {
        var overlap = GetOverlap(a, b);
        if (overlap == null) return Vector2.Zero;

        var o = overlap.Value;

        // Push along the smallest axis
        if (o.Width < o.Height)
        {
            // Push left or right
            float sign = (a.Center.X < b.Center.X) ? -1f : 1f;
            return new Vector2(o.Width * sign, 0);
        }
        else
        {
            // Push up or down
            float sign = (a.Center.Y < b.Center.Y) ? -1f : 1f;
            return new Vector2(0, o.Height * sign);
        }
    }

    /// <summary>
    /// Moves an entity through a tilemap, resolving collisions by sliding along walls.
    /// Uses axis-separated movement: move X first, resolve, then move Y, resolve.
    /// Returns the actual displacement after collision resolution.
    /// </summary>
    public static Vector2 MoveAndSlide(Rectangle entityBounds, Vector2 velocity, TileMap map)
    {
        var collisionLayer = map.GetCollisionLayer();
        if (collisionLayer == null) return velocity;

        var result = velocity;

        // Move X axis
        var movedX = entityBounds.Offset(new Vector2(velocity.X, 0));
        ResolveAxisCollisions(ref movedX, ref result, collisionLayer, map.TileWidth, map.TileHeight, isXAxis: true);

        // Move Y axis (from the X-resolved position)
        var movedXY = movedX.Offset(new Vector2(0, result.Y));
        ResolveAxisCollisions(ref movedXY, ref result, collisionLayer, map.TileWidth, map.TileHeight, isXAxis: false);

        return result;
    }

    private static void ResolveAxisCollisions(ref Rectangle bounds, ref Vector2 velocity,
        TileLayer collisionLayer, int tileWidth, int tileHeight, bool isXAxis)
    {
        // Find tile range overlapping the entity bounds
        int startCol = System.Math.Max(0, bounds.Left / tileWidth);
        int endCol = System.Math.Min(collisionLayer.Width - 1, (bounds.Right - 1) / tileWidth);
        int startRow = System.Math.Max(0, bounds.Top / tileHeight);
        int endRow = System.Math.Min(collisionLayer.Height - 1, (bounds.Bottom - 1) / tileHeight);

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                if (!collisionLayer.IsSolid(col, row)) continue;

                var tileBounds = new Rectangle(col * tileWidth, row * tileHeight, tileWidth, tileHeight);
                if (!Intersects(bounds, tileBounds)) continue;

                var sep = GetSeparation(bounds, tileBounds);
                if (isXAxis)
                {
                    bounds = bounds.Offset(new Vector2(sep.X, 0));
                    velocity = new Vector2(0, velocity.Y);
                }
                else
                {
                    bounds = bounds.Offset(new Vector2(0, sep.Y));
                    velocity = new Vector2(velocity.X, 0);
                }
            }
        }
    }
}
