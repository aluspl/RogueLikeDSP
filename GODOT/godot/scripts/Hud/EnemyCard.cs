using Godot;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Hud;

/// <summary>
/// Karta wroga pod trzymanym B (podgląd na GBA, w miejscu dziennika): portret, nazwa, HP z paskiem,
/// obrażenia z premią etapu i trudności, opis; „Nikogo w polu widzenia”, gdy lista jest pusta.
/// </summary>
public partial class EnemyCard : Control
{
    private const int CardH = 58;
    private CoreGame _g;
    private int _enemy = -1, _index, _count;

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
            f.Draw(this, new Vector2(w - 6, Size.Y - 18), "Puść Z: wróć", Ink.MapDim, TextAlign.Right);
            return;
        }
        var e = _g.Enemies[_enemy];
        var ed = _g.D.Enemies[e.DefId];
        var photo = new Rect2(6, top + 5, 36, 36);
        DrawStyleBox(Ui.Box(new Color(Pal.Late, 0.35f), 5), photo);
        Assets.DrawFrame(this, Assets.Actors, ed.Frame, Assets.Actor, photo.Position + new Vector2(2, 2));
        var x = photo.End.X + 8;
        var bonus = _g.EnemyDmgBonus();
        var nx = x + f.Draw(this, new Vector2(x, top + 2), ed.Name, Ink.MapLoot) + 8;
        var bar = new Rect2(nx, top + 8, 48, 6);
        DrawRect(bar.Grow(1), Pal.HpEdge);
        DrawRect(bar, Pal.HpBack);
        var fill = e.MaxHp > 0 ? Mathf.Clamp(e.Hp / (float)e.MaxHp, 0f, 1f) : 0f;
        DrawRect(new Rect2(bar.Position, new Vector2(Mathf.Max(1, Mathf.Round(bar.Size.X * fill)), bar.Size.Y)), Pal.HpMain[Pal.HpColor(e.Hp, e.MaxHp)]);
        var stats = $"HP {e.Hp}/{e.MaxHp}   obr. {ed.MinDamage + bonus}-{ed.MaxDamage + bonus}";
        f.Draw(this, new Vector2(bar.End.X + 8, top + 2), f.Fit(stats, (int)(w - bar.End.X - 14)), Ink.Map);
        f.Draw(this, new Vector2(x, top + 20), f.Fit(ed.Desc, (int)(w - x - 8)), Ink.MapDim);
        var hint = _count > 1 ? $"{_index + 1}/{_count}  Strzałki: następny   Puść Z: wróć" : "Puść Z: wróć";
        f.Draw(this, new Vector2(w - 6, top + 38), hint, Ink.MapDim, TextAlign.Right);
    }
}
