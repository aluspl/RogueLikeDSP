using LifeLike.Core;
namespace LifeLike.Game.Session;

/// <summary>
/// Zaślepka tabeli wyników: nic nie wysyła, tylko potwierdza (najlepsze wyniki dni są w profilu). Miejsce na
/// podłączenie Game Center / Google Play Games.
/// </summary>
public sealed class LocalLeaderboard : ILeaderboard
{
    public string Name => Loc.T("na_tym_urzadzeniu");

    /// <summary>Ostatnio „wysłany” wynik (dzień, wynik) – do testu dymnego.</summary>
    public (int Day, int Score) Last { get; private set; }

    public string Submit(int day, int score)
    {
        Last = (day, score);
        return Loc.F("wynik_zapisany_lokalnie_tabela", score);
    }

    /// <summary>Ostatnio „wysłany” wynik tygodnia (tabela, wynik) – do testu dymnego.</summary>
    public (string Board, int Score) LastWeekly { get; private set; }

    /// <summary>Zaślepka: identyfikator tabeli tygodnia (bez sieci; przyszła tabela Game Center / Google Play Games).</summary>
    public string WeeklyBoardId(int week) => $"pb.weekly.{week:000}";

    public string SubmitWeekly(int week, int score)
    {
        LastWeekly = (WeeklyBoardId(week), score);
        return Loc.F("wynik_tygodnia_zapisany", score, WeeklyBoardId(week));
    }
}
