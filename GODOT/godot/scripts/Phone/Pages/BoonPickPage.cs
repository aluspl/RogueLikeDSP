using LifeLike.Core;
using System.Collections.Generic;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Premia za etap (v0.21.50 cz. 2, jak w Hades / Slay the Spire): 3 karty z oferty (Game.BoonOffer) – pasek i pastylka
/// w kolorze rzadkości (zwykła szara, rzadka niebieska, legendarna złota), nazwa, skutek, znaczniki i „Synergia!”, gdy
/// wybór włączy nową synergię. Strzałki / dotknięcie wybierają, Enter / Spacja / drugie dotknięcie bierze, R losuje
/// ofertę jeszcze raz (raz na budowę za budżet, darmowo z Respektu).
/// </summary>
public sealed class BoonPickPage : PhonePage
{
    public const int RerollRow = 10;
    private readonly CoreGame _g;
    private string _note = "";

    public BoonPickPage(CoreGame g) => _g = g;

    public int Sel { get; set; }

    /// <summary>Wybór zatwierdzony (A / Enter / drugie dotknięcie) – ekran bierze premię.</summary>
    public System.Action Picked { get; set; }

    public override string Title => _g.St == LifeLike.Core.GameStatus.Playing ? Loc.T("premia_projekt_2") : Loc.T("premia_za_etap_2");
    public override string Sub => _g.St == LifeLike.Core.GameStatus.Playing ? Loc.T("znaleziony_projekt") : Loc.F("etap_zaliczony_2", _g.StageNumber());
    public override string Hint => Loc.T("strzalki_wybor_enter_biore") + (_g.RerollsLeft() > 0 ? Loc.T("r_losuj") : "");
    public override PageAction[] Actions =>
        _g.RerollsLeft() > 0 ? [new(Loc.T("biore"), GameAction.A), new(Loc.T("losuj"), GameAction.R)] : [new(Loc.T("biore"), GameAction.A)];

    public string RerollLabel()
    {
        if (_g.RerollsLeft() <= 0) return Loc.T("losowanie_zuzyte");
        var price = _g.RerollPrice();
        return price == 0 ? Loc.T("losuj_ponownie_za_darmo") : Loc.F("losuj_ponownie_zl", price);
    }

    public override bool TapRow(int index)
    {
        if (index == RerollRow)
        {
            Reroll();
            return true;
        }
        if (index < 0 || index > 2) return false;
        if (index == Sel) Picked?.Invoke();
        else
        {
            Sel = index;
            Sfx.Play("menu");
        }
        return true;
    }

    public void Reroll()
    {
        if (_g.RerollBoons())
        {
            Sfx.Play("buy");
            _note = Loc.T("nowa_oferta");
            Sel = 0;
        }
        else
        {
            _note = _g.RerollsLeft() <= 0 ? Loc.T("losowanie_juz_zuzyte") : Loc.F("za_maly_budzet_zl", _g.RerollPrice());
            Sfx.Play("menu");
        }
        Redraw();
    }

    public override bool Input(InputCmd e)
    {
        var d = e.VDir != 0 ? e.VDir : e.HDir;
        if (d != 0)
        {
            Sel = (Sel + d + 3) % 3;
            _note = "";
            Sfx.Play("menu");
            return true;
        }
        if (e.Is(GameAction.R))
        {
            Reroll();
            return true;
        }
        return false;
    }

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var rowH = PhonePainter.RowH;
        var tx0 = p.Left + 12;
        var width = (int)(p.Right - 6 - tx0);
        var y = p.Section(p.Top, Loc.T("wybierz_1_z_3"), Loc.F("premie_7", _g.BoonsOwned()));
        // wysokość kart: nazwa, skutek (1-2 linie), znaczniki
        var free = p.Bottom - y - (rowH + 8) - 6 * 3;
        var gap = 5f;
        for (var k = 0; k < 3; k++)
        {
            int b = _g.BoonOffer[k];
            if (b < 0) continue;
            var bd = d.Boons[b];
            var sel = k == Sel;
            var desc = p.F.Wrap(bd.Desc, width - 4);
            var rows = 2 + System.Math.Min(desc.Count, 2);
            if (rows * rowH + 8 > free / 3f + 4 && desc.Count > 1) rows = 3; // mało miejsca: skutek w jednej linii
            var card = p.Card(y, rows);
            var col = BoonLook.RarityColor(bd.Rarity);
            p.C.DrawRect(new Godot.Rect2(card.Position, new Godot.Vector2(4, card.Size.Y)), col);
            if (sel)
            {
                p.C.DrawRect(new Godot.Rect2(card.Position.X + 4, card.Position.Y, card.Size.X - 4, card.Size.Y), Pal.Group);
                p.C.DrawRect(card, col, false, 2);
            }
            p.Hit(card, k);
            var tx = p.TextX(card);
            var right = card.End.X - 6;
            var pw = p.Pill(right, p.RowY(card, 0), BoonLook.RarityName(d, bd.Rarity), BoonLook.RarityPill(bd.Rarity));
            p.Bold(tx, p.RowY(card, 0) + PhonePainter.TextDy, p.F.Fit(bd.Name, (int)(right - pw - 6 - tx)), sel ? Ink.Brand : Ink.Dark);
            for (var i = 0; i < rows - 2; i++) p.Text(tx, p.RowY(card, 1 + i), i < desc.Count ? desc[i] : "", BoonLook.RarityInk(bd.Rarity), TextAlign.Left, right - tx);
            var ty = p.RowY(card, rows - 1);
            var xr = right;
            var syn = BoonLook.NewSynergy(_g, b);
            if (syn >= 0)
            {
                var st = Loc.T("synergia") + d.Synergies[syn].Name;
                if (p.F.Measure(st) > (right - tx) * 0.6f) st = Loc.T("synergia_5");
                xr -= p.Pill(xr, ty, st, PillKind.Done) + 4;
            }
            var tags = BoonLook.Tags(d, bd.Tags);
            var tagText = string.Join(", ", tags) + (bd.Cls >= 0 ? Loc.T("zawod_4") : "");
            p.Text(tx, ty, p.F.Fit(tagText, (int)(xr - tx)), Ink.Dim);
            y = card.End.Y + gap;
        }
        var rc = p.Card(y + 1, 1);
        var can = _g.CanReroll();
        p.Stripe(rc, 0, _note.Length > 0 ? Pal.Brand : can ? Pal.Prog : Pal.Todo);
        p.HitRow(rc, 0, RerollRow);
        var rx = p.TextX(rc);
        var rright = rc.End.X - 6;
        var label = _note.Length > 0 ? _note : RerollLabel();
        var info = Loc.F("masz_zl", _g.Cash);
        var iw = p.Text(rright, p.RowY(rc, 0), info, Ink.Dim, TextAlign.Right);
        p.Text(rx, p.RowY(rc, 0), label, _note.Length > 0 ? Ink.Brand : can ? Ink.Prog : Ink.Dim, TextAlign.Left, rright - iw - 6 - rx);
    }
}
