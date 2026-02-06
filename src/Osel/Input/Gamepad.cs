using Osel.Math;

namespace Osel.Input;

/// <summary>
/// Static gamepad input API. Supports up to 4 gamepads.
/// </summary>
public static class Gamepad
{
    private const float DefaultDeadzone = 0.15f;

    public static bool IsConnected(int index = 0) =>
        InputManager.IsGamepadConnected(index);

    public static bool IsButtonDown(GamepadButtons button, int index = 0) =>
        InputManager.IsGamepadButtonDown(button, index);

    public static bool IsButtonUp(GamepadButtons button, int index = 0) =>
        !InputManager.IsGamepadButtonDown(button, index);

    public static bool IsButtonPressed(GamepadButtons button, int index = 0) =>
        InputManager.IsGamepadButtonDown(button, index) && !InputManager.WasGamepadButtonDown(button, index);

    public static bool IsButtonReleased(GamepadButtons button, int index = 0) =>
        !InputManager.IsGamepadButtonDown(button, index) && InputManager.WasGamepadButtonDown(button, index);

    /// <summary>
    /// Gets a single axis value, applying deadzone filtering.
    /// Returns 0 if the absolute value is below the deadzone threshold.
    /// </summary>
    public static float GetAxis(GamepadAxes axis, int index = 0, float deadzone = DefaultDeadzone)
    {
        float value = InputManager.GetGamepadAxis(axis, index);
        if (MathF.Abs(value) < deadzone) return 0f;
        return value;
    }

    /// <summary>
    /// Gets the left stick as a Vector2, with deadzone applied per-axis.
    /// </summary>
    public static Vector2 GetLeftStick(int index = 0, float deadzone = DefaultDeadzone)
    {
        return new Vector2(
            GetAxis(GamepadAxes.LeftX, index, deadzone),
            GetAxis(GamepadAxes.LeftY, index, deadzone));
    }

    /// <summary>
    /// Gets the right stick as a Vector2, with deadzone applied per-axis.
    /// </summary>
    public static Vector2 GetRightStick(int index = 0, float deadzone = DefaultDeadzone)
    {
        return new Vector2(
            GetAxis(GamepadAxes.RightX, index, deadzone),
            GetAxis(GamepadAxes.RightY, index, deadzone));
    }

    /// <summary>Gets the left trigger value (0.0 to 1.0).</summary>
    public static float GetLeftTrigger(int index = 0) =>
        MathF.Max(0f, InputManager.GetGamepadAxis(GamepadAxes.LeftTrigger, index));

    /// <summary>Gets the right trigger value (0.0 to 1.0).</summary>
    public static float GetRightTrigger(int index = 0) =>
        MathF.Max(0f, InputManager.GetGamepadAxis(GamepadAxes.RightTrigger, index));
}
