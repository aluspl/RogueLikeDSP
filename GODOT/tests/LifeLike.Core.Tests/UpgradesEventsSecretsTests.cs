namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp: 48 (v0.21.50 cz. 3) – wydarzenia z wyborem, ulepszanie narzędzia (rozpiska = walka), ukryte pomieszczenia.
/// </summary>
public class UpgradesEventsSecretsTests
{
    private static GameData D => TestData.D;

    private static int F0 => TestData.F0;

    private static int UpgradeItem() => Array.FindIndex(D.Hurtownia, it => it.Effect == ShopEffect.Upgrade);

    [Fact]
    public void EventTilesAreDeterministicWithoutRepeats()
    {
        int tiles = 0, stages = 0;
        for (uint seed = 1; seed <= 60; ++seed)
        {
            var cls = (int)(seed % (uint)D.Classes.Length);
            var a = TestData.Run(cls, seed * 131u);
            var b = TestData.Run(cls, seed * 131u);
            var seen = 0;
            for (var st = a.FirstStage; st < D.StagesCount; ++st)
            {
                if (st != a.FirstStage)
                {
                    a.StartStage(st);
                    b.StartStage(st);
                }
                int ev = -1, n = 0, evb = -1;
                for (var i = 0; i < a.PickupsCount; ++i)
                {
                    if (a.Pickups[i].Type != PickupType.EventTile) continue;
                    ev = a.Pickups[i].Arg;
                    ++n;
                    Assert.True(a.Lv.At(a.Pickups[i].X, a.Pickups[i].Y) == Tile.Floor && !a.Occupied(a.Pickups[i].X, a.Pickups[i].Y));
                }
                for (var i = 0; i < b.PickupsCount; ++i)
                {
                    if (b.Pickups[i].Type == PickupType.EventTile) evb = b.Pickups[i].Arg;
                }
                Assert.Equal(ev, evb);
                Assert.True(n <= 1);
                if (ev >= 0)
                {
                    Assert.True(D.Stages[st].Boss < 0 && st != a.FirstStage && ((seen >> ev) & 1) == 0);
                    seen |= 1 << ev;
                    ++tiles;
                }
                ++stages;
            }
        }
        Assert.InRange(tiles, stages / 5 + 1, stages * 3 / 4 - 1);
    }

    [Fact]
    public void EveryChoiceAppliesDeterministically()
    {
        for (var e = 0; e < D.ChoiceEvents.Length; ++e)
        {
            for (var k = 0; k < D.ChoiceEvents[e].Choices.Length; ++k)
            {
                var a = TestData.Run(1, 777u + (uint)e);
                a.StartStage(F0 + 1);
                a.EnemiesCount = 1;
                a.Hero.Hp = (short)(a.Hero.MaxHp - 5);
                a.Cash = 50;
                for (var m = 0; m < a.Mats.Length; ++m) a.Mats[m] = 3;
                var b = a.Clone();
                int cash0 = a.Cash, hp0 = a.Hero.Hp, n0 = a.EnemiesCount;
                a.PendingEvent = (sbyte)e;
                b.PendingEvent = (sbyte)e;
                Assert.True(a.ChooseEvent(k) && b.ChooseEvent(k));
                Assert.True(a.ChoiceDone == b.ChoiceDone && a.Cash == b.Cash && a.Hero.Hp == b.Hero.Hp && a.EnemiesCount == b.EnemiesCount);
                Assert.True(a.PendingEvent < 0 && a.StageChoice == e && a.StageChoicePick == k && a.Hero.Hp >= 1);
                var c = D.ChoiceEvents[e].Choices[k];
                for (var i = 0; i < c.Outs.Length; ++i)
                {
                    var o = c.Outs[i];
                    var done = ((a.ChoiceDone >> i) & 1) != 0;
                    Assert.True(done || o.Chance < 100);
                    if (!done) continue;
                    if (o.Effect == ChoiceEffect.Cash && c.Outs.Length == 1) Assert.Equal(Math.Max(0, cash0 + (o.Value > 0 ? a.Income(o.Value) : o.Value)), a.Cash);
                    if (o.Effect == ChoiceEffect.Hp && c.Outs.Length == 1) Assert.Equal(Math.Min(a.Hero.MaxHp, Math.Max(1, hp0 + o.Value)), a.Hero.Hp);
                    if (o.Effect == ChoiceEffect.Spawn) Assert.True(a.EnemiesCount > n0 && a.Enemies[a.EnemiesCount - 1].DefId == o.Arg);
                    if (o.Effect == ChoiceEffect.Boon) Assert.True(a.HasBoonOffer);
                    if (o.Effect == ChoiceEffect.StageDmg) Assert.True(a.EventDmg == o.Value && a.WeaponBreakdown().FlatEvent == o.Value);
                    if (o.Effect == ChoiceEffect.StageDef) Assert.Equal(o.Value, a.EventDef);
                    if (o.Effect == ChoiceEffect.Upgrade) Assert.Equal(1, a.WeaponLvl);
                }
                var label = ChoiceText.Label(D, new Message(), c);
                Assert.True(label.N > 0 && label.N < Message.Len - 1);
                a.BotPending();
                a.BotPending();
                Assert.True(!a.HasBoonOffer && !a.TraitPending);
            }
        }
    }

