using System.Runtime.InteropServices;
using Osel.Math;

namespace Osel.Core;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Color(float R, float G, float B, float A)
{
    /// <summary>Creates a Color from byte values (0-255).</summary>
    public Color(byte r, byte g, byte b, byte a = 255)
        : this(r / 255f, g / 255f, b / 255f, a / 255f) { }

    // Common colors
    public static readonly Color White = new(1f, 1f, 1f, 1f);
    public static readonly Color Black = new(0f, 0f, 0f, 1f);
    public static readonly Color TransparentBlack = new(0f, 0f, 0f, 0f);
    public static readonly Color Red = new(1f, 0f, 0f, 1f);
    public static readonly Color Green = new(0f, 1f, 0f, 1f);
    public static readonly Color Blue = new(0f, 0f, 1f, 1f);
    public static readonly Color Yellow = new(1f, 1f, 0f, 1f);
    public static readonly Color Magenta = new(1f, 0f, 1f, 1f);
    public static readonly Color Cyan = new(0f, 1f, 1f, 1f);
    public static readonly Color CornflowerBlue = new(100, 149, 237, 255);
    public static readonly Color DarkGray = new(64, 64, 64, 255);
    public static readonly Color Gray = new(128, 128, 128, 255);
    public static readonly Color LightGray = new(192, 192, 192, 255);

    /// <summary>Returns a Color multiplied by a scalar (e.g. for opacity).</summary>
    public static Color operator *(Color c, float s) => new(c.R * s, c.G * s, c.B * s, c.A * s);

    /// <summary>Returns a Vector4 representation (R, G, B, A).</summary>
    public Vector4 ToVector4() => new(R, G, B, A);

    public override string ToString() => $"({R:F2}, {G:F2}, {B:F2}, {A:F2})";
}
