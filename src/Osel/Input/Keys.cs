using SDL;

namespace Osel.Input;

/// <summary>
/// Keyboard keys mapped from SDL scancodes.
/// </summary>
public enum Keys
{
    None = 0,

    // Letters
    A = SDL_Scancode.SDL_SCANCODE_A,
    B = SDL_Scancode.SDL_SCANCODE_B,
    C = SDL_Scancode.SDL_SCANCODE_C,
    D = SDL_Scancode.SDL_SCANCODE_D,
    E = SDL_Scancode.SDL_SCANCODE_E,
    F = SDL_Scancode.SDL_SCANCODE_F,
    G = SDL_Scancode.SDL_SCANCODE_G,
    H = SDL_Scancode.SDL_SCANCODE_H,
    I = SDL_Scancode.SDL_SCANCODE_I,
    J = SDL_Scancode.SDL_SCANCODE_J,
    K = SDL_Scancode.SDL_SCANCODE_K,
    L = SDL_Scancode.SDL_SCANCODE_L,
    M = SDL_Scancode.SDL_SCANCODE_M,
    N = SDL_Scancode.SDL_SCANCODE_N,
    O = SDL_Scancode.SDL_SCANCODE_O,
    P = SDL_Scancode.SDL_SCANCODE_P,
    Q = SDL_Scancode.SDL_SCANCODE_Q,
    R = SDL_Scancode.SDL_SCANCODE_R,
    S = SDL_Scancode.SDL_SCANCODE_S,
    T = SDL_Scancode.SDL_SCANCODE_T,
    U = SDL_Scancode.SDL_SCANCODE_U,
    V = SDL_Scancode.SDL_SCANCODE_V,
    W = SDL_Scancode.SDL_SCANCODE_W,
    X = SDL_Scancode.SDL_SCANCODE_X,
    Y = SDL_Scancode.SDL_SCANCODE_Y,
    Z = SDL_Scancode.SDL_SCANCODE_Z,

    // Numbers
    D0 = SDL_Scancode.SDL_SCANCODE_0,
    D1 = SDL_Scancode.SDL_SCANCODE_1,
    D2 = SDL_Scancode.SDL_SCANCODE_2,
    D3 = SDL_Scancode.SDL_SCANCODE_3,
    D4 = SDL_Scancode.SDL_SCANCODE_4,
    D5 = SDL_Scancode.SDL_SCANCODE_5,
    D6 = SDL_Scancode.SDL_SCANCODE_6,
    D7 = SDL_Scancode.SDL_SCANCODE_7,
    D8 = SDL_Scancode.SDL_SCANCODE_8,
    D9 = SDL_Scancode.SDL_SCANCODE_9,

    // Function keys
    F1 = SDL_Scancode.SDL_SCANCODE_F1,
    F2 = SDL_Scancode.SDL_SCANCODE_F2,
    F3 = SDL_Scancode.SDL_SCANCODE_F3,
    F4 = SDL_Scancode.SDL_SCANCODE_F4,
    F5 = SDL_Scancode.SDL_SCANCODE_F5,
    F6 = SDL_Scancode.SDL_SCANCODE_F6,
    F7 = SDL_Scancode.SDL_SCANCODE_F7,
    F8 = SDL_Scancode.SDL_SCANCODE_F8,
    F9 = SDL_Scancode.SDL_SCANCODE_F9,
    F10 = SDL_Scancode.SDL_SCANCODE_F10,
    F11 = SDL_Scancode.SDL_SCANCODE_F11,
    F12 = SDL_Scancode.SDL_SCANCODE_F12,

    // Arrow keys
    Up = SDL_Scancode.SDL_SCANCODE_UP,
    Down = SDL_Scancode.SDL_SCANCODE_DOWN,
    Left = SDL_Scancode.SDL_SCANCODE_LEFT,
    Right = SDL_Scancode.SDL_SCANCODE_RIGHT,

    // Special keys
    Space = SDL_Scancode.SDL_SCANCODE_SPACE,
    Enter = SDL_Scancode.SDL_SCANCODE_RETURN,
    Escape = SDL_Scancode.SDL_SCANCODE_ESCAPE,
    Tab = SDL_Scancode.SDL_SCANCODE_TAB,
    Backspace = SDL_Scancode.SDL_SCANCODE_BACKSPACE,
    Delete = SDL_Scancode.SDL_SCANCODE_DELETE,
    Insert = SDL_Scancode.SDL_SCANCODE_INSERT,
    Home = SDL_Scancode.SDL_SCANCODE_HOME,
    End = SDL_Scancode.SDL_SCANCODE_END,
    PageUp = SDL_Scancode.SDL_SCANCODE_PAGEUP,
    PageDown = SDL_Scancode.SDL_SCANCODE_PAGEDOWN,

    // Modifiers
    LeftShift = SDL_Scancode.SDL_SCANCODE_LSHIFT,
    RightShift = SDL_Scancode.SDL_SCANCODE_RSHIFT,
    LeftControl = SDL_Scancode.SDL_SCANCODE_LCTRL,
    RightControl = SDL_Scancode.SDL_SCANCODE_RCTRL,
    LeftAlt = SDL_Scancode.SDL_SCANCODE_LALT,
    RightAlt = SDL_Scancode.SDL_SCANCODE_RALT,
}
