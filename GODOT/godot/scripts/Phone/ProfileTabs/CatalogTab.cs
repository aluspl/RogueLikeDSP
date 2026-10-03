using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.ProfileTabs;

/// <summary>
/// Katalog usterek (zakładka 1 profilu na GBA): poznane problemy budowy z portretem, opisem i zachowaniami, reszta „???”.
/// v0.21.52 cz. c, kolekcje: strony przełączane Spacją / przyciskiem (A na GBA) – Katalog z licznikiem pokonanych
/// („x12”), Kolekcje (komplety z postępem i nagrodą), Bossowie (karty bossów: ile razy pokonany) i Album Osiedla
/// (ozdoby: stoi / skąd).
/// </summary>
public sealed class CatalogTab : PhonePage
{
    private const int Window = 8;
    private static readonly string[] Pages = ["Katalog", "Kolekcje", "Bossowie", "Album"];
    public const int CollectionsPage = 1;
    public const int BossesPage = 2;
    public const int AlbumPage = 3;
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly ListState _list = new();
    private int _page;

    public CatalogTab(GameData d, Profile p)
    {
        _d = d;
        _p = p;
    }

    public override string Title => Pages[_page];

    public override string Sub => _page switch
    {
        CollectionsPage => $"{CollectionBook.Done(_d, _p)}/{_d.Collections.Length}",
        BossesPage => $"{BossesBeaten()}/{CollectionBook.BossesCount(_d)}",
        AlbumPage => $"{Story.EstateDecor(_d, _p)}/{_d.EstateDecor.Length}",
        _ => $"{Meta.CatalogCount(_d, _p)}/{_d.Enemies.Length}",
    };

    public override string Hint => $"Spacja: {Pages[(_page + 1) % Pages.Length]}  Q/E: zakładki  Esc: wróć";

    public override PageAction[] Actions => [new(Pages[(_page + 1) % Pages.Length] + " >", GameAction.A)];

    /// <summary>Bieżąca strona (0 Katalog, 1 Kolekcje, 2 Bossowie, 3 Album) – test dymny i zrzuty.</summary>
    public int Page
    {
        get => _page;
        set
        {
            _page = System.Math.Clamp(value, 0, Pages.Length - 1);
            _list.Reset();
        }
    }

    private int BossesBeaten()
    {
        var n = 0;
        for (var k = 0; k < CollectionBook.BossesCount(_d); ++k)
        {
            if (_p.KillCount[CollectionBook.BossAt(_d, k)] > 0) ++n;
        }
        return n;
    }

    private int Count => _page switch
    {
        CollectionsPage => _d.Collections.Length,
        BossesPage => CollectionBook.BossesCount(_d),
        AlbumPage => _d.EstateDecor.Length,
        _ => _d.Enemies.Length,
    };

    public override bool TapRow(int index)
    {
        _list.Sel = index;
        return true;
    }

    private bool Known(int i) => Meta.CatalogHas(_p, i);

