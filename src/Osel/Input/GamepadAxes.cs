using SDL;

namespace Osel.Input;

/// <summary>
/// Gamepad axes mapped to SDL_GamepadAxis values.
/// </summary>
public enum GamepadAxes
{
    LeftX = (int)SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTX,
    LeftY = (int)SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTY,
    RightX = (int)SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTX,
    RightY = (int)SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTY,
    LeftTrigger = (int)SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFT_TRIGGER,
    RightTrigger = (int)SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHT_TRIGGER,
}
