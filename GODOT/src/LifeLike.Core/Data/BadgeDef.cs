namespace LifeLike.Core.Data;

/// <summary>Odznaka; Xp = nagroda przy pierwszym zdobyciu.</summary>
public sealed record BadgeDef(string Id, string Name, string Desc, int Xp);
