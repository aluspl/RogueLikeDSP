using System.Linq;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.ProfileTabs;

/// <summary>
/// Profil: strony Odznaki / Zlecenia / Pamiątki przełączane A (jak zakładka 0 w run_shop na GBA): lista z pastylkami
/// (Zdobyta / +XP, Wykonane / postęp, Ranga / Zablok.) i opis zaznaczonej pozycji z premią lub nagrodą.
/// </summary>
public sealed class BadgesTab : PhonePage
{
    private const int Window = 7;
    private static readonly string[] Pages = ["Odznaki", "Zlecenia", "Pamiątki"];
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly ListState _list = new();
    private int _page;

    public BadgesTab(GameData d, Profile p, int page = 0)
    {
        _d = d;
        _p = p;
        _page = page;
    }

    public override string Title => Pages[_page];

    public override string Sub
    {
        get
        {
            var total = Count;
            var n = _page == 2 ? Enumerable.Range(0, total).Count(k => Meta.KeepsakeUnlocked(_d, _p, k))
                  : UiText.BitCount(_page == 1 ? _p.Contracts : _p.Badges);
            return $"{n}/{total}";
        }
    }

    public override string Hint => $"Spacja: {Pages[(_page + 1) % 3]}  Q/E: zakładki";

    private int Count => _page == 2 ? _d.Keepsakes.Length : _page == 1 ? _d.Contracts.Length : _d.Badges.Length;

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
            _page = (_page + 1) % 3;
            _list.Reset();
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
            string name, pill;
            PillKind kind;
            bool on;
            if (_page == 2)
            {
                on = Meta.KeepsakeUnlocked(_d, _p, i);
                name = _d.Keepsakes[i].Name;
                pill = on ? "Ranga " + UiText.Roman(Meta.KeepsakeRank(_d, _p, i) - 1) : "Zablok.";
                kind = !on ? PillKind.Gray : Meta.SelectedKeepsake(_d, _p) == i ? PillKind.Prog : PillKind.Group;
            }
            else if (_page == 1)
            {
                on = Meta.ContractDone(_p, i);
                var c = _d.Contracts[i];
                var pr = System.Math.Min(Meta.ContractProgress(_d, _p, i), c.Target);
                name = c.Name;
                pill = on ? "Wykonane" : $"{pr}/{c.Target}";
                kind = on ? PillKind.Done : pr > 0 ? PillKind.Prog : PillKind.Gray;
                on = true;
            }
            else
            {
                on = (_p.Badges & (1 << i)) != 0;
                name = _d.Badges[i].Name;
                pill = on ? "Zdobyta" : $"+{_d.Badges[i].Xp}";
                kind = on ? PillKind.Done : PillKind.Gray;
            }
            var pw = p.Pill(right, y, pill, kind);
            p.Text(tx, y, name, sel ? Ink.Brand : on ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
        }
        if (_list.Top > 0) p.Text(card.End.X - 4, card.Position.Y - 14, "^", Ink.Dim, TextAlign.Right);
        if (_list.Top + rows < n) p.Text(card.End.X - 4, card.End.Y - 6, "v", Ink.Dim, TextAlign.Right);

        var dc = p.Card(card.End.Y + 6, 3);
        var (desc, extra, ink) = Describe(_list.Sel);
        var lines = p.F.Wrap(desc, (int)(right - tx));
        for (var k = 0; k < 2 && k < lines.Count; k++) p.Text(tx, p.RowY(dc, k), lines[k], Ink.Dim);
        p.Divider(dc, 2);
        p.Stripe(dc, 2, ink.Fill);
        p.Text(tx, p.RowY(dc, 2), extra, ink, TextAlign.Left, right - tx);
    }

    /// <summary>Opis zaznaczonej pozycji: (opis, wiersz premii / nagrody / rangi, jego kolor).</summary>
    private (string, string, Ink) Describe(int i)
    {
        if (_page == 2)
        {
            var kd = _d.Keepsakes[i];
            var unl = Meta.KeepsakeUnlocked(_d, _p, i);
            var rank = Meta.KeepsakeRank(_d, _p, i);
            string how;
            if (!unl)
                how = kd.Badge >= 0 ? "Odznaka: " + _d.Badges[kd.Badge].Name
                    : "Zlecenie: " + (_d.Contracts.FirstOrDefault(c => c.Keepsake == i)?.Name ?? "?");
            else if (rank < 3) how = $"Ranga {UiText.Roman(rank)} po {_d.KeepsakeRankRuns[rank - 1]} bud. (ma {_p.KeepsakeRuns[i]})";
            else how = "Ranga maksymalna";
            return ($"{kd.Desc} Premia: {RunMods.PerkLabel(Meta.KeepsakePerk(_d, _p, i))}", how, unl ? Ink.Done : Ink.Brand);
        }
        if (_page == 1)
        {
            var c = _d.Contracts[i];
            var reward = $"Nagroda: +{c.Xp}" + (c.Keepsake >= 0 ? ", " + _d.Keepsakes[c.Keepsake].Name : " dośw.");
            return (c.Desc, reward, Meta.ContractDone(_p, i) ? Ink.Done : Ink.Brand);
        }
        var b = _d.Badges[i];
        var got = (_p.Badges & (1 << i)) != 0;
        return (b.Desc, "Premia: " + RunMods.PerkLabel(b.Bonus), got ? Ink.Done : Ink.Brand);
    }
}
