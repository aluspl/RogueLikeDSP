using Godot;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Hud;
using LifeLike.Game.Phone;
using LifeLike.Game.Screens.Views;
using LifeLike.Game.Touch;
using LifeLike.Game.Guide;
using LifeLike.Game.World;

namespace LifeLike.Game;

/// <summary>
/// Drzewo węzłów prezentacji (warstwy od dołu): mapa etapu, HUD (1), plansze pełnoekranowe (2),
/// telefon na rozmytym tle (3), powiadomienia push i klucz ustawień (4), dymek samouczka menu (5); przy dotyku pasek
/// akcji nad HUD (1); filtr ekranu (v0.21.53) nad planszami (2) albo nad wszystkim (6).
/// Ekrany tylko je pokazują i ustawiają.
/// </summary>
public sealed class SceneNodes
{
    public SceneNodes(Node root, bool sound, string version)
    {
        if (sound) root.AddChild(new Sfx()); // test dymny i zrzuty bez dźwięku (Sfx.Play jest wtedy pusty)
        World = new WorldView();
        root.AddChild(World);
        Hud = new HudLayer();
        root.AddChild(Hud);
        World.OnFlash = (c, a) => Hud.Tint.Flash(c, a);
        Touch = new TouchControls();
        root.AddChild(Touch);

        var screens = new CanvasLayer { Layer = 2 };
        root.AddChild(screens);
        TitleView = new TitleView { Visible = false, Version = version };
        ClassSelectView = new ClassSelectView { Visible = false };
        PrologueView = new PrologueView { Visible = false };
        screens.AddChild(TitleView);
        screens.AddChild(ClassSelectView);
        screens.AddChild(PrologueView);

        var phoneLayer = new CanvasLayer { Layer = 3 };
        root.AddChild(phoneLayer);
        Backdrop = new Backdrop();
        Phone = new PhoneView();
        phoneLayer.AddChild(Backdrop);
        phoneLayer.AddChild(Phone);

        Banners = new PushBanners();
        BannerLayer = new ScaledLayer { Layer = 4 };
        root.AddChild(BannerLayer);
        DogBubble = new DogBubble { Visible = false }; // v0.23.0: dymek Kawki nad mapą (pod ogłoszeniami)
        BannerLayer.Root.AddChild(DogBubble);
        DogBubble.Bind(World);
        Announce = new AnnounceView { Visible = false }; // v0.23.0: ogłoszenia „z przytupem” (pod banerami)
        BannerLayer.Root.AddChild(Announce);
        BannerLayer.Root.AddChild(Banners);
        Settings = new SettingsButton { Visible = false };
        BannerLayer.Root.AddChild(Settings);

        var coachLayer = new CanvasLayer { Layer = 5 };   // samouczek menu: nad wszystkim
        root.AddChild(coachLayer);
        Coach = new CoachView { Visible = false };
        coachLayer.AddChild(Coach);

        Filter = new ScreenFilter(); // v0.21.53: filtr ekranu - ostatni węzeł (nad planszami albo nad wszystkim)
        root.AddChild(Filter);
    }

    /// <summary>
    /// Banery: przy telefonie wąska kolumna obok niego, na mapie w prawym górnym rogu pod paskiem HUD.
    /// v0.22.0 (UX A): telefon na cały ekran – od dołu nad przyciskami; tytuł – pod statystykami i kluczem.
    /// </summary>
    public void SetBannerMode(BannerSpot spot, bool compact, bool underHud)
    {
        Banners.OnPhone = compact;
        Banners.Spot = spot;
        Banners.TopInset = spot == BannerSpot.BelowTitleStats ? TitleView.StatsBottom + 4
            : Layout.SafeTop + (underHud && !compact ? Mathf.Ceil((HudTop.Height + 1) * Hud.UiScale) + 4 : 6);
    }

    public WorldView World { get; }
    public TouchControls Touch { get; }
    public SettingsButton Settings { get; }
    public HudLayer Hud { get; }
    public TitleView TitleView { get; }
    public ClassSelectView ClassSelectView { get; }
    public PrologueView PrologueView { get; }
    public Backdrop Backdrop { get; }
    public PhoneView Phone { get; }
    public PushBanners Banners { get; }
    public AnnounceView Announce { get; }
    public DogBubble DogBubble { get; }
    public CoachView Coach { get; }
    public ScaledLayer BannerLayer { get; }
    public ScreenFilter Filter { get; }
}
