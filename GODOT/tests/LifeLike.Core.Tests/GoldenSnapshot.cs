using System.Text;

namespace LifeLike.Core.Tests;

/// <summary>Zrzut stanu gry w formacie identycznym z funkcją snapshot() w GBA/tests/golden_dump.cpp.</summary>
public static class GoldenSnapshot
{
    private static string B(bool v) => v ? "1" : "0";

    private static string Arr(IEnumerable<int> xs) => "[" + string.Join(",", xs) + "]";

    public static string Of(Game g, int step)
    {
        var s = new StringBuilder();
        void K(string k, string v)
        {
            if (s.Length > 1) s.Append(',');
            s.Append('"').Append(k).Append("\":").Append(v);
        }
        s.Append('{');
        K("step", step.ToString());
        K("stage", g.Stage.ToString());
        K("tier", g.Tier.ToString());
        K("st", ((int)g.St).ToString());
        K("turns", g.Turns.ToString());
        K("score", g.Score.ToString());
        K("cash", g.Cash.ToString());
        K("xpPct", g.XpPct.ToString());
        K("xpBanked", g.XpBanked.ToString());
        K("runXp", g.RunXp.ToString());
        K("heroLevel", g.HeroLevel.ToString());
        K("rng", g.R.S.ToString());
        K("hero", Arr([g.Hero.X, g.Hero.Y, g.Hero.Hp, g.Hero.MaxHp, g.Hero.Alive ? 1 : 0]));
        K("defBonus", g.DefBonus.ToString());
        K("dmgBonus", g.DmgBonus.ToString());
        K("kills", g.Kills.ToString());
        K("abilityCd", g.AbilityCd.ToString());
        K("boss", g.Boss.ToString());
        K("stairs", Arr([g.StairsX, g.StairsY]));
        K("weaponOverride", g.WeaponOverride.ToString());
        K("actBonus", g.ActBonus.ToString());
        K("actCleared", B(g.ActCleared));
        K("actKills", g.ActKills.ToString());
        K("toolsFound", g.ToolsFound.ToString());
        K("logSerial", g.LogSerial.ToString());
        K("stageDamage", g.StageDamage.ToString());
        K("stageKills", g.StageKills.ToString());
        K("stageStartTurn", g.StageStartTurn.ToString());
        K("slam", Arr([g.SlamTimer, g.SlamX, g.SlamY, g.SlamCounter]));
        K("equipped", Arr(g.Equipped.Select(x => (int)x)));
        K("equippedTrait", Arr(g.EquippedTrait.Select(x => (int)x)));
        K("thermos", g.Thermos.ToString());
        K("thermosCap", g.ThermosCap().ToString());
        K("stageEvent", g.StageEvent.ToString());
        K("weather", g.Weather.ToString());
        K("weaponRange", g.WeaponRange().ToString());
        K("investor", Arr([g.Bonus.Investor, g.Income(100), g.SlamEvery(), g.ShopClosed ? 1 : 0]));
        K("brigade", Arr([g.HelperCalled, g.GuardTurns, g.AllyTurns, g.AllyX, g.AllyY, g.HeroDefense()]));
        K("counters", Arr([g.PowersUsed, g.BrandFound, g.CleanBosses, g.BossWakeDamage]));
        K("stats", Arr([g.HeroStat(Stat.Str), g.HeroStat(Stat.Agi), g.HeroStat(Stat.Intel), g.Luck(), g.CritPct(), g.SightRadius(), g.AbilityCooldown()]));
        K("path", Arr([g.StagePath, g.PathOffer(0), g.PathOffer(1)]));
        K("mats", Arr(g.Mats.Take(3).Select(x => (int)x)));
        K("bridges", "[" + string.Join(",", Enumerable.Range(0, g.Bridges).Select(i => Arr([g.BridgeX[i], g.BridgeY[i]]))) + "]");
        K("daily", Arr([g.Daily ? 1 : 0, g.DailyDay]));
        K("stageDays", Arr(g.StageDays.Select(x => (int)x)));
        K("blast", Arr([g.BlastX, g.BlastY, g.BlastTimer, g.BlastDmg, g.GustIn(), g.GustDir()]));
        K("offer", Arr([g.OfferSlot, g.OfferRarity, g.OfferTrait]));
        K("respect", Arr([g.Respect, g.StageRespect(), g.DmgCarry, g.TakenCarry, g.SecondUsed ? 1 : 0, g.DodgePct(), g.CoffeeHeal(),
            g.Bonus.GearSlots, g.Bonus.Tools]));
        K("act0", Arr([g.FirstStage, g.Docs, g.DocsNeeded(), g.StairsLocked() ? 1 : 0, g.StageNumber(), g.StagesInRun()]));
        K("heroStatus", Arr(g.HeroStatus.Select(x => (int)x)));
        K("boons", "[" + string.Join(",", new long[]
        {
            (uint)g.Boons, (uint)(g.Boons >> 32), g.BoonOffer[0], g.BoonOffer[1], g.BoonOffer[2], g.BoonRerolls, g.SynergyMask(),
            g.RerollsLeft(), g.RerollPrice(), g.HeroDefense(), g.Luck(), g.DodgePct(),
        }) + "]");
        K("part3", Arr([g.BoonSalt, g.PendingEvent, g.StageChoice, g.StageChoicePick, g.ChoiceDone, g.EventsSeen, g.EventDmg, g.EventDef,
            g.WeaponLvl, g.WeaponTrait, g.TraitPending ? 1 : 0, g.ToolOffer, g.ToolOfferPickup, g.SecretX, g.SecretY, g.SecretKind, g.SecretDir,
            g.SecretRx, g.SecretRy, g.SecretRw, g.SecretRh, g.SecretOpen ? 1 : 0, g.KeyHolder, g.Keys, g.SecretsFound, g.UpgradePrice(),
            g.CanOpenSecret() ? 1 : 0]));
        var p4 = new List<int>();
        foreach (var h in g.LastHits.Append(g.WorstHit)) p4.AddRange([h.Src, h.Elite, h.Kind, h.Stage, h.Amount]);
        p4.AddRange([g.BestHit, g.BestHitDef, g.BestHitCrit ? 1 : 0, g.BlastSrc]);
        for (var i = 0; i < Game.MaxStages; i++) p4.AddRange([g.StageKillLog[i], g.StageBoon[i], g.StageEventLog[i], g.StageFlags[i]]);
        p4.AddRange([g.ElitesKilled, g.CombosRun, g.WeeklyWeek, g.Bonus.Weekly, g.EliteChance(), g.ShopClosed ? 1 : 0, Recap.TipIndex(g.D, g)]);
        K("part4", Arr(p4));
        K("part5", Arr([g.CoffeeDrunk, g.ShockCombos, g.PaperHits, g.SecretFlags, g.HelperCtx, g.BorrowCls, g.MarkTarget, g.MarkTurns,
            g.PowerCls(), g.BuildDays(), g.Bonus.StartCoffee, g.AbilityCooldown()]));
        K("killsByType", Arr(g.KillsByType.Select(x => (int)x)));
        K("rooms", "[" + string.Join(",", g.Lv.Rooms.Take(g.Lv.RoomsCount).Select(r => Arr([r.X, r.Y, r.W, r.H]))) + "]");
        var map = new List<string>();
        var fov = new List<string>();
        for (var y = 0; y < Level.H; y++)
        {
            var row = new StringBuilder("\"");
            var frow = new StringBuilder("\"");
            for (var x = 0; x < Level.W; x++)
            {
                row.Append(g.Lv[x, y] switch { Tile.Wall => '#', Tile.Floor => '.', _ => '>' });
                frow.Append((char)('0' + (int)g.Fov[y * Level.W + x]));
            }
            map.Add(row.Append('"').ToString());
            fov.Add(frow.Append('"').ToString());
        }
        K("map", "[" + string.Join(",", map) + "]");
        K("fov", "[" + string.Join(",", fov) + "]");
        K("enemies", "[" + string.Join(",", g.Enemies.Take(g.EnemiesCount).Select(e =>
            Arr([e.DefId, e.X, e.Y, e.Hp, e.MaxHp, e.Alive ? 1 : 0, e.Awake ? 1 : 0, e.Stun, e.Flags, e.Grow, e.Timer, e.Elite, e.Wet]))) + "]");
        K("pickups", "[" + string.Join(",", g.Pickups.Take(g.PickupsCount).Select(p =>
            Arr([p.X, p.Y, (int)p.Type, p.Active ? 1 : 0, p.Arg, p.Trait]))) + "]");
        K("walls", "[" + string.Join(",", g.Walls.Take(g.WallsCount).Select(w => Arr([w.X, w.Y, w.Turns]))) + "]");
        K("log", "[" + string.Join(",", g.Log.Select(m =>
            $"{{\"hex\":\"{Convert.ToHexString(m.S, 0, m.N).ToLowerInvariant()}\",\"kind\":{(int)m.Kind},\"repeat\":{m.Repeat}}}")) + "]");
        s.Append('}');
        return s.ToString();
    }

