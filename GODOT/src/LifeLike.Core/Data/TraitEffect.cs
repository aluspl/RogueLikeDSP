namespace LifeLike.Core.Data;

/// <summary>
/// Rodzaj cechy przedmiotu sprzętu (core::trait_effect). Unknown: cecha z nowszej wersji danych GBA,
/// jeszcze nieprzeniesiona (bez działania).
/// </summary>
public enum TraitEffect : byte { Luck, Crit, PoisonRes, Sight, Cooldown, Str, Agi, Intel, Unknown = 255 }
