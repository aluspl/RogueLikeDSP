namespace LifeLike.Core.Data;

/// <summary>Rada w podsumowaniu budowy (#33): warunek i 2 linie tekstu (core::recap_tip_def).</summary>
public sealed record RecapTipDef(RecapTip When, string[] Lines);
