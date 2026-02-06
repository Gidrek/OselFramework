namespace Osel.Input;

public static class Keyboard
{
    public static bool IsKeyDown(Keys key) => InputManager.IsKeyDown(key);
    public static bool IsKeyUp(Keys key) => !InputManager.IsKeyDown(key);

    /// <summary>Returns true only on the frame the key was first pressed.</summary>
    public static bool IsKeyPressed(Keys key) => InputManager.IsKeyDown(key) && !InputManager.WasKeyDown(key);

    /// <summary>Returns true only on the frame the key was released.</summary>
    public static bool IsKeyReleased(Keys key) => !InputManager.IsKeyDown(key) && InputManager.WasKeyDown(key);
}