    [Fact]
    public void EventBoonOfferDiffersAndStageResetsEventBonuses()
    {
        var g = TestData.Run(0, 4242);
        g.StartStage(F0 + 1);
        g.RollBoons();
        var o1 = (sbyte[])g.BoonOffer.Clone();
        g.RollBoons(50);
        Assert.NotEqual(o1, g.BoonOffer);
        g.SkipBoons();
        g.EventDmg = 2;
        g.EventDef = 2;
        var d0 = g.HeroDefense();
        g.StartStage(F0 + 2);
        Assert.True(g.EventDmg == 0 && g.EventDef == 0 && g.HeroDefense() == d0 - 2);
        g.PendingEvent = 0;
        Assert.Equal(0, g.BotEventChoice());
    }

    [Fact]
    public void UpgradeCostsTraitsAndBreakdownMatchesCombat()
    {
        var g = TestData.Arena(1);
        var up = UpgradeItem();
        Assert.True(up >= 0);
        g.Cash = 0;
        g.Mats[1] = 0;
        Assert.False(g.HurtowniaCan(up));
        g.Cash = 500;
        g.Mats[1] = 9;
        var b0 = g.WeaponBreakdown();
        Assert.Equal(D.ToolLevels[0].Cash, g.HurtowniaPrice(up));
        Assert.True(g.HurtowniaBuy(up) && g.WeaponLvl == 1);
        Assert.True(g.Cash == 500 - D.ToolLevels[0].Cash && g.Mats[1] == 9 - D.ToolLevels[0].Count);
        var b1 = g.WeaponBreakdown();
        Assert.True(b1.Min == b0.Min + 1 && b1.Max == b0.Max + 1 && b1.UpgLevel == 1);
        Assert.Contains("+1 ", DamageHelp.Text(D, b1, DmgText.Weapon));
        Assert.True(g.HurtowniaBuy(up) && g.WeaponLvl == 2 && g.TraitPending && !g.HurtowniaCan(up));
        Assert.True(!g.ChooseTrait(9) && g.ChooseTrait(0) && g.WeaponTrait == 0 && !g.TraitPending);
        g.Mats[1] = 9;
        Assert.True(g.HurtowniaBuy(up) && g.WeaponLvl == 3 && !g.HurtowniaCan(up));
        Assert.Contains("+3", g.WeaponTitle());
        foreach (var t in new[] { 0, 1, 2 })
        {
            foreach (var ed in new[] { D.EnemyIndex("kamien"), D.EnemyIndex("przeciek"), D.EnemyIndex("zbrojenie") })
            {
                g.WeaponTrait = (sbyte)t;
                g.EventDmg = (sbyte)t;
                var b = g.WeaponBreakdown(ed);
                Assert.Equal(g.CritPct(), b.CritPct);
                var pick = new Rng();
                pick.Seed((uint)(9 + t * 7 + ed));
                int lo = 999, hi = 0;
                for (var k = 0; k < 2000; ++k)
                {
                    g.DmgCarry = pick.Range(0, 99);
                    g.HitsCount = 0;
                    g.Spawn(ed, 8, 7);
                    g.Enemies[g.EnemiesCount - 1].Hp = g.Enemies[g.EnemiesCount - 1].MaxHp = 30000;
                    g.HeroAttack(g.EnemiesCount - 1);
                    if (g.Hits[0].Kind != HitKind.Crit)
                    {
                        lo = Math.Min(lo, g.Hits[0].Amount);
                        hi = Math.Max(hi, g.Hits[0].Amount);
                    }
                    g.EnemiesCount = 0;
                }
                Assert.Equal(b.Min, lo);
                Assert.Equal(b.Max, hi);
                foreach (var x in Enum.GetValues<DmgText>())
                {
                    var m = new Message();
                    DamageHelp.Line(D, m, b, x);
                    Assert.True(m.N > 0 && m.N < Message.Len - 1);
                }
            }
        }
        g.EventDmg = 0;
    }

