using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.ProfileTabs;

/// <summary>Katalog usterek (zakładka 1 profilu na GBA): poznane problemy budowy z portretem i opisem, reszta „???”.</summary>
public sealed class CatalogTab : PhonePage
{
    private const int Window = 8;
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly ListState _list = new();

    public CatalogTab(GameData d, Profile p)
    {
        _d = d;
        _p = p;
    }

    public override string Title => "Katalog";
    public override string Sub => $"{UiText.BitCount(_p.Catalog)}/{_d.Enemies.Length}";
    public override string Hint => "Q/E: zakładki  Esc: wróć";

    public override bool TapRow(int index)
    {
        _list.Sel = index;
        return true;
    }

    private bool Known(int i) => (_p.Catalog & (1 << i)) != 0;

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v == 0) return false;
        _list.Move(v, _d.Enemies.Length, Window);
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        var n = _d.Enemies.Length;
        _list.Clamp(n, Window);
        var rows = System.Math.Min(Window, n - _list.Top);
        var card = p.Card(p.Top, rows);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var r = 0; r < rows; r++)
        {
            var i = _list.Top + r;
            var y = p.RowY(card, r);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            p.HitRow(card, r, i);
            var known = Known(i);
            var pw = p.Pill(right, y, known ? "ZAMKNIĘTA" : "NIEZNANA", known ? PillKind.Done : PillKind.Gray);
            p.Text(tx, y, $"#{i + 1} {(known ? _d.Enemies[i].Name : "???")}", sel ? Ink.Brand : known ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
        }
        var dc = p.CardH(card.End.Y + 6, 48);
        var s = _list.Sel;
        var photo = new Godot.Rect2(dc.Position.X + 8, dc.Position.Y + 8, 32, 32);
        p.C.DrawStyleBox(Ui.Box(Known(s) ? Pal.DoneBg : Pal.Group, 5), photo);
        p.IconTinted(Assets.Actors, _d.Enemies[s].Frame, Assets.Actor, photo.Position, 1, Known(s) ? Godot.Colors.White : new Godot.Color(0, 0, 0, 0.8f));
        var x = photo.End.X + 8;
        var lines = p.F.Wrap(Known(s) ? _d.Enemies[s].Desc : "Pokonaj, żeby poznać", (int)(right - x));
        p.Text(x, dc.Position.Y + 4, Known(s) ? _d.Enemies[s].Name : "???", Ink.Dark, TextAlign.Left, right - x);
        if (lines.Count > 0) p.Text(x, dc.Position.Y + 22, lines[0], Ink.Dim, TextAlign.Left, right - x);
    }
}
