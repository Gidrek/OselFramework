using Osel.Math;

namespace Osel.Input;

public static class Mouse
{
    public static Vector2 Position => InputManager.MousePosition;

    public static bool IsButtonDown(MouseButtons button) => InputManager.IsMouseButtonDown(button);
    public static bool IsButtonUp(MouseButtons button) => !InputManager.IsMouseButtonDown(button);

    /// <summary>Returns true only on the frame the button was first pressed.</summary>
    public static bool IsButtonPressed(MouseButtons button) =>
        InputManager.IsMouseButtonDown(button) && !InputManager.WasMouseButtonDown(button);

    /// <summary>Returns true only on the frame the button was released.</summary>
    public static bool IsButtonReleased(MouseButtons button) =>
        !InputManager.IsMouseButtonDown(button) && InputManager.WasMouseButtonDown(button);
}
