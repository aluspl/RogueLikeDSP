using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.21.52 cz. c – wspólne dla drzewka, kolekcji, zadań i serii dni: nagrody za próg z listy (kolor kasku) i migracja
/// profilu v14 -> v15 (meta.h: goal_helmet, migrate_v15).
/// </summary>
public static class Goals
{
    /// <summary>Kolor kasku k z listy nagród za próg (zadania łącznie, seria dni), gdy have osiągnęło próg.</summary>
    public static bool Helmet(ProgressLevel[] lv, int have, int k) => lv.Any(l => l.Reward == ProgressReward.Helmet && l.Index == k && have >= l.Xp);

    /// <summary>
    /// v14 -> v15: drzewko bez wyborów, kolekcje – rodzaje z Katalogu jako 1 pokonany, zadania od zera, seria dni z wyników
    /// ostatnich dni budowy dnia (kolejne dni do najnowszego); komplety już osiągnięte – bez banera.
    /// </summary>
    public static void MigrateV15(GameData d, Profile p)
    {
        p.Tree = 0;
        for (var e = 0; e < Profile.MaxEnemyTypes; ++e)
        {
            p.KillCount[e] = (byte)(e < d.Enemies.Length && Meta.CatalogHas(p, e) ? 1 : 0);
            p.KillMark[e] = 0;
        }
        p.TaskDay = 0;
        p.TaskWeek = 0;
        Array.Clear(p.TaskProgress);
        Array.Clear(p.TaskMark);
        p.TaskDone = 0;
        p.TasksTotal = 0;
        p.Collections = 0;
        var last = 0;
        for (var i = 0; i < Profile.DailySlots; ++i) last = Math.Max(last, p.DailyDay[i]);
        var n = 0;
        if (last > 0)
        {
            for (n = 1; n < Profile.DailySlots; ++n)
            {
                var has = false;
                for (var i = 0; i < Profile.DailySlots; ++i) has |= p.DailyDay[i] == last - n;
                if (!has || last - n <= 0) break;
            }
        }
        p.Streak = (byte)n;
        p.StreakBest = (byte)n;
        p.StreakDay = (ushort)last;
        for (var i = 0; i < d.Collections.Length; ++i)
        {
            if (CollectionBook.Complete(d, p, i)) p.Collections = (byte)(p.Collections | (1 << i));
        }
    }
}
