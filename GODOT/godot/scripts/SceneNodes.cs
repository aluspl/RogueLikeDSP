using Godot;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Hud;
using LifeLike.Game.Phone;
using LifeLike.Game.Screens.Views;
using LifeLike.Game.Touch;
using LifeLike.Game.World;

namespace LifeLike.Game;

/// <summary>
/// Drzewo węzłów prezentacji (warstwy od dołu): mapa etapu, HUD (1), plansze pełnoekranowe (2),
/// telefon na rozmytym tle (3), powiadomienia push i klucz ustawień (4); przy dotyku pasek akcji nad HUD (1).
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
        EndView = new EndView { Visible = false };
        PrologueView = new PrologueView { Visible = false };
        screens.AddChild(TitleView);
        screens.AddChild(ClassSelectView);
        screens.AddChild(EndView);
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
        BannerLayer.Root.AddChild(Banners);
        Settings = new SettingsButton { Visible = false };
        BannerLayer.Root.AddChild(Settings);
    }

    /// <summary>
    /// Banery: przy telefonie wąska kolumna obok niego (pionowo: na górze aplikacji), na mapie w prawym górnym rogu
    /// pod paskiem HUD.
    /// </summary>
    public void SetBannerMode(bool compact, bool underHud)
    {
        Banners.Compact = compact && !PhoneView.Full;
        Banners.TopInset = Layout.SafeTop + (underHud && !compact ? Mathf.Ceil((HudTop.Height + 1) * Hud.UiScale) + 4 : 6);
    }

    public WorldView World { get; }
    public TouchControls Touch { get; }
    public SettingsButton Settings { get; }
    public HudLayer Hud { get; }
    public TitleView TitleView { get; }
    public ClassSelectView ClassSelectView { get; }
    public EndView EndView { get; }
    public PrologueView PrologueView { get; }
    public Backdrop Backdrop { get; }
    public PhoneView Phone { get; }
    public PushBanners Banners { get; }
    public ScaledLayer BannerLayer { get; }
}
