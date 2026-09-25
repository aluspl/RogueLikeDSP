using System;
using Godot;

namespace LifeLike.Game.Input;

/// <summary>
/// Mapa wejścia rejestrowana w kodzie (klawiatura + pad), jak przyciski GBA:
/// A = atak celu, B = czekaj, R = moc zawodu, START = menu akcji (Enter), SELECT = telefon (Tab),
/// L = podgląd mapy (M). W telefonie Q/E (LB/RB) i strzałki w lewo/prawo przełączają zakładki (L/R na GBA).
/// Na wyborze zawodu Q/E (LB/RB) - pamiątka, P (pad X) - profil, K (pad Y) - Szkolenia.
/// Translate zamienia InputEvent Godota na InputCmd - reszta gry nie zna nazw akcji Godota.
/// Wirtualny kontroler (dotyk: gałka i przyciski) wstrzykuje akcje przez Press / Release.
/// </summary>
public static class GameInput
{
    private static GameAction _injectedHeld;

    /// <summary>Akcje z wirtualnego kontrolera (Main przekazuje je ekranowi bieżącemu jak zdarzenia klawiatury).</summary>
    public static event Action<InputCmd> Injected;

    private static readonly (GameAction Action, string Name)[] Actions =
    [
        (GameAction.Up, "move_up"), (GameAction.Down, "move_down"), (GameAction.Left, "move_left"),
        (GameAction.Right, "move_right"), (GameAction.A, "attack"), (GameAction.B, "wait_turn"),
        (GameAction.R, "ability"), (GameAction.L, "map_overview"), (GameAction.Start, "confirm"),
        (GameAction.Select, "phone"), (GameAction.Cancel, "cancel"), (GameAction.TabPrev, "tab_prev"),
        (GameAction.TabNext, "tab_next"), (GameAction.KeepPrev, "keepsake_prev"), (GameAction.KeepNext, "keepsake_next"),
        (GameAction.Profile, "profile"), (GameAction.Shop, "shop"),
    ];

    public static string NameOf(GameAction a)
    {
        foreach (var (action, name) in Actions)
        {
            if (action == a) return name;
        }
        return "";
    }

    public static void Register()
    {
        Add(GameAction.Up, Key.W, Key.Up, Key.Kp8);
        Pad(GameAction.Up, JoyButton.DpadUp);
        Axis(GameAction.Up, JoyAxis.LeftY, -1);
        Add(GameAction.Down, Key.S, Key.Down, Key.Kp2);
        Pad(GameAction.Down, JoyButton.DpadDown);
        Axis(GameAction.Down, JoyAxis.LeftY, 1);
        Add(GameAction.Left, Key.A, Key.Left, Key.Kp4);
        Pad(GameAction.Left, JoyButton.DpadLeft);
        Axis(GameAction.Left, JoyAxis.LeftX, -1);
        Add(GameAction.Right, Key.D, Key.Right, Key.Kp6);
        Pad(GameAction.Right, JoyButton.DpadRight);
        Axis(GameAction.Right, JoyAxis.LeftX, 1);
        Add(GameAction.A, Key.Space, Key.X, Key.J);
        Pad(GameAction.A, JoyButton.A);
        Add(GameAction.B, Key.Z, Key.Kp5, Key.Period);
        Pad(GameAction.B, JoyButton.B);
        Add(GameAction.R, Key.R, Key.F);
        Pad(GameAction.R, JoyButton.RightShoulder);
        Add(GameAction.Select, Key.Tab);
        Pad(GameAction.Select, JoyButton.Back);
        Add(GameAction.L, Key.M);
        Pad(GameAction.L, JoyButton.LeftStick);
        Add(GameAction.Start, Key.Enter, Key.KpEnter);
        Pad(GameAction.Start, JoyButton.Start);
        Add(GameAction.Cancel, Key.Escape, Key.Backspace);
        Add(GameAction.Shop, Key.K);
        Pad(GameAction.Shop, JoyButton.Y);
        Add(GameAction.KeepPrev, Key.Q);
        Pad(GameAction.KeepPrev, JoyButton.LeftShoulder);
        Add(GameAction.KeepNext, Key.E);
        Pad(GameAction.KeepNext, JoyButton.RightShoulder);
        Add(GameAction.TabPrev, Key.Q);
        Pad(GameAction.TabPrev, JoyButton.LeftShoulder);
        Add(GameAction.TabNext, Key.E);
        Pad(GameAction.TabNext, JoyButton.RightShoulder);
        Add(GameAction.Profile, Key.P);
        Pad(GameAction.Profile, JoyButton.X);
    }

    /// <summary>Wciśnięcie akcji z wirtualnego kontrolera (przycisk ekranowy, gałka).</summary>
    public static void Press(GameAction a)
    {
        _injectedHeld |= a;
        Injected?.Invoke(InputCmd.Of(a));
    }

    /// <summary>Puszczenie akcji z wirtualnego kontrolera.</summary>
    public static void Release(GameAction a)
    {
        _injectedHeld &= ~a;
        Injected?.Invoke(InputCmd.Release(a));
    }

    /// <summary>Czy akcja jest teraz przytrzymana (klawiatura, pad albo wirtualny kontroler).</summary>
    public static bool IsHeld(GameAction a)
    {
        if ((_injectedHeld & a) != 0) return true;
        foreach (var (action, name) in Actions)
        {
            if ((action & a) != 0 && Godot.Input.IsActionPressed(name)) return true;
        }
        return false;
    }

    /// <summary>Zdarzenie Godota -> akcje gry (wciśnięte z echem, puszczone), dowolny klawisz, klik myszy.</summary>
    public static InputCmd Translate(InputEvent e)
    {
        GameAction pressed = GameAction.None, released = GameAction.None;
        foreach (var (action, name) in Actions)
        {
            if (e.IsActionPressed(name, true)) pressed |= action;
            else if (e.IsActionReleased(name)) released |= action;
        }
        var anyKey = e is InputEventKey { Pressed: true } or InputEventJoypadButton { Pressed: true };
        var click = e is InputEventMouseButton { Pressed: true } mb ? mb.ButtonIndex : MouseButton.None;
        var pointer = e is InputEventMouse m ? m.Position : Vector2.Zero;
        return new InputCmd(pressed, released, e.IsEcho(), anyKey, click, pointer);
    }

    private static void Ensure(GameAction a)
    {
        var name = NameOf(a);
        if (!InputMap.HasAction(name)) InputMap.AddAction(name, deadzone: 0.5f);
    }

    private static void Add(GameAction a, params Key[] keys)
    {
        Ensure(a);
        foreach (var k in keys) InputMap.ActionAddEvent(NameOf(a), new InputEventKey { PhysicalKeycode = k });
    }

    private static void Pad(GameAction a, JoyButton b)
    {
        Ensure(a);
        InputMap.ActionAddEvent(NameOf(a), new InputEventJoypadButton { ButtonIndex = b, Device = -1 });
    }

    private static void Axis(GameAction a, JoyAxis axis, float dir)
    {
        Ensure(a);
        InputMap.ActionAddEvent(NameOf(a), new InputEventJoypadMotion { Axis = axis, AxisValue = dir, Device = -1 });
    }
}
