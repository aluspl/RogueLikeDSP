namespace LifeLike.Game.Screens.Views;

/// <summary>Cele dotyku / kliknięcia na wyborze zawodu.</summary>
public enum ClassSelectHit
{
    None,
    Portrait,
    Difficulty,
    Keepsake,
    Start,
    Back,
    Investor,
    Stat,       // wiersz statystyki (Arg = 0..5: HP, SIŁ, ZRĘ, INT, OBR, SZCZ) - dymek z opisem
    Stats,      // przycisk „i” - strona opisu statystyk
    Details,    // v0.22.0 (UX A): pion – „Szczegóły” / „Wróć” z pełnej karty
}
