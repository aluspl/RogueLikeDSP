using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>Podsumowanie budowy (#33, port z GBA/include/meta.h): rada (pierwsza pasująca) i najbliższy cel w profilu.</summary>
public static class Recap
{
    public static int TipIndex(GameData d, Game g)
    {
        var h = g.LastHits[0];
        var dead = g.St == GameStatus.Dead;
        for (var i = 0; i < d.RecapTips.Length; ++i)
        {
            var ok = d.RecapTips[i].When switch
            {
                RecapTip.Shock => dead && h.Kind == (byte)RecapKind.Shock,
                RecapTip.Slam => dead && h.Kind == (byte)RecapKind.Slam,
                RecapTip.Blast => dead && (h.Kind == (byte)RecapKind.Blast || h.Kind == (byte)RecapKind.Dust),
                RecapTip.Coffee => dead && g.Thermos > 0 && !g.WeeklyHas(WeeklyRule.NoCoffee),
                RecapTip.Ranged => dead && h.Kind == (byte)RecapKind.Ranged,
                RecapTip.Elite => dead && h.Elite >= 0,
                RecapTip.Boss => dead && h.Src >= 0 && d.Enemies[h.Src].Slam,
                RecapTip.NoCombo => g.CombosRun == 0,
                RecapTip.Won => g.St == GameStatus.Won,
                RecapTip.Any => true,
                _ => false,
            };
            if (ok) return i;
        }
        return d.RecapTips.Length - 1;
    }

    /// <summary>
    /// Najbliższy cel: najtańsza ranga Respektu („Jeszcze 3 Respektu do:” + „Pewna ręka II”); wszystko kupione – najbliższe
    /// Szkolenie za doświadczenie. false = nic nie zostało.
    /// </summary>
    public static bool Goal(GameData d, Profile p, out string lead, out string name)
    {
        int best = -1, bc = 0;
        for (var i = 0; i < d.Respect.Length; ++i)
        {
            var c = Meta.RespectUnlocked(d, p, i) ? Meta.RespectCost(d, p, i) : -1;
            if (c >= 0 && (best < 0 || c < bc))
            {
                best = i;
                bc = c;
            }
        }
        if (best >= 0)
        {
            lead = p.Respect >= bc ? Loc.T("stac_cie_respekt") : Loc.F("jeszcze_respektu_do", bc - p.Respect);
            name = $"{d.Respect[best].Name} {Roman(Meta.RespectRank(d, p, best) + 1)}";
            return true;
        }
        var cost = Meta.NextUnlock(d, p, out var kind, out var idx);
        if (cost < 0)
        {
            lead = name = "";
            return false;
        }
        lead = p.Xp >= cost ? Loc.T("stac_cie_szkolenia") : Loc.F("jeszcze_dosw_do", cost - p.Xp);
        name = kind switch
        {
            0 => $"{d.Upgrades[idx].Name} {Roman(p.Levels[idx] + 1)}",
            1 => d.Classes[idx].Name,
            2 => d.Weapons[d.Tools[idx].Weapon].Name,
            3 => d.Brigade[idx].Name,
            5 => Loc.F("drzewko_3", d.TreeBranches[d.TreeNodes[idx].Branch].Name),
            _ => d.Difficulties[^1].Name,
        };
        return true;
    }

    /// <summary>Rzymska liczba rangi (1-5), jak core::roman_numeral.</summary>
    public static string Roman(int n) => new[] { "", "I", "II", "III", "IV", "V" }[Math.Clamp(n, 0, 5)];
}
