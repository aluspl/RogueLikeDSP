using LifeLike.Game.Input;
using LifeLike.Game.Session;

namespace LifeLike.Game.Screens;

/// <summary>
/// Ekran gry (odpowiednik sceny run_* w GBA/src/main.cpp): deklaruje, które warstwy są widoczne (mapa z HUD,
/// telefon na rozmytym tle, plansze), muzykę, a wejście i logikę przejść obsługuje sam. Rysują widoki (węzły),
/// ekran je tylko ustawia. Przełącza ScreenFlow.Go.
/// </summary>
public abstract class Screen
{
    protected Screen(App app) => App = app;

    protected App App { get; }
    protected GameSession S => App.Session;
    protected SceneNodes N => App.Nodes;
    protected ScreenFlow Flow => App.Flow;

    /// <summary>Plansze pełnoekranowe widoczne na tym ekranie.</summary>
    public virtual ViewSet Views => ViewSet.None;

    /// <summary>Mapa etapu z HUD pod spodem (w trakcie budowy).</summary>
    public virtual bool InRun => false;

    /// <summary>Telefon na rozmytym i przyciemnionym tle.</summary>
    public virtual bool UsesPhone => false;

    /// <summary>Muzyka: "title", "game" albo "" (cisza).</summary>
    public virtual string Music => "game";

    /// <summary>Znacznik celu krótkiego A nad najbliższym wrogiem w zasięgu.</summary>
    public virtual bool ShowsTarget => false;

    /// <summary>Wejście na ekran (instant = bez animacji wjazdu, np. zrzuty ekranu).</summary>
    public virtual void Enter(bool instant)
    {
    }

    /// <summary>Wyjście z ekranu (przed wejściem na kolejny).</summary>
    public virtual void Exit()
    {
    }

    /// <summary>Wejście gracza; true = obsłużone.</summary>
    public virtual bool HandleInput(InputCmd e) => false;

    public virtual void Process(double delta)
    {
    }
}
