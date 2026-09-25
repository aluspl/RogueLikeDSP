namespace LifeLike.Core.Tests;

// core_tests.cpp (v0.21.43): 29 (statystyki, Warsztaty, Kurs BHP II), 30 (uprawnienia z odznak), 31/31a/31b/31c (zlecenia,
// Czysta robota, migracja profilu v3 -> v5, postęp na żywo), 31d/31e (v0.21.44: migracja v4 -> v5, domyślna pamiątka,
// wznowienie budowy bez podwójnego liczenia zleceń), 32 (pamiątki), 33 (wydarzenia na placu).
public class PerksContractsEventsTests
{
    private static GameData D => TestData.D;

    private static int TraitOf(TraitEffect e) => Array.FindIndex(D.GearTraits, t => t.Effect == e);

    private static int EventOf(EventEffect e) => Array.FindIndex(D.SiteEvents, x => x.Effect == e);

    private static int ContractOf(ContractKind k) => Array.FindIndex(D.Contracts, c => c.Kind == k);

    // 29. statystyki: cechy SIŁ/ZRĘ/INT, Warsztaty (statystyka broni zawodu), Kurs BHP II, narzędzia INT
    [Fact]
    public void StatTraitsCraftAndLuckUpgrade()
    {
        int tStr = TraitOf(TraitEffect.Str), tInt = TraitOf(TraitEffect.Intel);
        Assert.True(tStr >= 0 && tInt >= 0 && TraitOf(TraitEffect.Agi) >= 0);
        var g = TestData.Arena(1); // Murarz: Kielnia skaluje się z SIŁ
        Assert.True(g.HeroStat(Stat.Str) == D.Classes[1].Strength && g.StatBonus(Stat.Str) == 0);
        g.Equip(0, 0, tStr);
        g.Equip(1, 0, tInt);
        Assert.True(g.HeroStat(Stat.Str) == D.Classes[1].Strength + 1 && g.HeroStat(Stat.Intel) == D.Classes[1].Intelligence + 1);
        Assert.Equal(D.Classes[1].Agility, g.HeroStat(Stat.Agi));
        var m = RunMods.Default(D);
        m.Craft = 2;
        m.Luck = 1;
        var w = TestData.NewGame();
        w.NewRun(1, 9, D.DefaultDifficulty, m);
        Assert.True(w.HeroStat(Stat.Str) == D.Classes[1].Strength + 2 && w.HeroStat(Stat.Intel) == D.Classes[1].Intelligence);
        Assert.Equal(D.Classes[1].Luck + 1, w.Luck());
        var k = TestData.NewGame(); // Kierownik: Dziennik skaluje się z INT
        k.NewRun(0, 9, D.DefaultDifficulty, m);
        Assert.True(k.HeroStat(Stat.Intel) == D.Classes[0].Intelligence + 2 && k.HeroStat(Stat.Str) == D.Classes[0].Strength);
        Assert.True(RunMods.StatBonus(D, m, 0, Stat.Intel) == 2 && RunMods.StatBonus(D, m, 0, Stat.Str) == 0);
        // wyższa statystyka broni = większe obrażenia (ten sam rzut)
        var a0 = TestData.Arena(1);
        var a1 = TestData.Arena(1);
        a1.Bonus.Craft = 2;
        a0.Spawn(8, 8, 7);
        a1.Spawn(8, 8, 7);
        a0.Enemies[0].Hp = a1.Enemies[0].Hp = 999;
        a0.HeroAttack(0);
        a1.HeroAttack(0);
        Assert.True(a1.Enemies[0].Hp < a0.Enemies[0].Hp);
        // Szkolenia: Kurs BHP II i Warsztaty w Mods()
        var p = Meta.NewProfile(D);
        int iLuck = Array.FindIndex(D.Upgrades, u => u.Effect == UpgradeEffect.Luck);
        int iCraft = Array.FindIndex(D.Upgrades, u => u.Effect == UpgradeEffect.Craft);
        Assert.True(iLuck >= 0 && iCraft >= 0 && D.Upgrades[iLuck].Levels == 2 && D.Upgrades[iCraft].Levels == 2);
        p.Levels[iLuck] = 2;
        p.Levels[iCraft] = 1;
        var pm = Meta.Mods(D, p);
        Assert.True(pm.Luck == 2 && pm.Craft == 1);
        // co najmniej 3 narzędzia skalowane INT do odblokowania
        var intTools = D.Tools.Count(t => D.Weapons[t.Weapon].ScalesWith == Stat.Intel && t.Cost > 0);
        Assert.True(intTools >= 3);
    }