    [Fact]
    public void NewToolOnFieldAsksBeforeLosingUpgrade()
    {
        var g = TestData.Arena(1);
        g.WeaponLvl = 3;
        g.WeaponTrait = 0;
        g.Pickups[0] = new Pickup(g.Hero.X + 1, g.Hero.Y, PickupType.Tool, true, 0);
        g.PickupsCount = 1;
        g.PlayerMove(1, 0);
        Assert.True(g.HasToolOffer && g.Pickups[0].Active && g.WeaponLvl == 3);
        var cur = g.WeaponBreakdown();
        var nw = g.WeaponBreakdown(-1, D.Tools[0].Weapon);
        Assert.True(nw.UpgLevel == 0 && cur.UpgLevel == 3 && g.BotToolAccept() == nw.Avg10 > cur.Avg10);
        g.DeclineTool();
        Assert.True(!g.HasToolOffer && g.WeaponLvl == 3 && g.Pickups[0].Active);
        g.PlayerMove(-1, 0);
        g.PlayerMove(1, 0);
        Assert.True(g.HasToolOffer);
        g.AcceptTool();
        Assert.True(g.WeaponLvl == 0 && g.WeaponTrait < 0 && g.WeaponOverride == D.Tools[0].Weapon && !g.Pickups[0].Active);
        g.WeaponLvl = 2;
        g.WeaponTrait = 1;
        for (var i = 0; i < D.Hurtownia.Length; ++i)
        {
            if (D.Hurtownia[i].Effect != ShopEffect.Tool) continue;
            g.Cash = 500;
            g.HurtowniaBuy(i);
        }
        Assert.True(g.WeaponLvl == 0 && g.WeaponTrait < 0);
        var h = TestData.Run(2, 99);
        h.ActCleared = true;
        h.Cash = 300;
        for (var m = 0; m < h.Mats.Length; ++m) h.Mats[m] = 9;
        h.BotUpgrade();
        h.BotUpgrade();
        Assert.True(h.WeaponLvl == 2 && h.WeaponTrait == h.BotTraitChoice() && !h.TraitPending);
    }

