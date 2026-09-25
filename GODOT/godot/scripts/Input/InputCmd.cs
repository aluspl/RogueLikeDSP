using Godot;

namespace LifeLike.Game.Input;

/// <summary>
/// Jedno zdarzenie wejścia przetłumaczone na akcje gry (GameInput.Translate): wciśnięte / puszczone akcje,
/// powtórzenie klawisza (echo), dowolny klawisz albo przycisk pada, klik myszy z pozycją na ekranie.
/// Ekrany i strony telefonu czytają WYŁĄCZNIE to (bez InputEvent Godota).
/// </summary>
public readonly struct InputCmd
{
    public readonly GameAction Pressed;
    public readonly GameAction Released;
    public readonly bool Echo;
    public readonly bool AnyKey;
    public readonly MouseButton Click;
    public readonly Vector2 Pointer;

    public InputCmd(GameAction pressed, GameAction released, bool echo, bool anyKey, MouseButton click, Vector2 pointer)
    {
        Pressed = pressed;
        Released = released;
        Echo = echo;
        AnyKey = anyKey;
        Click = click;
        Pointer = pointer;
    }

    /// <summary>Syntetyczne wciśnięcie akcji (test dymny, dotyk).</summary>
    public static InputCmd Of(GameAction a) => new(a, GameAction.None, false, true, MouseButton.None, Vector2.Zero);

    /// <summary>Dotknięcie / lewy klik w punkcie (piksele UI).</summary>
    public static InputCmd Tap(Vector2 p) => new(GameAction.None, GameAction.None, false, false, MouseButton.Left, p);

    /// <summary>Syntetyczne puszczenie akcji.</summary>
    public static InputCmd Release(GameAction a) => new(GameAction.None, a, false, false, MouseButton.None, Vector2.Zero);

    /// <summary>Wciśnięta akcja (albo którakolwiek z kilku: A | Start); echo = także powtórzenie przytrzymanego klawisza.</summary>
    public bool Is(GameAction a, bool echo = false) => (Pressed & a) != 0 && (echo || !Echo);

    public bool IsReleased(GameAction a) => (Released & a) != 0;

    public bool IsClick => Click != MouseButton.None;

    /// <summary>Lewy klik albo dotknięcie ekranu (dotyk jest emulowany jako mysz).</summary>
    public bool IsTap => Click == MouseButton.Left;

    /// <summary>Strzałka góra/dół z powtarzaniem: -1 / 1 / 0.</summary>
    public int VDir => Is(GameAction.Up, true) ? -1 : Is(GameAction.Down, true) ? 1 : 0;

    /// <summary>Strzałka lewo/prawo z powtarzaniem: -1 / 1 / 0.</summary>
    public int HDir => Is(GameAction.Left, true) ? -1 : Is(GameAction.Right, true) ? 1 : 0;
}
