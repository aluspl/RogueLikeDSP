using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>v0.23.0: rzadkość broni w ręku (Zwykła, Solidna, Markowa; Mistrzowska stała dla unikatów i broni mistrza zawodu).</summary>
public sealed partial class Game
{
    /// <summary>Najwyższy poziom z losowania (Markowa); Mistrzowska (3) nigdy nie wypada z dropu ani z Hurtowni.</summary>
    public const int MaxRolledWeaponRarity = 2;

    /// <summary>Rzadkość broni w ręku: 0 Zwykła, 1 Solidna, 2 Markowa, 3 Mistrzowska (GameData.WeaponRarities).</summary>
    public byte WeaponRarity;

    /// <summary>Rzadkość narzędzia, o którego zamianę pyta gracz (ulepszone narzędzie w ręku).</summary>
    public sbyte ToolOfferRarity;

    /// <summary>
    /// Losuje rzadkość nowej broni (drop, Hurtownia): rzut k100 + premia etapu jak przy sprzęcie (rarityRoll.stageBonus) i szczęście;
    /// progi z WeaponRarities[i].RollFrom. Wynik 0..2, nigdy Mistrzowska.
    /// </summary>
    public int RollWeaponRarity()
    {
        var q = R.Range(1, 100) + Math.Max(0, PatternStage()) * D.GearStageBonus + D.RarityPerLuck * Luck();
        var rarity = 0;
        for (var i = 1; i <= MaxRolledWeaponRarity && i < D.WeaponRarities.Length; ++i)
        {
            if (q >= D.WeaponRarities[i].RollFrom) rarity = i;
        }
        return rarity;
    }

    /// <summary>Rzadkość broni domyślnej na starcie budowy: Szkolenie Narzędzia (0..2) albo Mistrzowska przy broni mistrza zawodu.</summary>
    public static int StartRarity(GameData d, RunMods m) =>
        (m.Mastery & MasteryBit.Weapon) != 0 ? d.WeaponRarities.Length - 1 : Math.Min(m.WeaponRarity, MaxRolledWeaponRarity);

    /// <summary>Premia do obrażeń od rzadkości broni w ręku (dodawana jak UpgradeDmg).</summary>
    public int WeaponDmgBonus() => RarityDmg(WeaponRarity);

    private int RarityDmg(int rarity) => rarity >= 0 && rarity < D.WeaponRarities.Length ? D.WeaponRarities[rarity].DmgBonus : 0;

    /// <summary>Mistrzowska: unikat (WeaponDef.Master) albo broń mistrza zawodu (mistrzostwo, broń domyślna zawodu).</summary>
    public bool WeaponMaster() => Weapon.Master || (MasterWeapon() && WeaponOverride < 0);

    /// <summary>Rzadkość broni bez losowania: unikat i broń mistrza = Mistrzowska, inaczej podana.</summary>
    private int ResolveRarity(int weapon, int rolled) => D.Weapons[weapon].Master ? D.WeaponRarities.Length - 1 : rolled;

    /// <summary>Zmiana odnowienia trybu B od rzadkości broni w ręku (WeaponRarityDef.CooldownDelta).</summary>
    private int WeaponCdDelta() => WeaponRarity < D.WeaponRarities.Length ? D.WeaponRarities[WeaponRarity].CooldownDelta : 0;

    /// <summary>Narzędzia, które mogą wypaść na budowie: kupione i odblokowane oraz nowe bronie z flagą drop.</summary>
    private ulong DropToolPool() => Bonus.Tools | D.DropToolsMask;

    /// <summary>Narzędzia w puli „Nowe narzędzie” w Hurtowni: jak drop, ale z flagą hurtownia.</summary>
    private ulong ShopToolPool() => Bonus.Tools | D.HurtowniaToolsMask;

    /// <summary>k-te (od zera) ustawione bity maski, -1 gdy brak.</summary>
    private static int NthTool(ulong mask, int k)
    {
        for (var t = 0; t < 64; ++t)
        {
            if (((mask >> t) & 1UL) != 0 && k-- == 0) return t;
        }
        return -1;
    }

    private static int CountBits(ulong mask) => System.Numerics.BitOperations.PopCount(mask);
}
