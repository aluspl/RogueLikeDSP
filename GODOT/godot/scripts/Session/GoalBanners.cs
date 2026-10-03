using System.Collections.Generic;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game.Session;

/// <summary>
/// v0.21.52 cz. c: banery celów (push_goals na GBA) – zadania dnia / tygodnia (; kilka naraz – jeden zbiorczy), nagroda
/// za liczbę wykonanych zadań, komplety kolekcji i nagrody za serię dni budowy dnia.
/// </summary>
public static class GoalBanners
{
    public static List<(string Title, string Body)> Of(GameData d, Profile p, int tasks, int collections, int tasksBefore,
                                                        int streakBefore = 0, int streakAfter = 0)
    {
        var list = new List<(string, string)>();
        int n = 0, resp = 0, last = -1;
        for (var s = 0; s < DailyTasks.Slots; ++s)
        {
            if (((tasks >> s) & 1) == 0) continue;
            ++n;
            resp += DailyTasks.Of(d, p, s).Respect;
            last = s;
        }
        if (n == 1) list.Add((Loc.F("respektu_2", (last < DailyTasks.DailySlots ? Loc.T("zadanie_dnia_2") : Loc.T("zadanie_tygodnia")), resp), DailyTasks.Of(d, p, last).Name));
        else if (n > 1) list.Add((Loc.F("zadania_wykonane", n), Loc.F("respekt_5", resp)));
        foreach (var l in d.TaskRewards)
        {
            if (tasksBefore < l.Xp && p.TasksTotal >= l.Xp) list.Add((Loc.F("zadania_wykonanych", l.Xp), Progress.RewardLabel(d, l, -1)));
        }
        for (var i = 0; i < d.Collections.Length; ++i)
        {
            if (((collections >> i) & 1) != 0) list.Add((Loc.T("komplet") + d.Collections[i].Name, CollectionBook.RewardLabel(d, i)));
        }
        foreach (var l in d.StreakRewards)
        {
            if (streakBefore < l.Xp && streakAfter >= l.Xp) list.Add((Loc.F("seria_dni_2", l.Xp), Progress.RewardLabel(d, l, -1)));
        }
        return list;
    }
}
