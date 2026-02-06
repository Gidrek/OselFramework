using System.Runtime.InteropServices;

namespace Osel.Math;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector2(float X, float Y)
{
    public static readonly Vector2 Zero = new(0, 0);
    public static readonly Vector2 One = new(1, 1);
    public static readonly Vector2 UnitX = new(1, 0);
    public static readonly Vector2 UnitY = new(0, 1);

    public float Length() => MathF.Sqrt(X * X + Y * Y);
    public float LengthSquared() => X * X + Y * Y;

    public Vector2 Normalized()
    {
        var len = Length();
        if (len == 0) return Zero;
        return new(X / len, Y / len);
    }

    public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;
    public static float Distance(Vector2 a, Vector2 b) => (a - b).Length();
    public static float DistanceSquared(Vector2 a, Vector2 b) => (a - b).LengthSquared();
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
    public static Vector2 Min(Vector2 a, Vector2 b) => new(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y));
    public static Vector2 Max(Vector2 a, Vector2 b) => new(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y));
    public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max) => Min(Max(value, min), max);

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 v, float s) => new(v.X * s, v.Y * s);
    public static Vector2 operator *(float s, Vector2 v) => new(v.X * s, v.Y * s);
    public static Vector2 operator /(Vector2 v, float s) => new(v.X / s, v.Y / s);
    public static Vector2 operator -(Vector2 v) => new(-v.X, -v.Y);

    public static implicit operator System.Numerics.Vector2(Vector2 v) => new(v.X, v.Y);
    public static implicit operator Vector2(System.Numerics.Vector2 v) => new(v.X, v.Y);

    public override string ToString() => $"({X}, {Y})";
}
