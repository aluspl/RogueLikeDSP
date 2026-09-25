using Godot;

namespace LifeLike.Game;

/// <summary>
/// Mapa wejścia rejestrowana w kodzie (klawiatura + pad), jak przyciski GBA:
/// A = atak celu, B = czekaj, R = moc zawodu, START = menu akcji (Enter), SELECT = telefon (Tab),
/// L = podgląd mapy (M). W telefonie Q/E (LB/RB) i strzałki w lewo/prawo przełączają zakładki (L/R na GBA).
/// Na wyborze zawodu Q/E (LB/RB) - pamiątka, P (pad X) - profil, K (pad Y) - Szkolenia.
/// Reszta gry używa WYŁĄCZNIE nazw akcji.
/// </summary>
public static class GameInput
{
    public const string Up = "move_up", Down = "move_down", Left = "move_left", Right = "move_right";
    public const string Attack = "attack", Wait = "wait_turn", Ability = "ability";
    public const string Phone = "phone", Confirm = "confirm", Cancel = "cancel", Shop = "shop", Map = "map_overview";
    public const string KeepPrev = "keepsake_prev", KeepNext = "keepsake_next", Profile = "profile";
    public const string TabPrev = "tab_prev", TabNext = "tab_next";

    public static void Register()
    {
        Add(Up, Key.W, Key.Up, Key.Kp8);
        Pad(Up, JoyButton.DpadUp);
        Axis(Up, JoyAxis.LeftY, -1);
        Add(Down, Key.S, Key.Down, Key.Kp2);
        Pad(Down, JoyButton.DpadDown);
        Axis(Down, JoyAxis.LeftY, 1);
        Add(Left, Key.A, Key.Left, Key.Kp4);
        Pad(Left, JoyButton.DpadLeft);
        Axis(Left, JoyAxis.LeftX, -1);
        Add(Right, Key.D, Key.Right, Key.Kp6);
        Pad(Right, JoyButton.DpadRight);
        Axis(Right, JoyAxis.LeftX, 1);
        Add(Attack, Key.Space, Key.X, Key.J);
        Pad(Attack, JoyButton.A);
        Add(Wait, Key.Z, Key.Kp5, Key.Period);
        Pad(Wait, JoyButton.B);
        Add(Ability, Key.R, Key.F);
        Pad(Ability, JoyButton.RightShoulder);
        Add(Phone, Key.Tab);
        Pad(Phone, JoyButton.Back);
        Add(Map, Key.M);
        Pad(Map, JoyButton.LeftStick);
        Add(Confirm, Key.Enter, Key.KpEnter);
        Pad(Confirm, JoyButton.Start);
        Add(Cancel, Key.Escape, Key.Backspace);
        Add(Shop, Key.K);
        Pad(Shop, JoyButton.Y);
        Add(KeepPrev, Key.Q);
        Pad(KeepPrev, JoyButton.LeftShoulder);
        Add(KeepNext, Key.E);
        Pad(KeepNext, JoyButton.RightShoulder);
        Add(TabPrev, Key.Q);
        Pad(TabPrev, JoyButton.LeftShoulder);
        Add(TabNext, Key.E);
        Pad(TabNext, JoyButton.RightShoulder);
        Add(Profile, Key.P);
        Pad(Profile, JoyButton.X);
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
