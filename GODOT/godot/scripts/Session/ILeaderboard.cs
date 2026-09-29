namespace LifeLike.Game.Session;

/// <summary>
/// Tabela wyników codziennej budowy. Na razie tylko lokalna zaślepka (LocalLeaderboard); później Game Center /
/// Google Play Games (TODO #10) – bez kodu sieciowego w grze.
/// </summary>
public interface ILeaderboard
{
    /// <summary>Nazwa usługi w komunikatach („Na tym urządzeniu”, „Game Center”).</summary>
    string Name { get; }

    /// <summary>Wysłanie wyniku dnia; zwraca komunikat dla gracza.</summary>
    string Submit(int day, int score);

    /// <summary>Identyfikator tabeli wyzwania tygodnia (#34) – miejsce na tabelę Game Center / Google Play Games.</summary>
    string WeeklyBoardId(int week);

    /// <summary>Wysłanie wyniku tygodnia; zwraca komunikat dla gracza.</summary>
    string SubmitWeekly(int week, int score);
}
