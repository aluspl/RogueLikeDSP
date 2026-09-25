using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Hud;

/// <summary>
/// HUD w trakcie budowy (warstwa nad mapą): błyski ekranu, górny pasek (HP, poziom, etap, stany, termos, moc)
/// i dolny pas dziennika / podpowiedzi menu akcji. Układ jak na GBA; warstwa w skali Layout.HudScale
/// (większy tekst niż telefon i plansze), szerokość od rozmiaru ekranu.
/// </summary>
public partial class HudLayer : ScaledLayer
{
    public HudLayer() : base(() => Layout.HudScale)
    {
    }

    private readonly ScreenTint _tint = new();
    private readonly HudTop _top = new();
    private readonly HudLog _log = new();

    public ScreenTint Tint => _tint;

    public override void _Ready()
    {
        Layer = 1;
        Root.AddChild(_tint);
        Root.AddChild(_top);
        Root.AddChild(_log);
        base._Ready();
    }

    public void ShowGame(CoreGame g)
    {
        _top.SetGame(g);
        _log.SetGame(g);
        _tint.LowHp = g.Hero.Hp > 0 && g.Hero.Hp * 4 <= g.Hero.MaxHp;
    }

    /// <summary>Podpowiedź menu akcji w dolnym pasie; null chowa.</summary>
    public void ShowHint(string first, string second = "") => _log.Hint(first, second);

    public void SilenceLog() => _log.Silence();
}