    // 30. uprawnienia: każda zdobyta odznaka daje trwałą premię (Mods), premie działają w budowie
    [Fact]
    public void BadgePerksApplyToRun()
    {
        var p = Meta.NewProfile(D);
        var m0 = Meta.Mods(D, p);
        Assert.True(m0.Hp == 0 && m0.Dmg == 0 && m0.Cash == 0 && m0.XpPct == 0);
        p.Badges = (ushort)((1 << D.Badges.Length) - 1);
        var m = Meta.Mods(D, p);
        var sum = RunMods.Default(D);
        foreach (var b in D.Badges) sum.AddPerk(b.Bonus);
        Assert.True(m.Hp == sum.Hp && m.Def == sum.Def && m.Dmg == sum.Dmg && m.Luck == sum.Luck && m.Cooldown == sum.Cooldown);
        Assert.True(m.ToolPct == sum.ToolPct && m.XpPct == sum.XpPct && m.Cash == sum.Cash && m.Crit == sum.Crit);
        Assert.True(D.Badges[D.BadgeBezUsterek].Bonus.Effect == PerkEffect.Hp && m.Hp == 2);
        Assert.True(m.Dmg >= 1 && m.Cooldown >= 1 && m.ToolPct >= 10 && m.Luck >= 1 && m.XpPct >= 10);
        foreach (var b in D.Badges)
        {
            var label = RunMods.PerkLabel(new Message(), b.Bonus);
            Assert.True(label.N > 0 && label.N < 30);
        }
        // premie w budowie
        var pm = RunMods.Default(D);
        pm.Cash = 20;
        pm.Sight = 1;
        pm.Cooldown = 1;
        pm.Thermos = 1;
        pm.Crit = 5;
        pm.XpPct = 50;
        var a = TestData.Run(0, 11);
        var g = TestData.NewGame();
        g.NewRun(0, 11, D.DefaultDifficulty, pm);
        Assert.True(g.Cash == a.Cash + 20 && g.SightRadius() == a.SightRadius() + 1 && g.ThermosCap() == a.ThermosCap() + 1);
        Assert.True(g.AbilityCooldown() == a.AbilityCooldown() - 1 && g.CritPct() == a.CritPct() + 5);
        a.GainXp(10);
        g.GainXp(10);
        Assert.Equal(a.Xp * 3 / 2, g.Xp);
        // Kolekcjoner: 100% – każdy drop to narzędzie
        var t = TestData.Arena(1);
        t.Bonus.ToolPct = 100;
        t.Bonus.Tools = D.StartToolsMask;
        int drops = 0, tools = 0;
        for (var k = 0; k < 300; ++k)
        {
            t.PickupsCount = 0;
            t.MaybeDrop(3, 3);
            if (t.PickupsCount == 0) continue;
            ++drops;
            if (t.Pickups[0].Type == PickupType.Tool) ++tools;
        }
        Assert.True(drops > 0 && tools == drops);
    }

    [Fact]
    public void PerkLabelsMatchGba()
    {
        Assert.Equal("+2 max HP", RunMods.PerkLabel(new Perk(PerkEffect.Hp, 2)));
        Assert.Equal("Moc -1 t. odnowienia", RunMods.PerkLabel(new Perk(PerkEffect.Cooldown, 1)));
        Assert.Equal("Termos +1 miejsce", RunMods.PerkLabel(new Perk(PerkEffect.Thermos, 1)));
        Assert.Equal("Termos +3 miejsca", RunMods.PerkLabel(new Perk(PerkEffect.Thermos, 3)));
        Assert.Equal("+10% szans na narzędzie", RunMods.PerkLabel(new Perk(PerkEffect.ToolPct, 10)));
        Assert.Equal("+20 zł na start", RunMods.PerkLabel(new Perk(PerkEffect.Cash, 20)));
        Assert.Equal("Kryt +5%", RunMods.PerkLabel(new Perk(PerkEffect.Crit, 5)));
        Assert.Equal("", RunMods.PerkLabel(new Perk(PerkEffect.Unknown, 5)));
    }

