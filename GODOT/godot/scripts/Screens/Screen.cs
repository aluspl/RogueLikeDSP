using Godot;
using LifeLike.Game.Input;
using LifeLike.Game.Session;
using LifeLike.Game.Touch;

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

    /// <summary>Klucz ustawień w prawym górnym rogu (tytuł, mapa).</summary>
    public virtual bool ShowsSettings => false;

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

    /// <summary>
    /// Gest dotyku (sterowanie dotykiem). Domyślnie: telefon obsługuje swoje przyciski, zakładki i listy;
    /// dotknięcie = klik w punkcie (HandleInput z IsTap), przesunięcie = strzałka jak przewijanie na telefonie
    /// (palec w lewo = następny w prawo, palec w górę = niżej na liście). true = obsłużone.
    /// </summary>
    public virtual bool HandleGesture(in Gesture g)
    {
        if (UsesPhone && N.Phone.IsOpen)
        {
            if (N.Phone.HandleGesture(g)) return true;
            if (g.IsStep) return true; // przesunięcia na telefonie tylko przełączają zakładki / listy
        }
        if (g.Kind == GestureKind.Tap) return HandleInput(InputCmd.Tap(g.Pos));
        if (!g.IsStep) return false;
        var a = ScrollAction(g.Dir);
        if (a == GameAction.None) return false;
        var done = HandleInput(InputCmd.Of(a));
        HandleInput(InputCmd.Release(a));
        return done;
    }

    /// <summary>Kierunek przesunięcia palca -> strzałka w stylu przewijania (odwrotnie do ruchu palca).</summary>
    protected static GameAction ScrollAction(Vector2I dir) =>
        dir.X < 0 ? GameAction.Right : dir.X > 0 ? GameAction.Left : dir.Y < 0 ? GameAction.Down : dir.Y > 0 ? GameAction.Up : GameAction.None;
}
