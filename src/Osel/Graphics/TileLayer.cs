namespace Osel.Graphics;

/// <summary>
/// A single layer of tiles in a TileMap.
/// Tile IDs are 1-based (0 = empty/no tile).
/// </summary>
public class TileLayer
{
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int TileWidth { get; }
    public int TileHeight { get; }
    public bool Visible { get; set; } = true;
    public float Opacity { get; set; } = 1f;
    public bool IsCollision { get; set; }

    private readonly int[] _tiles;

    public TileLayer(string name, int width, int height, int tileWidth, int tileHeight, int[] tiles, bool isCollision = false)
    {
        Name = name;
        Width = width;
        Height = height;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        IsCollision = isCollision;
        _tiles = tiles.Length == width * height
            ? tiles
            : throw new ArgumentException($"Tiles array length ({tiles.Length}) must match width*height ({width * height}).");
    }

    public int GetTile(int col, int row)
    {
        if (col < 0 || col >= Width || row < 0 || row >= Height)
            return 0;
        return _tiles[row * Width + col];
    }

    public void SetTile(int col, int row, int tileId)
    {
        if (col < 0 || col >= Width || row < 0 || row >= Height)
            return;
        _tiles[row * Width + col] = tileId;
    }

    /// <summary>Returns true if this is a collision layer and the tile at (col, row) is non-empty.</summary>
    public bool IsSolid(int col, int row) => IsCollision && GetTile(col, row) != 0;
}
