using System;

namespace LifeLike.Game.Screens;

/// <summary>Plansze pełnoekranowe (rysowane pod telefonem), które ekran pokazuje.</summary>
[Flags]
public enum ViewSet
{
    None = 0,
    Title = 1,
    ClassSelect = 2,
    End = 4,
    Prologue = 8,
}
