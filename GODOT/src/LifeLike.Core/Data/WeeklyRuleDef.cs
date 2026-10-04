namespace LifeLike.Core.Data;

/// <summary>Zasada tygodnia z wartością: zawód (Cls), pogoda (Weather), procent albo zł (core::weekly_rule_def).</summary>
public sealed record WeeklyRuleDef(WeeklyRule Rule, int Value);
