using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// v0.23.2 (UI 2): pauza w trakcie budowy (Esc, przycisk pauzy w prawym górnym rogu mapy) – gra stoi, mapa przyciemniona,
/// duże przyciski: Wznów, Postać, Budowa (telefon), Ustawienia, Wyjdź do menu (zapis budowy, z potwierdzeniem).
/// Postać, Budowa i Ustawienia wracają tu po zamknięciu.
/// </summary>
public sealed class PauseScreen : Screen
{
    public PauseScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public PausePage Page { get; private set; }

    public void Open(bool instant = false)
    {
        Flow.Game.Menu.Close();
        Flow.Game.Touch.Reset();
        N.Banners.Clear();
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        Page = new PausePage(S.Game) { Picked = Pick };
        N.Phone.OpenSingle(Page, -1, true);
        Sfx.Play("menu");
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.IsBack && !e.Is(GameAction.Select)) return false;
        Pick(PausePage.Resume);
        return true;
    }

    /// <summary>Przycisk pauzy (dotknięcie, A / Enter).</summary>
    public void Pick(int k)
    {
        switch (k)
        {
            case PausePage.Resume:
                Flow.Game.Open();
                App.Refresh();
                break;
            case PausePage.Character:
                Flow.Character.Open(this);
                break;
            case PausePage.Site:
                Flow.Phone.Open(-1, false, this);
                break;
            case PausePage.Settings:
                Flow.Settings.Open(this);
                break;
            case PausePage.Quit:
                Page.Confirm(true);
                Sfx.Play("menu");
                break;
            case PausePage.QuitNo:
                Page.Confirm(false);
                break;
            case PausePage.QuitYes:
                S.SaveRun();
                S.Note = Loc.T("budowa_zapisana_kontynuuj");
                Flow.Title.Open();
                break;
        }
    }
}
