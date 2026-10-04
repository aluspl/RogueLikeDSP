using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>Tryb inwestora: stawka i premia doświadczenia za zestaw modyfikatorów (investor_stake / investor_xp).</summary>
public static class Investor
{
    public static int Stake(GameData d, int mask)
    {
        var s = 0;
        for (var i = 0; i < d.Investor.Length; ++i)
        {
            if ((mask & (1 << i)) != 0) s += d.Investor[i].Stake;
        }
        return s;
    }

    public static int Xp(GameData d, int mask)
    {
        var s = 0;
        for (var i = 0; i < d.Investor.Length; ++i)
        {
            if ((mask & (1 << i)) != 0) s += d.Investor[i].XpPct;
        }
        return s;
    }
}