    /// <summary>Zaznacza pozycję i (sceny zrzutów).</summary>
    public void Select(int i)
    {
        _list.Sel = System.Math.Max(0, i);
        _list.Top = System.Math.Max(0, i - Window / 2);
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, Count, Window);
            return true;
        }
        if (e.Is(GameAction.A))
        {
            Page = (_page + 1) % Pages.Length;
            Sfx.Play("menu");
            return true;
        }
        return false;
    }

    public override void Draw(PhonePainter p)
    {
        var n = Count;
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
            DrawRow(p, i, tx, y, right, sel);
        }
        if (_list.Top > 0) p.Text(card.End.X - 4, card.Position.Y - 14, "^", Ink.Dim, TextAlign.Right);
        if (_list.Top + rows < n) p.Text(card.End.X - 4, card.End.Y - 6, "v", Ink.Dim, TextAlign.Right);
        switch (_page)
        {
            case CollectionsPage:
                DrawCollection(p, card);
                break;
            case BossesPage:
                DrawEnemyCard(p, card, CollectionBook.BossAt(_d, _list.Sel), true);
                break;
            case AlbumPage:
                DrawDecor(p, card);
                break;
            default:
                DrawEnemyCard(p, card, _list.Sel, false);
                break;
        }
    }

    private void DrawRow(PhonePainter p, int i, float tx, float y, float right, bool sel)
    {
        switch (_page)
        {
            case CollectionsPage:
            {
                var (have, need) = CollectionBook.Progress(_d, _p, i);
                var done = CollectionBook.Complete(_d, _p, i);
                var pw = p.Pill(right, y, done ? "Komplet" : $"{have}/{need}", done ? PillKind.Done : have > 0 ? PillKind.Prog : PillKind.Gray);
                p.Text(tx, y, _d.Collections[i].Name, sel ? Ink.Brand : done ? Ink.Done : Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
                return;
            }
            case BossesPage:
            {
                var e = CollectionBook.BossAt(_d, i);
                var count = _p.KillCount[e];
                var pw = p.Pill(right, y, count > 0 ? $"x{count}" : "Brak", count > 0 ? PillKind.Done : PillKind.Gray);
                var ic = new Godot.Rect2(tx - 2, y + (PhonePainter.RowH - 16) / 2f, 16, 16);
                p.C.DrawTextureRectRegion(Assets.Actors, ic, Assets.Frame(_d.Enemies[e].Frame, Assets.Actor),
                    count > 0 ? Godot.Colors.White : new Godot.Color(0, 0, 0, 0.8f));
                p.Text(tx + 18, y, count > 0 ? _d.Enemies[e].Name : "???", sel ? Ink.Brand : count > 0 ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx - 18);
                return;
            }
            case AlbumPage:
            {
                var dd = _d.EstateDecor[i];
                var own = Story.DecorUnlocked(_d, _p, i);
                var pill = own ? "Stoi" : dd.Inspector > 0 ? $"Inspektor {dd.Inspector}" : $"{dd.Wins} wygr.";
                var pw = p.Pill(right, y, pill, own ? PillKind.Done : PillKind.Gray);
                p.Text(tx, y, dd.Name, sel ? Ink.Brand : own ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
                return;
            }
            default:
            {
                var known = Known(i);
                var pw = p.Pill(right, y, known ? $"x{_p.KillCount[i]}" : "NIEZNANA", known ? PillKind.Done : PillKind.Gray);
                p.Text(tx, y, $"#{i + 1} {(known ? _d.Enemies[i].Name : "???")}", sel ? Ink.Brand : known ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
                return;
            }
        }
    }

    /// <summary>Karta problemu (Katalog) albo karta bossa (Bossowie): portret, nazwa, opis, cechy / komplet.</summary>
    private void DrawEnemyCard(PhonePainter p, Godot.Rect2 card, int s, bool boss)
    {
        var right = card.End.X - 6;
        var dc = p.CardH(card.End.Y + 6, 62);
        var known = boss ? _p.KillCount[s] > 0 : Known(s);
        var photo = new Godot.Rect2(dc.Position.X + 8, dc.Position.Y + 8, 32, 32);
        p.C.DrawStyleBox(Ui.Box(known ? Pal.DoneBg : Pal.Group, 5), photo);
        p.IconTinted(Assets.Actors, _d.Enemies[s].Frame, Assets.Actor, photo.Position, 1, known ? Godot.Colors.White : new Godot.Color(0, 0, 0, 0.8f));
        var x = photo.End.X + 8;
        var lines = p.F.Wrap(known ? _d.Enemies[s].Desc : boss ? "Pokonaj, żeby zdobyć kartę" : "Pokonaj, żeby poznać", (int)(right - x));
        p.Text(x, dc.Position.Y + 4, known ? _d.Enemies[s].Name + (boss ? $" – pokonany {_p.KillCount[s]}x" : "") : "???", Ink.Dark, TextAlign.Left, right - x);
        if (lines.Count > 0) p.Text(x, dc.Position.Y + 22, lines[0], Ink.Dim, TextAlign.Left, right - x);
        if (boss)
        {
            var ci = System.Array.FindIndex(_d.Collections, c => c.Kind == CollectionKind.Bosses);
            if (ci >= 0) p.Text(x, dc.Position.Y + 40, "Komplet kart: " + CollectionBook.RewardLabel(_d, ci), Ink.Brand, TextAlign.Left, right - x);
            return;
        }
        var tags = UiText.Behaviors(_d, s);
        if (known && tags.Length > 0) p.Text(x, dc.Position.Y + 40, "Cechy: " + tags, Ink.Late, TextAlign.Left, right - x);
    }

    /// <summary>Kolekcja: co liczyć (problemy aktu x10, bossowie, ozdoby), najbliższy brakujący i nagroda.</summary>
    private void DrawCollection(PhonePainter p, Godot.Rect2 card)
    {
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        var dc = p.Card(card.End.Y + 6, 2);
        var i = _list.Sel;
        var cd = _d.Collections[i];
        var (have, need) = CollectionBook.Progress(_d, _p, i);
        var what = cd.Kind == CollectionKind.Kills ? $"{cd.Desc}: każdy x{cd.Count} ({have}/{need})" : $"{cd.Desc} ({have}/{need})";
        if (cd.Kind == CollectionKind.Kills && have < need)
        {
            int best = -1, bestCount = -1;
            for (var e = 0; e < _d.Enemies.Length; ++e)
            {
                if (((cd.Enemies >> e) & 1) == 0 || _p.KillCount[e] >= cd.Count || _p.KillCount[e] <= bestCount) continue;
                bestCount = _p.KillCount[e];
                best = e;
            }
            if (best >= 0) what += $", np. {(Known(best) ? _d.Enemies[best].Name : "???")} {bestCount}/{cd.Count}";
        }
        p.Text(tx, p.RowY(dc, 0), what, Ink.Dim, TextAlign.Left, right - tx);
        p.Text(tx, p.RowY(dc, 1), "Nagroda: " + CollectionBook.RewardLabel(_d, i), CollectionBook.Complete(_d, _p, i) ? Ink.Done : Ink.Brand, TextAlign.Left, right - tx);
    }

    /// <summary>Album Osiedla: skąd ozdoba i nagroda za komplet.</summary>
    private void DrawDecor(PhonePainter p, Godot.Rect2 card)
    {
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        var dc = p.Card(card.End.Y + 6, 2);
        var dd = _d.EstateDecor[_list.Sel];
        p.Text(tx, p.RowY(dc, 0), dd.Inspector > 0 ? $"{dd.Name}: poziom inspektora {dd.Inspector}" : $"{dd.Name}: {dd.Wins}. wygrana budowa", Ink.Dim, TextAlign.Left, right - tx);
        var ci = System.Array.FindIndex(_d.Collections, c => c.Kind == CollectionKind.Decor);
        if (ci >= 0) p.Text(tx, p.RowY(dc, 1), "Komplet albumu: " + CollectionBook.RewardLabel(_d, ci), CollectionBook.Complete(_d, _p, ci) ? Ink.Done : Ink.Brand, TextAlign.Left, right - tx);
    }
}
