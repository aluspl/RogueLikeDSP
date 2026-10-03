using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.21.52 cz. d (#47): mapa kariery (meta.h: career_*) – kontrakty: Dom jednorodzinny zawsze, kolejne po wygranych albo
/// na poziomie inspektora; wybór przed wyborem zawodu (budowa dnia i tygodnia – zawsze Dom). Pierwsza wygrana kontraktu:
/// Respekt (raz), tytuł i kolor kasku (od wygranej). Profil v16: wybrany, ogłoszone, wygrane, wygrane i najlepszy etap.
/// </summary>
public static class Career
{
    public static bool Unlocked(GameData d, Profile p, int c)
    {
        if (c < 0 || c >= d.Career.Length) return false;
        var k = d.Career[c];
        return k.Unlock switch
        {
            CareerUnlock.Wins => p.Wins >= k.UnlockValue,
            CareerUnlock.Inspector => Progress.InspectorLevel(d, p) >= k.UnlockValue,
            _ => true,
        };
    }

    public static bool Won(Profile p, int c) => c >= 0 && c < Profile.MaxContracts && ((p.CareerDone >> c) & 1) != 0;

    public static int UnlockedCount(GameData d, Profile p) => Enumerable.Range(0, d.Career.Length).Count(c => Unlocked(d, p, c));

    public static int WonCount(GameData d, Profile p) => Enumerable.Range(0, d.Career.Length).Count(c => Won(p, c));

    /// <summary>Wybrany kontrakt (zablokowany albo spoza danych – Dom jednorodzinny).</summary>
    public static int Selected(GameData d, Profile p) => Unlocked(d, p, p.Contract) ? p.Contract : 0;

    /// <summary>Odblokowane, a jeszcze nieogłoszone kontrakty (baner „Nowy kontrakt” raz) – zwraca bity i zapamiętuje je.</summary>
    public static int Announce(GameData d, Profile p)
    {
        var got = 0;
        for (var c = 1; c < d.Career.Length; ++c)
        {
            if (Unlocked(d, p, c) && ((p.CareerSeen >> c) & 1) == 0) got |= 1 << c;
        }
        p.CareerSeen = (byte)(p.CareerSeen | got);
        return got;
    }

    /// <summary>Ukończone etapy kontraktu w tej budowie (bez Aktu 0; NG+ – do końca budynku).</summary>
    public static int StagesDone(Game g)
    {
        if (g.Tier > 0 || g.St == GameStatus.Won) return g.RouteCount() - g.PreludeCount();
        var to = g.St == GameStatus.StageClear ? g.Stage + 1 : g.Stage;
        return Math.Max(0, to - Math.Max(g.FirstStage, g.PreludeCount()));
    }

    /// <summary>Koniec etapu / budowy: najlepszy wynik kontraktu (można wołać wielokrotnie).</summary>
    public static void Record(Profile p, Game g)
    {
        var c = g.Contract;
        if (c < 0 || c >= Profile.MaxContracts) return;
        p.CareerBest[c] = (byte)Math.Max(p.CareerBest[c], Math.Min(255, StagesDone(g)));
    }

    /// <summary>
    /// Wygrana budowa w kontrakcie (raz na wygraną, obok Meta.RecordWin). Zwraca true przy pierwszej wygranej kontraktu
    /// (Respekt z nagrody już w profilu; tytuł i kask – od teraz).
    /// </summary>
    public static bool Win(GameData d, Profile p, Game g)
    {
        var c = g.Contract;
        if (c < 0 || c >= d.Career.Length) return false;
        Record(p, g);
        p.CareerWins[c] = Meta.AddSat8(p.CareerWins[c], 1);
        if (Won(p, c)) return false;
        p.CareerDone = (byte)(p.CareerDone | (1 << c));
        var r = d.Career[c].Respect;
        if (r > 0)
        {
            p.Respect = Meta.AddSat16(p.Respect, r);
            p.RespectTotal = Meta.AddSat16(p.RespectTotal, r);
        }
        return true;
    }

    /// <summary>Nagroda za pierwszą wygraną słowami: „Respekt +20, Letnik, Sosnowy kask”.</summary>
    public static string RewardLabel(GameData d, int c)
    {
        var k = d.Career[c];
        var parts = new List<string>();
        if (k.Respect > 0) parts.Add($"Respekt +{k.Respect}");
        if (k.Title != "") parts.Add(k.Title);
        if (k.Helmet >= 0) parts.Add(d.Cosmetics[k.Helmet].Name);
        return string.Join(", ", parts);
    }

    /// <summary>Warunek odblokowania słowami: „1 wygrana”, „3 wygrane”, „Inspektor 8”.</summary>
    public static string UnlockLabel(GameData d, int c)
    {
        var k = d.Career[c];
        if (k.Unlock == CareerUnlock.Wins)
        {
            var v = k.UnlockValue;
            var few = v % 10 is >= 2 and <= 4 && (v % 100 < 10 || v % 100 >= 20);
            return v + (v == 1 ? " wygrana" : few ? " wygrane" : " wygranych");
        }
        return k.Unlock == CareerUnlock.Inspector ? $"Inspektor {k.UnlockValue}" : "";
    }

    /// <summary>
    /// v15 -> v16: Dom jednorodzinny z dotychczasowych wygranych (wygrany, gdy była wygrana; najlepszy etap – cała budowa).
    /// Kontrakty już odblokowane czekają na baner jak nowe.
    /// </summary>
    public static void MigrateV16(GameData d, Profile p)
    {
        p.Contract = 0;
        p.CareerSeen = 0;
        p.CareerDone = 0;
        Array.Clear(p.CareerWins);
        Array.Clear(p.CareerBest);
        if (p.Wins <= 0) return;
        p.CareerWins[0] = (byte)Math.Min(255, p.Wins);
        p.CareerBest[0] = (byte)(d.Career[0].Count - d.Career[0].Prelude);
        p.CareerDone = 1;
    }
}