    // 31. zlecenia: liczniki budowy -> profil (bez podwójnego liczenia), ukończenie daje doświadczenie
    [Fact]
    public void ContractCountersBankOnceAndReward()
    {
        var p = Meta.NewProfile(D);
        Assert.True(D.Contracts.Length >= 5);
        var g = TestData.Arena(0);
        g.Spawn(8, 8, 7);
        g.Enemies[0].Awake = true;
        g.Enemies[0].Hp = 1;
        Assert.True(g.PlayerAbility() && g.PowersUsed == 1); // Odprawa: moc użyta
        g.HeroAttack(0);
        Assert.Equal(1, g.Kills);
        g.Equip(0, 2, 0);
        g.Equip(1, 1, 0);
        Assert.Equal(1, g.BrandFound);
        Meta.RecordRun(D, p, g);
        Meta.RecordRun(D, p, g); // drugi raz nic nie dodaje
        Assert.True(p.KillsTotal == 1 && p.PowersTotal == 1 && p.BrandTotal == 1 && p.CleanBosses == 0);
        var iPow = ContractOf(ContractKind.Powers);
        Assert.True(iPow >= 0 && Meta.ContractProgress(D, p, iPow) == 1);
        p.PowersTotal = (ushort)(D.Contracts[iPow].Target - 1);
        Assert.Equal(0, Meta.CheckContracts(D, p));
        g.AbilityCd = 0;
        g.Spawn(8, 9, 7);
        g.Enemies[1].Awake = true;
        Assert.True(g.PlayerAbility());
        Meta.RecordRun(D, p, g);
        int xp0 = p.Xp, got = Meta.CheckContracts(D, p);
        Assert.True(got == 1 << iPow && Meta.ContractDone(p, iPow) && p.Xp == xp0 + D.Contracts[iPow].Xp);
        Assert.Equal(0, Meta.CheckContracts(D, p)); // raz
        p.Wins = 1000;
        Assert.NotEqual(0, Meta.CheckContracts(D, p)); // Stały klient itp.
    }

    // 31c. postęp zleceń na żywo w trakcie budowy
    [Fact]
    public void ContractProgressLive()
    {
        var p = Meta.NewProfile(D);
        var iK = ContractOf(ContractKind.Kills);
        var g = TestData.Arena(1);
        g.Kills = 7;
        Assert.True(Meta.ContractProgressLive(D, p, g, iK) == 7 && Meta.ContractProgress(D, p, iK) == 0);
        Meta.RecordRun(D, p, g);
        Assert.True(Meta.ContractProgressLive(D, p, g, iK) == 7 && Meta.ContractProgress(D, p, iK) == 7);
        Assert.True(Meta.NextContract(D, p, g) >= 0);
        p.Contracts = (byte)((1 << D.Contracts.Length) - 1);
        Assert.Equal(-1, Meta.NextContract(D, p, g));
    }

    // 31a. boss aktu bez obrażeń w walce z nim (Czysta robota)
    [Fact]
    public void CleanBossCountsOnlyDamageDuringFight()
    {
        var bs = Array.FindIndex(D.Stages, s => s.Boss >= 0);
        var g = TestData.Run(1, 21);
        g.StartStage(bs);
        g.StageDamage = 7; // obrażenia przed walką się nie liczą
        Assert.True(g.BossWakeDamage < 0);
        g.Enemies[g.Boss].Hp = 1;
        g.HeroAttack(g.Boss);
        Assert.True(g.CleanBosses == 1 && g.BossWakeDamage == 7);
        var h = TestData.Run(1, 21);
        h.StartStage(bs);
        h.HeroAttack(h.Boss);
        h.StageDamage += 3; // trafiony w walce z bossem
        h.Enemies[h.Boss].Hp = 1;
        h.HeroAttack(h.Boss);
        Assert.Equal(0, h.CleanBosses);
    }

