namespace LifeLike.Core.Data;

/// <summary>Kombinacja stanów (core::combo_def): zapowiedź, skutek na problemie i na bohaterze ("" = nie dotyczy).</summary>
public sealed record ComboDef(string Id, string Name, string Short, string Info, string Hero, ComboEffect Effect, int Value, int Radius, int HeroValue);
