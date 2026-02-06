using SDL;
using Osel.Math;

namespace Osel.Input;

internal static unsafe class InputManager
{
    private static readonly HashSet<Keys> _currentKeys = new();
    private static readonly HashSet<Keys> _previousKeys = new();
    private static readonly HashSet<MouseButtons> _currentButtons = new();
    private static readonly HashSet<MouseButtons> _previousButtons = new();

    internal static Vector2 MousePosition { get; private set; }

    // Gamepad state — up to 4 controllers
    private const int MaxGamepads = 4;
    private static readonly SDL_Gamepad*[] _gamepads = new SDL_Gamepad*[MaxGamepads];
    private static readonly SDL_JoystickID[] _gamepadIds = new SDL_JoystickID[MaxGamepads];
    private static readonly HashSet<GamepadButtons>[] _currentGamepadButtons;
    private static readonly HashSet<GamepadButtons>[] _previousGamepadButtons;
    private static readonly float[][] _gamepadAxes;

    static InputManager()
    {
        _currentGamepadButtons = new HashSet<GamepadButtons>[MaxGamepads];
        _previousGamepadButtons = new HashSet<GamepadButtons>[MaxGamepads];
        _gamepadAxes = new float[MaxGamepads][];
        for (int i = 0; i < MaxGamepads; i++)
        {
            _currentGamepadButtons[i] = new HashSet<GamepadButtons>();
            _previousGamepadButtons[i] = new HashSet<GamepadButtons>();
            _gamepadAxes[i] = new float[6]; // 6 axes
        }
    }

    internal static void BeginFrame()
    {
        _previousKeys.Clear();
        foreach (var k in _currentKeys)
            _previousKeys.Add(k);

        _previousButtons.Clear();
        foreach (var b in _currentButtons)
            _previousButtons.Add(b);

        for (int i = 0; i < MaxGamepads; i++)
        {
            _previousGamepadButtons[i].Clear();
            foreach (var b in _currentGamepadButtons[i])
                _previousGamepadButtons[i].Add(b);
        }
    }

    internal static void ProcessEvent(ref SDL_Event evt)
    {
        var eventType = (SDL_EventType)evt.type;

        switch (eventType)
        {
            case SDL_EventType.SDL_EVENT_KEY_DOWN:
            {
                var key = (Keys)evt.key.scancode;
                _currentKeys.Add(key);
                break;
            }
            case SDL_EventType.SDL_EVENT_KEY_UP:
            {
                var key = (Keys)evt.key.scancode;
                _currentKeys.Remove(key);
                break;
            }
            case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
            {
                MousePosition = new Vector2(evt.motion.x, evt.motion.y);
                break;
            }
            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
            {
                var button = (MouseButtons)evt.button.button;
                _currentButtons.Add(button);
                break;
            }
            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
            {
                var button = (MouseButtons)evt.button.button;
                _currentButtons.Remove(button);
                break;
            }
            case SDL_EventType.SDL_EVENT_GAMEPAD_ADDED:
            {
                var id = evt.gdevice.which;
                int slot = FindFreeGamepadSlot();
                if (slot >= 0)
                {
                    var gamepad = SDL3.SDL_OpenGamepad(id);
                    if (gamepad != null)
                    {
                        _gamepads[slot] = gamepad;
                        _gamepadIds[slot] = id;
                    }
                }
                break;
            }
            case SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED:
            {
                var id = evt.gdevice.which;
                int slot = FindGamepadSlot(id);
                if (slot >= 0)
                {
                    SDL3.SDL_CloseGamepad(_gamepads[slot]);
                    _gamepads[slot] = null;
                    _gamepadIds[slot] = default;
                    _currentGamepadButtons[slot].Clear();
                    Array.Clear(_gamepadAxes[slot]);
                }
                break;
            }
            case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN:
            {
                var id = evt.gbutton.which;
                int slot = FindGamepadSlot(id);
                if (slot >= 0)
                    _currentGamepadButtons[slot].Add((GamepadButtons)evt.gbutton.button);
                break;
            }
            case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP:
            {
                var id = evt.gbutton.which;
                int slot = FindGamepadSlot(id);
                if (slot >= 0)
                    _currentGamepadButtons[slot].Remove((GamepadButtons)evt.gbutton.button);
                break;
            }
            case SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION:
            {
                var id = evt.gaxis.which;
                int slot = FindGamepadSlot(id);
                if (slot >= 0)
                {
                    int axis = evt.gaxis.axis;
                    if (axis >= 0 && axis < 6)
                        _gamepadAxes[slot][axis] = evt.gaxis.value / 32767f;
                }
                break;
            }
        }
    }

    private static int FindFreeGamepadSlot()
    {
        for (int i = 0; i < MaxGamepads; i++)
        {
            if (_gamepads[i] == null) return i;
        }
        return -1;
    }

    private static int FindGamepadSlot(SDL_JoystickID id)
    {
        for (int i = 0; i < MaxGamepads; i++)
        {
            if (_gamepads[i] != null && _gamepadIds[i] == id) return i;
        }
        return -1;
    }

    // Keyboard queries
    internal static bool IsKeyDown(Keys key) => _currentKeys.Contains(key);
    internal static bool WasKeyDown(Keys key) => _previousKeys.Contains(key);

    // Mouse queries
    internal static bool IsMouseButtonDown(MouseButtons button) => _currentButtons.Contains(button);
    internal static bool WasMouseButtonDown(MouseButtons button) => _previousButtons.Contains(button);

    // Gamepad queries
    internal static bool IsGamepadConnected(int index) =>
        index >= 0 && index < MaxGamepads && _gamepads[index] != null;

    internal static bool IsGamepadButtonDown(GamepadButtons button, int index) =>
        index >= 0 && index < MaxGamepads && _currentGamepadButtons[index].Contains(button);

    internal static bool WasGamepadButtonDown(GamepadButtons button, int index) =>
        index >= 0 && index < MaxGamepads && _previousGamepadButtons[index].Contains(button);

    internal static float GetGamepadAxis(GamepadAxes axis, int index)
    {
        if (index < 0 || index >= MaxGamepads || _gamepads[index] == null)
            return 0f;
        int a = (int)axis;
        if (a < 0 || a >= 6) return 0f;
        return _gamepadAxes[index][a];
    }

    internal static void Shutdown()
    {
        for (int i = 0; i < MaxGamepads; i++)
        {
            if (_gamepads[i] != null)
            {
                SDL3.SDL_CloseGamepad(_gamepads[i]);
                _gamepads[i] = null;
            }
        }
    }
}
