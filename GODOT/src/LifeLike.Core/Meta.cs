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
        fresh.Magic = Profile.MagicBytes(Profile.MagicCurrent);
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
        dst.DailyD = copy.DailyD;
        dst.DailyM = copy.DailyM;
        dst.DailyY = copy.DailyY;
        dst.DailyDay = copy.DailyDay;
        dst.DailyWon = copy.DailyWon;
        dst.DailyRuns = copy.DailyRuns;
        dst.DailyScore = copy.DailyScore;
        dst.Respect = copy.Respect;
        dst.RespectTotal = copy.RespectTotal;
        dst.RunRespect = copy.RunRespect;
        dst.Rewards = copy.Rewards;
        dst.ClassWinsHi = copy.ClassWinsHi;
        dst.RespectRanks = copy.RespectRanks;
        dst.BestStakeHi = copy.BestStakeHi;
        dst.CatalogHi = copy.CatalogHi;
        dst.Tutorial = copy.Tutorial;
        dst.ClassesSeen = copy.ClassesSeen;
        dst.WeeklyWeek = copy.WeeklyWeek;
        dst.WeeklyWon = copy.WeeklyWon;
        dst.WeeklyRuns = copy.WeeklyRuns;
        dst.WeeklyScore = copy.WeeklyScore;
        dst.Story = copy.Story;
        dst.StoryNew = copy.StoryNew;
        dst.Secrets = copy.Secrets;
        dst.SecretsNew = copy.SecretsNew;
        dst.Cosmetic = copy.Cosmetic;
        dst.RespectRanksHi = copy.RespectRanksHi;
        dst.Title = copy.Title;
        dst.Helmet = copy.Helmet;
    }

    // ------------------------------------------------------------------ katalog usterek (rodzaje 0-15 w Catalog, 16-47 w CatalogHi)
    public static bool CatalogHas(Profile p, int d) => d < 16 ? ((p.Catalog >> d) & 1) != 0 : ((p.CatalogHi >> (d - 16)) & 1) != 0;

    public static void CatalogAdd(Profile p, int d)
    {
        if (d < 16) p.Catalog = (ushort)(p.Catalog | (1u << d));
        else p.CatalogHi |= 1u << (d - 16);
    }

    public static int CatalogCount(GameData d, Profile p)
    {
        var n = 0;
        for (var e = 0; e < d.Enemies.Length; ++e)
        {
            if (CatalogHas(p, e)) n++;
        }
        return n;
    }

    // ------------------------------------------------------------------ nagrody za odbiór
    /// <summary>Nagroda i jest odebrana, gdy i &lt; p.Rewards (każda wygrana odblokowuje kolejną; „wkrótce” się nie odblokowuje).</summary>
    public static bool RewardOwned(Profile p, int i) => i < p.Rewards;

    public static bool RewardUnlocked(GameData d, Profile p, RewardKind k, int index)
    {
        for (var i = 0; i < d.Rewards.Length && i < p.Rewards; ++i)
        {
            if (d.Rewards[i].Kind == k && d.Rewards[i].Index == index) return true;
        }
        return false;
    }

    /// <summary>Ile nagród da się odebrać (bez „wkrótce” na końcu listy).</summary>
    public static int RewardsAvailable(GameData d)
    {
        var n = 0;
        while (n < d.Rewards.Length && d.Rewards[n].Kind != RewardKind.Soon) ++n;
        return n;
    }

    /// <summary>Numer wygranej (licząc od 1), która odblokuje nagrodę i (dla odebranych i „wkrótce”: -1).</summary>
    public static int RewardWin(GameData d, Profile p, int i)
    {
        if (RewardOwned(p, i) || i >= RewardsAvailable(d)) return -1;
        return p.Wins + (i - p.Rewards) + 1;
    }

    /// <summary>Wygrana budowa: licznik i kolejna nagroda. Zwraca indeks odblokowanej nagrody albo -1.</summary>
    public static int RecordWin(GameData d, Profile p)
    {
        ++p.Wins;
        if (p.Rewards >= RewardsAvailable(d)) return -1;
        return p.Rewards++;
    }

    /// <summary>Zawód wygrany (odznaka Pełny zespół, zlecenie Trzy fachy): bity 0-7 w ClassWins, 8-15 w ClassWinsHi.</summary>
    public static bool ClassWon(Profile p, int c) => c < 8 ? ((p.ClassWins >> c) & 1) != 0 : ((p.ClassWinsHi >> (c - 8)) & 1) != 0;

    public static void SetClassWon(Profile p, int c)
    {
        if (c < 8) p.ClassWins = (byte)(p.ClassWins | (1u << c));
        else p.ClassWinsHi = (byte)(p.ClassWinsHi | (1u << (c - 8)));
    }

    public static int ClassesWon(GameData d, Profile p)
    {
        var n = 0;
        for (var c = 0; c < d.Classes.Length; ++c) n += ClassWon(p, c) ? 1 : 0;
        return n;
    }

    /// <summary>Wygrane zwykłymi zawodami (bez zawodów z sekretów): odznaka Pełny zespół, sekret Każdy fach się przyda.</summary>
    public static int OpenClassesWon(GameData d, Profile p)
    {
        var n = 0;
        for (var c = 0; c < d.OpenClassesCount; ++c) n += ClassWon(p, c) ? 1 : 0;
        return n;
    }

    public static int BestStake(Profile p, int c) => c < 8 ? p.BestStake[c] : p.BestStakeHi[c - 8];

    public static void SetBestStake(Profile p, int c, int v)
    {
        if (c < 8) p.BestStake[c] = (byte)v;
        else p.BestStakeHi[c - 8] = (byte)v;
    }

    /// <summary>
    /// v0.21.49 (profil sprzed v8): nagrody za odbiór za dotychczasowe wygrane (zwrot za zmienione Szkolenia BHP i Kurs
    /// fachowy robi teraz MigrateV13 – ta sama kwota: stary koszt poziomu).
    /// </summary>
    public static void MigrateV8(GameData d, Profile p)
    {
        p.Rewards = (byte)Math.Min(Math.Max(0, p.Wins), RewardsAvailable(d));
    }

    /// <summary>Zwrot po starej cenie (sprzed v0.21.52) za kupione poziomy Szkolenia i; poziom ponad stare maksimum: Refund.</summary>
    public static int LegacyRefund(GameData d, Profile p, int i)
    {
        var u = d.Upgrades[i];
        var t = 0;
        for (var l = 0; l < p.Levels[i]; ++l) t += l < u.LegacyCosts.Length ? u.LegacyCosts[l] : u.Refund;
        return t;
    }

    /// <summary>
    /// v12 -> v13 (v0.21.52): Szkolenia mają 4-5 poziomów z mniejszymi krokami i wyższą ceną – kupione poziomy wracają jako
    /// doświadczenie po starej cenie, poziomy od zera. Zawody, narzędzia, Trudny i brygada zostają; tytuł i kask bez wyboru.
    /// </summary>
    public static void MigrateV13(GameData d, Profile p)
    {
        for (var i = 0; i < Profile.MaxUpgrades; ++i)
        {
            if (i < d.Upgrades.Length) p.Xp += LegacyRefund(d, p, i);
            p.Levels[i] = 0;
        }
        p.Title = 0;
        p.Helmet = 0;
    }

    /// <summary>
    /// Szkolenia z mniejszą liczbą poziomów niż w starym profilu (np. po zmianie balansu): poziomy ponad maksimum wracają
    /// jako doświadczenie (Refund z danych). Zwraca true, jeśli coś zmieniono.
    /// </summary>
    public static bool ClampLevels(GameData d, Profile p)
    {
        var changed = false;
        for (var i = 0; i < d.Upgrades.Length; ++i)
        {
            while (p.Levels[i] > d.Upgrades[i].Levels)
            {
                --p.Levels[i];
                p.Xp += d.Upgrades[i].Refund;
                changed = true;
            }
        }
        return changed;
    }

    /// <summary>Naprawia wczytany profil. Zwraca true, jeśli trzeba go zapisać (migracja albo pusta pamięć).</summary>
    public static bool ProfileFix(GameData d, Profile p)
    {
        if (p.MagicIs(Profile.MagicCurrent)) return ClampLevels(d, p);
        if (p.MagicIs(Profile.MagicV12)) // v12 -> v13: zwrot za Szkolenia, tytuł i kask od zera
        {
            var b12 = p.ToBytes();
            Array.Clear(b12, Profile.V12Size, b12.Length - Profile.V12Size);
            CopyInto(Profile.FromBytes(b12), p);
            p.Magic = Profile.MagicBytes(Profile.MagicCurrent);
            MigrateV13(d, p);
            return true;
        }
        if (p.MagicIs(Profile.MagicV11)) // v11 -> v12: sekretne zlecenia z tego, co już widać w profilu
        {
            var b11 = p.ToBytes();
            Array.Clear(b11, Profile.V11Size, b11.Length - Profile.V11Size);
            CopyInto(Profile.FromBytes(b11), p);
            p.Magic = Profile.MagicBytes(Profile.MagicCurrent);
            MigrateV13(d, p);
            Secrets.MigrateV12(d, p);
            return true;
        }
        if (p.MagicIs(Profile.MagicV10)) // v10 -> v11: wyzwania tygodnia i fabuła od zera
        {
            var b10 = p.ToBytes();
            Array.Clear(b10, Profile.V10Size, b10.Length - Profile.V10Size);
            CopyInto(Profile.FromBytes(b10), p);
            p.Magic = Profile.MagicBytes(Profile.MagicCurrent);
            MigrateV13(d, p);
            Story.MigrateV11(d, p);
            Secrets.MigrateV12(d, p);
            return true;
        }
        var v9 = p.MagicIs(Profile.MagicV9);
        if (v9 || p.MagicIs(Profile.MagicV8)) // v8 -> v9: katalog 16-47 od zera; v9 -> v10: samouczek
        {
            var keep8 = v9 ? Profile.V9Size : Profile.V8Size;
            var b8 = p.ToBytes();
            Array.Clear(b8, keep8, b8.Length - keep8);
            CopyInto(Profile.FromBytes(b8), p);
            p.Magic = Profile.MagicBytes(Profile.MagicCurrent);
            MigrateV13(d, p);
            MigrateV10(d, p);
            Story.MigrateV11(d, p);
            Secrets.MigrateV12(d, p);
            return true;
        }
        // v7/v6/v5/v4/v3/v2 -> v9: stare pola zostają, nowe od zera (jak memset od profile_v7_size / v6 / ...);
        // bez wybranej pamiątki – pierwsza odblokowana
        var keep = p.MagicIs(Profile.MagicV7) ? Profile.V7Size
            : p.MagicIs(Profile.MagicV6) ? Profile.V6Size
            : p.MagicIs(Profile.MagicV5) ? Profile.V5Size
            : p.MagicIs(Profile.MagicV4) ? Profile.V4Size
            : (p.MagicIs(Profile.MagicV3) ? Profile.V3Size : (p.MagicIs(Profile.MagicV2) ? Profile.V2Size : 0));
        if (keep > 0)
        {
            var b = p.ToBytes();
            Array.Clear(b, keep, b.Length - keep);
            CopyInto(Profile.FromBytes(b), p);
            p.Magic = Profile.MagicBytes(Profile.MagicCurrent);
            DefaultKeepsake(d, p);
            MigrateV13(d, p);
            MigrateV8(d, p);
            MigrateV10(d, p);
            Story.MigrateV11(d, p);
            Secrets.MigrateV12(d, p);
            return true;
        }
        if (p.MagicIs(Profile.MagicV1))
        {
            int best = p.Best, runs = p.Runs, wins = p.Wins;
            ProfileReset(d, p);
            p.Best = best;
            p.Runs = runs;
            p.Wins = wins;
            MigrateV8(d, p);
            MigrateV10(d, p);
            Story.MigrateV11(d, p);
            Secrets.MigrateV12(d, p);
            return true;
        }
        ProfileReset(d, p);
        return true;
    }

    /// <summary>Zawód z sekretnego zlecenia (v0.21.51 cz. 2).</summary>
    public static bool ClassSecret(GameData d, int c) => ((d.SecretClassesMask >> c) & 1) != 0;

    /// <summary>Zawód nie do kupienia w Szkoleniach: z nagrody za odbiór albo z sekretnego zlecenia.</summary>
    public static bool ClassReward(GameData d, int c) => (((d.RewardClassesMask | d.SecretClassesMask) >> c) & 1) != 0;

    /// <summary>Zawód: startowy / kupiony w Szkoleniach (bitmaska), z nagrody za odbiór albo z sekretnego zlecenia.</summary>
    public static bool ClassUnlocked(GameData d, Profile p, int c)
    {
        if (ClassSecret(d, c)) return Secrets.Owned(d, p, SecretReward.Cls, c);
        return ClassReward(d, c) ? RewardUnlocked(d, p, RewardKind.Cls, c) : (p.Classes & (1u << c)) != 0;
    }

    public static bool DifficultyUnlocked(GameData d, Profile p, int diff) => diff < d.Difficulties.Length - 1 || p.Hard != 0;

    /// <summary>
    /// v9 -> v10: nagroda Akt 0 za dotychczasowe wygrane (wcześniej „wkrótce”); samouczek – kto już grał, nie ogląda
    /// głównego samouczka ani dymków o tym, co już zna (Akt 0 z migracji dostaje dymek).
    /// </summary>
    public static void MigrateV10(GameData d, Profile p)
    {
        p.Rewards = (byte)Math.Max(p.Rewards, Math.Min(Math.Max(0, p.Wins), RewardsAvailable(d)));
        p.Tutorial = 0;
        p.ClassesSeen = 0;
        if (p.Runs > 0) p.Tutorial = Tutorial.Title | Tutorial.Class | Tutorial.Daily;
        if (p.RespectTotal > 0) p.Tutorial |= Tutorial.Respect;
        if (p.Wins > 0) p.Tutorial |= Tutorial.Investor;
        for (var c = 0; c < d.Classes.Length; ++c)
        {
            if (ClassReward(d, c) && ClassUnlocked(d, p, c)) p.ClassesSeen = (ushort)(p.ClassesSeen | (1u << c));
        }
    }

    /// <summary>Akt 0 (Papierologia) z nagrody za odbiór: budowa zaczyna się od jego etapów.</summary>
    public static bool Act0Unlocked(GameData d, Profile p)
    {
        for (var i = 0; i < d.Rewards.Length && i < p.Rewards; ++i)
        {
            if (d.Rewards[i].Kind == RewardKind.Act) return true;
        }
        return false;
    }

    // ------------------------------------------------------------------ samouczek menu (#25)
    // Pierwsze uruchomienie: dymki po kolei na tytule i wyborze zawodu; potem jeden dymek przy pierwszym odblokowaniu
    // (Respekt, codzienna budowa, tryb inwestora, Akt 0, nowy zawód). „Pokaż samouczek jeszcze raz” w Jak grać.

    /// <summary>Główny samouczek ekranu (0 tytuł, 1 wybór zawodu) jeszcze nieobejrzany.</summary>
    public static bool TutorialPending(Profile p, int screen) => (p.Tutorial & (screen == 0 ? Tutorial.Title : Tutorial.Class)) == 0;

    public static void TutorialDone(Profile p, int screen) => p.Tutorial |= screen == 0 ? Tutorial.Title : Tutorial.Class;

    public static void TutorialReset(Profile p) => p.Tutorial = (ushort)(p.Tutorial & ~(Tutorial.Title | Tutorial.Class));

    /// <summary>Krok samouczka widoczny na ekranie (bez kroków tylko dla Godota na GBA; tryb inwestora po odblokowaniu).</summary>
    public static bool TutorialStepShown(GameData d, Profile p, int i, bool godot)
    {
        var t = d.TutorialSteps[i];
        return (godot || !t.GodotOnly) && (!t.NeedsInvestor || InvestorUnlocked(p));
    }

    /// <summary>Dymek odblokowania do pokazania na ekranie (-1 = brak; indeks = TutorialUnlock); cls = nowy zawód z nagrody.</summary>
    public static int PendingUnlock(GameData d, Profile p, int screen, out int cls)
    {
        cls = -1;
        if (TutorialPending(p, screen)) return -1; // najpierw główny samouczek
        if (screen == 0)
        {
            if (p.RespectTotal > 0 && (p.Tutorial & Tutorial.Respect) == 0) return TutorialUnlock.Respect;
            if (p.Runs > 0 && (p.Tutorial & Tutorial.Daily) == 0) return TutorialUnlock.Daily;
            if (Act0Unlocked(d, p) && (p.Tutorial & Tutorial.Act0) == 0) return TutorialUnlock.Act0;
            for (var i = 0; i < d.Secrets.Length; ++i)
            {
                if (((p.SecretsNew >> i) & 1) == 0) continue;
                cls = i;
                return TutorialUnlock.Secret;
            }
            return -1;
        }
        if (InvestorUnlocked(p) && (p.Tutorial & Tutorial.Investor) == 0) return TutorialUnlock.Investor;
        for (var c = 0; c < d.Classes.Length; ++c)
        {
            if (ClassReward(d, c) && ClassUnlocked(d, p, c) && ((p.ClassesSeen >> c) & 1) == 0)
            {
                cls = c;
                return TutorialUnlock.Class;
            }
        }
        return -1;
    }

    public static void MarkUnlock(Profile p, int u, int cls)
    {
        ushort[] bits = [Tutorial.Respect, Tutorial.Daily, Tutorial.Investor, Tutorial.Act0];
        if (u is >= 0 and < 4) p.Tutorial |= bits[u];
        if (u == TutorialUnlock.Class && cls >= 0) p.ClassesSeen = (ushort)(p.ClassesSeen | (1u << cls));
        if (u == TutorialUnlock.Secret && cls >= 0) p.SecretsNew = (ushort)(p.SecretsNew & ~(1u << cls));
    }

    /// <summary>Koszt kolejnego poziomu ulepszenia; -1 = maksymalny poziom.</summary>
    public static int UpgradeCost(GameData d, Profile p, int i)
    {
        var u = d.Upgrades[i];
        return p.Levels[i] < u.Levels ? u.Cost(p.Levels[i]) : -1;
    }

    /// <summary>Łączna wartość działania e z poziomów 1..levels Szkolenia i (upgrade_total).</summary>
    public static int UpgradeTotal(GameData d, int i, int levels, UpgradeEffect e)
    {
        var u = d.Upgrades[i];
        var t = 0;
        for (var l = 0; l < levels && l < u.Levels; ++l)
        {
            if (u.Steps[l].Effect == e) t += u.Steps[l].Value;
        }
        return t;
    }

    /// <summary>Razem z poziomów 1..levels Szkolenia i, działania w kolejności poziomów („Kryt +1%, +1 szczęścia”).</summary>
    public static Message UpgradeSummary(GameData d, Message m, int i, int levels)
    {
        var u = d.Upgrades[i];
        var any = false;
        for (var l = 0; l < levels && l < u.Levels; ++l)
        {
            var seen = false;
            for (var k = 0; k < l; ++k) seen |= u.Steps[k].Effect == u.Steps[l].Effect;
            if (seen) continue;
            if (any) m.Add(", ");
            RunMods.UpgradeLabel(m, u.Steps[l].Effect, UpgradeTotal(d, i, levels, u.Steps[l].Effect));
            any = true;
        }
        return m;
    }

    public static string UpgradeSummary(GameData d, int i, int levels) => UpgradeSummary(d, new Message(), i, levels).Text;

    /// <summary>v0.21.52: zawód na sprzedaż w Szkoleniach (nie startowy, nie z nagrody ani sekretu).</summary>
    public static bool ClassForSale(GameData d, int c) => (d.StartClassesMask & (1 << c)) == 0 && !ClassReward(d, c);

    public static int ClassesBought(GameData d, Profile p)
    {
        var n = 0;
        for (var c = 0; c < d.Classes.Length; ++c)
        {
            if (ClassForSale(d, c) && (p.Classes & (1u << c)) != 0) ++n;
        }
        return n;
    }

    /// <summary>Cena zawodu kupowanego jako bought+1 (rośnie z każdym zakupem; ostatnia dla następnych).</summary>
    public static int ClassPrice(GameData d, int bought) => d.ClassCosts[Math.Min(bought, d.ClassCosts.Length - 1)];

    public static int ClassCost(GameData d, Profile p) => ClassPrice(d, ClassesBought(d, p));

    public static int ToolsBought(GameData d, Profile p)
    {
        var n = 0;
        for (var i = 0; i < d.Tools.Length; ++i)
        {
            if (d.Tools[i].Shop && ((p.Tools >> i) & 1) != 0) ++n;
        }
        return n;
    }

    public static int ToolPrice(GameData d, int bought) => d.ToolCosts[Math.Min(bought, d.ToolCosts.Length - 1)];

    public static int ToolCost(GameData d, Profile p) => ToolPrice(d, ToolsBought(d, p));

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
        var cost = ClassCost(d, p);
        if (ClassReward(d, c) || ClassUnlocked(d, p, c) || p.Xp < cost) return false;
        p.Xp -= cost;
        p.Classes = (byte)(p.Classes | (1u << c));
        return true;
    }

    public static int ToolsMask(GameData d, Profile p)
    {
        var m = p.Tools | d.StartToolsMask;
        for (var i = 0; i < d.Tools.Length; ++i)
        {
            if (d.Tools[i].Reward) m = RewardUnlocked(d, p, RewardKind.Tool, i) ? m | (1 << i) : m & ~(1 << i);
            if (d.Tools[i].Secret) m = Secrets.Owned(d, p, SecretReward.Tool, i) ? m | (1 << i) : m & ~(1 << i);
        }
        return m;
    }

    /// <summary>Sloty sprzętu w dropach: kask, rękawice, kamizelka + odebrane w nagrodach (buty, pas).</summary>
    public static int GearSlotsMask(GameData d, Profile p)
    {
        var m = d.GearBaseMask;
        for (var i = 0; i < d.GearSlotsCount; ++i)
        {
            if (RewardUnlocked(d, p, RewardKind.Gear, i)) m |= 1 << i;
        }
        return m;
    }

    public static bool ToolUnlocked(GameData d, Profile p, int i) => ((ToolsMask(d, p) >> i) & 1) != 0;

    public static bool BuyTool(GameData d, Profile p, int i)
    {
        var cost = ToolCost(d, p);
        if (!d.Tools[i].Shop || ToolUnlocked(d, p, i) || p.Xp < cost) return false;
        p.Xp -= cost;
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

    // ------------------------------------------------------------------ Respekt (telefon profilu, strona Respekt)
    /// <summary>Kupiona ranga (surowa): 0-15 w RespectRanks, 16-18 w RespectRanksHi (v12).</summary>
    public static int RespectSlot(Profile p, int i) => i < Profile.MaxRespect ? p.RespectRanks[i] : p.RespectRanksHi[i - Profile.MaxRespect];

    public static void SetRespectRank(Profile p, int i, int r)
    {
        if (i < Profile.MaxRespect) p.RespectRanks[i] = (byte)r;
        else p.RespectRanksHi[i - Profile.MaxRespect] = (byte)r;
    }

    /// <summary>Ranga z sekretnego zlecenia (Zaprawiony w boju) – dopiero po jego wykonaniu.</summary>
    public static bool RespectUnlocked(GameData d, Profile p, int i) => d.Respect[i].Secret < 0 || Secrets.Done(p, d.Respect[i].Secret);

    public static int RespectRank(GameData d, Profile p, int i) => Math.Min(RespectSlot(p, i), d.Respect[i].Ranks);

    /// <summary>Koszt kolejnej rangi; -1 = maksymalna.</summary>
    public static int RespectCost(GameData d, Profile p, int i)
    {
        var r = RespectRank(d, p, i);
        return r < d.Respect[i].Ranks ? d.Respect[i].Costs[r] : -1;
    }

    public static bool BuyRespect(GameData d, Profile p, int i)
    {
        var c = RespectCost(d, p, i);
        if (c < 0 || p.Respect < c || !RespectUnlocked(d, p, i)) return false;
        p.Respect = (ushort)(p.Respect - c);
        SetRespectRank(p, i, RespectSlot(p, i) + 1);
        return true;
    }

    /// <summary>Wartość kupionej rangi (0 = nic nie kupiono).</summary>
    public static int RespectValue(GameData d, Profile p, int i)
    {
        var r = RespectRank(d, p, i);
        return r > 0 ? d.Respect[i].Values[r - 1] : 0;
    }

    public static int RespectTotalCost(GameData d)
    {
        var t = 0;
        foreach (var x in d.Respect)
        {
            foreach (var c in x.Costs) t += c;
        }
        return t;
    }

    public static int RespectSpent(GameData d, Profile p)
    {
        var t = 0;
        for (var i = 0; i < d.Respect.Length; ++i)
        {
            for (var r = 0; r < RespectRank(d, p, i); ++r) t += d.Respect[i].Costs[r];
        }
        return t;
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
        for (var i = 0; i < d.Upgrades.Length; ++i) m.AddUpgradeLevels(d.Upgrades[i], p.Levels[i]);
        for (var i = 0; i < d.Respect.Length; ++i) // Respekt: kupione rangi
        {
            if (RespectRank(d, p, i) > 0) m.AddRespect(d.Respect[i].Effect, RespectValue(d, p, i));
        }
        m.GearSlots = GearSlotsMask(d, p); // nagrody za odbiór: buty, pas
        m.Act0 = Act0Unlocked(d, p) ? 1 : 0; // nagroda za odbiór: Akt 0 przed budową
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

    /// <summary>
    /// Premie profilu z jednego źródła (rozpiska obrażeń #26): 0 Szkolenia, 1 Respekt, 2 odznaki, 3 pamiątka
    /// (DamageHelp.SourceName). Suma premii bojowych = Mods() (bez narzędzi, brygady, slotów i trybu inwestora).
    /// </summary>
    public static RunMods ModsPart(GameData d, Profile p, int src)
    {
        var m = RunMods.Default(d);
        if (src == 0)
        {
            for (var i = 0; i < d.Upgrades.Length; ++i) // premie bojowe (bez kawy, termosu, znajdziek, materiałów, sprzętu, zł)
            {
                var u = RunMods.Default(d);
                u.AddUpgradeLevels(d.Upgrades[i], p.Levels[i]);
                m.Hp += u.Hp;
                m.Def += u.Def;
                m.Dmg += u.Dmg;
                m.Luck += u.Luck;
                m.Craft += u.Craft;
                m.DmgPct += u.DmgPct;
                m.TakenPct += u.TakenPct;
                m.Crit += u.Crit;
                m.Dodge += u.Dodge;
            }
        }
        else if (src == 1)
        {
            for (var i = 0; i < d.Respect.Length; ++i)
            {
                if (RespectRank(d, p, i) > 0) m.AddRespect(d.Respect[i].Effect, RespectValue(d, p, i));
            }
        }
        else if (src == 2)
        {
            for (var i = 0; i < d.Badges.Length; ++i)
            {
                if ((p.Badges & (1u << i)) != 0) m.AddPerk(d.Badges[i].Bonus);
            }
        }
        else
        {
            var k = SelectedKeepsake(d, p);
            if (k >= 0) m.AddPerk(KeepsakePerk(d, p, k));
        }
        return m;
    }

    /// <summary>Premie profilu ze wszystkich źródeł (DmgBreakdown.SetSources).</summary>
    public static RunMods[] ModsParts(GameData d, Profile p)
    {
        var parts = new RunMods[DmgBreakdown.Sources];
        for (var s = 0; s < parts.Length; ++s) parts[s] = ModsPart(d, p, s);
        return parts;
    }

    /// <summary>Ekran „Koszty”: ile kosztuje cały sklep (v0.21.52: zawody i narzędzia po cenach rosnących).</summary>
    public static int ShopTotalCost(GameData d)
    {
        var t = d.HardCost;
        foreach (var u in d.Upgrades)
        {
            foreach (var st in u.Steps) t += st.Cost;
        }
        var nc = 0;
        for (var i = 0; i < d.Classes.Length; ++i)
        {
            if (ClassForSale(d, i)) t += ClassPrice(d, nc++);
        }
        var nt = 0;
        foreach (var tool in d.Tools)
        {
            if (tool.Shop) t += ToolPrice(d, nt++);
        }
        foreach (var h in d.Brigade) t += h.Cost;
        return t;
    }

    /// <summary>Ile już wydano (pasek budżetu).</summary>
    public static int ShopSpent(GameData d, Profile p)
    {
        var t = p.Hard != 0 ? d.HardCost : 0;
        for (var i = 0; i < d.Upgrades.Length; ++i)
        {
            for (var l = 0; l < p.Levels[i] && l < d.Upgrades[i].Levels; ++l) t += d.Upgrades[i].Cost(l);
        }
        for (int k = 0, n = ClassesBought(d, p); k < n; ++k) t += ClassPrice(d, k);
        for (int k = 0, n = ToolsBought(d, p); k < n; ++k) t += ToolPrice(d, k);
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
        var h = (byte)((g.Cls & 15) | (Math.Min(3, g.Score / 1000) << 4)); // klatka domu: wielkość * liczba zawodów + zawód
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
        p.RunRespect = 0;
        var k = SelectedKeepsake(d, p);
        if (k >= 0 && p.KeepsakeRuns[k] < 255) ++p.KeepsakeRuns[k];
    }

    /// <summary>
    /// Po budowie: najtańsze niekupione w Szkoleniach (motywacja). Kind: 0 ulepszenie, 1 zawód, 2 narzędzie, 3 brygada,
    /// 4 poziom Trudny. Zwraca koszt albo -1, gdy wszystko kupione.
    /// </summary>
    public static int NextUnlock(GameData d, Profile p, out int kind, out int index)
    {
        int best = -1, k0 = -1, i0 = -1;
        void Take(int k, int i, int c)
        {
            if (c >= 0 && (best < 0 || c < best))
            {
                best = c;
                k0 = k;
                i0 = i;
            }
        }
        for (var i = 0; i < d.Upgrades.Length; ++i) Take(0, i, UpgradeCost(d, p, i));
        for (var i = 0; i < d.Classes.Length; ++i)
        {
            if (!ClassUnlocked(d, p, i) && !ClassReward(d, i)) Take(1, i, ClassCost(d, p));
        }
        for (var i = 0; i < d.Tools.Length; ++i)
        {
            if (!ToolUnlocked(d, p, i) && d.Tools[i].Shop) Take(2, i, ToolCost(d, p));
        }
        for (var i = 0; i < d.Brigade.Length; ++i)
        {
            if (!HelperUnlocked(d, p, i)) Take(3, i, d.Brigade[i].Cost);
        }
        if (p.Hard == 0) Take(4, 0, d.HardCost);
        kind = k0;
        index = i0;
        return best;
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
        var rd = g.Respect - p.RunRespect; // Respekt za ukończone etapy – od razu w profilu (śmierć go nie zabiera)
        if (rd > 0)
        {
            p.Respect = AddSat16(p.Respect, rd);
            p.RespectTotal = AddSat16(p.RespectTotal, rd);
        }
        p.RunRespect = (ushort)Math.Max(p.RunRespect, Math.Min(65535, g.Respect));
        p.KillsTotal = AddSat16(p.KillsTotal, g.Kills - p.RunKills);
        p.RunKills = (ushort)Math.Max(p.RunKills, Math.Min(65535, g.Kills));
        p.PowersTotal = AddSat16(p.PowersTotal, g.PowersUsed - p.RunPowers);
        p.RunPowers = Math.Max(p.RunPowers, g.PowersUsed);
        p.BrandTotal = AddSat8(p.BrandTotal, g.BrandFound - p.RunBrand);
        p.RunBrand = Math.Max(p.RunBrand, g.BrandFound);
        p.CleanBosses = AddSat8(p.CleanBosses, g.CleanBosses - p.RunClean);
        p.RunClean = Math.Max(p.RunClean, g.CleanBosses);
    }

    /// <summary>Postęp zlecenia (licznik z profilu).</summary>
    public static int ContractProgress(GameData d, Profile p, int i) => d.Contracts[i].Kind switch
    {
        ContractKind.Kills => p.KillsTotal,
        ContractKind.Powers => p.PowersTotal,
        ContractKind.Brand => p.BrandTotal,
        ContractKind.CleanBoss => p.CleanBosses,
        ContractKind.ClassWins => ClassesWon(d, p),
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
            if (g.KillsByType[e] != 0) CatalogAdd(p, e);
        }
        p.ToolsFound = (byte)(p.ToolsFound | g.ToolsFound);
        if (g.St == GameStatus.Won) SetClassWon(p, g.Cls);
        var stake = Investor.Stake(d, g.Bonus.Investor); // rekord stawki zawodu (wygrana w trybie inwestora)
        if (g.St == GameStatus.Won && stake > BestStake(p, g.Cls)) SetBestStake(p, g.Cls, stake);
    }

    /// <summary>Sprawdza odznaki po ważnym momencie; nowe dają doświadczenie. Zwraca bitmaskę zdobytych teraz.</summary>
    public static int CheckBadges(GameData d, Profile p, Game g)
    {
        RecordRun(d, p, g);
        var cleared = g.St == GameStatus.StageClear || g.St == GameStatus.Won;
        var won = g.St == GameStatus.Won;
        var allTools = ((1 << d.Tools.Length) - 1) & ~d.SecretToolsMask; // sekretne narzędzia się nie liczą
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
        Set(d.BadgePelnyZespol, OpenClassesWon(d, p) == d.OpenClassesCount); // zawody z sekretów się nie liczą
        Set(d.BadgeKolekcjoner, (p.ToolsFound & allTools) == allTools);
        Set(d.BadgeKatalog, CatalogCount(d, p) == d.Enemies.Length);
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
