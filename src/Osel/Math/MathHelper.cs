namespace Osel.Math;

public static class MathHelper
{
    public const float Pi = MathF.PI;
    public const float TwoPi = MathF.PI * 2f;
    public const float HalfPi = MathF.PI / 2f;

    public static float Clamp(float value, float min, float max) =>
        value < min ? min : value > max ? max : value;

    public static float Lerp(float a, float b, float t) => a + (b - a) * t;

    public static float ToRadians(float degrees) => degrees * (Pi / 180f);

    public static float ToDegrees(float radians) => radians * (180f / Pi);
}
