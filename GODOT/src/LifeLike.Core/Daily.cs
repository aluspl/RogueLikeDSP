using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Codzienna budowa (port z GBA/include/meta.h): seed z daty, zawód i modyfikatory dnia (dla wszystkich takie same),
/// najlepsze wyniki ostatnich dni w profilu. GBA nie ma zegara – data wpisana ręcznie (pamiętana w profilu); Godot bierze
/// datę z systemu.
/// </summary>
public static class Daily
{
    public static bool LeapYear(int y) => (y % 4 == 0 && y % 100 != 0) || y % 400 == 0;

    public static int DaysInMonth(int y, int m)
    {
        int[] dm = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        return m == 2 && LeapYear(y) ? 29 : dm[(m - 1) % 12];
    }

    /// <summary>Dni od 1970-01-01 (kalendarz gregoriański; algorytm days_from_civil).</summary>
    public static int DaysFromCivil(int y, int m, int d)
    {
        y -= m <= 2 ? 1 : 0;
        var era = (y >= 0 ? y : y - 399) / 400;
        var yoe = y - era * 400;
        var doy = (153 * (m + (m > 2 ? -3 : 9)) + 2) / 5 + d - 1;
        var doe = yoe * 365 + yoe / 4 - yoe / 100 + doy;
        return era * 146097 + doe - 719468;
    }

    public static (int Y, int M, int D) CivilFromDays(int z)
    {
        z += 719468;
        var era = (z >= 0 ? z : z - 146096) / 146097;
        var doe = z - era * 146097;
        var yoe = (doe - doe / 1460 + doe / 36524 - doe / 146096) / 365;
        var doy = doe - (365 * yoe + yoe / 4 - yoe / 100);
        var mp = (5 * doy + 2) / 153;
        var d = doy - (153 * mp + 2) / 5 + 1;
        var m = mp < 10 ? mp + 3 : mp - 9;
        return (yoe + era * 400 + (m <= 2 ? 1 : 0), m, d);
    }

    /// <summary>Numer „Budowy dnia” (1 = GameData.DailyEpoch).</summary>
    public static int Number(GameData d, int y, int m, int day) =>
        DaysFromCivil(y, m, day) - DaysFromCivil(d.DailyEpoch[0], d.DailyEpoch[1], d.DailyEpoch[2]) + 1;

    public static uint Seed(int day)
    {
        var h = (uint)day * 2654435761u + 0x9E3779B9u;
        h ^= h >> 16;
        h *= 2246822519u;
        h ^= h >> 13;
        return h != 0 ? h : 1u;
    }

    public static int ClassOf(GameData d, uint seed) => (int)(seed % (uint)d.Classes.Length);

    /// <summary>Modyfikatory dnia (tryb inwestora): DailyInvestorMods różnych bitów z seeda.</summary>
    public static int InvestorOf(GameData d, uint seed)
    {
        var mask = 0;
        var h = seed;
        for (var k = 0; k < d.DailyInvestorMods; ++k)
        {
            h = h * 1664525u + 1013904223u;
            var i = (int)((h >> 16) % (uint)d.Investor.Length);
            for (var guard = 0; ((mask >> i) & 1) != 0 && guard < d.Investor.Length; ++guard) i = (i + 1) % d.Investor.Length;
            mask |= 1 << i;
        }
        return mask;
    }

    /// <summary>Codzienna budowa jest równa dla wszystkich: bez Szkoleń, odznak i pamiątek, tylko modyfikatory dnia.</summary>
    public static RunMods Mods(GameData d, uint seed)
    {
        var m = RunMods.Default(d);
        m.Investor = InvestorOf(d, seed);
        m.XpPct = Investor.Xp(d, m.Investor);
        return m;
    }

    public static void Start(Game g, int day)
    {
        var seed = Seed(day);
        g.NewRun(ClassOf(g.D, seed), seed, g.D.DailyDifficulty, Mods(g.D, seed));
        g.Daily = true;
        g.DailyDay = (ushort)day;
    }

    /// <summary>Data codziennej budowy z profilu (0 = domyślna z danych); dzień obcięty do długości miesiąca.</summary>
    public static (int Y, int M, int D) DateOf(GameData d, Profile p)
    {
        if (p.DailyY == 0 || p.DailyM is < 1 or > 12 || p.DailyD < 1) return (d.DailyDefaultDate[0], d.DailyDefaultDate[1], d.DailyDefaultDate[2]);
        return (p.DailyY, p.DailyM, Math.Min(p.DailyD, DaysInMonth(p.DailyY, p.DailyM)));
    }

    public static void SetDate(Profile p, int y, int m, int day)
    {
        p.DailyY = (ushort)y;
        p.DailyM = (byte)m;
        p.DailyD = (byte)day;
    }

    /// <summary>Najlepszy wynik dnia (-1 = brak).</summary>
    public static int Best(GameData d, Profile p, int day)
    {
        for (var i = 0; i < d.DailyHistory; ++i)
        {
            if (p.DailyDay[i] == day && day > 0) return p.DailyScore[i];
        }
        return -1;
    }

    public static bool Won(GameData d, Profile p, int day)
    {
        for (var i = 0; i < d.DailyHistory; ++i)
        {
            if (p.DailyDay[i] == day && day > 0) return ((p.DailyWon >> i) & 1) != 0;
        }
        return false;
    }

    /// <summary>Wynik codziennej budowy: najlepszy dnia zostaje; nowy dzień zastępuje najstarszy. true = nowy rekord dnia.</summary>
    public static bool Record(GameData d, Profile p, int day, int score, bool won)
    {
        if (p.DailyRuns < 255) ++p.DailyRuns;
        int slot = -1, oldest = 0;
        for (var i = 0; i < d.DailyHistory; ++i)
        {
            if (p.DailyDay[i] == day)
            {
                slot = i;
                break;
            }
            if (p.DailyDay[i] < p.DailyDay[oldest]) oldest = i;
        }
        if (slot < 0)
        {
            slot = oldest;
            p.DailyDay[slot] = (ushort)day;
            p.DailyScore[slot] = score;
            p.DailyWon = (byte)((p.DailyWon & ~(1 << slot)) | (won ? 1 << slot : 0));
            return true;
        }
        if (won) p.DailyWon = (byte)(p.DailyWon | (1 << slot));
        if (score <= p.DailyScore[slot]) return false;
        p.DailyScore[slot] = score;
        return true;
    }
}
