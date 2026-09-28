namespace LifeLike.Core.Data;

/// <summary>
/// Premia po etapie (core::boon_def, #27): 1 z 3 po każdym etapie. Rarity: 0 zwykła, 1 rzadka, 2 legendarna; Tags: bity
/// GameData.BoonTags; Cls: premia zawodu (-1 = dla każdego).
/// </summary>
public sealed record BoonDef(string Id, string Name, string Desc, int Rarity, int Tags, BoonEffect Effect, int Value, int Cls);
