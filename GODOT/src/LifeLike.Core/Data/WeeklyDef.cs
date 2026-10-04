namespace LifeLike.Core.Data;

/// <summary>Wyzwanie tygodnia (#34, core::weekly_def): nazwa, skrót, opis w 2 liniach, do 3 zasad.</summary>
public sealed record WeeklyDef(string Id, string Name, string Short, string[] Desc, WeeklyRuleDef[] Rules);
