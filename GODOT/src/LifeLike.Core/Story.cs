using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Fabuła odkrywana z budowami (#35, port z GBA/include/meta.h): wątek SMS-ów odblokowuje się, gdy warunek jest spełniony
/// (profil po budowie + ta budowa); archiwum Wiadomości w telefonie profilu, nowe wątki z kropką do przeczytania.
/// Osiedle rośnie z wygranymi (ozdoby GameData.EstateDecor).
/// </summary>
public static class Story
{
    /// <summary>g = null: tylko profil (migracja).</summary>
    public static bool Condition(GameData d, Profile p, Game g, int i)
    {
        var t = d.StoryArc[i];
        switch (t.Trigger)
        {
            case StoryTrigger.Runs: return p.Runs >= t.Value;
            case StoryTrigger.Wins: return p.Wins >= t.Value;
            case StoryTrigger.Boss: return Meta.CatalogHas(p, t.Value);
            case StoryTrigger.Elite: return g is not null && g.ElitesKilled > 0;
            case StoryTrigger.Secret: return g is not null && g.SecretsFound > 0;
            case StoryTrigger.Event: return g is not null && g.StageEventLog.Any(x => x != 255);
            case StoryTrigger.Synergy: return g is not null && g.SynergyMask() != 0;
            case StoryTrigger.Daily: return p.DailyRuns > 0;
            case StoryTrigger.Weekly: return p.WeeklyRuns > 0;
            case StoryTrigger.Act0: return Meta.Act0Unlocked(d, p);
            default: return false;
        }
    }

    /// <summary>Sprawdza wątki (koniec budowy). Zwraca bitmaskę odblokowanych właśnie teraz.</summary>
    public static uint Check(GameData d, Profile p, Game g)
    {
        var got = 0u;
        for (var i = 0; i < d.StoryArc.Length; ++i)
        {
            if (((p.Story >> i) & 1) == 0 && Condition(d, p, g, i)) got |= 1u << i;
        }
        p.Story |= got;
        p.StoryNew |= got;
        return got;
    }

    public static bool Unlocked(Profile p, int i) => ((p.Story >> i) & 1) != 0;

    public static bool Unread(Profile p, int i) => ((p.StoryNew >> i) & 1) != 0;

    public static void MarkRead(Profile p, int i) => p.StoryNew &= ~(1u << i);

    public static int Count(GameData d, Profile p) => Enumerable.Range(0, d.StoryArc.Length).Count(i => Unlocked(p, i));

    public static int UnreadCount(GameData d, Profile p) => Enumerable.Range(0, d.StoryArc.Length).Count(i => Unread(p, i));

    /// <summary>v10 -> v11: wątki za to, co już osiągnięte (liczniki, bossowie z Katalogu, Akt 0) – czekają jako nowe.</summary>
    public static void MigrateV11(GameData d, Profile p)
    {
        p.Story = 0;
        p.StoryNew = 0;
        Check(d, p, null);
    }

    /// <summary>Osiedle rośnie z wygranymi: ile ozdób już stoi.</summary>
    public static int EstateDecor(GameData d, Profile p)
    {
        var n = 0;
        while (n < d.EstateDecor.Length && p.Wins >= d.EstateDecor[n].Wins) ++n;
        return n;
    }
}
