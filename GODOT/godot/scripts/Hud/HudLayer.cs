using Godot;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Hud;

/// <summary>
/// HUD w trakcie budowy (warstwa nad mapą): pogoda dnia (deszcz, śnieg, wiatr, upał), błyski ekranu, górny pasek (HP, poziom, etap, stany, termos, moc)
/// i dolny pas dziennika / podpowiedzi menu akcji. Układ jak na GBA; warstwa w skali Layout.HudScale
/// (większy tekst niż telefon i plansze), szerokość od rozmiaru ekranu.
/// </summary>
public partial class HudLayer : ScaledLayer
{
    public HudLayer() : base(() => Layout.HudScale)
    {
    }

    private readonly WeatherOverlay _weather = new();
    private readonly ScreenTint _tint = new();
    private readonly HudTop _top = new();
    private readonly HudLog _log = new();
    private readonly EnemyCard _card = new();
    private readonly AttackTip _tip = new();

    public ScreenTint Tint => _tint;

    public override void _Ready()
    {
        Layer = 1;
        Root.AddChild(_weather);
        Root.AddChild(_tint);
        Root.AddChild(_top);
        Root.AddChild(_log);
        Root.AddChild(_card);
        Root.AddChild(_tip);
        base._Ready();
        Layout.Changed += Place;
        Place();
    }

    public override void _ExitTree()
    {
        Layout.Changed -= Place;
        base._ExitTree();
    }

    /// <summary>Górny pasek pod wyspą / wycięciem, dziennik i karta wroga nad paskiem akcji i paskiem domowym.</summary>
    private void Place()
    {
        var s = Layout.HudScale;
        _top.OffsetTop = Mathf.Round(Layout.SafeTop / s);
        _log.OffsetBottom = -Mathf.Round(Layout.BottomReserve / s);
        _card.OffsetBottom = _log.OffsetBottom;
        foreach (var c in new Control[] { _top, _log, _card })
        {
            c.OffsetLeft = Mathf.Round(Layout.SafeLeft / s);
            c.OffsetRight = -Mathf.Round(Layout.SafeRight / s);
        }
    }

    public void ShowGame(CoreGame g)
    {
        _weather.SetGame(g);
        _top.SetGame(g);
        _log.SetGame(g);
        _tint.LowHp = g.Hero.Hp > 0 && g.Hero.Hp * 4 <= g.Hero.MaxHp;
    }

    /// <summary>Podpowiedź menu akcji w dolnym pasie; null chowa.</summary>
    public void ShowHint(string first, string second = "") => _log.Hint(first, second);

    public void SilenceLog() => _log.Silence();

    /// <summary>Karta wroga w miejscu dziennika (podgląd pod B); enemy -1 = nikogo w polu widzenia.</summary>
    public void ShowEnemyCard(CoreGame g, int enemy, int index, int count)
    {
        _card.Show(g, enemy, index, count);
        _log.Visible = false;
    }

    /// <summary>v0.23.0: karta problemu (test dymny: wiersz mnożnika ciosu).</summary>
    public EnemyCard Card => _card;

    /// <summary>v0.23.0: dymek opisu ataku (Cios / Fach).</summary>
    public AttackTip Tip => _tip;

    /// <summary>v0.23.0: opis ataku nad prostokątem w układzie węzła from (np. przycisk paska akcji).</summary>
    public void ShowAttackTip(CoreGame g, bool modeB, Rect2 anchor, CanvasItem from) => _tip.ShowFor(g, modeB, anchor, from);

    /// <summary>v0.23.0: opis Fachu pod polem broni w górnym pasku (mysz, pad LB bez paska dotykowego).</summary>
    public void ShowAttackTipAtHud(CoreGame g, bool modeB) =>
        _tip.ShowFor(g, modeB, _top.WeaponRect, _top);

    /// <summary>v0.23.0: czy mysz jest nad polem broni w górnym pasku.</summary>
    public bool MouseOverWeapon() => _top.Visible && _top.WeaponRect.Size.X > 0 && _top.WeaponRect.HasPoint(_top.GetLocalMousePosition());

    public void HideAttackTip() => _tip.HideTip();

    public void HideEnemyCard()
    {
        _card.Visible = false;
        _log.Visible = true;
    }
}
