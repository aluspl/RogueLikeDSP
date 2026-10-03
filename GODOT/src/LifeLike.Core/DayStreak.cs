using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.21.52 cz. c – port z meta.h: seria dni budowy dnia. Dzień zaraz po ostatnim = seria +1; dalszy = od nowa (1);
/// ten sam albo wcześniejszy – bez zmian (GBA: data wpisana ręcznie – liczy się tylko kolejny dzień). Nagrody za najdłuższą
/// serię (GameData.StreakRewards: pamiątka, kask, tytuł).
/// </summary>
public static class DayStreak
{
    /// <summary>Budowa dnia day rozegrana (Daily.Record). Zwraca bity nagród osiągniętych teraz.</summary>
    public static int Record(GameData d, Profile p, int day)
    {
        if (day <= 0) return 0;
        if (p.StreakDay != 0 && day <= p.StreakDay) return 0;
        p.Streak = p.StreakDay != 0 && day == p.StreakDay + 1 ? (byte)Math.Min(255, p.Streak + 1) : (byte)1;
        p.StreakDay = (ushort)day;
        int before = p.StreakBest;
        p.StreakBest = Math.Max(p.StreakBest, p.Streak);
        var got = 0;
        for (var l = 0; l < d.StreakRewards.Length; ++l)
        {
            if (before < d.StreakRewards[l].Xp && p.StreakBest >= d.StreakRewards[l].Xp) got |= 1 << l;
        }
        return got;
    }

    /// <summary>Seria widoczna dnia today: przerwana (dzień przerwy) = 0.</summary>
    public static int Now(Profile p, int today) => p.StreakDay != 0 && today <= p.StreakDay + 1 ? p.Streak : 0;

    /// <summary>Kolejna nagroda za serię (-1 = wszystkie).</summary>
    public static int NextReward(GameData d, Profile p) => Array.FindIndex(d.StreakRewards, l => p.StreakBest < l.Xp);
}
