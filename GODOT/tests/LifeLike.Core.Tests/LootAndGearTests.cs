namespace LifeLike.Core.Tests;

// core_tests.cpp: 15 (dropy), 21 (sprzęt i porównanie przy paczce), 27a (cechy sprzętu).
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
    public void GearBoxOnEmptySlotEquipsOtherwiseAsksToCompare()
    {
        var g = TestData.Arena(1);
        for (var i = 0; i < D.GearSlotsCount; ++i) Assert.Equal(-1, g.Equipped[i]);
        int hp0 = g.Hero.MaxHp;
        void Give(int slot, int rarity, int trait = 0)
        {
            g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.GearBox, true, slot * 3 + rarity, trait);
            g.PickupsCount = 1;
            g.Collect();
        }
        Give(2, 1, 3); // kamizelka ocieplana +8 HP: pusty slot – od razu
        Assert.True(g.Equipped[2] == 1 && g.EquippedTrait[2] == 3 && g.Hero.MaxHp == hp0 + 8 && !g.HasOffer);
        var xp0 = g.RunXp;
        Give(2, 0); // zajęty slot: porównanie
        Assert.True(g.HasOffer && !g.OfferIsBetter && g.Equipped[2] == 1);
        Give(2, 2);
        Assert.True(g.Pickups[0].Active); // druga paczka czeka, aż zdecydujesz
        g.DeclineOffer(); // zostawiam stary: doświadczenie
        Assert.True(!g.HasOffer && g.Equipped[2] == 1 && g.Hero.MaxHp == hp0 + 8 && g.RunXp == xp0 + D.GearDeclineXp);
        Give(2, 0, 1);
        g.AcceptOffer(); // gorszy, ale gracz wybrał
        Assert.True(g.Equipped[2] == 0 && g.EquippedTrait[2] == 1 && g.Hero.MaxHp == hp0 + 4);
        Give(2, 2);
        Assert.True(g.OfferIsBetter);
        g.AcceptOffer(); // lepsza zastępuje
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

    private static int TraitOf(TraitEffect e)
    {
        for (var i = 0; i < D.GearTraitsCount; ++i)
            if (D.GearTraits[i].Effect == e) return i;
        return -1;
    }

    [Fact]
    public void GearTraitsLuckCritPoisonResistanceCooldown()
    {
        var g = TestData.Arena(1);
        int luck0 = g.Luck(), crit0 = g.CritPct(), cd0 = g.AbilityCooldown();
        g.Equip(0, 0, TraitOf(TraitEffect.Luck));
        Assert.True(g.Luck() == luck0 + 1 && g.CritPct() == crit0 + D.CritPerLuckPct);
        g.Equip(1, 0, TraitOf(TraitEffect.Crit));
        Assert.Equal(crit0 + D.CritPerLuckPct + 5, g.CritPct());
        g.Equip(2, 0, TraitOf(TraitEffect.Cooldown));
        Assert.Equal(cd0 - 1, g.AbilityCooldown());
        g.Equip(0, 0, TraitOf(TraitEffect.PoisonRes));
        g.ApplyStatus(StatusEffect.Poison, 3);
        Assert.Equal(0, g.StatusTurns(StatusEffect.Poison));
        g.Hero.X = 7;
        g.Hero.Y = 7;
        g.UpdateFov();
        Assert.True(!g.Visible(7, 7 + Game.FovRadius + 1) || g.Lv.At(7, 7 + Game.FovRadius + 1) == Tile.Wall);
    }

    [Fact]
    public void SightTraitWidensFieldOfView()
    {
        var w = TestData.NewGame();
        w.NewRun(0, 3);
        w.Lv.Fill(Tile.Floor);
        w.EnemiesCount = 0;
        w.Hero.X = 15;
        w.Hero.Y = 15;
        w.UpdateFov();
        Assert.False(w.Visible(15, 15 + Game.FovRadius + 1));
        w.Equip(0, 0, TraitOf(TraitEffect.Sight));
        Assert.True(w.SightRadius() == Game.FovRadius + 1 && w.Visible(15, 15 + Game.FovRadius + 1));
    }

    [Fact]
    public void GearDropsHaveEveryTrait()
    {
        var seen = 0;
        for (uint seed = 1; seed <= 1500; ++seed)
        {
            var h = KillAdjacent(seed);
            h.PlayerMove(1, 0);
            var p = h.Pickups[h.PickupsCount - 1];
            if (p.Type == PickupType.GearBox)
            {
                Assert.True(p.Trait < D.GearTraitsCount);
                seen |= 1 << p.Trait;
            }
        }
        Assert.Equal((1 << D.GearTraitsCount) - 1, seen);
    }
}
