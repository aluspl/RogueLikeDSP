using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Hud;

/// <summary>
/// Karta wroga pod trzymanym B (podgląd na GBA, w miejscu dziennika): portret, nazwa, HP z paskiem, obrona,
/// obrażenia w obie strony (rozpiska #26: „Zadasz 2-5 (kryt 4-10), on Tobie 1-3” po OBR i procentach, unik),
/// opis na zmianę z zachowaniami („Cechy: ...”); „Nikogo w polu widzenia”, gdy lista jest pusta.
/// </summary>
public partial class EnemyCard : Control
{
    private const int CardH = 76;
    private CoreGame _g;
    private int _enemy = -1, _index, _count;
    private float _clock;

    public override void _Process(double delta)
    {
        if (!Visible) return;
        var before = (int)(_clock / 2f);
        _clock += (float)delta;
        if ((int)(_clock / 2f) != before) QueueRedraw(); // opis / zachowania na zmianę co 2 s
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Visible = false;
    }

    public void Show(CoreGame g, int enemy, int index, int count)
    {
        _g = g;
        _enemy = enemy;
        _index = index;
        _count = count;
        Visible = true;
        QueueRedraw();
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("EnemyCard", ex);
        }
    }

    private void DrawContent()
    {
        if (_g is null) return;
        var f = PixelFont.I;
        var w = Size.X;
        var top = Size.Y - CardH;
        DrawRect(new Rect2(0, top, w, CardH), new Color(Pal.Text, 0.78f));
        DrawRect(new Rect2(0, top - 1, w, 1), new Color(Pal.Brand, 0.7f));
        if (_enemy < 0)
        {
            f.Draw(this, new Vector2(8, top + 12), "Nikogo w polu widzenia", Ink.MapDim);
            f.Draw(this, new Vector2(w - 6, Size.Y - 18), ButtonNames.Pick("Puść Z: wróć", "Puść: wróć"), Ink.MapDim, TextAlign.Right);
            return;
        }
        var e = _g.Enemies[_enemy];
        var ed = _g.D.Enemies[e.DefId];
        var photo = new Rect2(6, top + 5, 36, 36);
        DrawStyleBox(Ui.Box(new Color(Pal.Late, 0.35f), 5), photo);
        Assets.DrawFrame(this, Assets.Actors, ed.Frame, Assets.Actor, photo.Position + new Vector2(2, 2));
        var x = photo.End.X + 8;
        var nx = x + f.Draw(this, new Vector2(x, top + 2), ed.Name, Ink.MapLoot) + 8;
        var bar = new Rect2(nx, top + 8, 48, 6);
        DrawRect(bar.Grow(1), Pal.HpEdge);
        DrawRect(bar, Pal.HpBack);
        var fill = e.MaxHp > 0 ? Mathf.Clamp(e.Hp / (float)e.MaxHp, 0f, 1f) : 0f;
        DrawRect(new Rect2(bar.Position, new Vector2(Mathf.Max(1, Mathf.Round(bar.Size.X * fill)), bar.Size.Y)), Pal.HpMain[Pal.HpColor(e.Hp, e.MaxHp)]);
        var stats = $"HP {e.Hp}/{e.MaxHp}" + (ed.Defense > 0 ? $"   OBR {ed.Defense}" : "");
        f.Draw(this, new Vector2(bar.End.X + 8, top + 2), f.Fit(stats, (int)(w - bar.End.X - 14)), Ink.Map);
        var tags = UiText.Behaviors(_g.D, e.DefId);
        var showTags = tags.Length > 0 && ((int)(_clock / 2f) & 1) == 1;
        var vs = DamageHelp.VersusLine(new Message(), _g.WeaponBreakdown(e.DefId), _g.EnemyHit(_enemy)).Text + $", unik {_g.DodgePct()}%";
        if (f.Measure(vs) > w - x - 8) vs = DamageHelp.VersusLine(new Message(), _g.WeaponBreakdown(e.DefId), _g.EnemyHit(_enemy)).Text;
        f.Draw(this, new Vector2(x, top + 20), f.Fit(vs, (int)(w - x - 8)), Ink.Map);
        f.Draw(this, new Vector2(x, top + 38), f.Fit(showTags ? "Cechy: " + tags : ed.Desc, (int)(w - x - 8)), showTags ? Ink.MapBad : Ink.MapDim);
        var back = ButtonNames.Pick("Puść Z: wróć", "Puść: wróć");
        var hint = _count > 1 ? $"{_index + 1}/{_count}  {ButtonNames.Pick("Strzałki: następny", "Przesuń: następny")}   {back}" : back;
        f.Draw(this, new Vector2(w - 6, top + 56), hint, Ink.MapDim, TextAlign.Right);
    }
}