    public static string OfProfile(Profile p) =>
        $"{{\"best\":{p.Best},\"runs\":{p.Runs},\"wins\":{p.Wins},\"xp\":{p.Xp},\"badges\":{p.Badges},\"catalog\":{p.Catalog}," +
        $"\"classWins\":{p.ClassWins},\"toolsFound\":{p.ToolsFound},\"houses\":{Arr(p.Houses.Take(p.HousesCount).Select(h => (int)h))}," +
        $"\"killsTotal\":{p.KillsTotal},\"powersTotal\":{p.PowersTotal},\"brandTotal\":{p.BrandTotal},\"cleanBosses\":{p.CleanBosses}," +
        $"\"contracts\":{p.Contracts},\"keepsake\":{p.Keepsake},\"brigade\":{p.Brigade},\"investor\":{p.Investor},\"bestStake\":{Arr(p.BestStake.Select(x => (int)x))},\"keepsakeRuns\":{Arr(p.KeepsakeRuns.Select(x => (int)x))}," +
        $"\"daily\":{Arr([p.DailyWon, p.DailyRuns])},\"dailyDay\":{Arr(p.DailyDay.Select(x => (int)x))},\"dailyScore\":{Arr(p.DailyScore)}," +
        $"\"respect\":{Arr([p.Respect, p.RespectTotal, p.RunRespect, p.Rewards, p.ClassWinsHi])},\"catalogHi\":{p.CatalogHi}," +
        $"\"tutorial\":{Arr([p.Tutorial, p.ClassesSeen])}," +
        $"\"weekly\":{Arr([p.WeeklyWon, p.WeeklyRuns, p.WeeklyWeek[0], p.WeeklyScore[0], p.WeeklyWeek[1], p.WeeklyScore[1], p.WeeklyWeek[2], p.WeeklyScore[2]])}," +
        $"\"story\":{Arr([(int)p.Story, (int)p.StoryNew, Story.EstateDecor(TestData.D, p)])}," +
        $"\"secrets\":{Arr([p.Secrets, p.SecretsNew, p.Cosmetic, Secrets.DoneCount(TestData.D, p)])}," +
        $"\"looks\":{Arr([p.Title, p.Helmet, Titles.OwnedCount(TestData.D, p), Secrets.HelmetsUnlocked(TestData.D, p), Meta.ClassCost(TestData.D, p), Meta.ToolCost(TestData.D, p), Meta.ShopSpent(TestData.D, p)])}," +
        $"\"sram\":\"{Convert.ToHexString(p.ToBytes()).ToLowerInvariant()}\"}}";
}
