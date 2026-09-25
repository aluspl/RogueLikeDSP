namespace LifeLike.Core.Tests;

// core_tests.cpp: 15 (dropy), 21 (sprzęt).
public class LootAndGearTests
{
    private static GameData D => TestData.D;

    private static Game KillAdjacent(uint seed)
    {
        var g = TestData.Run(1, seed);
        g.EnemiesCount = 0;
        g.Spawn(0, g.Hero.X + 1, g.Hero.Y);
        g.Enemies[0].Hp = 1;
        return g;
    }

    [Fact]
    public void DropsAppearWhereEnemyDiedOnlyUnlockedTools()
    {
        int drops = 0, toolsSeen = 0;
        for (uint seed = 1; seed <= 300; ++seed)
        {
            var g = KillAdjacent(seed);
            var before = g.PickupsCount;
            g.PlayerMove(1, 0);
            if (g.PickupsCount > before)
            {
                ++drops;
                var p = g.Pickups[g.PickupsCount - 1];
                Assert.True(p.Active && p.X == g.Hero.X + 1 && p.Y == g.Hero.Y);
                if (p.Type == PickupType.Tool)
                {
                    ++toolsSeen;
                    Assert.True((D.StartToolsMask & (1 << p.Arg)) != 0);
                }
            }
        }
        Assert.True(drops > 300 * D.DropChancePct / 200 && drops < 300 * D.DropChancePct * 2 / 100);
        Assert.True(toolsSeen > 0);
    }

    [Fact]
    public void BetterGearEquipsWorseGivesXp()
    {
        var g = TestData.Arena(1);
        for (var i = 0; i < D.GearSlotsCount; ++i) Assert.Equal(-1, g.Equipped[i]);
        int hp0 = g.Hero.MaxHp;
        void Give(int slot, int rarity)
        {
            g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.GearBox, true, slot * 3 + rarity);
            g.PickupsCount = 1;
            g.Collect();
        }
        Give(2, 1);
        Assert.True(g.Equipped[2] == 1 && g.Hero.MaxHp == hp0 + 8);
        var xp0 = g.RunXp;
        Give(2, 0);
        Assert.True(g.Equipped[2] == 1 && g.Hero.MaxHp == hp0 + 8 && g.RunXp > xp0);
        Give(2, 2);
        Assert.True(g.Equipped[2] == 2 && g.Hero.MaxHp == hp0 + 12);
        Give(0, 2);
        Give(1, 2);
        Assert.True(g.GearBonus(GearStat.Def) == 3 && g.GearBonus(GearStat.Dmg) == 3);
    }

    [Fact]
    public void HelmetReducesAndGlovesIncreaseDamage()
    {
        int[] lost = new int[2], dealt = new int[2];
        for (var k = 0; k < 2; ++k)
        {
            var g = TestData.Run(3, 9);
            if (k == 1)
            {
                g.Equipped[0] = 2;
                g.Equipped[1] = 2;
            }
            g.EnemiesCount = 0;
            g.Spawn(8, g.Hero.X + 1, g.Hero.Y);
            g.Enemies[0].Awake = true;
            int ehp = g.Enemies[0].Hp, hhp = g.Hero.Hp;
            g.PlayerMove(1, 0);
            dealt[k] = ehp - g.Enemies[0].Hp;
            lost[k] = hhp - g.Hero.Hp;
        }
        Assert.Equal(dealt[0] + 3, dealt[1]);
        Assert.True(lost[1] < lost[0]);
    }

    [Fact]
    public void GearRarityImprovesOnLateStages()
    {
        var brand = new int[2];
        var gearDrops = 0;
        for (var st = 0; st < 2; ++st)
        {
            for (uint seed = 1; seed <= 1500; ++seed)
            {
                var g = KillAdjacent(seed);
                g.Stage = st == 0 ? 0 : D.Stages.Length - 1;
                g.PlayerMove(1, 0);
                var p = g.Pickups[g.PickupsCount - 1];
                if (p.Type == PickupType.GearBox)
                {
                    Assert.True(p.Arg < D.GearSlotsCount * 3);
                    ++gearDrops;
                    if (p.Arg % 3 == 2) brand[st]++;
                }
            }
        }
        Assert.True(gearDrops > 0 && brand[1] > brand[0]);
    }
}
