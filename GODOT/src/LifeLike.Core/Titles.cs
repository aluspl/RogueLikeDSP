using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Tytuły (v0.21.52, #43) – port z meta.h: z odznak i zleceń (zamiast dużej nagrody w doświadczeniu). Tytuł t:
/// 0..odznaki-1 = odznaki, dalej zlecenia, (cz. b) dalej poziom inspektora i stopnie inwestora (GameData.ProgressTitles).
/// Wybór w profilu (Profile.Title = t + 1), widać go w profilu i na końcu budowy.
/// </summary>
public static class Titles
{
    public static int Count(GameData d) => d.Badges.Length + d.Contracts.Length + d.ProgressTitles.Length;

    /// <summary>Pierwszy tytuł z poziomu inspektora / stopni inwestora.</summary>
    public static int ProgressFrom(GameData d) => d.Badges.Length + d.Contracts.Length;

    public static string Name(GameData d, int t)
    {
        if (t >= ProgressFrom(d)) return d.ProgressTitles[t - ProgressFrom(d)].Name;
        return t < d.Badges.Length ? d.Badges[t].Title : d.Contracts[t - d.Badges.Length].Title;
    }

    public static bool Owned(GameData d, Profile p, int t)
    {
        if (t >= ProgressFrom(d))
        {
            var pt = d.ProgressTitles[t - ProgressFrom(d)];
            return pt.Source == 0 ? Progress.InspectorLevel(d, p) >= pt.Level : Progress.MaxStake(d, p) >= pt.Level;
        }
        return t < d.Badges.Length ? ((p.Badges >> t) & 1) != 0 : ((p.Contracts >> (t - d.Badges.Length)) & 1) != 0;
    }

    /// <summary>Tytuł z nagrody poziomu inspektora (source 0, level 1..) albo stopnia inwestora (1, stawka); -1 = brak.</summary>
    public static int ProgressIndex(GameData d, int source, int level)
    {
        var i = Array.FindIndex(d.ProgressTitles, x => x.Source == source && x.Level == level);
        return i < 0 ? -1 : ProgressFrom(d) + i;
    }

    public static int OwnedCount(GameData d, Profile p) => Enumerable.Range(0, Count(d)).Count(t => Owned(d, p, t));

    /// <summary>Wybrany tytuł (-1 = bez tytułu albo już nie należy do gracza).</summary>
    public static int Selected(GameData d, Profile p)
    {
        var t = p.Title - 1;
        return t >= 0 && t < Count(d) && Owned(d, p, t) ? t : -1;
    }

    public static void Cycle(GameData d, Profile p, int dir)
    {
        var n = Count(d) + 1;
        int t = p.Title;
        for (var i = 0; i < n; ++i)
        {
            t = (t + dir + n) % n;
            if (t == 0 || Owned(d, p, t - 1)) break;
        }
        p.Title = (byte)t;
    }
}
