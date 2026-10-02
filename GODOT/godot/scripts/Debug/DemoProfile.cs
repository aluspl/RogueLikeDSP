using System;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game.Debug;

/// <summary>Profil pokazowy do zrzutów: część odznak (uprawnienia), zlecenia z postępem, pamiątka z rangą II, Warsztaty, domy.</summary>
public static class DemoProfile
{
    public static Profile Create(GameData d)
    {
        var p = Meta.NewProfile(d);
        p.Runs = 14;
        p.Wins = 3;
        p.Best = 5613;
        p.Xp = 85;
        p.Badges = (ushort)((1 << d.BadgeBezUsterek) | (1 << d.BadgeSeryjny) | (1 << d.BadgeKolekcjoner));
        p.ClassWins = 0x03;
        p.KillsTotal = 163;
        p.PowersTotal = 71;
        p.BrandTotal = 5;
        p.CleanBosses = 1;
        p.Catalog = 0x5F;
        p.HousesCount = 3;
        p.Houses[0] = 0x00;
        p.Houses[1] = 0x21;
        p.Houses[2] = 0x34;
        Meta.CheckContracts(d, p);
        var craft = Array.FindIndex(d.Upgrades, u => u.Effect == UpgradeEffect.Craft);
        if (craft >= 0) p.Levels[craft] = (byte)Math.Min(2, d.Upgrades[craft].Levels); // Warsztaty mają dziś 1 poziom
        var kielnia = Array.FindIndex(d.Keepsakes, k => k.Effect == PerkEffect.Luck);
        if (kielnia >= 0)
        {
            p.Keepsake = (byte)(kielnia + 1);
            p.KeepsakeRuns[kielnia] = 4;
        }
        p.KeepsakeRuns[0] = 2;
        p.Rewards = 3;            // nagrody za odbiór za 3 wygrane: Młot udarowy, Dekarz, Buty robocze
        p.Respect = 140;
        p.RespectTotal = 320;
        p.RespectRanks[0] = 1;
        p.RespectRanks[5] = 2;
        p.RespectRanks[8] = 3;
        return p;
    }
}
