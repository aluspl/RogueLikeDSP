using Godot;

namespace LifeLike.Game;

/// <summary>
/// Rejestruje akcje w InputMap z kodu: klawiatura (WSAD + strzałki + numpad), pad (D-pad + lewa gałka + przyciski).
/// Z kodu, a nie z project.godot — łatwiej czytać w diffie i to jest też fundament pod ekran remappingu.
/// Reszta gry używa WYŁĄCZNIE nazw akcji.
/// </summary>
public static class GameInput
{
    public const string Up = "move_up", Down = "move_down", Left = "move_left", Right = "move_right";
    public const string Wait = "wait_turn", Restart = "restart", NextClass = "next_class";

    public static void Register()
    {
        Add(Up, Key.W, Key.Up, Key.Kp8);       Pad(Up, JoyButton.DpadUp);       Axis(Up, JoyAxis.LeftY, -1);
        Add(Down, Key.S, Key.Down, Key.Kp2);   Pad(Down, JoyButton.DpadDown);   Axis(Down, JoyAxis.LeftY, 1);
        Add(Left, Key.A, Key.Left, Key.Kp4);   Pad(Left, JoyButton.DpadLeft);   Axis(Left, JoyAxis.LeftX, -1);
        Add(Right, Key.D, Key.Right, Key.Kp6); Pad(Right, JoyButton.DpadRight); Axis(Right, JoyAxis.LeftX, 1);
        Add(Wait, Key.Space, Key.Kp5);         Pad(Wait, JoyButton.A);
        Add(Restart, Key.R);                   Pad(Restart, JoyButton.Start);
        Add(NextClass, Key.Tab);               Pad(NextClass, JoyButton.Back);
    }

    private static void Ensure(string action)
    {
        if (!InputMap.HasAction(action)) InputMap.AddAction(action, deadzone: 0.5f);
    }

    private static void Add(string action, params Key[] keys)
    {
        Ensure(action);
        foreach (var k in keys) InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = k });
    }

    private static void Pad(string action, JoyButton b)
    {
        Ensure(action);
        InputMap.ActionAddEvent(action, new InputEventJoypadButton { ButtonIndex = b, Device = -1 });
    }

    private static void Axis(string action, JoyAxis axis, float dir)
    {
        Ensure(action);
        InputMap.ActionAddEvent(action, new InputEventJoypadMotion { Axis = axis, AxisValue = dir, Device = -1 });
    }
}
