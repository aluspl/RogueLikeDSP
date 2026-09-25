using System.Collections.Generic;
using Godot;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Usterki = problemy budowy jako zgłoszenia (tab_issues na GBA): numer, „zdjęcie” (portret problemu), nazwa,
/// liczba usuniętych, opis i status OTWARTA / ZAMKNIĘTA. Na liście są problemy etapu i już usunięte.
/// </summary>
public sealed class IssuesTab : PhonePage
{
    private const int Row = 36;
    private readonly CoreGame _g;

    public IssuesTab(CoreGame g) => _g = g;

    public override string Title => "Usterki";
    public override string Sub => $"Otwarte: {Rows(out _).FindAll(r => r.Open).Count}";

    private List<(int Def, bool Open)> Rows(out int total)
    {
        var d = _g.D;
        var sd = d.Stages[_g.Stage];
        var list = new List<(int, bool)>();
        total = 0;
        for (var t = 0; t < d.Enemies.Length; t++)
        {
            var inStage = sd.Boss == t || System.Array.IndexOf(sd.Pool, t) >= 0;
            var alive = 0;
            for (var i = 0; i < _g.EnemiesCount; i++)
            {
                if (_g.Enemies[i].Alive && _g.Enemies[i].DefId == t) ++alive;
            }
            if (!inStage && _g.KillsByType[t] == 0) continue;
            list.Add((t, alive > 0));
            total++;
        }
        return list;
    }

    public override void Draw(PhonePainter p)
    {
        var rows = Rows(out _);
        var max = (int)((p.Bottom - p.Top - 8) / Row);
        if (rows.Count > max) rows = rows.GetRange(0, max);
        var card = p.CardH(p.Top, rows.Count * Row + 8);
        var d = _g.D;
        for (var r = 0; r < rows.Count; r++)
        {
            var (def, open) = rows[r];
            var ed = d.Enemies[def];
            var y = card.Position.Y + 4 + r * Row;
            if (r > 0) p.C.DrawRect(new Rect2(card.Position.X + 10, y - 1, card.Size.X - 20, 1), Pal.Bg);
            p.C.DrawRect(new Rect2(card.Position.X + 4, y + 3, 3, Row - 6), open ? Pal.Late : Pal.Done);
            var photo = new Rect2(card.Position.X + 11, y + 2, 32, 32);
            p.C.DrawStyleBox(Ui.Box(open ? Pal.LateBg : Pal.DoneBg, 5), photo);
            p.Icon(Assets.Actors, ed.Frame, Assets.Actor, photo.Position);
            var x = photo.End.X + 6;
            var pw = p.Pill(card.End.X - 6, y, open ? "OTWARTA" : "ZAMKNIĘTA", open ? PillKind.Late : PillKind.Done);
            var kills = _g.KillsByType[def] > 0 ? $" x{_g.KillsByType[def]}" : "";
            var boss = d.Stages[_g.Stage].Boss == def ? " (boss)" : "";
            p.Text(x, y, $"#{r + 1} {ed.Name}{kills}{boss}", Ink.Dark, TextAlign.Left, card.End.X - 10 - pw - x);
            p.Text(x, y + 16, ed.Desc, Ink.Dim, TextAlign.Left, card.End.X - 8 - x);
        }
    }
}
