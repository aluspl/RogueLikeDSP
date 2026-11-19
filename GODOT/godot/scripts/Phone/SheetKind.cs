namespace LifeLike.Game.Phone;

/// <summary>v0.23.2 (UI 2): skąd jest przedmiot na karcie – plecak, slot sprzętu, broń w ręku, kabura, znalezisko.</summary>
public enum SheetKind
{
    Bag,
    Gear,
    Hand,
    Holster,

    /// <summary>Znalezisko z mapy (karta paczki / broni): porównanie z noszonym.</summary>
    Find,
}
