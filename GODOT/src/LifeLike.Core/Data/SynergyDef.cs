namespace LifeLike.Core.Data;

/// <summary>Synergia: 2+ premie z tym samym znacznikiem (albo po jednej z dwóch) włączają dodatkowy skutek (core::synergy_def).</summary>
public sealed record SynergyDef(string Id, string Name, string Desc, int Tags, SynergyEffect Effect, int Value);
