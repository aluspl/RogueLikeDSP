namespace LifeLike.Core.Data;

/// <summary>Odznaka; Xp = nagroda przy pierwszym zdobyciu, Bonus = uprawnienie (trwała premia na każdą kolejną budowę);
/// v0.21.52: Title = tytuł do wyboru w profilu, Cosmetic = odblokowany wygląd (kolor kasku, -1 = brak).</summary>
public sealed record BadgeDef(string Id, string Name, string Desc, int Xp, Perk Bonus, string Title = "", int Cosmetic = -1);
