using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.22.0 (UX A): co kupić za doświadczenie między budowami. Szybki zakup jednym stuknięciem obejmuje tylko pozycje bez
/// wyboru (rodzaje 0-4 z Meta.NextUnlock); węzeł drzewka (5) wymaga wyboru opcji w Szkoleniach.
/// </summary>
public static class Spend
{
    /// <summary>Koszt najtańszej pozycji bez wyboru, na którą stać gracza; -1 = nic takiego.</summary>
    public static int QuickKind(GameData d, Profile p, out int kind, out int idx)
    {
        var c = Meta.NextUnlock(d, p, false, out kind, out idx);
        if (c >= 0 && p.Xp >= c) return c;
        kind = idx = -1;
        return -1;
    }

    /// <summary>Kupuje to, co wskazuje QuickKind. false = nic nie kupiono.</summary>
    public static bool QuickBuy(GameData d, Profile p)
    {
        if (QuickKind(d, p, out var kind, out var idx) < 0) return false;
        return kind switch
        {
            0 => Meta.BuyUpgrade(d, p, idx),
            1 => Meta.BuyClass(d, p, idx),
            2 => Meta.BuyTool(d, p, idx),
            3 => Meta.BuyHelper(d, p, idx),
            4 => Meta.BuyHard(d, p),
            _ => false,
        };
    }

    /// <summary>Najtańszy otwarty, niewybrany węzeł drzewka, na który stać gracza; -1 = brak.</summary>
    public static int TreePick(GameData d, Profile p)
    {
        int best = -1;
        for (var n = 0; n < d.TreeNodes.Length; ++n)
        {
            var c = d.TreeNodes[n].Cost;
            if (SkillTree.Pick(p, n) != 0 || !SkillTree.Open(d, p, n) || p.Xp < c) continue;
            if (best < 0 || c < d.TreeNodes[best].Cost) best = n;
        }
        return best;
    }

    /// <summary>Indeks w d.Rewards nagrody za następny odbiór albo -1; toolWeapon = broń z nagrody-narzędzia albo -1.</summary>
    public static int NextReward(GameData d, Profile p, out int toolWeapon)
    {
        toolWeapon = -1;
        if (p.Rewards >= Meta.RewardsAvailable(d)) return -1;
        var r = d.Rewards[p.Rewards];
        if (r.Kind == RewardKind.Tool) toolWeapon = d.Tools[r.Index].Weapon;
        return p.Rewards;
    }
}
