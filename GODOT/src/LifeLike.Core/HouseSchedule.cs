namespace LifeLike.Core;

/// <summary>
/// Harmonogram domu po wygranej (port z GBA/include/meta.h): dni etapu z liczby tur (ScheduleMinDays + tury /
/// ScheduleTurnsPerDay), koszty etapów z danych, daty liczone wstecz od dnia odbioru; od pierwszego etapu budowy (Akt 0 tylko,
/// gdy był w budowie).
/// </summary>
public static class HouseSchedule
{
    public static int Days(Game g, int s) => g.D.ScheduleMinDays + g.StageDays[s] / g.D.ScheduleTurnsPerDay;

    public static int TotalDays(Game g)
    {
        var t = 0;
        for (var s = g.FirstStage; s < g.RouteCount(); ++s) t += Days(g, s);
        return t;
    }

    public static int TotalCost(Game g)
    {
        var t = 0;
        for (var s = g.FirstStage; s < g.RouteCount(); ++s) t += g.SDef(s).Cost;
        return t;
    }

    /// <summary>Dzień (Daily.DaysFromCivil) rozpoczęcia etapu s, gdy odbiór był w dniu endDay.</summary>
    public static int StartDay(Game g, int s, int endDay)
    {
        var d = endDay - TotalDays(g);
        for (var i = g.FirstStage; i < s; ++i) d += Days(g, i);
        return d;
    }
}