    [Fact]
    public void HiddenRoomsAreSealedUntilOpened()
    {
        int found = 0, guards = 0, stages = 0;
        var kinds = new int[2];
        for (uint seed = 1; seed <= 80; ++seed)
        {
            for (var st = F0; st < D.StagesCount; ++st)
            {
                var g = TestData.Run((int)(seed % (uint)D.Classes.Length), seed * 7u + 3u);
                g.StartStage(st);
                ++stages;
                if (!g.HasSecret) continue;
                ++found;
                ++kinds[g.SecretKind];
                Assert.True(D.Stages[st].Boss < 0 && g.Lv.At(g.SecretX, g.SecretY) == Tile.Wall);
                Assert.True(g.Lv.At(g.SecretFrontX(), g.SecretFrontY()) == Tile.Floor && !TestData.Connected(g.Lv, g.Hero.X, g.Hero.Y));
                Assert.True(TestData.Connected(g.Lv, g.Hero.X, g.Hero.Y, g));
                var chest = -1;
                for (var i = 0; i < g.PickupsCount; ++i)
                {
                    if (g.Pickups[i].Type == PickupType.Chest) chest = i;
                }
                Assert.True(chest >= 0 && g.InSecret(g.Pickups[chest].X, g.Pickups[chest].Y));
                Assert.True(g.KeyHolder >= 0 && g.KeyHolder < g.EnemiesCount && !g.InSecret(g.Enemies[g.KeyHolder].X, g.Enemies[g.KeyHolder].Y));
                for (var i = 0; i < g.EnemiesCount; ++i)
                {
                    if (!g.InSecret(g.Enemies[i].X, g.Enemies[i].Y)) continue;
                    ++guards;
                    Assert.True(g.Enemies[i].Elite >= 0);
                }
                var o = g.Clone();
                o.Keys = 1;
                o.Hero.X = (sbyte)o.SecretFrontX();
                o.Hero.Y = (sbyte)o.SecretFrontY();
                for (var i = 0; i < o.EnemiesCount; ++i)
                {
                    if (o.Enemies[i].Alive && Game.Cheb(o.Enemies[i].X, o.Enemies[i].Y, o.Hero.X, o.Hero.Y) <= 1) o.Enemies[i].Alive = false;
                }
                Assert.True(o.CanOpenSecret() && o.PlayerMove(o.SecretX - o.Hero.X, o.SecretY - o.Hero.Y));
                Assert.True(o.SecretOpen && o.Keys == 0 && TestData.Connected(o.Lv, o.Hero.X, o.Hero.Y) && o.SecretsFound == 1);
                if (seed > 6) continue;
                var k = g.Clone(); // klucz z problemu, wybuch, Operator
                var kh = k.KeyHolder;
                int kx = k.Enemies[kh].X, ky = k.Enemies[kh].Y;
                k.DamageEnemy(kh, 30000, false, "test");
                if (!k.Enemies[kh].Alive && (k.Enemies[kh].Flags & ActorFlag.Reviving) == 0)
                {
                    var keyI = -1;
                    for (var i = 0; i < k.PickupsCount; ++i)
                    {
                        if (k.Pickups[i].Type == PickupType.StoreKey && k.Pickups[i].Active) keyI = i;
                    }
                    Assert.True((keyI >= 0 && Game.Cheb(k.Pickups[keyI].X, k.Pickups[keyI].Y, kx, ky) <= 1) || k.Keys == 1);
                    Assert.True(k.KeyHolder < 0);
                }
                var b = g.Clone();
                b.BlastSecret(b.SecretX, b.SecretY + 1, 1);
                Assert.Equal(b.SecretDef.Breakable, b.SecretOpen);
                var op = g.Clone();
                op.Cls = 8;
                Assert.Equal(op.SecretDef.Breakable, op.CanOpenSecret());
            }
        }
        Assert.True(found > stages / 8 && kinds[0] > 0 && kinds[1] > 0 && guards > 0);
    }

    [Fact]
    public void ChestGivesRewardsAndBotGoesForKeyFirst()
    {
        var g = TestData.Arena(0);
        g.Pickups[0] = new Pickup(8, 7, PickupType.Chest, true);
        g.PickupsCount = 1;
        int r0 = g.Respect, c0 = g.Cash;
        g.PlayerMove(1, 0);
        Assert.True(g.Respect == r0 + D.ChestRespect && g.Cash == c0 + g.Income(D.ChestCash) && g.Mats[0] == D.ChestMats);
        var gear = false;
        for (var s = 0; s < D.GearSlotsCount; ++s) gear |= g.Equipped[s] == D.ChestGearMin;
        Assert.True(gear);
        g.Pickups[1] = new Pickup(3, 3, PickupType.StoreKey, true);
        g.PickupsCount = 2;
        Assert.True(g.BotGoal(out var gx, out var gy) && gx == 3 && gy == 3);
    }
}
