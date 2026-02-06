# Input

Static classes for polling keyboard, mouse, and gamepad state. All input is automatically updated each frame.

## Keyboard

```csharp
public static class Keyboard
```

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `IsKeyDown(Keys key)` | `bool` | True while the key is held |
| `IsKeyUp(Keys key)` | `bool` | True while the key is not held |
| `IsKeyPressed(Keys key)` | `bool` | True only on the frame the key was first pressed |
| `IsKeyReleased(Keys key)` | `bool` | True only on the frame the key was released |

### Example

```csharp
protected override void Update(GameTime gameTime)
{
    if (Keyboard.IsKeyDown(Keys.Escape)) Exit();

    float speed = 200f * gameTime.DeltaTime;

    if (Keyboard.IsKeyDown(Keys.W)) _position += new Vector2(0, -speed);
    if (Keyboard.IsKeyDown(Keys.S)) _position += new Vector2(0, speed);
    if (Keyboard.IsKeyDown(Keys.A)) _position += new Vector2(-speed, 0);
    if (Keyboard.IsKeyDown(Keys.D)) _position += new Vector2(speed, 0);

    // Single-press detection
    if (Keyboard.IsKeyPressed(Keys.Space)) Jump();
}
```

## Mouse

```csharp
public static class Mouse
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Position` | `Vector2` | Current mouse position in window coordinates |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `IsButtonDown(MouseButtons button)` | `bool` | True while the button is held |
| `IsButtonUp(MouseButtons button)` | `bool` | True while the button is not held |
| `IsButtonPressed(MouseButtons button)` | `bool` | True only on the frame the button was first pressed |
| `IsButtonReleased(MouseButtons button)` | `bool` | True only on the frame the button was released |

### Example

```csharp
if (Mouse.IsButtonPressed(MouseButtons.Left))
{
    var clickPos = Mouse.Position;
    // Handle click at clickPos
}
```

## Gamepad

Supports up to 4 controllers. All methods accept an optional `index` parameter (default 0).

```csharp
public static class Gamepad
```

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `IsConnected(int index = 0)` | `bool` | Check if a gamepad is connected |
| `IsButtonDown(GamepadButtons button, int index = 0)` | `bool` | True while the button is held |
| `IsButtonUp(GamepadButtons button, int index = 0)` | `bool` | True while the button is not held |
| `IsButtonPressed(GamepadButtons button, int index = 0)` | `bool` | True only on frame the button was first pressed |
| `IsButtonReleased(GamepadButtons button, int index = 0)` | `bool` | True only on frame the button was released |
| `GetAxis(GamepadAxes axis, int index = 0, float deadzone = 0.15f)` | `float` | Raw axis value with deadzone filter |
| `GetLeftStick(int index = 0, float deadzone = 0.15f)` | `Vector2` | Left stick as Vector2 |
| `GetRightStick(int index = 0, float deadzone = 0.15f)` | `Vector2` | Right stick as Vector2 |
| `GetLeftTrigger(int index = 0)` | `float` | Left trigger (0.0 to 1.0) |
| `GetRightTrigger(int index = 0)` | `float` | Right trigger (0.0 to 1.0) |

### Example

```csharp
if (Gamepad.IsConnected())
{
    var stick = Gamepad.GetLeftStick();
    if (stick != Vector2.Zero)
        _position += stick * speed * gameTime.DeltaTime;

    if (Gamepad.IsButtonPressed(GamepadButtons.South))
        Jump();

    float rightTrigger = Gamepad.GetRightTrigger();
    if (rightTrigger > 0.5f)
        Shoot();
}
```

## Enums

### Keys

```
None
A-Z                    // Letter keys
D0-D9                  // Number keys (top row)
F1-F12                 // Function keys
Up, Down, Left, Right  // Arrow keys
Space, Enter, Escape, Tab, Backspace, Delete, Insert
Home, End, PageUp, PageDown
LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt
```

### MouseButtons

| Value | Description |
|-------|-------------|
| `Left` | Left mouse button |
| `Middle` | Middle mouse button (scroll wheel click) |
| `Right` | Right mouse button |

### GamepadButtons

| Value | PlayStation | Xbox |
|-------|------------|------|
| `South` | X | A |
| `East` | Circle | B |
| `West` | Square | X |
| `North` | Triangle | Y |
| `Back` | Select | Back |
| `Guide` | PS | Xbox |
| `Start` | Start | Start |
| `LeftStick` | L3 | LS |
| `RightStick` | R3 | RS |
| `LeftShoulder` | L1 | LB |
| `RightShoulder` | R1 | RB |
| `DPadUp` | D-Pad Up | D-Pad Up |
| `DPadDown` | D-Pad Down | D-Pad Down |
| `DPadLeft` | D-Pad Left | D-Pad Left |
| `DPadRight` | D-Pad Right | D-Pad Right |

### GamepadAxes

| Value | Description |
|-------|-------------|
| `LeftX` | Left stick horizontal |
| `LeftY` | Left stick vertical |
| `RightX` | Right stick horizontal |
| `RightY` | Right stick vertical |
| `LeftTrigger` | Left trigger (0.0 to 1.0) |
| `RightTrigger` | Right trigger (0.0 to 1.0) |
