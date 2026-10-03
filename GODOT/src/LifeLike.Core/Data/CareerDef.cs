namespace LifeLike.Core.Data;

/// <summary>
/// v0.21.52 cz. d (#47): kontrakt mapy kariery (budynek) – etapy First..First+Count-1 w GameData.Stages (Dom jednorodzinny
/// z Aktem 0: Prelude etapów na początku), warunek odblokowania (UnlockValue: wygrane / poziom inspektora), porywy aktu II
/// co Gust tur (0 = jak w akcie), bliźniak (Twins), boss kontraktu (-1 = Dom) i nagroda za pierwszą wygraną: Respekt,
/// tytuł, kolor kasku (GameData.Cosmetics, -1 = brak).
/// </summary>
public sealed record CareerDef(string Id, string Name, string Short, string Desc, int First, int Count, int Prelude, CareerUnlock Unlock,
    int UnlockValue, int Gust, bool Twins, int Boss, int Respect, string Title, int Helmet);
