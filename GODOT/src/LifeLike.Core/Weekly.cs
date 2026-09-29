using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Wyzwanie tygodnia (#34, port z GBA/include/meta.h): tydzień nr 1 zaczyna się w poniedziałek GameData.WeeklyEpoch; seed
/// z numeru tygodnia, zasady z listy po kolei (zawód, bez kawy, elity, pogoda, bez Hurtowni, materiały, HP, ciosy, budżet),
/// bez Szkoleń i pamiątek – równo dla wszystkich; najlepsze wyniki ostatnich tygodni w profilu. GBA: data z ekranu budowy
/// dnia, Godot: z systemu.
/// </summary>
public static class Weekly
{
    public static int Number(GameData d, int y, int m, int day)
    {
        var t = Daily.DaysFromCivil(y, m, day) - Daily.DaysFromCivil(d.WeeklyEpoch[0], d.WeeklyEpoch[1], d.WeeklyEpoch[2]);
        return Math.Max(1, (t >= 0 ? t / 7 : -((-t + 6) / 7)) + 1);
    }

    /// <summary>Poniedziałek tygodnia (dni od 1970-01-01).</summary>
    public static int FirstDay(GameData d, int week) =>
        Daily.DaysFromCivil(d.WeeklyEpoch[0], d.WeeklyEpoch[1], d.WeeklyEpoch[2]) + (week - 1) * 7;

    public static int Index(GameData d, int week) => ((week - 1) % d.Weekly.Length + d.Weekly.Length) % d.Weekly.Length;

    public static uint Seed(int week)
    {
        var h = (uint)week * 2246822519u + 0x85EBCA6Bu;
        h ^= h >> 15;
        h *= 2654435761u;
        h ^= h >> 13;
        return h != 0 ? h : 1u;
    }

    public static int RuleValue(GameData d, int wi, WeeklyRule w, int fallback)
    {
        foreach (var r in d.Weekly[wi].Rules)
        {
            if (r.Rule == w) return r.Value;
        }
        return fallback;
    }

    /// <summary>Zawód tygodnia: z zasady albo z seeda (jak budowa dnia).</summary>
    public static int ClassOf(GameData d, int week) => RuleValue(d, Index(d, week), WeeklyRule.Cls, Daily.ClassOf(d, Seed(week)));

    /// <summary>Premie z zasad (obrażenia %, budżet, HP %) – bez meta-progresji.</summary>
    public static RunMods Mods(GameData d, int week)
    {
        var wi = Index(d, week);
        var m = RunMods.Default(d);
        m.Weekly = wi;
        m.DmgPct = RuleValue(d, wi, WeeklyRule.DmgPct, 0);
        m.Cash = RuleValue(d, wi, WeeklyRule.Cash, 0);
        m.Hp = d.Classes[ClassOf(d, week)].MaxHealth * RuleValue(d, wi, WeeklyRule.HpPct, 0) / 100;
        return m;
    }

    public static void Start(Game g, int week)
    {
        g.NewRun(ClassOf(g.D, week), Seed(week), g.D.WeeklyDifficulty, Mods(g.D, week));
        g.WeeklyWeek = (ushort)week;
    }

    /// <summary>Najlepszy wynik tygodnia (-1 = brak).</summary>
    public static int Best(GameData d, Profile p, int week)
    {
        for (var i = 0; i < d.WeeklyHistory; ++i)
        {
            if (p.WeeklyWeek[i] == week && week > 0) return p.WeeklyScore[i];
        }
        return -1;
    }

    public static bool Won(GameData d, Profile p, int week)
    {
        for (var i = 0; i < d.WeeklyHistory; ++i)
        {
            if (p.WeeklyWeek[i] == week && week > 0) return ((p.WeeklyWon >> i) & 1) != 0;
        }
        return false;
    }

    /// <summary>Wynik wyzwania: najlepszy tygodnia zostaje, nowy tydzień zastępuje najstarszy. true = nowy rekord tygodnia.</summary>
    public static bool Record(GameData d, Profile p, int week, int score, bool won)
    {
        if (p.WeeklyRuns < 255) ++p.WeeklyRuns;
        int slot = -1, oldest = 0;
        for (var i = 0; i < d.WeeklyHistory; ++i)
        {
            if (p.WeeklyWeek[i] == week)
            {
                slot = i;
                break;
            }
            if (p.WeeklyWeek[i] < p.WeeklyWeek[oldest]) oldest = i;
        }
        if (slot < 0)
        {
            slot = oldest;
            p.WeeklyWeek[slot] = (ushort)week;
            p.WeeklyScore[slot] = score;
            p.WeeklyWon = (byte)((p.WeeklyWon & ~(1 << slot)) | (won ? 1 << slot : 0));
            return true;
        }
        if (won) p.WeeklyWon = (byte)(p.WeeklyWon | (1 << slot));
        if (score <= p.WeeklyScore[slot]) return false;
        p.WeeklyScore[slot] = score;
        return true;
    }
}
