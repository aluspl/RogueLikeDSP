using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.21.52 cz. c – port z meta.h: zadania dnia i tygodnia. 3 zadania dnia (pula GameData.DailyTasks) i 2 tygodnia
/// (GameData.WeeklyTasks) z seeda numeru dnia / tygodnia – te same dla wszystkich. Dzień: Godot – data z systemu, GBA – data
/// ustawiona dla budowy dnia. Postęp z każdej budowy (też dnia i tygodnia), w profilu ze znakiem wodnym TaskMark; wykonane =
/// Respekt od razu, wykonane łącznie – nagrody za liczbę (GameData.TaskRewards).
/// </summary>
public static class DailyTasks
{
    public const int Slots = Profile.TaskSlots;
    public const int DailySlots = Profile.DailyTaskSlots;

    private static void PickTasks(uint seed, int pool, int n, int[] output)
    {
        var h = seed;
        for (var k = 0; k < n; ++k)
        {
            h = h * 1664525u + 1013904223u;
            var i = (int)((h >> 16) % (uint)pool);
            for (var guard = 0; guard < pool; ++guard)
            {
                var used = false;
                for (var j = 0; j < k; ++j) used |= output[j] == i;
                if (!used) break;
                i = (i + 1) % pool;
            }
            output[k] = i;
        }
    }

    /// <summary>Zadanie w slocie s (0-2 dnia, 3-4 tygodnia) dla dnia i tygodnia.</summary>
    public static TaskDef At(GameData d, int day, int week, int s)
    {
        var ids = new int[DailySlots];
        if (s < DailySlots)
        {
            PickTasks(Daily.Seed(day) ^ 0x7A5Bu, d.DailyTasks.Length, DailySlots, ids);
            return d.DailyTasks[ids[s]];
        }
        PickTasks(Weekly.Seed(week) ^ 0x7A5Bu, d.WeeklyTasks.Length, Slots - DailySlots, ids);
        return d.WeeklyTasks[ids[s - DailySlots]];
    }

    public static TaskDef Of(GameData d, Profile p, int s) => At(d, p.TaskDay, p.TaskWeek, s);

    /// <summary>Etapy ukończone w budowie (z budowami NG+) – jak RunProgressXp.</summary>
    public static int StagesDone(GameData d, Game g)
    {
        var per = g.RouteCount() - g.FirstStage;
        var won = g.St == GameStatus.Won;
        return g.Tier * per + (won ? per : (g.St == GameStatus.StageClear ? g.Stage + 1 : g.Stage) - g.FirstStage);
    }

    public static int Metric(GameData d, Game g, TaskKind k)
    {
        var won = g.St == GameStatus.Won;
        switch (k)
        {
            case TaskKind.Kills: return g.Kills;
            case TaskKind.Elites: return g.ElitesKilled;
            case TaskKind.Bosses:
                {
                    var n = 0;
                    for (var e = 0; e < d.Enemies.Length; ++e)
                    {
                        if (CollectionBook.EnemyBoss(d, e)) n += g.KillsByType[e];
                    }
                    return n;
                }
            case TaskKind.Stages: return StagesDone(d, g);
            case TaskKind.Brigade: return g.HelpersCalled;
            case TaskKind.Powers: return g.PowersUsed;
            case TaskKind.Coffee: return g.CoffeeDrunk;
            case TaskKind.Combos: return g.CombosRun;
            case TaskKind.Storerooms: return g.SecretsFound;
            case TaskKind.Events: return g.StageEventLog.Count(x => x != 255);
            case TaskKind.Win: return g.Tier + (won ? 1 : 0);
            case TaskKind.WinNoShop: return g.ShopBuys == 0 ? g.Tier + (won ? 1 : 0) : 0;
            default: return 0;
        }
    }