    // 31b. profil v3 -> v5: wszystkie dotychczasowe pola zostają, nowe od zera
    [Fact]
    public void MigrationV3ToV5KeepsOldFields()
    {
        var v3 = Meta.NewProfile(D);
        v3.Magic = Profile.MagicBytes(Profile.MagicV3);
        v3.Best = 1234;
        v3.Runs = 9;
        v3.Wins = 4;
        v3.Xp = 321;
        v3.Levels[1] = 2;
        v3.Classes = 0x1F;
        v3.Hard = 1;
        v3.Flags = 3;
        v3.Tools = 5;
        v3.Badges = 0x0123;
        v3.Catalog = 0x07FF;
        v3.ClassWins = 0x05;
        v3.ToolsFound = 0x0B;
        v3.HousesCount = 3;
        v3.Houses[0] = 0x21;
        v3.Houses[2] = 0x35;
        var raw = v3.ToBytes();
        for (var i = Profile.V3Size; i < raw.Length; i++) raw[i] = 0xCD; // śmieci
        v3 = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, v3) && v3.MagicIs(Profile.MagicV5));
        Assert.True(v3.Best == 1234 && v3.Runs == 9 && v3.Wins == 4 && v3.Xp == 321 && v3.Levels[1] == 2 && v3.Classes == 0x1F);
        Assert.True(v3.Hard == 1 && v3.Flags == 3 && v3.Tools == 5 && v3.Badges == 0x0123 && v3.Catalog == 0x07FF);
        Assert.True(v3.ClassWins == 0x05 && v3.ToolsFound == 0x0B && v3.HousesCount == 3 && v3.Houses[0] == 0x21 && v3.Houses[2] == 0x35);
        Assert.True(v3.KillsTotal == 0 && v3.PowersTotal == 0 && v3.BrandTotal == 0 && v3.CleanBosses == 0);
        Assert.True(v3.Contracts == 0 && Meta.SelectedKeepsake(D, v3) >= 0 && D.Keepsakes[Meta.SelectedKeepsake(D, v3)].Start);
        Assert.All(v3.KeepsakeRuns, r => Assert.Equal(0, r));
        Assert.False(Meta.ProfileFix(D, v3));
    }

    // 31d. profil v4 -> v5: pola zostają, znak wodny liczników od zera; bez pamiątki – pierwsza odblokowana
    [Fact]
    public void MigrationV4ToV5DefaultKeepsake()
    {
        var v4 = Meta.NewProfile(D);
        v4.Magic = Profile.MagicBytes(Profile.MagicV4);
        v4.Best = 77;
        v4.Xp = 12;
        v4.KillsTotal = 150;
        v4.PowersTotal = 40;
        v4.Contracts = 0x03;
        v4.Keepsake = 0;
        v4.KeepsakeRuns[0] = 4;
        var raw = v4.ToBytes();
        for (var i = Profile.V4Size; i < raw.Length; i++) raw[i] = 0xEE; // śmieci
        v4 = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, v4) && v4.MagicIs(Profile.MagicV5));
        Assert.True(v4.Best == 77 && v4.Xp == 12 && v4.KillsTotal == 150 && v4.PowersTotal == 40 && v4.Contracts == 0x03);
        Assert.True(v4.KeepsakeRuns[0] == 4 && v4.RunKills == 0 && v4.RunPowers == 0 && v4.RunBrand == 0 && v4.RunClean == 0);
        Assert.True(Meta.SelectedKeepsake(D, v4) >= 0 && D.Keepsakes[Meta.SelectedKeepsake(D, v4)].Start);
        Assert.False(Meta.ProfileFix(D, v4));
        var w = Meta.NewProfile(D);
        w.Magic = Profile.MagicBytes(Profile.MagicV4);
        w.Badges = 0x01;
        var kb = Array.FindIndex(D.Keepsakes, k => k.Badge == 0);
        w.Keepsake = (byte)(kb >= 0 ? kb + 1 : 0); // wybrana pamiątka zostaje
        Assert.True(Meta.ProfileFix(D, w) && w.Keepsake == (kb >= 0 ? kb + 1 : 1));
        var n = Meta.NewProfile(D); // nowy profil
        Assert.True(Meta.SelectedKeepsake(D, n) >= 0 && D.Keepsakes[Meta.SelectedKeepsake(D, n)].Start);
    }

    // 31e. wyłączenie konsoli po zaliczonym etapie i wznowienie z autozapisu na starcie etapu:
    // liczniki zleceń z tego etapu nie liczą się drugi raz
    [Fact]
    public void ResumedStageDoesNotDoubleCountContracts()
    {
        var p = Meta.NewProfile(D);
        var g = new Game(D);
        g.NewRun(1, 4242, 0, Meta.Mods(D, p));
        Meta.StartRun(D, p);
        var rs = RunSave.Make(g); // autozapis na starcie etapu
        for (var k = 0; k < 4000 && g.St == GameStatus.Playing; ++k) Bot.Step(g);
        Assert.True(g.St == GameStatus.StageClear && g.Kills > 0);
        g.PowersUsed = 2;
        Meta.CheckBadges(D, p, g); // koniec etapu: liczniki do profilu (SRAM)
        int kills1 = p.KillsTotal, pw1 = p.PowersTotal;
        Assert.True(kills1 == g.Kills && pw1 == 2);
        var h = RunSave.FromBytes(rs.ToBytes()).Load(D); // wznowienie: stan ze startu etapu
        var iK = ContractOf(ContractKind.Kills);
        Assert.Equal(kills1, Meta.ContractProgressLive(D, p, h, iK)); // telefon nie pokazuje etapu dwa razy
        for (var k = 0; k < 4000 && h.St == GameStatus.Playing; ++k) Bot.Step(h);
        Assert.True(h.St == GameStatus.StageClear && h.Kills == g.Kills); // ten sam etap jeszcze raz
        h.PowersUsed = 2;
        Meta.CheckBadges(D, p, h);
        Assert.True(p.KillsTotal == kills1 && p.PowersTotal == pw1); // bez podwójnego liczenia
        h.Kills += 2;
        h.PowersUsed = 3;
        Meta.RecordRun(D, p, h); // powtórka dała więcej: tylko nadwyżka
        Assert.True(p.KillsTotal == kills1 + 2 && p.PowersTotal == pw1 + 1);
        Meta.StartRun(D, p); // nowa budowa liczy od zera
        var n = TestData.Run(1, 5);
        n.Kills = 3;
        Meta.RecordRun(D, p, n);
        Assert.True(p.KillsTotal == kills1 + 5 && p.RunKills == 3);
    }

    /// <summary>Układ bajtów profilu v5 jak struktura core::profile w SRAM (offsety z static_assert w meta.h).</summary>
    [Fact]
    public void ProfileV5SramLayout()
    {
        var p = Meta.NewProfile(D);
        p.KillsTotal = 0x1234;
        p.PowersTotal = 0x0201;
        p.BrandTotal = 7;
        p.CleanBosses = 8;
        p.Contracts = 0x2A;
        p.Keepsake = 3;
        p.KeepsakeRuns[0] = 11;
        p.KeepsakeRuns[7] = 99;
        p.RunKills = 0x0506;
        p.RunPowers = 0x0102;
        p.RunBrand = 3;
        p.RunClean = 4;
        var b = p.ToBytes();
        Assert.Equal(80, b.Length);
        Assert.Equal("PBRL005\0"u8.ToArray(), b[..8]);
        Assert.Equal(new byte[] { 0x34, 0x12, 0x01, 0x02, 7, 8, 0x2A, 3, 11 }, b[56..65]);
        Assert.Equal(99, b[71]);
        Assert.Equal(new byte[] { 0x06, 0x05, 0x02, 0x01, 3, 4, 0, 0 }, b[72..80]);
        Assert.Equal(b, Profile.FromBytes(b).ToBytes());
    }

    // 32. pamiątki: odblokowanie (start / odznaka / zlecenie), wybór, ranga po 3 i 8 budowach, premia w Mods
    [Fact]
    public void KeepsakesUnlockSelectRankAndApply()
    {
        var p = Meta.NewProfile(D);
        Assert.True(D.Keepsakes.Length >= 5);
        int startK = -1, badgeK = -1, contractK = -1, contractI = -1;
        for (var k = 0; k < D.Keepsakes.Length; ++k)
        {
            if (D.Keepsakes[k].Start) startK = k;
            else if (D.Keepsakes[k].Badge >= 0) badgeK = k;
        }
        for (var i = 0; i < D.Contracts.Length; ++i)
        {
            if (D.Contracts[i].Keepsake < 0) continue;
            contractI = i;
            contractK = D.Contracts[i].Keepsake;
        }
        Assert.True(startK >= 0 && badgeK >= 0 && contractK >= 0);
        Assert.True(Meta.KeepsakeUnlocked(D, p, startK) && !Meta.KeepsakeUnlocked(D, p, badgeK) && !Meta.KeepsakeUnlocked(D, p, contractK));
        Assert.Equal(startK, Meta.SelectedKeepsake(D, p)); // nowy profil: pamiątka startowa
        Meta.CycleKeepsake(D, p, 1);
        Assert.Equal(0, p.Keepsake); // zablokowane są pomijane -> „bez pamiątki”
        Meta.CycleKeepsake(D, p, 1);
        Assert.Equal(startK, Meta.SelectedKeepsake(D, p));
        Meta.CycleKeepsake(D, p, -1);
        Assert.Equal(0, p.Keepsake);
        p.Badges = (ushort)(1 << D.Keepsakes[badgeK].Badge);
        Assert.True(Meta.KeepsakeUnlocked(D, p, badgeK));
        p.Contracts = (byte)(1 << contractI);
        Assert.True(Meta.KeepsakeUnlocked(D, p, contractK));
        p.Keepsake = (byte)(contractK + 1);
        Assert.True(Meta.SelectedKeepsake(D, p) == contractK && Meta.KeepsakeRank(D, p, contractK) == 1);
        var m1 = Meta.Mods(D, p);
        var e = RunMods.Default(D);
        e.AddPerk(new Perk(D.Keepsakes[contractK].Effect, D.Keepsakes[contractK].Values[0]));
        Assert.True(m1.Luck + m1.Sight + m1.Cooldown + m1.Thermos + m1.Def >= e.Luck + e.Sight + e.Cooldown + e.Thermos + e.Def);
        var runs0 = p.Runs;
        for (var r = 0; r < D.KeepsakeRankRuns[0]; ++r) Meta.StartRun(D, p);
        Assert.True(p.Runs == runs0 + D.KeepsakeRankRuns[0] && Meta.KeepsakeRank(D, p, contractK) == 2);
        Assert.Equal(D.Keepsakes[contractK].Values[1], Meta.KeepsakePerk(D, p, contractK).Value);
        for (var r = D.KeepsakeRankRuns[0]; r < D.KeepsakeRankRuns[1]; ++r) Meta.StartRun(D, p);
        Assert.True(Meta.KeepsakeRank(D, p, contractK) == 3 && Meta.KeepsakePerk(D, p, contractK).Value == D.Keepsakes[contractK].Values[2]);
        Assert.Equal(0, p.KeepsakeRuns[startK]); // licznik tylko wybranej
        p.Contracts = 0;
        Assert.Equal(-1, Meta.SelectedKeepsake(D, p)); // zablokowana nie działa
        // Termos babci: miejsce w termosie
        var q = Meta.NewProfile(D);
        q.Keepsake = (byte)(startK + 1);
        var g = TestData.NewGame();
        g.NewRun(1, 3, D.DefaultDifficulty, Meta.Mods(D, q));
        if (D.Keepsakes[startK].Effect == PerkEffect.Thermos) Assert.Equal(D.ThermosCapacity + D.Keepsakes[startK].Values[0], g.ThermosCap());
    }

    // 33. wydarzenia na placu: nie na pierwszym etapie i nie u bossa, efekty
    [Fact]
    public void SiteEventsFrequencyAndEffects()
    {
        var counts = new int[D.Stages.Length];
        for (var k = 0; k < 200; ++k)
        {
            var g0 = TestData.Run(k % D.Classes.Length, (uint)(500 + k * 31));
            for (var st = 0; st < D.Stages.Length; ++st)
            {
                if (st > 0) g0.NextStage();
                if (g0.StageEvent >= 0) ++counts[st];
                Assert.True(g0.StageEvent < D.SiteEvents.Length);
            }
        }
        Assert.Equal(0, counts[0]);
        for (var st = 0; st < D.Stages.Length; ++st)
        {
            if (D.Stages[st].Boss >= 0) Assert.Equal(0, counts[st]);
            else if (st > 0) Assert.True(counts[st] > 200 * D.SiteEventChancePct / 200 && counts[st] < 200 * (D.SiteEventChancePct + 20) / 100, $"etap {st}: {counts[st]}");
        }
        foreach (var e in new[] { EventEffect.FewerPickups, EventEffect.Cash, EventEffect.Inspection, EventEffect.Rain, EventEffect.Thermos })
            Assert.True(EventOf(e) >= 0);
        var g = TestData.Run(1, 44);
        g.StageEvent = -1;
        int pk = g.PickupsCount, cash = g.Cash;
        g.ApplyEvent(EventOf(EventEffect.FewerPickups));
        Assert.True(g.PickupsCount == Math.Max(1, pk - D.SiteEvents[EventOf(EventEffect.FewerPickups)].Value) && g.EventActive(EventEffect.FewerPickups));
        g.ApplyEvent(EventOf(EventEffect.Cash));
        Assert.Equal(cash + D.SiteEvents[EventOf(EventEffect.Cash)].Value, g.Cash);
        g.Thermos = 0;
        g.ApplyEvent(EventOf(EventEffect.Thermos));
        Assert.Equal(g.ThermosCap(), g.Thermos);
        // inspekcja: etap bez obrażeń = premia doświadczenia; z obrażeniami nic
        var a = TestData.Run(1, 44);
        a.ApplyEvent(EventOf(EventEffect.Inspection));
        var b = TestData.Run(1, 44);
        a.DebugSkip();
        b.DebugSkip();
        Assert.True(a.St == GameStatus.StageClear && a.Xp == b.Xp + D.SiteEvents[EventOf(EventEffect.Inspection)].Value);
        var c = TestData.Run(1, 44);
        c.ApplyEvent(EventOf(EventEffect.Inspection));
        c.StageDamage = 1;
        c.DebugSkip();
        Assert.Equal(b.Xp, c.Xp);
        // ulewa: ciosy częściej dają poślizg
        int slipsRain = 0, slipsDry = 0;
        for (var k = 0; k < 200; ++k)
        {
            for (var rain = 0; rain < 2; ++rain)
            {
                var h = TestData.Arena(1);
                h.Hero.MaxHp = h.Hero.Hp = 999;
                if (rain != 0) h.ApplyEvent(EventOf(EventEffect.Rain));
                h.R.Seed((uint)(900 + k));
                h.Spawn(D.EnemyIndex("kornik"), 8, 7);
                h.Enemies[0].Awake = true;
                h.PlayerWait();
                if (h.StatusTurns(StatusEffect.Slip) > 0)
                {
                    if (rain != 0) ++slipsRain;
                    else ++slipsDry;
                }
            }
        }
        Assert.True(slipsDry == 0 && slipsRain > 20, $"sucho {slipsDry}, ulewa {slipsRain}");
    }

    /// <summary>Zapis budowy w trakcie: nowe pola (liczniki zleceń, wydarzenie, premie) przechodzą przez RunSave.</summary>
    [Fact]
    public void RunSaveKeepsV43State()
    {
        var p = Meta.NewProfile(D);
        p.Badges = 0x1FF;
        p.Keepsake = 1;
        var g = TestData.NewGame();
        g.NewRun(2, 4242, D.DefaultDifficulty, Meta.Mods(D, p));
        g.NextStage();
        g.ApplyEvent(EventOf(EventEffect.Rain));
        g.PowersUsed = 12;
        g.BrandFound = 2;
        g.CleanBosses = 1;
        g.BossWakeDamage = 5;
        var s = RunSave.FromBytes(RunSave.Make(g).ToBytes());
        Assert.True(s.Valid(D));
        var l = s.Load(D);
        Assert.True(l.StageEvent == g.StageEvent && l.PowersUsed == 12 && l.BrandFound == 2 && l.CleanBosses == 1 && l.BossWakeDamage == 5);
        Assert.True(l.Bonus.Crit == g.Bonus.Crit && l.Bonus.XpPct == g.Bonus.XpPct && l.Bonus.Thermos == g.Bonus.Thermos && l.ThermosCap() == g.ThermosCap());
        Assert.Equal(StateDigest.Of(g), StateDigest.Of(l));
        Assert.Equal("PBRUN06", RunSave.RunMagic);
    }
}
