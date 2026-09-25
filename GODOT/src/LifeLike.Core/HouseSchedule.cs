namespace LifeLike.Core;

/// <summary>
/// Harmonogram domu po wygranej (port z GBA/include/meta.h): dni etapu z liczby tur (ScheduleMinDays + tury /
/// ScheduleTurnsPerDay), koszty etapów z danych, daty liczone wstecz od dnia odbioru.
/// </summary>
public static class HouseSchedule
{
    public static int Days(Game g, int s) => g.D.ScheduleMinDays + g.StageDays[s] / g.D.ScheduleTurnsPerDay;

    public static int TotalDays(Game g)
    {
        var t = 0;
        for (var s = 0; s < g.D.Stages.Length; ++s) t += Days(g, s);
        return t;
    }

    public static int TotalCost(Game g)
    {
        var t = 0;
        foreach (var st in g.D.Stages) t += st.Cost;
        return t;
    }

    /// <summary>Dzień (Daily.DaysFromCivil) rozpoczęcia etapu s, gdy odbiór był w dniu endDay.</summary>
    public static int StartDay(Game g, int s, int endDay)
    {
        var d = endDay - TotalDays(g);
        for (var i = 0; i < s; ++i) d += Days(g, i);
        return d;
    }
}
