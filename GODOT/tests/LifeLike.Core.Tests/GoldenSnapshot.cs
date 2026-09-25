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
        K("counters", Arr([g.PowersUsed, g.BrandFound, g.CleanBosses, g.BossWakeDamage]));
        K("stats", Arr([g.HeroStat(Stat.Str), g.HeroStat(Stat.Agi), g.HeroStat(Stat.Intel), g.Luck(), g.CritPct(), g.SightRadius(), g.AbilityCooldown()]));
        K("offer", Arr([g.OfferSlot, g.OfferRarity, g.OfferTrait]));
        K("heroStatus", Arr(g.HeroStatus.Select(x => (int)x)));
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
            Arr([e.DefId, e.X, e.Y, e.Hp, e.MaxHp, e.Alive ? 1 : 0, e.Awake ? 1 : 0, e.Stun]))) + "]");
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
        $"\"contracts\":{p.Contracts},\"keepsake\":{p.Keepsake},\"keepsakeRuns\":{Arr(p.KeepsakeRuns.Select(x => (int)x))}," +
        $"\"sram\":\"{Convert.ToHexString(p.ToBytes()).ToLowerInvariant()}\"}}";
}
