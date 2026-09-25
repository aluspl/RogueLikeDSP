using Godot;
using LifeLike.Game.Audio;
using LifeLike.Game.Hud;
using LifeLike.Game.Phone;
using LifeLike.Game.Screens.Views;
using LifeLike.Game.World;

namespace LifeLike.Game;

/// <summary>
/// Drzewo węzłów prezentacji (warstwy od dołu): mapa etapu, HUD (1), plansze pełnoekranowe (2),
/// telefon na rozmytym tle (3), powiadomienia push (4). Ekrany tylko je pokazują i ustawiają.
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

        var screens = new CanvasLayer { Layer = 2 };
        root.AddChild(screens);
        TitleView = new TitleView { Visible = false, Version = version };
        ClassSelectView = new ClassSelectView { Visible = false };
        EndView = new EndView { Visible = false };
        screens.AddChild(TitleView);
        screens.AddChild(ClassSelectView);
        screens.AddChild(EndView);

        var phoneLayer = new CanvasLayer { Layer = 3 };
        root.AddChild(phoneLayer);
        Backdrop = new Backdrop();
        Phone = new PhoneView();
        phoneLayer.AddChild(Backdrop);
        phoneLayer.AddChild(Phone);

        var top = new CanvasLayer { Layer = 4 };
        root.AddChild(top);
        Banners = new PushBanners();
        top.AddChild(Banners);
    }

    public WorldView World { get; }
    public HudLayer Hud { get; }
    public TitleView TitleView { get; }
    public ClassSelectView ClassSelectView { get; }
    public EndView EndView { get; }
    public Backdrop Backdrop { get; }
    public PhoneView Phone { get; }
    public PushBanners Banners { get; }
}
