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
        fresh.Magic = Profile.MagicBytes(Profile.MagicV6);
        fresh.Classes = (byte)d.StartClassesMask;
        DefaultKeepsake(d, fresh);
        CopyInto(fresh, p);
    }

    /// <summary>Bez wybranej pamiątki: pierwsza odblokowana (nowy profil zaczyna z Termosem babci).</summary>
    public static void DefaultKeepsake(GameData d, Profile p)
    {
        if (p.Keepsake != 0) return;
        for (var k = 0; k < d.Keepsakes.Length; ++k)
        {
            if (!KeepsakeUnlocked(d, p, k)) continue;
            p.Keepsake = (byte)(k + 1);
            return;
        }
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
        dst.KillsTotal = copy.KillsTotal;
        dst.PowersTotal = copy.PowersTotal;
        dst.BrandTotal = copy.BrandTotal;
        dst.CleanBosses = copy.CleanBosses;
        dst.Contracts = copy.Contracts;
        dst.Keepsake = copy.Keepsake;
        dst.KeepsakeRuns = copy.KeepsakeRuns;
        dst.RunKills = copy.RunKills;
        dst.RunPowers = copy.RunPowers;
        dst.RunBrand = copy.RunBrand;
        dst.RunClean = copy.RunClean;
        dst.Brigade = copy.Brigade;
        dst.Investor = copy.Investor;
        dst.BestStake = copy.BestStake;
    }

    /// <summary>Naprawia wczytany profil. Zwraca true, jeśli trzeba go zapisać (migracja albo pusta pamięć).</summary>
    public static bool ProfileFix(GameData d, Profile p)
    {
        if (p.MagicIs(Profile.MagicV6)) return false;
        // v5/v4/v3/v2 -> v6: stare pola zostają, nowe od zera (jak memset od profile_v5_size / v4 / v3 / v2);
        // bez wybranej pamiątki – pierwsza odblokowana
        var keep = p.MagicIs(Profile.MagicV5) ? Profile.V5Size
            : p.MagicIs(Profile.MagicV4) ? Profile.V4Size
            : (p.MagicIs(Profile.MagicV3) ? Profile.V3Size : (p.MagicIs(Profile.MagicV2) ? Profile.V2Size : 0));
        if (keep > 0)
        {
            var b = p.ToBytes();
            Array.Clear(b, keep, b.Length - keep);
            CopyInto(Profile.FromBytes(b), p);
            p.Magic = Profile.MagicBytes(Profile.MagicV6);
            DefaultKeepsake(d, p);
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

    /// <summary>Brygada: fachowcy do wezwania (startowi zawsze, reszta za doświadczenie w Szkoleniach).</summary>
    public static int HelpersMask(GameData d, Profile p) => p.Brigade | d.StartHelpersMask;

    public static bool HelperUnlocked(GameData d, Profile p, int i) => ((HelpersMask(d, p) >> i) & 1) != 0;

    public static bool BuyHelper(GameData d, Profile p, int i)
    {
        if (HelperUnlocked(d, p, i) || p.Xp < d.Brigade[i].Cost) return false;
        p.Xp -= d.Brigade[i].Cost;
        p.Brigade = (byte)(p.Brigade | (1u << i));
        return true;
    }

    public static bool BuyHard(GameData d, Profile p)
    {
        if (p.Hard != 0 || p.Xp < d.HardCost) return false;
        p.Xp -= d.HardCost;
        p.Hard = 1;
        return true;
    }

    /// <summary>Tryb inwestora: odblokowany po pierwszej wygranej; wybór na ekranie zawodu.</summary>
    public static bool InvestorUnlocked(Profile p) => p.Wins > 0;

    public static int InvestorMask(GameData d, Profile p) => InvestorUnlocked(p) ? p.Investor & ((1 << d.Investor.Length) - 1) : 0;

    public static void ToggleInvestor(Profile p, int i) => p.Investor = (byte)(p.Investor ^ (1u << i));

    public static RunMods Mods(GameData d, Profile p)
    {
        var m = RunMods.Default(d);
        m.Tools = ToolsMask(d, p);
        m.Helpers = HelpersMask(d, p);
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
                case UpgradeEffect.Luck: m.Luck += v; break;
                case UpgradeEffect.Craft: m.Craft += v; break;
            }
        }
        for (var i = 0; i < d.Badges.Length; ++i) // uprawnienia ze zdobytych odznak
        {
            if ((p.Badges & (1u << i)) != 0) m.AddPerk(d.Badges[i].Bonus);
        }
        var k = SelectedKeepsake(d, p); // pamiątka zabrana na budowę
        if (k >= 0) m.AddPerk(KeepsakePerk(d, p, k));
        m.Investor = InvestorMask(d, p); // tryb inwestora: modyfikatory i premia doświadczenia
        m.XpPct += Investor.Xp(d, m.Investor);
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
        foreach (var h in d.Brigade) t += h.Cost;
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
        for (var i = 0; i < d.Brigade.Length; ++i)
        {
            if (HelperUnlocked(d, p, i)) t += d.Brigade[i].Cost;
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

    // ------------------------------------------------------------------ pamiątki
    public static bool KeepsakeUnlocked(GameData d, Profile p, int k)
    {
        var kd = d.Keepsakes[k];
        if (kd.Start || (kd.Badge >= 0 && (p.Badges & (1u << kd.Badge)) != 0)) return true;
        for (var i = 0; i < d.Contracts.Length; ++i)
        {
            if (d.Contracts[i].Keepsake == k && (p.Contracts & (1u << i)) != 0) return true;
        }
        return false;
    }

    /// <summary>Ranga 1-3: rośnie po KeepsakeRankRuns budowach z tą pamiątką.</summary>
    public static int KeepsakeRank(GameData d, Profile p, int k) =>
        1 + (p.KeepsakeRuns[k] >= d.KeepsakeRankRuns[0] ? 1 : 0) + (p.KeepsakeRuns[k] >= d.KeepsakeRankRuns[1] ? 1 : 0);

    public static Perk KeepsakePerk(GameData d, Profile p, int k) =>
        new(d.Keepsakes[k].Effect, d.Keepsakes[k].Values[KeepsakeRank(d, p, k) - 1]);

    /// <summary>Wybrana pamiątka (indeks) albo -1.</summary>
    public static int SelectedKeepsake(GameData d, Profile p)
    {
        var k = p.Keepsake - 1;
        return k >= 0 && k < d.Keepsakes.Length && KeepsakeUnlocked(d, p, k) ? k : -1;
    }

    /// <summary>Wybór na ekranie zawodu (L/R): kolejna odblokowana pamiątka albo „bez pamiątki”.</summary>
    public static void CycleKeepsake(GameData d, Profile p, int dir)
    {
        int n = d.Keepsakes.Length + 1, k = p.Keepsake;
        for (var i = 0; i < n; ++i)
        {
            k = (k + dir + n) % n;
            if (k == 0 || KeepsakeUnlocked(d, p, k - 1)) break;
        }
        p.Keepsake = (byte)k;
    }

    /// <summary>Start budowy: licznik budów i budów z wybraną pamiątką (Mods() wołać wcześniej – ranga z budów przed tą).</summary>
    public static void StartRun(GameData d, Profile p)
    {
        ++p.Runs;
        p.RunKills = 0; // nowa budowa nie ma jeszcze nic przeniesionego do liczników zleceń
        p.RunPowers = 0;
        p.RunBrand = 0;
        p.RunClean = 0;
        var k = SelectedKeepsake(d, p);
        if (k >= 0 && p.KeepsakeRuns[k] < 255) ++p.KeepsakeRuns[k];
    }

    // ------------------------------------------------------------------ zlecenia
    private static ushort AddSat16(ushort a, int delta) => (ushort)Math.Min(65535, a + Math.Max(0, delta));

    private static byte AddSat8(byte a, int delta) => (byte)Math.Min(255, a + Math.Max(0, delta));

    /// <summary>
    /// Przenosi do profilu nowe wartości liczników zleceń z budowy (bez podwójnego liczenia): dolicza tylko nadwyżkę
    /// ponad znak wodny Run* w profilu. Po wznowieniu budowy z wcześniejszego autozapisu liczniki gry są mniejsze
    /// niż znak wodny – powtórzony etap dolicza się dopiero, gdy go przebije.
    /// </summary>
    public static void BankCounters(Profile p, Game g)
    {
        p.KillsTotal = AddSat16(p.KillsTotal, g.Kills - p.RunKills);
        p.RunKills = (ushort)Math.Max(p.RunKills, Math.Min(65535, g.Kills));
        p.PowersTotal = AddSat16(p.PowersTotal, g.PowersUsed - p.RunPowers);
        p.RunPowers = Math.Max(p.RunPowers, g.PowersUsed);
        p.BrandTotal = AddSat8(p.BrandTotal, g.BrandFound - p.RunBrand);
        p.RunBrand = Math.Max(p.RunBrand, g.BrandFound);
        p.CleanBosses = AddSat8(p.CleanBosses, g.CleanBosses - p.RunClean);
        p.RunClean = Math.Max(p.RunClean, g.CleanBosses);
    }

    private static int PopCount(uint v)
    {
        var n = 0;
        for (; v != 0; v &= v - 1) ++n;
        return n;
    }

    /// <summary>Postęp zlecenia (licznik z profilu).</summary>
    public static int ContractProgress(GameData d, Profile p, int i) => d.Contracts[i].Kind switch
    {
        ContractKind.Kills => p.KillsTotal,
        ContractKind.Powers => p.PowersTotal,
        ContractKind.Brand => p.BrandTotal,
        ContractKind.CleanBoss => p.CleanBosses,
        ContractKind.ClassWins => PopCount(p.ClassWins),
        ContractKind.Wins => p.Wins,
        _ => 0,
    };

    public static bool ContractDone(Profile p, int i) => (p.Contracts & (1u << i)) != 0;

    /// <summary>Postęp zlecenia w trakcie budowy: profil + liczniki budowy jeszcze nieprzeniesione do profilu.</summary>
    public static int ContractProgressLive(GameData d, Profile p, Game g, int i)
    {
        var v = ContractProgress(d, p, i);
        return d.Contracts[i].Kind switch
        {
            ContractKind.Kills => v + Math.Max(0, g.Kills - p.RunKills),
            ContractKind.Powers => v + Math.Max(0, g.PowersUsed - p.RunPowers),
            ContractKind.Brand => v + Math.Max(0, g.BrandFound - p.RunBrand),
            ContractKind.CleanBoss => v + Math.Max(0, g.CleanBosses - p.RunClean),
            _ => v,
        };
    }

    /// <summary>Najbliższe ukończenia (największy % postępu) nieukończone zlecenie; -1 gdy wszystkie wykonane.</summary>
    public static int NextContract(GameData d, Profile p, Game g)
    {
        int best = -1, bestPct = -1;
        for (var i = 0; i < d.Contracts.Length; ++i)
        {
            if (ContractDone(p, i)) continue;
            var pct = Math.Min(100, ContractProgressLive(d, p, g, i) * 100 / d.Contracts[i].Target);
            if (pct > bestPct)
            {
                bestPct = pct;
                best = i;
            }
        }
        return best;
    }

    /// <summary>Sprawdza zlecenia: ukończone dają doświadczenie (i pamiątkę). Zwraca bitmaskę ukończonych właśnie teraz.</summary>
    public static int CheckContracts(GameData d, Profile p)
    {
        var got = 0;
        for (var i = 0; i < d.Contracts.Length; ++i)
        {
            if (!ContractDone(p, i) && ContractProgress(d, p, i) >= d.Contracts[i].Target)
            {
                p.Contracts = (byte)(p.Contracts | (1u << i));
                p.Xp += d.Contracts[i].Xp;
                got |= 1 << i;
            }
        }
        return got;
    }

    /// <summary>
    /// Przenosi do profilu trwałe osiągnięcia budowy (katalog, narzędzia, wygrane zawody, liczniki zleceń).
    /// Można wołać wielokrotnie.
    /// </summary>
    public static void RecordRun(GameData d, Profile p, Game g)
    {
        BankCounters(p, g);
        for (var e = 0; e < d.Enemies.Length; ++e)
        {
            if (g.KillsByType[e] != 0) p.Catalog = (ushort)(p.Catalog | (1u << e));
        }
        p.ToolsFound = (byte)(p.ToolsFound | g.ToolsFound);
        if (g.St == GameStatus.Won) p.ClassWins = (byte)(p.ClassWins | (1u << g.Cls));
        var stake = Investor.Stake(d, g.Bonus.Investor); // rekord stawki zawodu (wygrana w trybie inwestora)
        if (g.St == GameStatus.Won && stake > p.BestStake[g.Cls]) p.BestStake[g.Cls] = (byte)stake;
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
