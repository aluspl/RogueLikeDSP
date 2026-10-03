using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.21.53: filtry ekranu (meta.h: filter_*) – klasyczny i tryby dla daltonistów (Protanopia, Deuteranopia,
/// Tritanopia, Wysoki kontrast) zawsze; zabawowe (Noir, Retro LCD, Neon nocy, Kwas) po spełnieniu dowolnego z warunków
/// z danych. Zablokowany: „???” z podpowiedzią. Profil v17: wybrany filtr (GBA) i ogłoszone filtry (baner raz).
/// </summary>
public static class ScreenFilters
{
    public static bool CondMet(GameData d, Profile p, FilterCond c) => c.Kind switch
    {
        FilterUnlock.Inspector => Progress.InspectorLevel(d, p) >= c.Value,
        FilterUnlock.Collection => CollectionBook.Complete(d, p, c.Value),
        FilterUnlock.Career => Career.Won(p, c.Value),
        FilterUnlock.Secret => c.Value >= 0 && c.Value < d.Secrets.Length && Secrets.Done(p, c.Value),
        FilterUnlock.Wins => p.Wins >= c.Value,
        _ => false,
    };

    public static bool Unlocked(GameData d, Profile p, int f)
    {
        if (f < 0 || f >= d.ScreenFilters.Length) return false;
        var def = d.ScreenFilters[f];
        return def.Kind != FilterKind.Fun || def.Unlock.Any(c => CondMet(d, p, c));
    }

    public static int UnlockedCount(GameData d, Profile p) => Enumerable.Range(0, d.ScreenFilters.Length).Count(f => Unlocked(d, p, f));

    /// <summary>Wybrany filtr (zablokowany albo spoza danych – klasyczny).</summary>
    public static int Selected(GameData d, Profile p) => Valid(d, p, p.Filter);

    /// <summary>Filtr f, jeśli odblokowany, inaczej klasyczny (Godot: wybór z ustawień urządzenia).</summary>
    public static int Valid(GameData d, Profile p, int f) => Unlocked(d, p, f) ? f : 0;

    /// <summary>Kolejny odblokowany filtr od f w kierunku dir (Wygląd, Ustawienia).</summary>
    public static int Next(GameData d, Profile p, int f, int dir)
    {
        var n = d.ScreenFilters.Length;
        f = Valid(d, p, f);
        for (var i = 0; i < n; i++)
        {
            f = (f + (dir < 0 ? -1 : 1) + n) % n;
            if (Unlocked(d, p, f)) break;
        }
        return f;
    }

    /// <summary>GBA: przełącza zapisany w profilu wybór (cycle_filter).</summary>
    public static void Cycle(GameData d, Profile p, int dir) => p.Filter = (byte)Next(d, p, p.Filter, dir);

    /// <summary>Odblokowane, a jeszcze nieogłoszone filtry zabawowe (baner „Nowy filtr ekranu” raz) – zwraca bity i zapamiętuje je.</summary>
    public static int Announce(GameData d, Profile p)
    {
        var got = 0;
        for (var f = 0; f < d.ScreenFilters.Length; f++)
        {
            if (d.ScreenFilters[f].Kind == FilterKind.Fun && Unlocked(d, p, f) && ((p.FiltersSeen >> f) & 1) == 0) got |= 1 << f;
        }
        p.FiltersSeen = (ushort)(p.FiltersSeen | got);
        return got;
    }

    /// <summary>Warunek odblokowania słowami (jak filter_unlock_label): „Inspektor 5”, „Kolekcja: Stan surowy”,
    /// „Wygrana: Kamienica”, „Sekret: …”, „10 wygranych”; dwa warunki – „… albo …”.</summary>
    public static string UnlockLabel(GameData d, int f)
    {
        var def = d.ScreenFilters[f];
        if (def.Kind == FilterKind.Access) return d.FilterText("alwaysAccess");
        if (def.Kind == FilterKind.Classic) return d.FilterText("always");
        return string.Join(d.FilterText("or"), def.Unlock.Select(c => c.Kind switch
        {
            FilterUnlock.Inspector => d.FilterText("inspector") + c.Value,
            FilterUnlock.Collection => d.FilterText("collection") + d.Collections[c.Value].Name,
            FilterUnlock.Career => d.FilterText("career") + d.Career[c.Value].Short,
            FilterUnlock.Secret => d.FilterText("secret") + d.Secrets[c.Value].Hint,
            _ => c.Value + d.FilterText("wins"),
        }));
    }

    /// <summary>v16 -> v17: filtr klasyczny; odblokowane już filtry czekają na baner jak nowe.</summary>
    public static void MigrateV17(Profile p)
    {
        p.Filter = 0;
        p.FiltersSeen = 0;
    }
}
