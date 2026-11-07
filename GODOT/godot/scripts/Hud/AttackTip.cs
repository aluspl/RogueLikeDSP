using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Hud;

/// <summary>
/// v0.23.0: dymek z opisem ataku nad przyciskiem Cios / Patent (AttackInfo z rdzenia): nagłówek („Patent · Kielnia”), tytuł
/// (nazwa ruchu), mini-siatka wzoru (bohater w środku, w prawo; pola trafione w kolorze rzadkości, zasięg przyciemniony)
/// i wiersze opisu. Kotwica w jednostkach warstwy HUD; dymek nad nią (pod nią, gdy brak miejsca), zawsze w ekranie.
/// </summary>
public partial class AttackTip : Control
{
    private const int Cell = 7, Pad = 8, LineH = 16;

    private AttackInfo _info;
    private Rect2 _anchor;
    private CanvasItem _from;
    private Color _hit = Pal.Accent;

    public bool Shown => Visible && _info is not null;

    /// <summary>Tryb pokazanego opisu (Patent albo Cios).</summary>
    public bool ModeB => _info is not null && _info.ModeB;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Visible = false;
    }

    /// <summary>Opis nad prostokątem anchor w układzie węzła from (przycisk paska akcji, pole broni w HUD); przeliczany przy
    /// każdym rysowaniu – warstwy mają różne skale i przesunięcia (pion telefonu, zmiana rozmiaru okna).</summary>
    public void ShowFor(CoreGame g, bool modeB, Rect2 anchor, CanvasItem from)
    {
        _info = AttackInfo.For(g, modeB);
        _anchor = anchor;
        _from = from;
        _hit = WeaponLook.RarityColor(g.D, g.WeaponRarity).Lightened(0.2f);
        Visible = true;
        QueueRedraw();
    }

    public void HideTip()
    {
        if (!Visible) return;
        Visible = false;
        _info = null;
    }

    public override void _Process(double delta)
    {
        if (Visible) QueueRedraw();
    }

    public override void _Draw()
    {
        try
        {
            DrawTip();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("AttackTip", ex);
        }
    }

    /// <summary>Prostokąt dymku (do testów i zrzutów): w ekranie, nad kotwicą albo pod nią.</summary>
    public Rect2 Box { get; private set; }

    private void DrawTip()
    {
        var a = _info;
        if (a is null) return;
        var f = PixelFont.I;
        var gridW = a.Side * Cell;
        var maxW = Mathf.Max(120f, Size.X - 16);
        var textRoom = maxW - 3 * Pad - gridW;
        var lines = new List<string>();
        foreach (var l in a.Lines) lines.AddRange(f.Wrap(l, (int)textRoom));
        var tw = f.Measure(a.Header);
        tw = Mathf.Max(tw, f.Measure(a.Title, 1, true) + 2);
        var lw = 0f;
        foreach (var l in lines) lw = Mathf.Max(lw, f.Measure(l));
        var w = Mathf.Min(maxW, Mathf.Max(tw + 2 * Pad, gridW + lw + 3 * Pad));
        var bodyH = Mathf.Max(gridW, lines.Count * LineH);
        var h = Pad + 2 * LineH + 4 + bodyH + Pad;
        var anchor = _anchor;
        if (_from is not null && IsInstanceValid(_from))
        {
            var t = GetGlobalTransformWithCanvas().AffineInverse() * _from.GetGlobalTransformWithCanvas();
            var p0 = t * _anchor.Position;
            anchor = new Rect2(p0, t * _anchor.End - p0);
        }
        var x = Mathf.Clamp(anchor.GetCenter().X - w / 2, 8, Mathf.Max(8, Size.X - w - 8));
        var y = anchor.Position.Y - h - 6;
        if (y < 4) y = anchor.End.Y + 6;
        y = Mathf.Clamp(y, 4, Mathf.Max(4, Size.Y - h - 4));
        var box = new Rect2(Mathf.Round(x), Mathf.Round(y), Mathf.Round(w), Mathf.Round(h));
        Box = box;
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.3f), 8), new Rect2(box.Position + new Vector2(0, 2), box.Size));
        DrawStyleBox(Ui.Box(Pal.Text, 8, Pal.Brand), box);
        var tx = box.Position.X + Pad;
        var ty = box.Position.Y + Pad - 4;
        f.Draw(this, new Vector2(tx, ty), f.Fit(a.Header, (int)(w - 2 * Pad)), Ink.MapDim);
        f.Draw(this, new Vector2(tx, ty + LineH), a.Title, a.ModeB ? Ink.MapLoot : Ink.Map, TextAlign.Left, 1, true);
        var by = box.Position.Y + Pad + 2 * LineH + 4;
        DrawGrid(a, new Vector2(tx, by + (bodyH - gridW) / 2));
        var lx = tx + gridW + Pad;
        for (var i = 0; i < lines.Count; i++) f.Draw(this, new Vector2(lx, by - 4 + i * LineH), lines[i], i == 0 ? Ink.Map : Ink.MapDim);
    }

    private void DrawGrid(AttackInfo a, Vector2 at)
    {
        for (var dy = -a.Radius; dy <= a.Radius; dy++)
        {
            for (var dx = -a.Radius; dx <= a.Radius; dx++)
            {
                var r = new Rect2(at.X + (dx + a.Radius) * Cell, at.Y + (dy + a.Radius) * Cell, Cell - 1, Cell - 1);
                var c = a.Cell(dx, dy) switch
                {
                    AttackInfo.Hero => Colors.White,
                    AttackInfo.Hit => _hit,
                    AttackInfo.Reach => new Color(1, 1, 1, 0.22f),
                    _ => new Color(1, 1, 1, 0.08f),
                };
                DrawRect(r, c);
            }
        }
    }
}
