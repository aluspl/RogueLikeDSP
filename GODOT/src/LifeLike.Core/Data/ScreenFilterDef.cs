namespace LifeLike.Core.Data;

/// <summary>
/// v0.21.53 (#53, #54): filtr ekranu (core::screen_filter_def, sekcja „screenFilters”) – nazwa, skrót, opis, podpowiedź przy
/// „???”, rodzaj, Cues (wzory zamiast samego koloru: paski na czerwonych polach, litery rzadkości), Motion (ruchomy efekt –
/// ostrzeżenie i ograniczony ruch) i warunki odblokowania (dowolny z nich; zabawowe).
/// </summary>
public sealed record ScreenFilterDef(string Id, string Name, string Short, string Desc, string Hint, FilterKind Kind, bool Cues, bool Motion,
    FilterCond[] Unlock);
