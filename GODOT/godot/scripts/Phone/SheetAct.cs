namespace LifeLike.Game.Phone;

/// <summary>v0.23.2 (UI 2): przycisk karty przedmiotu (ItemSheet).</summary>
public enum SheetAct
{
    /// <summary>Plecak: załóż (zamiana z noszonym).</summary>
    Equip,
    /// <summary>Plecak: broń do kabury.</summary>
    Holster,
    /// <summary>Noszone: wybór przedmiotu z plecaka na to miejsce.</summary>
    Change,
    /// <summary>Noszone: zdejmij do plecaka.</summary>
    Unequip,
    /// <summary>Oddaj do PSZOK-u (dośw. + HP; na placu 1 tura).</summary>
    Recycle,
    /// <summary>Hurtownia: Skup.</summary>
    Sell,
    Details,
    Breakdown,
    /// <summary>Kabura: zamiana z bronią w ręku.</summary>
    Swap,
    Close,
}
