using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Meta-progresja (port GBA/include/meta.h): sklep „Szkolenia”, odblokowania zawodów, narzędzi i poziomu Trudny,
/// odznaki, Katalog usterek, Osiedle, bankowanie doświadczenia z budowy.
/// </summary>
public static class Meta
{
    public static void ProfileReset(GameData d, Profile p)
    {
        var fresh = Profile.FromBytes(new byte[Profile.Size]);
        fresh.Magic = Profile.MagicBytes(Profile.MagicV3);
        fresh.Classes = (byte)d.StartClassesMask;
        CopyInto(fresh, p);
    }

    public static Profile NewProfile(GameData d)
    {
        var p = new Profile();
        ProfileReset(d, p);
        return p;
    }

    private static void CopyInto(Profile src, Profile dst)
    {
        var copy = Profile.FromBytes(src.ToBytes());
        dst.Magic = copy.Magic;
        dst.Best = copy.Best;
        dst.Runs = copy.Runs;
        dst.Wins = copy.Wins;
        dst.Xp = copy.Xp;
        dst.Levels = copy.Levels;
        dst.Classes = copy.Classes;
        dst.Hard = copy.Hard;
        dst.Flags = copy.Flags;
        dst.Tools = copy.Tools;
        dst.Badges = copy.Badges;
        dst.Catalog = copy.Catalog;
        dst.ClassWins = copy.ClassWins;
        dst.ToolsFound = copy.ToolsFound;
        dst.HousesCount = copy.HousesCount;
        dst.Houses = copy.Houses;
    }

    /// <summary>Naprawia wczytany profil. Zwraca true, jeśli trzeba go zapisać (migracja albo pusta pamięć).</summary>
    public static bool ProfileFix(GameData d, Profile p)
    {
        if (p.MagicIs(Profile.MagicV3)) return false;
        if (p.MagicIs(Profile.MagicV2)) // v2 -> v3: nowe pola od zera
        {
            p.Badges = 0;
            p.Catalog = 0;
            p.ClassWins = 0;
            p.ToolsFound = 0;
            p.HousesCount = 0;
            p.Houses = new byte[Profile.MaxHouses];
            p.Magic = Profile.MagicBytes(Profile.MagicV3);
            return true;
        }
        if (p.MagicIs(Profile.MagicV1))
        {
            int best = p.Best, runs = p.Runs, wins = p.Wins;
            ProfileReset(d, p);
            p.Best = best;
            p.Runs = runs;
            p.Wins = wins;
            return true;
        }
        ProfileReset(d, p);
        return true;
    }

    public static bool ClassUnlocked(Profile p, int c) => (p.Classes & (1u << c)) != 0;

    public static bool DifficultyUnlocked(GameData d, Profile p, int diff) => diff < d.Difficulties.Length - 1 || p.Hard != 0;

    /// <summary>Koszt kolejnego poziomu ulepszenia; -1 = maksymalny poziom.</summary>
    public static int UpgradeCost(GameData d, Profile p, int i)
    {
        var u = d.Upgrades[i];
        return p.Levels[i] < u.Levels ? u.Costs[p.Levels[i]] : -1;
    }

    public static bool BuyUpgrade(GameData d, Profile p, int i)
    {
        var c = UpgradeCost(d, p, i);
        if (c < 0 || p.Xp < c) return false;
        p.Xp -= c;
        ++p.Levels[i];
        return true;
    }

    public static bool BuyClass(GameData d, Profile p, int c)
    {
        if (ClassUnlocked(p, c) || p.Xp < d.ClassCost) return false;
        p.Xp -= d.ClassCost;
        p.Classes = (byte)(p.Classes | (1u << c));
        return true;
    }

    public static int ToolsMask(GameData d, Profile p) => p.Tools | d.StartToolsMask;

    public static bool ToolUnlocked(GameData d, Profile p, int i) => ((ToolsMask(d, p) >> i) & 1) != 0;

    public static bool BuyTool(GameData d, Profile p, int i)
    {
        if (ToolUnlocked(d, p, i) || p.Xp < d.Tools[i].Cost) return false;
        p.Xp -= d.Tools[i].Cost;
        p.Tools = (byte)(p.Tools | (1u << i));
        return true;
    }

    public static bool BuyHard(GameData d, Profile p)
    {
        if (p.Hard != 0 || p.Xp < d.HardCost) return false;
        p.Xp -= d.HardCost;
        p.Hard = 1;
        return true;
    }

    public static RunMods Mods(GameData d, Profile p)
    {
        var m = RunMods.Default(d);
        m.Tools = ToolsMask(d, p);
        for (var i = 0; i < d.Upgrades.Length; ++i)
        {
            var v = d.Upgrades[i].Value * p.Levels[i];
            switch (d.Upgrades[i].Effect)
            {
                case UpgradeEffect.Hp: m.Hp += v; break;
                case UpgradeEffect.Def: m.Def += v; break;
                case UpgradeEffect.Dmg: m.Dmg += v; break;
                case UpgradeEffect.Coffee: m.Coffee += v; break;
                case UpgradeEffect.Pickups: m.Pickups += v; break;
            }
        }
        return m;
    }

