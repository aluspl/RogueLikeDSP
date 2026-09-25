using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Nakładki na podłogę pod postaciami: pulsujące pola zapowiedzianego ciosu bossa (czerwona ramka z kreskami),
/// ramki pól w zasięgu broni (mignięcie, gdy atak nie ma celu), poświata schodów (pulsowanie palety na GBA)
/// i kałuże w deszczu (wejście = poślizg).
/// </summary>
public partial class OverlayLayer : Node2D
{
    private CoreGame _g;
    private float _clock;
    private float _range;

    public void Bind(CoreGame g) => _g = g;

    /// <summary>Pokaż zasięg broni na chwilę (range_flash na GBA).</summary>
    public void FlashRange(float seconds = 0.45f) => _range = seconds;

    /// <summary>Zasięg widoczny, dopóki gracz trzyma A (celowanie).</summary>
    public bool RangeHeld { get; set; }

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        if (_range > 0) _range = Mathf.Max(0, _range - (float)delta);
        QueueRedraw();
    }

    /// <summary>Kałuża: elipsa wody z odblaskiem (jak kafel 9 na GBA).</summary>
    private void DrawPuddle(Rect2 r, float pulse, bool lit)
    {
        var center = r.GetCenter() + new Vector2(0, 2);
        var pts = new Vector2[20];
        for (var i = 0; i < pts.Length; i++)
        {
            var a = i * Mathf.Tau / pts.Length;
            pts[i] = center + new Vector2(Mathf.Cos(a) * 13f, Mathf.Sin(a) * 8f);
        }
        DrawColoredPolygon(pts, new Color(0.25f, 0.44f, 0.69f, lit ? 0.85f : 0.3f));
        if (lit) DrawLine(center + new Vector2(-6, -3), center + new Vector2(3, -3), new Color(0.75f, 0.88f, 1f, 0.5f + 0.3f * pulse), 2f);
    }

    public override void _Draw()
    {
        if (_g is null) return;
        const int c = Assets.Cell;
        var pulse = 0.5f + 0.5f * Mathf.Sin(_clock * 7f);
        for (var y = 0; y < Level.H; y++)
        {
            for (var x = 0; x < Level.W; x++)
            {
                if (!_g.Explored(x, y)) continue;
                var r = new Rect2(x * c, y * c, c, c);
                var t = _g.Lv[x, y];
                if (_g.Puddle(x, y)) DrawPuddle(r, pulse, _g.Visible(x, y));
                if (t == Tile.Stairs && _g.Visible(x, y))
                    DrawRect(r.Grow(-3), new Color(Pal.StairsGlow, 0.12f + 0.16f * pulse));
                if (t != Tile.Wall && _g.SlamCell(x, y))
                    DrawTextureRect(Assets.Danger, r, false, new Color(1, 1, 1, 0.55f + 0.45f * pulse));
                if ((_range > 0 || RangeHeld) && t != Tile.Wall && _g.Visible(x, y) && !(x == _g.Hero.X && y == _g.Hero.Y)
                    && CoreGame.Cheb(_g.Hero.X, _g.Hero.Y, x, y) <= _g.WeaponRange())
                    DrawTextureRect(Assets.Range, r, false, new Color(1, 1, 1, RangeHeld ? 0.75f + 0.25f * pulse : Mathf.Min(1f, _range * 4f)));
            }
        }
    }
}
