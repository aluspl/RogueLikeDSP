using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.21.52 cz. b – port z meta.h: poziom inspektora (#44, konto gracza), mistrzostwo zawodu 1-10 (#45), stopnie inwestora
/// (#48). Poziom = ile kolejnych progów (dośw. na poziom z listy w danych) mieści się w dośw.; Respekt przychodzi w chwili
/// osiągnięcia poziomu, reszta nagród działa od poziomu.
/// </summary>
public static class Progress
{
    public static int LevelOf(ProgressLevel[] lv, int xp)
    {
        int l = 0, need = 0;
        while (l < lv.Length && xp >= need + lv[l].Xp) need += lv[l++].Xp;
        return l;
    }

    /// <summary>Dośw. potrzebne na poziom level (suma progów poniżej).</summary>
    public static int Floor(ProgressLevel[] lv, int level)
    {
        var t = 0;
        for (var l = 0; l < level; ++l) t += lv[l].Xp;
        return t;
    }

    /// <summary>Dośw. inspektora jako int (uszkodzony zapis: najwyżej 10^9).</summary>
    public static int InspectorXpInt(Profile p) => (int)Math.Min(p.InspectorXp, 1000000000u);

    public static int InspectorLevel(GameData d, Profile p) => LevelOf(d.InspectorLevels, InspectorXpInt(p));

    public static int MasteryXp(Profile p, int c) => c >= 0 && c < Profile.MaxClasses ? p.MasteryXp[c] : 0;

    public static int MasteryLevel(GameData d, Profile p, int c) => LevelOf(d.MasteryLevels, MasteryXp(p, c));

    /// <summary>Pasek do kolejnego poziomu inspektora: dośw. w poziomie (cur) i potrzebne (need; 0 = maksimum).</summary>
    public static void InspectorBar(GameData d, Profile p, out int cur, out int need)
    {
        var l = InspectorLevel(d, p);
        need = l < d.InspectorLevels.Length ? d.InspectorLevels[l].Xp : 0;
        cur = need != 0 ? InspectorXpInt(p) - Floor(d.InspectorLevels, l) : 0;
    }

    public static void MasteryBar(GameData d, Profile p, int c, out int cur, out int need)
    {
        var l = MasteryLevel(d, p, c);
        need = l < d.MasteryLevels.Length ? d.MasteryLevels[l].Xp : 0;
        cur = need != 0 ? MasteryXp(p, c) - Floor(d.MasteryLevels, l) : 0;
    }

    /// <summary>Poziom mistrzostwa, od którego działa nagroda r (99 = żaden).</summary>
    public static int MasteryRewardLevel(GameData d, ProgressReward r)
    {
        var i = Array.FindIndex(d.MasteryLevels, l => l.Reward == r);
        return i < 0 ? 99 : i + 1;
    }

    public static bool MasteryHas(GameData d, Profile p, int c, ProgressReward r) => MasteryLevel(d, p, c) >= MasteryRewardLevel(d, r);

    /// <summary>Wariant mocy: odblokowany na poziomie „power”, włączany na wyborze zawodu.</summary>
    public static bool PowerVariantOn(GameData d, Profile p, int c) => MasteryHas(d, p, c, ProgressReward.Power) && ((p.PowerAlt >> c) & 1) != 0;

    public static void TogglePowerVariant(GameData d, Profile p, int c)
    {
        if (MasteryHas(d, p, c, ProgressReward.Power)) p.PowerAlt = (ushort)(p.PowerAlt ^ (1 << c));
    }

    /// <summary>Bity mistrzostwa do budowy zawodem c (RunMods.Mastery).</summary>
    public static int MasteryBits(GameData d, Profile p, int c)
    {
        var b = 0;
        if (PowerVariantOn(d, p, c)) b |= MasteryBit.Power;
        if (MasteryHas(d, p, c, ProgressReward.Weapon)) b |= MasteryBit.Weapon;
        if (MasteryHas(d, p, c, ProgressReward.Boon)) b |= MasteryBit.Boon;
        return b;
    }

    /// <summary>Stopnie inwestora: najwyższa stawka wygranej budowy (dowolnym zawodem).</summary>
    public static int MaxStake(GameData d, Profile p)
    {
        var m = 0;
        for (var c = 0; c < d.Classes.Length; ++c) m = Math.Max(m, Meta.BestStake(p, c));
        return m;
    }

    /// <summary>Ile progów z GameData.StakeRanks osiągnięto.</summary>
    public static int StakeRank(GameData d, Profile p)
    {
        var n = 0;
        var m = MaxStake(d, p);
        while (n < d.StakeRanks.Length && m >= d.StakeRanks[n].Xp) ++n;
        return n;
    }

    /// <summary>Poziom inspektora, od którego działa nagroda r (np. slot drugiej pamiątki; 99 = żaden).</summary>
    public static int InspectorRewardLevel(GameData d, ProgressReward r)
    {
        var i = Array.FindIndex(d.InspectorLevels, l => l.Reward == r);
        return i < 0 ? 99 : i + 1;
    }

    public static bool KeepsakeSlot2(GameData d, Profile p) => InspectorLevel(d, p) >= InspectorRewardLevel(d, ProgressReward.KeepsakeSlot);

    public static void GrantLevel(Profile p, ProgressLevel l)
    {
        if (l.Reward != ProgressReward.Respect) return;
        p.Respect = Meta.AddSat16(p.Respect, l.Value);
        p.RespectTotal = Meta.AddSat16(p.RespectTotal, l.Value);
    }

    /// <summary>Respekt za nowe progi stopni inwestora (od progu before); zwraca liczbę nowych.</summary>
    public static int GrantStakeRanks(GameData d, Profile p, int before)
    {
        var now = StakeRank(d, p);
        for (var l = before; l < now; ++l) GrantLevel(p, d.StakeRanks[l]);
        return now - before;
    }

    /// <summary>
    /// Dośw. inspektora z budowy (łącznie od startu, z budowami NG+): budowa, ukończone etapy, bossowie, elity, magazyny,
    /// wygrane; x procent trudności. Dośw. mistrzostwa zawodu = to samo.
    /// </summary>
    public static int RunProgressXp(GameData d, Game g)
    {
        int stages = 0, bosses = 0;
        void Count(int to)
        {
            for (var s = g.FirstStage; s < to; ++s)
            {
                ++stages;
                if (d.Stages[s].Boss >= 0) ++bosses;
            }
        }
        for (var t = 0; t < g.Tier; ++t) Count(d.Stages.Length); // budowy ukończone przed „Kolejną budową”
        var won = g.St == GameStatus.Won;
        Count(won ? d.Stages.Length : (g.St == GameStatus.StageClear ? g.Stage + 1 : g.Stage));
        var wins = g.Tier + (won ? 1 : 0);
        var xp = d.InspectorXpRun + stages * d.InspectorXpStage + bosses * d.InspectorXpBoss + g.ElitesKilled * d.InspectorXpElite
                 + g.SecretsFound * d.InspectorXpStoreroom + wins * d.InspectorXpWin;
        return xp * d.InspectorDiffPct[g.Diff] / 100;
    }

    public static void AddInspectorXp(GameData d, Profile p, int delta, ProgressGain r)
    {
        r.InspBefore = InspectorLevel(d, p);
        p.InspectorXp = (uint)Math.Min(1000000000, InspectorXpInt(p) + Math.Max(0, delta));
        r.InspAfter = InspectorLevel(d, p);
        for (var l = r.InspBefore; l < r.InspAfter; ++l) GrantLevel(p, d.InspectorLevels[l]);
    }

    public static void AddMasteryXp(GameData d, Profile p, int cls, int delta, ProgressGain r)
    {
        if (cls < 0 || cls >= d.Classes.Length) return;
        r.Cls = cls;
        r.MasteryBefore = MasteryLevel(d, p, cls);
        p.MasteryXp[cls] = Meta.AddSat16(p.MasteryXp[cls], delta);
        r.MasteryAfter = MasteryLevel(d, p, cls);
        for (var l = r.MasteryBefore; l < r.MasteryAfter; ++l)
        {
            GrantLevel(p, d.MasteryLevels[l]);
            if (d.MasteryLevels[l].Reward == ProgressReward.Power) p.PowerAlt = (ushort)(p.PowerAlt | (1 << cls));
        }
    }

    /// <summary>
    /// Przenosi nowe dośw. inspektora i mistrzostwa z budowy (bez podwójnego liczenia – znak wodny RunProgress, jak NG+).
    /// Wołać na końcu budowy (śmierć, wygrana, porzucenie), przed Story.Check (wątki inspektora).
    /// </summary>
    public static ProgressGain Bank(GameData d, Profile p, Game g)
    {
        var r = new ProgressGain();
        var total = RunProgressXp(d, g);
        var delta = Math.Max(0, total - p.RunProgress);
        p.RunProgress = (ushort)Math.Min(65535, Math.Max(p.RunProgress, total));
        var r0 = p.RespectTotal;
        r.Gained = delta;
        AddInspectorXp(d, p, delta, r);
        AddMasteryXp(d, p, g.Cls, delta, r);
        r.Respect = p.RespectTotal - r0;
        return r;
    }

    /// <summary>
    /// Nagroda poziomu słowami (banery, listy w profilu): „Respekt +10”, „Tytuł: Praktykant”, „SMS: Pierwsza kontrola”…
    /// cls – zawód (mistrzostwo: wariant mocy, broń mistrza, premia).
    /// </summary>
    public static string RewardLabel(GameData d, ProgressLevel l, int cls)
    {
        var mc = d.MasteryClasses[cls >= 0 && cls < d.Classes.Length ? cls : 0];
        return l.Reward switch
        {
            ProgressReward.Respect => $"Respekt +{l.Value}",
            ProgressReward.Title => $"Tytuł: {l.Title}",
            ProgressReward.Helmet => d.Cosmetics[l.Index].Name,
            ProgressReward.Story => $"SMS: {d.StoryArc[l.Index].Name}",
            ProgressReward.Decor => $"Ozdoba: {d.EstateDecor[l.Index].Name}",
            ProgressReward.KeepsakeSlot => "Druga pamiątka",
            ProgressReward.Power => $"Moc: {mc.PowerName}",
            ProgressReward.Weapon => mc.WeaponName,
            ProgressReward.Boon => $"Premia: {d.Boons[mc.Boon].Name}",
            ProgressReward.Keepsake => $"Pamiątka: {d.Keepsakes[l.Index].Name}", // v0.21.52 cz. c
            _ => "",
        };
    }

    /// <summary>
    /// v13 -> v14: dośw. inspektora i mistrzostwa z dotychczasowych statystyk (budowy, wygrane, Respekt; mistrzostwo – domy
    /// zawodu na Osiedlu i zawody z wygraną); Respekt za osiągnięte poziomy, wątki SMS.
    /// </summary>
    public static void MigrateV14(GameData d, Profile p)
    {
        p.InspectorXp = 0;
        p.PowerAlt = 0;
        p.RunProgress = 0;
        p.Keepsake2 = 0;
        Array.Clear(p.MasteryXp);
        var insp = Math.Min(10000, Math.Max(0, p.Runs)) * d.InspectorMigrateRun + Math.Min(10000, Math.Max(0, p.Wins)) * d.InspectorMigrateWin
                   + p.RespectTotal * d.InspectorMigrateRespectPct / 100;
        var r = new ProgressGain();
        AddInspectorXp(d, p, insp, r);
        for (var c = 0; c < d.Classes.Length; ++c)
        {
            var houses = 0;
            for (var i = 0; i < p.HousesCount; ++i)
            {
                if ((p.Houses[i] & 15) == c) ++houses;
            }
            AddMasteryXp(d, p, c, houses * d.MasteryMigrateWin + (Meta.ClassWon(p, c) ? d.MasteryMigrateClassWin : 0), r);
        }
        Story.Check(d, p, null); // wątki od inspektora za osiągnięty poziom
    }
}
