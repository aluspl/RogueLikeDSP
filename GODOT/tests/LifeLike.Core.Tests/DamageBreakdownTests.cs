namespace LifeLike.Core.Tests;

// core_tests.cpp: 46 (v0.21.50: rozpiska obrażeń broni #26) – zakres z rozpiski = to, co naprawdę zadaje walka.
public class DamageBreakdownTests
{
    private static GameData D => TestData.D;

    private static Profile FullProfile()
    {
        var p = Meta.NewProfile(D);
        for (var i = 0; i < D.Upgrades.Length; ++i) p.Levels[i] = (byte)D.Upgrades[i].Levels;
        for (var i = 0; i < D.Respect.Length; ++i) p.RespectRanks[i] = (byte)D.Respect[i].Ranks;
        p.Badges = (ushort)((1 << D.Badges.Length) - 1);
        for (var k = 0; k < D.Keepsakes.Length; ++k) p.KeepsakeRuns[k] = 9;
        p.Keepsake = 3;
        return p;
    }

    [Fact]
    public void SourcesSumToModsAndClassMatchesRunStart()
    {
        var p = FullProfile();
        var full = Meta.Mods(D, p);
        var parts = Meta.ModsParts(D, p);
        int sd = 0, sp = 0, sc = 0, sl = 0, sk = 0, sf = 0, st = 0, sh = 0;
        foreach (var q in parts)
        {
            sd += q.Dmg; sp += q.DmgPct; sc += q.Crit; sl += q.Luck; sk += q.Craft; sf += q.Def; st += q.TakenPct; sh += q.Hp;
        }
        Assert.True(sd == full.Dmg && sp == full.DmgPct && sc == full.Crit && sl == full.Luck && sk == full.Craft && sf == full.Def
                    && st == full.TakenPct && sh == full.Hp);
        Assert.True(full.DmgPct > 0 && full.Crit > 0 && parts[1].DmgPct > 0 && parts[2].Crit > 0);
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            var a = DmgBreakdown.ForClass(D, c, full);
            var g = TestData.NewGame();
            g.NewRun(c, 5, D.DefaultDifficulty, full);
            var b = g.WeaponBreakdown();
            Assert.True(a.Min == b.Min && a.Max == b.Max && a.CritPct == b.CritPct && a.Avg10 == b.Avg10 && a.StatValue == g.HeroStat(g.Weapon.ScalesWith));
            Assert.True(b.CritPct == g.CritPct() && b.Range == g.WeaponRange());
            b.SetSources(parts);
            Assert.True(b.Split);
        }
    }

    [Fact]
    public void BreakdownMatchesCombatOverSeededRolls()
    {
        var parts = Meta.ModsParts(D, FullProfile());
        int configs = 0, reached = 0;
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            for (var v = 0; v < 6; ++v)
            {
                var g = TestData.Arena(c);
                var pick = new Rng();
                pick.Seed((uint)(1000 + c * 17 + v));
                g.Bonus.DmgPct = v == 0 ? 0 : pick.Range(0, 30);
                g.Bonus.Crit = pick.Range(0, 10);
                g.Bonus.Dmg = pick.Range(0, 2);
                g.DmgBonus = g.Bonus.Dmg;
                for (var l = 0; l < v; ++l) g.GainXp(20);
                g.DmgBonus += pick.Range(0, 2);
                for (var s = 0; s < D.GearSlotsCount; ++s)
                {
                    if (pick.Range(0, 2) != 0) g.Equip(s, pick.Range(0, 2), pick.Range(0, D.GearTraitsCount - 1));
                }
                if (v >= 3) g.WeaponOverride = D.Tools[pick.Range(0, D.Tools.Length - 1)].Weapon;
                var ed = pick.Range(0, D.Enemies.Length - 1);
                var b = g.WeaponBreakdown(ed);
                Assert.True(b.CritPct == g.CritPct() && b.StatValue == g.HeroStat(g.Weapon.ScalesWith));
                Assert.True(b.Flat == g.DmgBonus + g.GearBonus(GearStat.Dmg) && b.FlatFound >= 0);
                int lo = 999, hi = 0, clo = 999, chi = 0, crits = 0;
                const int n = 3000;
                for (var k = 0; k < n; ++k)
                {
                    g.DmgCarry = pick.Range(0, 99);
                    g.HitsCount = 0;
                    g.Spawn(ed, 8, 7);
                    var ei = g.EnemiesCount - 1;
                    g.Enemies[ei].Hp = g.Enemies[ei].MaxHp = 30000;
                    g.HeroAttack(ei);
                    var dealt = 30000 - g.Enemies[ei].Hp;
                    var crit = g.HitsCount > 0 && g.Hits[0].Kind == HitKind.Crit;
                    if (crit)
                    {
                        ++crits;
                        clo = Math.Min(clo, dealt);
                        chi = Math.Max(chi, dealt);
                    }
                    else
                    {
                        lo = Math.Min(lo, dealt);
                        hi = Math.Max(hi, dealt);
                    }
                    g.EnemiesCount = 0;
                }
                Assert.True(lo >= b.Min && hi <= b.Max);
                if (crits > 0) Assert.True(clo >= b.CritMin && chi <= b.CritMax);
                if (b.CritChance() < 100) Assert.True(lo == b.Min && hi == b.Max);
                Assert.True(Math.Abs(crits * 100 - b.CritChance() * n) <= 4 * n);
                ++configs;
                if (crits > 0 && clo == b.CritMin && chi == b.CritMax) ++reached;
                b.SetSources(parts);
                for (var t = 0; t < 13; ++t)
                {
                    var m = new Message();
                    DamageHelp.Line(D, m, b, (DmgText)t);
                    Assert.True(m.N > 0 && m.N < Message.Len - 1);
                }
                g.Spawn(ed, 8, 7);
                var ej = g.EnemiesCount - 1;
                g.Bonus.TakenPct = pick.Range(0, 20);
                var h = g.EnemyHit(ej);
                int tlo = 999, thi = 0;
                for (var k = 0; k < 600; ++k)
                {
                    g.TakenCarry = pick.Range(0, 99);
                    g.HitsCount = 0;
                    g.Hero.Hp = 30000;
                    g.Hero.Alive = true;
                    g.St = GameStatus.Playing;
                    g.EnemyStrike(ej, true);
                    if (g.HitsCount > 0 && g.Hits[0].Kind == HitKind.Dodge) continue;
                    var got = 30000 - g.Hero.Hp;
                    tlo = Math.Min(tlo, got);
                    thi = Math.Max(thi, got);
                }
                Assert.True(tlo == h.Min && thi == h.Max);
                Assert.True(DamageHelp.VersusLine(new Message(), b, h).N < Message.Len - 1);
            }
        }
        Assert.True(reached * 10 >= configs * 8);
    }

    [Fact]
    public void SwapPreviewEqualsStateAfterSwap()
    {
        var g = TestData.Arena(1);
        for (var s = 0; s < D.GearSlotsCount; ++s)
        {
            for (var r = 0; r < 3; ++r)
            {
                for (var t = 0; t < D.GearTraitsCount; ++t)
                {
                    var w = g.WeaponBreakdown(-1, -1, s, r, t);
                    var h = g.Clone();
                    h.Equip(s, r, t);
                    var x = h.WeaponBreakdown();
                    Assert.True(w.Min == x.Min && w.Max == x.Max && w.CritPct == x.CritPct && w.Avg10 == x.Avg10);
                }
            }
        }
        foreach (var tool in D.Tools)
        {
            var w = g.WeaponBreakdown(-1, tool.Weapon);
            var h = g.Clone();
            h.WeaponOverride = tool.Weapon;
            var x = h.WeaponBreakdown();
            Assert.True(w.Min == x.Min && w.Max == x.Max && w.Range == x.Range);
        }
    }

    [Fact]
    public void MurarzExampleAndTexts()
    {
        var g = TestData.Arena(1);
        var b = g.WeaponBreakdown();
        Assert.True(b.WMin == 4 && b.WMax == 6 && b.StatDmg == 2 && b.Min == 6 && b.Max == 8 && b.Avg10 == 70);
        Assert.True(b.CritMin == 12 && b.CritMax == 16 && b.CritChance() == D.CritBasePct);
        Assert.Equal("Cios 6-8, średnio 7", DamageHelp.Text(D, b, DmgText.Total));
        Assert.Equal("Kielnia 4-6, zasięg 1", DamageHelp.Text(D, b, DmgText.Weapon));
        Assert.Equal("SIŁ 5: +2 (+1 co 2 pkt)", DamageHelp.Text(D, b, DmgText.Stat));
        Assert.Equal("Kryt x2: 12-16, szansa 5%", DamageHelp.Text(D, b, DmgText.Crit));
        b.Pct = 10;
        b.Finish(D);
        Assert.True(b.Min == 6 && b.Max == 9 && b.Avg10 == 77);
        var bud = D.EnemyIndex("budzet");
        var v = g.WeaponBreakdown(bud);
        Assert.Equal(Math.Max(1, 6 - D.Enemies[bud].Defense / 2), v.Min);
        var gl = g.WeaponBreakdown(-1, -1, 1, 2, 0);
        Assert.Equal("teraz 6-8 -> 9-11 (średnio +3)", DamageHelp.CompareLine(new Message(), g.WeaponBreakdown(), gl).Text);
        Assert.Equal("+3 obrażeń", DamageHelp.GearLabel(D.Gear[5]));
    }
}
