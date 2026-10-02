using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Tytuły (v0.21.52, #43) – port z meta.h: z odznak i zleceń (zamiast dużej nagrody w doświadczeniu). Tytuł t:
/// 0..odznaki-1 = odznaki, dalej zlecenia. Wybór w profilu (Profile.Title = t + 1), widać go w profilu i na końcu budowy.
/// </summary>
public static class Titles
{
    public static int Count(GameData d) => d.Badges.Length + d.Contracts.Length;

    public static string Name(GameData d, int t) => t < d.Badges.Length ? d.Badges[t].Title : d.Contracts[t - d.Badges.Length].Title;

    public static bool Owned(GameData d, Profile p, int t) =>
        t < d.Badges.Length ? ((p.Badges >> t) & 1) != 0 : ((p.Contracts >> (t - d.Badges.Length)) & 1) != 0;

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
