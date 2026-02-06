using SDL;

namespace Osel.Input;

/// <summary>
/// Gamepad buttons mapped to SDL_GamepadButton values.
/// </summary>
public enum GamepadButtons
{
    South = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_SOUTH,
    East = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_EAST,
    West = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_WEST,
    North = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_NORTH,
    Back = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_BACK,
    Guide = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_GUIDE,
    Start = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_START,
    LeftStick = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_STICK,
    RightStick = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_STICK,
    LeftShoulder = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_SHOULDER,
    RightShoulder = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_SHOULDER,
    DPadUp = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_UP,
    DPadDown = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_DOWN,
    DPadLeft = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_LEFT,
    DPadRight = (int)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_RIGHT,
}