    /// <summary>Nowy dzień / tydzień: zadania od zera (postęp, znak wodny, wykonane). Zwraca true, jeśli coś zmieniono.</summary>
    public static bool Roll(Profile p, int day, int week)
    {
        var changed = false;
        if (day > 0 && p.TaskDay != day)
        {
            p.TaskDay = (ushort)day;
            for (var s = 0; s < DailySlots; ++s)
            {
                p.TaskProgress[s] = 0;
                p.TaskMark[s] = 0;
                p.TaskDone = (byte)(p.TaskDone & ~(1 << s));
            }
            changed = true;
        }
        if (week > 0 && p.TaskWeek != week)
        {
            p.TaskWeek = (ushort)week;
            for (var s = DailySlots; s < Slots; ++s)
            {
                p.TaskProgress[s] = 0;
                p.TaskMark[s] = 0;
                p.TaskDone = (byte)(p.TaskDone & ~(1 << s));
            }
            changed = true;
        }
        return changed;
    }

    public static bool Done(Profile p, int s) => ((p.TaskDone >> s) & 1) != 0;

    /// <summary>Postęp w trakcie budowy: profil + licznik budowy jeszcze nieprzeniesiony (do celu); g = null – sam profil.</summary>
    public static int ProgressLive(GameData d, Profile p, Game g, int s)
    {
        var td = Of(d, p, s);
        int v = p.TaskProgress[s];
        if (g != null && !Done(p, s)) v += Math.Max(0, Metric(d, g, td.Kind) - p.TaskMark[s]);
        return Math.Min(v, td.Target);
    }

    /// <summary>Przenosi postęp zadań z budowy (koniec etapu, koniec budowy); wykonane dają Respekt. Zwraca bity wykonanych teraz.</summary>
    public static int Bank(GameData d, Profile p, Game g, int day, int week)
    {
        Roll(p, day, week);
        var got = 0;
        for (var s = 0; s < Slots; ++s)
        {
            var td = Of(d, p, s);
            int v = Math.Min(255, Metric(d, g, td.Kind)), add = v - p.TaskMark[s];
            p.TaskMark[s] = (byte)Math.Max(p.TaskMark[s], v);
            if (Done(p, s) || add <= 0) continue;
            p.TaskProgress[s] = (byte)Math.Min(255, p.TaskProgress[s] + add);
            if (p.TaskProgress[s] < td.Target) continue;
            p.TaskDone = (byte)(p.TaskDone | (1 << s));
            p.Respect = Meta.AddSat16(p.Respect, td.Respect);
            p.RespectTotal = Meta.AddSat16(p.RespectTotal, td.Respect);
            var before = p.TasksTotal;
            p.TasksTotal = Meta.AddSat16(p.TasksTotal, 1);
            foreach (var l in d.TaskRewards) // nagroda za liczbę zadań: Respekt raz
            {
                if (before < l.Xp && p.TasksTotal >= l.Xp) LifeLike.Core.Progress.GrantLevel(p, l);
            }
            got |= 1 << s;
        }
        return got;
    }

    /// <summary>Najbliższe wykonania zadanie (największy % postępu na żywo, przy remisie wcześniejszy slot); -1 = wszystkie wykonane.</summary>
    public static int Next(GameData d, Profile p, Game g)
    {
        int best = -1, bestPct = -1;
        for (var s = 0; s < Slots; ++s)
        {
            if (Done(p, s)) continue;
            var pct = ProgressLive(d, p, g, s) * 100 / Math.Max(1, Of(d, p, s).Target);
            if (pct <= bestPct) continue;
            bestPct = pct;
            best = s;
        }
        return best;
    }

    public static int DoneToday(Profile p) => Enumerable.Range(0, DailySlots).Count(s => Done(p, s));

    public static int DoneWeek(Profile p) => Enumerable.Range(DailySlots, Slots - DailySlots).Count(s => Done(p, s));

    /// <summary>Kolejna nagroda za zadania łącznie (-1 = wszystkie).</summary>
    public static int NextReward(GameData d, Profile p) => Array.FindIndex(d.TaskRewards, l => p.TasksTotal < l.Xp);
}
