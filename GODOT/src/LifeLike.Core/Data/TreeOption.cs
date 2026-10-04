namespace LifeLike.Core.Data;

/// <summary>
/// Opcja węzła drzewka Szkoleń (v0.21.52 cz. c, core::tree_option): działanie i wartość jak poziom Szkolenia;
/// Short – krótka nazwa (wąska kolumna drzewka).
/// </summary>
public sealed record TreeOption(string Name, string Short, string Desc, UpgradeEffect Effect, int Value);
