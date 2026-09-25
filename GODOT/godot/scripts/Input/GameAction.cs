using System;

namespace LifeLike.Game.Input;

/// <summary>
/// Akcje gry jak przyciski GBA (jedno zdarzenie klawisza może dać kilka akcji, np. Q = zakładka i pamiątka):
/// A = atak celu, B = czekaj / podgląd, R = moc zawodu, L = podgląd mapy, START = menu akcji / dalej,
/// SELECT = telefon. Reszta to skróty z klawiatury (Esc, Q/E, P, K).
/// </summary>
[Flags]
public enum GameAction : uint
{
    None = 0,
    Up = 1u << 0,
    Down = 1u << 1,
    Left = 1u << 2,
    Right = 1u << 3,
    A = 1u << 4,
    B = 1u << 5,
    R = 1u << 6,
    L = 1u << 7,
    Start = 1u << 8,
    Select = 1u << 9,
    Cancel = 1u << 10,
    TabPrev = 1u << 11,
    TabNext = 1u << 12,
    KeepPrev = 1u << 13,
    KeepNext = 1u << 14,
    Profile = 1u << 15,
    Shop = 1u << 16,
}