    /// <summary>Ekran „Koszty”: ile kosztuje cały sklep.</summary>
    public static int ShopTotalCost(GameData d)
    {
        var t = d.HardCost;
        foreach (var u in d.Upgrades)
        {
            foreach (var c in u.Costs) t += c;
        }
        for (var i = 0; i < d.Classes.Length; ++i)
        {
            if ((d.StartClassesMask & (1 << i)) == 0) t += d.ClassCost;
        }
        foreach (var tool in d.Tools) t += tool.Cost;
        return t;
    }

    /// <summary>Ile już wydano (pasek budżetu).</summary>
    public static int ShopSpent(GameData d, Profile p)
    {
        var t = p.Hard != 0 ? d.HardCost : 0;
        for (var i = 0; i < d.Upgrades.Length; ++i)
        {
            for (var l = 0; l < p.Levels[i]; ++l) t += d.Upgrades[i].Costs[l];
        }
        for (var i = 0; i < d.Classes.Length; ++i)
        {
            if (ClassUnlocked(p, i) && (d.StartClassesMask & (1 << i)) == 0) t += d.ClassCost;
        }
        for (var i = 0; i < d.Tools.Length; ++i)
        {
            if (ToolUnlocked(d, p, i)) t += d.Tools[i].Cost;
        }
        return t;
    }

    /// <summary>Dom na Osiedlu po wygranej budowie; wielkość z wyniku (0-3). Pełne Osiedle: najstarszy dom ustępuje.</summary>
    public static bool AddHouse(Profile p, Game g)
    {
        if (g.St != GameStatus.Won) return false;
        var h = (byte)((g.Cls & 15) | (Math.Min(3, g.Score / 1000) << 4));
        if (p.HousesCount < Profile.MaxHouses)
        {
            p.Houses[p.HousesCount++] = h;
        }
        else
        {
            for (var i = 1; i < Profile.MaxHouses; ++i) p.Houses[i - 1] = p.Houses[i];
            p.Houses[Profile.MaxHouses - 1] = h;
        }
        return true;
    }

    /// <summary>Przenosi do profilu trwałe osiągnięcia budowy (katalog, narzędzia, wygrane zawody).</summary>
    public static void RecordRun(GameData d, Profile p, Game g)
    {
        for (var e = 0; e < d.Enemies.Length; ++e)
        {
            if (g.KillsByType[e] != 0) p.Catalog = (ushort)(p.Catalog | (1u << e));
        }
        p.ToolsFound = (byte)(p.ToolsFound | g.ToolsFound);
        if (g.St == GameStatus.Won) p.ClassWins = (byte)(p.ClassWins | (1u << g.Cls));
    }

    /// <summary>Sprawdza odznaki po ważnym momencie; nowe dają doświadczenie. Zwraca bitmaskę zdobytych teraz.</summary>
    public static int CheckBadges(GameData d, Profile p, Game g)
    {
        RecordRun(d, p, g);
        var cleared = g.St == GameStatus.StageClear || g.St == GameStatus.Won;
        var won = g.St == GameStatus.Won;
        int allClasses = (1 << d.Classes.Length) - 1, allTools = (1 << d.Tools.Length) - 1;
        var cond = new bool[16];
        void Set(int idx, bool v)
        {
            if (idx >= 0) cond[idx] = v;
        }
        Set(d.BadgeBezUsterek, cleared && g.StageDamage == 0);
        Set(d.BadgePrzedTerminem, won && g.Turns - g.StageStartTurn <= 150);
        Set(d.BadgeSeryjny, g.StageKills >= 8);
        Set(d.BadgeZawodowiec, g.HeroLevel >= d.MaxHeroLevel);
        Set(d.BadgeTwardziel, won && g.Diff == d.Difficulties.Length - 1);
        Set(d.BadgePelnyZespol, (p.ClassWins & allClasses) == allClasses);
        Set(d.BadgeKolekcjoner, (p.ToolsFound & allTools) == allTools);
        Set(d.BadgeKatalog, p.Catalog == (1 << d.Enemies.Length) - 1);
        Set(d.BadgeOsiedle, p.HousesCount >= 5);
        var got = 0;
        for (var i = 0; i < d.Badges.Length; ++i)
        {
            if (cond[i] && (p.Badges & (1u << i)) == 0)
            {
                p.Badges = (ushort)(p.Badges | (1u << i));
                p.Xp += d.Badges[i].Xp;
                got |= 1 << i;
            }
        }
        return got;
    }

    /// <summary>Przenosi nowe doświadczenie z budowy do profilu. Zwraca, ile dodano.</summary>
    public static int BankXp(Profile p, Game g)
    {
        var delta = g.Xp - g.XpBanked;
        g.XpBanked = g.Xp;
        p.Xp += delta;
        return delta;
    }
}
