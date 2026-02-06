using System.Runtime.InteropServices;
using SysMatrix = System.Numerics.Matrix4x4;

namespace Osel.Math;

/// <summary>
/// Thin wrapper around System.Numerics.Matrix4x4 for convenience methods.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Matrix4x4(
    float M11, float M12, float M13, float M14,
    float M21, float M22, float M23, float M24,
    float M31, float M32, float M33, float M34,
    float M41, float M42, float M43, float M44)
{
    public static readonly Matrix4x4 Identity = FromSystem(SysMatrix.Identity);

    /// <summary>
    /// Creates an orthographic projection for 2D rendering.
    /// Maps (0,0)-(width,height) to NDC with (0,0) at top-left.
    /// </summary>
    public static Matrix4x4 CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
    {
        return FromSystem(SysMatrix.CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar));
    }

    public static Matrix4x4 CreateTranslation(float x, float y)
    {
        return FromSystem(SysMatrix.CreateTranslation(x, y, 0));
    }

    public static Matrix4x4 CreateScale(float scale)
    {
        return FromSystem(SysMatrix.CreateScale(scale));
    }

    public static Matrix4x4 CreateScale(float scaleX, float scaleY)
    {
        return FromSystem(SysMatrix.CreateScale(scaleX, scaleY, 1));
    }

    public static Matrix4x4 CreateRotationZ(float radians)
    {
        return FromSystem(SysMatrix.CreateRotationZ(radians));
    }

    public static Matrix4x4? Invert(Matrix4x4 matrix)
    {
        if (SysMatrix.Invert(ToSystem(matrix), out var result))
            return FromSystem(result);
        return null;
    }

    public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b) =>
        FromSystem(ToSystem(a) * ToSystem(b));

    public static implicit operator SysMatrix(Matrix4x4 m) => ToSystem(m);
    public static implicit operator Matrix4x4(SysMatrix m) => FromSystem(m);

    private static SysMatrix ToSystem(Matrix4x4 m) => new(
        m.M11, m.M12, m.M13, m.M14,
        m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34,
        m.M41, m.M42, m.M43, m.M44);

    private static Matrix4x4 FromSystem(SysMatrix m) => new(
        m.M11, m.M12, m.M13, m.M14,
        m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34,
        m.M41, m.M42, m.M43, m.M44);
}
