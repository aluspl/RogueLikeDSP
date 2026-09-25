using LifeLike.Game.Audio;

namespace LifeLike.Game.Screens;

/// <summary>
/// Maszyna stanów ekranów (pętla scen w main() na GBA): jeden bieżący ekran, przejście ustawia widoczność warstw
/// (mapa + HUD, telefon z rozmytym tłem, plansze), muzykę i woła Enter nowego ekranu.
/// </summary>
public sealed class ScreenFlow
{
    private readonly App _app;

    public ScreenFlow(App app)
    {
        _app = app;
        Title = new TitleScreen(app);
        Profile = new ProfileScreen(app);
        ClassSelect = new ClassSelectScreen(app);
        Prologue = new PrologueScreen(app);
        PrologueMessage = new PrologueMessageScreen(app);
        Game = new GameScreen(app);
        Phone = new PhoneScreen(app);
        StageCard = new StageCardScreen(app);
        Schedule = new ScheduleScreen(app);
        Hurtownia = new HurtowniaScreen(app);
        Offer = new OfferScreen(app);
        EndMessage = new EndMessageScreen(app);
        End = new EndScreen(app);
    }

    public Screen Current { get; private set; }

    public TitleScreen Title { get; }
    public ProfileScreen Profile { get; }
    public ClassSelectScreen ClassSelect { get; }
    public PrologueScreen Prologue { get; }
    public PrologueMessageScreen PrologueMessage { get; }
    public GameScreen Game { get; }
    public PhoneScreen Phone { get; }
    public StageCardScreen StageCard { get; }
    public ScheduleScreen Schedule { get; }
    public HurtowniaScreen Hurtownia { get; }
    public OfferScreen Offer { get; }
    public EndMessageScreen EndMessage { get; }
    public EndScreen End { get; }

    /// <summary>Przejście na ekran; instant = bez animacji (telefon od razu na miejscu, tło od razu rozmyte).</summary>
    public void Go(Screen next, bool instant = false)
    {
        Current?.Exit();
        Current = next;
        var n = _app.Nodes;
        n.World.Visible = next.InRun;
        n.Hud.Visible = next.InRun;
        n.TitleView.Visible = (next.Views & ViewSet.Title) != 0;
        n.ClassSelectView.Visible = (next.Views & ViewSet.ClassSelect) != 0;
        n.EndView.Visible = (next.Views & ViewSet.End) != 0;
        n.PrologueView.Visible = (next.Views & ViewSet.Prologue) != 0;
        n.Backdrop.SetOn(next.UsesPhone, instant);
        n.SetBannerMode(next.UsesPhone, next.InRun);
        if (!next.UsesPhone) n.Phone.Close();
        n.Hud.ShowHint(null);
        Sfx.Music(next.Music);
        next.Enter(instant);
    }
}
