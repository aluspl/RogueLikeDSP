using Godot;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Screens;

/// <summary>
/// Maszyna stanów ekranów (pętla scen w main() na GBA): jeden bieżący ekran, przejście ustawia widoczność warstw
/// (mapa + HUD, telefon z rozmytym tłem, plansze), muzykę i woła Enter nowego ekranu.
/// v0.21.51: blokada wejścia - przez LockSeconds po otwarciu każdego okna / przejścia (poza mapą) i dopóki telefon
/// wjeżdża, Main ignoruje wciśnięcia i dotknięcia (stuknięcie w mapę nie wybiera od razu premii, nie pomija SMS-a).
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
        Help = new HelpScreen(app);
        Game = new GameScreen(app);
        Phone = new PhoneScreen(app);
        StageCard = new StageCardScreen(app);
        Schedule = new ScheduleScreen(app);
        Hurtownia = new HurtowniaScreen(app);
        Offer = new OfferScreen(app);
        EndMessage = new EndMessageScreen(app);
        End = new EndScreen(app);
        Settings = new SettingsScreen(app);
        Brigade = new BrigadeScreen(app);
        Investor = new InvestorScreen(app);
        Daily = new DailyScreen(app);
        Weekly = new WeeklyScreen(app);
        Recap = new RecapScreen(app);
        HouseSchedule = new HouseScheduleScreen(app);
        Stats = new StatsScreen(app);
        Boons = new BoonScreen(app);
        BoonList = new BoonListScreen(app);
        Event = new EventScreen(app);
        Trait = new TraitScreen(app);
        ToolOffer = new ToolOfferScreen(app);
        Career = new CareerScreen(app);
    }

    public Screen Current { get; private set; }

    /// <summary>Czas blokady wejścia po otwarciu okna (s).</summary>
    public const float LockSeconds = 0.4f;

    private ulong _lockUntil;

    /// <summary>Czy wejście gracza (wciśnięcia, dotknięcia) jest teraz ignorowane: świeżo otwarte okno albo animacja telefonu.</summary>
    public bool InputLocked => Current is not null && Current != Game
        && (Time.GetTicksMsec() < _lockUntil || _app.Nodes.Phone.Sliding);

    /// <summary>Blokada wejścia na seconds od teraz (przejście, animacja).</summary>
    public void LockInput(float seconds = LockSeconds) => _lockUntil = System.Math.Max(_lockUntil, Time.GetTicksMsec() + (ulong)(seconds * 1000));

    public TitleScreen Title { get; }
    public ProfileScreen Profile { get; }
    public ClassSelectScreen ClassSelect { get; }
    public PrologueScreen Prologue { get; }
    public PrologueMessageScreen PrologueMessage { get; }
    public HelpScreen Help { get; }
    public GameScreen Game { get; }
    public PhoneScreen Phone { get; }
    public StageCardScreen StageCard { get; }
    public ScheduleScreen Schedule { get; }
    public HurtowniaScreen Hurtownia { get; }
    public OfferScreen Offer { get; }
    public EndMessageScreen EndMessage { get; }
    public EndScreen End { get; }
    public SettingsScreen Settings { get; }
    public BrigadeScreen Brigade { get; }
    public InvestorScreen Investor { get; }
    public DailyScreen Daily { get; }
    /// <summary>v0.21.50 cz. 4: wyzwanie tygodnia (#34) i podsumowanie budowy (#33).</summary>
    public WeeklyScreen Weekly { get; }
    public RecapScreen Recap { get; }
    public HouseScheduleScreen HouseSchedule { get; }
    public StatsScreen Stats { get; }
    /// <summary>v0.21.50 cz. 2: premia 1 z 3 po etapie i lista premii w telefonie.</summary>
    public BoonScreen Boons { get; }
    public BoonListScreen BoonList { get; }
    /// <summary>v0.21.50 cz. 3: wydarzenie z wyborem, cecha ulepszonego narzędzia, nowe narzędzie przy ulepszonym.</summary>
    public EventScreen Event { get; }
    public TraitScreen Trait { get; }
    public ToolOfferScreen ToolOffer { get; }
    /// <summary>v0.21.52 cz. d (#47): mapa kariery przed wyborem zawodu.</summary>
    public CareerScreen Career { get; }

    /// <summary>Przejście na ekran; instant = bez animacji (telefon od razu na miejscu, tło od razu rozmyte).</summary>
    public void Go(Screen next, bool instant = false)
    {
        Current?.Exit();
        Current = next;
        if (next != Game) LockInput();
        var n = _app.Nodes;
        n.World.Visible = next.InRun;
        n.Hud.Visible = next.InRun;
        n.Touch.Visible = Layout.Touch && next == Game;
        n.Settings.Visible = next.ShowsSettings;
        n.Coach.Visible = false;   // samouczek: pokazuje go ekran-właściciel (Coach.Update)
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
