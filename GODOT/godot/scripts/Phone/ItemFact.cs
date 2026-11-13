namespace LifeLike.Game.Phone;

/// <summary>v0.23.2 (UI 2): wiersz liczby na karcie przedmiotu – etykieta, wartość, zmiana względem noszonego
/// (Sign: 1 lepiej – zielona, -1 gorzej – czerwona, 0 bez koloru).</summary>
public readonly record struct ItemFact(string Label, string Value, string Delta, int Sign);
