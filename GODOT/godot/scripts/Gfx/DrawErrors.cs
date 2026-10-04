using System;
using Godot;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Wyjątki z rysowania (_Draw) telefonu, HUD i ekranów: Godot tylko je loguje, więc zbieramy je tutaj,
/// żeby test dymny mógł zakończyć się błędem, jeśli któryś ekran nie daje się narysować.
/// </summary>
public static class DrawErrors
{
    public static int Count { get; private set; }
    public static string Last { get; private set; } = "";

    public static void Record(string where, Exception ex)
    {
        Count++;
        Last = $"{where}: {ex.Message}";
        GD.PushError($"Rysowanie {where}: {ex}");
    }
}
