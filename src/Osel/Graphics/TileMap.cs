using Osel.Core;
using Osel.Math;

namespace Osel.Graphics;

/// <summary>
/// A tile-based map with multiple layers, rendered from a single tileset texture.
/// Tile IDs are 1-based (0 = empty). The tileset is sliced into a grid.
/// </summary>
public class TileMap
{
    public Texture2D Tileset { get; }
    public int TileWidth { get; }
    public int TileHeight { get; }
    public int TilesetColumns { get; }
    public List<TileLayer> Layers { get; }

    public TileMap(Texture2D tileset, int tileWidth, int tileHeight, List<TileLayer> layers)
    {
        Tileset = tileset;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        TilesetColumns = tileset.Width / tileWidth;
        Layers = layers;
    }

    /// <summary>
    /// Returns the source rectangle in the tileset for a given tile ID (1-based).
    /// Returns null for tile ID 0 (empty).
    /// </summary>
    public Rectangle? GetSourceRect(int tileId)
    {
        if (tileId <= 0) return null;
        int idx = tileId - 1; // Convert to 0-based
        int col = idx % TilesetColumns;
        int row = idx / TilesetColumns;
        return new Rectangle(col * TileWidth, row * TileHeight, TileWidth, TileHeight);
    }

    /// <summary>
    /// Draws all visible layers. When a camera is provided, only tiles
    /// within the camera's visible area are drawn (culling).
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Camera2D? camera = null,
        int viewportWidth = 0, int viewportHeight = 0)
    {
        foreach (var layer in Layers)
        {
            if (!layer.Visible) continue;
            DrawLayer(spriteBatch, layer, camera, viewportWidth, viewportHeight);
        }
    }

    private void DrawLayer(SpriteBatch spriteBatch, TileLayer layer, Camera2D? camera,
        int viewportWidth, int viewportHeight)
    {
        int startCol = 0, startRow = 0;
        int endCol = layer.Width - 1, endRow = layer.Height - 1;

        // Cull to visible area if camera is provided
        if (camera != null && viewportWidth > 0 && viewportHeight > 0)
        {
            var visible = camera.GetVisibleArea(viewportWidth, viewportHeight);
            startCol = System.Math.Max(0, visible.X / TileWidth - 1);
            startRow = System.Math.Max(0, visible.Y / TileHeight - 1);
            endCol = System.Math.Min(layer.Width - 1, visible.Right / TileWidth + 1);
            endRow = System.Math.Min(layer.Height - 1, visible.Bottom / TileHeight + 1);
        }

        var color = layer.Opacity >= 1f
            ? Color.White
            : new Color(1f, 1f, 1f, layer.Opacity);

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                int tileId = layer.GetTile(col, row);
                var src = GetSourceRect(tileId);
                if (src == null) continue;

                var pos = new Vector2(col * TileWidth, row * TileHeight);
                spriteBatch.Draw(Tileset, pos, src.Value, color);
            }
        }
    }

    /// <summary>Returns the first layer marked as collision, or null.</summary>
    public TileLayer? GetCollisionLayer()
    {
        return Layers.Find(l => l.IsCollision);
    }

    /// <summary>Queries the collision layer. Returns true if the tile at (col, row) is solid.</summary>
    public bool IsSolid(int col, int row)
    {
        var layer = GetCollisionLayer();
        return layer != null && layer.IsSolid(col, row);
    }
}
