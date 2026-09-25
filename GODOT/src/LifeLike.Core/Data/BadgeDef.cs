namespace LifeLike.Core.Data;

/// <summary>Odznaka; Xp = nagroda przy pierwszym zdobyciu, Bonus = uprawnienie (trwała premia na każdą kolejną budowę).</summary>
public sealed record BadgeDef(string Id, string Name, string Desc, int Xp, Perk Bonus);
