namespace Osel.Math;

public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    public static readonly Rectangle Empty = new(0, 0, 0, 0);

    public int Left => X;
    public int Top => Y;
    public int Right => X + Width;
    public int Bottom => Y + Height;
    public Vector2 Center => new(X + Width / 2f, Y + Height / 2f);
    public Vector2 Location => new(X, Y);
    public Vector2 Size => new(Width, Height);

    public bool Contains(int x, int y) => x >= X && x < Right && y >= Y && y < Bottom;
    public bool Contains(Vector2 point) => Contains((int)point.X, (int)point.Y);

    public bool Intersects(Rectangle other) =>
        Left < other.Right && Right > other.Left &&
        Top < other.Bottom && Bottom > other.Top;

    public static Rectangle Intersect(Rectangle a, Rectangle b)
    {
        int x = System.Math.Max(a.X, b.X);
        int y = System.Math.Max(a.Y, b.Y);
        int right = System.Math.Min(a.Right, b.Right);
        int bottom = System.Math.Min(a.Bottom, b.Bottom);

        if (right > x && bottom > y)
            return new Rectangle(x, y, right - x, bottom - y);
        return Empty;
    }

    public static Rectangle Union(Rectangle a, Rectangle b)
    {
        int x = System.Math.Min(a.X, b.X);
        int y = System.Math.Min(a.Y, b.Y);
        int right = System.Math.Max(a.Right, b.Right);
        int bottom = System.Math.Max(a.Bottom, b.Bottom);
        return new Rectangle(x, y, right - x, bottom - y);
    }

    public Rectangle Offset(Vector2 amount) => new(X + (int)amount.X, Y + (int)amount.Y, Width, Height);
    public Rectangle Offset(int dx, int dy) => new(X + dx, Y + dy, Width, Height);

    public override string ToString() => $"({X}, {Y}, {Width}, {Height})";
}
