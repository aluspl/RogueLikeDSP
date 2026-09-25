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
}
