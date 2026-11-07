using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: losowe profity broni wg rzadkości (GameData.WeaponPerkPool): Zwykła 0, Solidna 1, Markowa 2 bez powtórzeń,
/// Mistrzowska – 2 stałe (unikat). Maska bitów = indeksy puli. Profity sumują się z cechą ulepszenia (ten sam skutek).
/// </summary>
public sealed partial class Game
{
    /// <summary>Profity broni w ręku (maska bitów puli).</summary>
    public byte WeaponPerks;

    /// <summary>Profity broni w kaburze.</summary>
    public byte Weapon2Perks;

    /// <summary>Profity narzędzia z oferty wymiany (z Pickup.Perks).</summary>
    public byte ToolOfferPerks;

    /// <summary>Licznik losowań profitów (osobny generator z seeda budowy – bez wpływu na RNG gry).</summary>
    public ushort PerkRolls;

    /// <summary>Mistrzowska (ostatnia rzadkość) – profity stałe, nie losowane.</summary>
    private bool MasterRarity(int rarity) => D.WeaponRarities.Length > 0 && rarity >= D.WeaponRarities.Length - 1;

    /// <summary>
    /// Profity nowej broni o rzadkości rarity (już rozstrzygniętej – ResolveRarity): Mistrzowska = stałe (GameData.MasterPerkMask),
    /// inaczej perRarity[rarity] różnych z puli. Losuje osobny generator z seeda budowy i licznika PerkRolls (deterministycznie).
    /// </summary>
    public int RollWeaponPerks(int weapon, int rarity)
    {
        if (MasterRarity(rarity)) return D.MasterPerkMask(weapon);
        var n = rarity >= 0 && rarity < D.WeaponPerksPerRarity.Length ? D.WeaponPerksPerRarity[rarity] : 0;
        var pool = D.WeaponPerkPool.Length;
        n = Math.Min(n, pool);
        if (n <= 0) return 0;
        var r = new Rng();
        r.Seed((RunSeed ^ (++PerkRolls * 2654435761u)) * 2246822519u + 0x9E3779B9u);
        var mask = 0;
        for (var k = 0; k < n; ++k)
        {
            var pick = r.Range(0, pool - 1 - k); // k-ty wolny indeks puli – bez powtórzeń
            for (var i = 0; i < pool; ++i)
            {
                if ((mask & (1 << i)) != 0) continue;
                if (pick-- == 0)
                {
                    mask |= 1 << i;
                    break;
                }
            }
        }
        return mask;
    }

    /// <summary>Profity broni po wzięciu: Mistrzowska zawsze stałe, inaczej zapisane przy powstaniu broni (Pickup.Perks).</summary>
    private byte ResolvePerks(int weapon, int rarity, int perks) => (byte)(MasterRarity(rarity) ? D.MasterPerkMask(weapon) : perks);

    /// <summary>Suma wartości profitów z maski o danym skutku.</summary>
    public int PerkValue(int perks, ToolTraitEffect e)
    {
        var v = 0;
        for (var i = 0; i < D.WeaponPerkPool.Length; ++i)
        {
            if ((perks & (1 << i)) != 0 && D.WeaponPerkPool[i].Effect == e) v += D.WeaponPerkPool[i].Value;
        }
        return v;
    }

    /// <summary>Skutek broni w ręku: cecha ulepszenia + profity (ten sam skutek dodaje wartości).</summary>
    public int WeaponBonus(ToolTraitEffect e) => ToolTraitValue(e) + PerkValue(WeaponPerks, e);
}
