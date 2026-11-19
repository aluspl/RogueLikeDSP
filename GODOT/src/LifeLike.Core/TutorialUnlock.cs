namespace LifeLike.Core;

/// <summary>Dymki przy pierwszym odblokowaniu (core::tutorial_unlock); indeks = GameData.TutorialUnlocks.</summary>
public static class TutorialUnlock
{
    public const int Respect = 0;
    public const int Daily = 1;
    public const int Investor = 2;
    public const int Act0 = 3;
    public const int Class = 4;
    /// <summary>v0.21.51 cz. 2: wykonane sekretne zlecenie (cls = indeks GameData.Secrets).</summary>
    public const int Secret = 5;
    /// <summary>v0.23.0: dymki dawnego pełnego samouczka, teraz przy pierwszej okazji.</summary>
    public const int Training = 6;
    public const int Options = 7;
    public const int Help = 8;
    public const int Keepsake = 9;
    public const int Stats = 10;
}
