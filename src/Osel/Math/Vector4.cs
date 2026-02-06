using System.Runtime.InteropServices;

namespace Osel.Math;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector4(float X, float Y, float Z, float W)
{
    public static readonly Vector4 Zero = new(0, 0, 0, 0);
    public static readonly Vector4 One = new(1, 1, 1, 1);

    public float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);
    public float LengthSquared() => X * X + Y * Y + Z * Z + W * W;

    public static Vector4 Lerp(Vector4 a, Vector4 b, float t) => new(
        a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t,
        a.Z + (b.Z - a.Z) * t, a.W + (b.W - a.W) * t);

    public static Vector4 operator +(Vector4 a, Vector4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    public static Vector4 operator -(Vector4 a, Vector4 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    public static Vector4 operator *(Vector4 v, float s) => new(v.X * s, v.Y * s, v.Z * s, v.W * s);
    public static Vector4 operator *(float s, Vector4 v) => new(v.X * s, v.Y * s, v.Z * s, v.W * s);

    public static implicit operator System.Numerics.Vector4(Vector4 v) => new(v.X, v.Y, v.Z, v.W);
    public static implicit operator Vector4(System.Numerics.Vector4 v) => new(v.X, v.Y, v.Z, v.W);

    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
}
