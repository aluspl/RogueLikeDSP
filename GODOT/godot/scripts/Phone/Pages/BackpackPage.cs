using System;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// v0.23.2: Skup (sprzedaż za materiały) i PSZOK (oddanie za dośw. i HP) w Hurtowni. v0.23.2 (UI 2): zakładki Hurtowni,
/// jedno zdanie opisu, plecak siatką 3×2 kafli z ceną pod każdym, pod spodem materiały albo HP. Dotknięcie kafla
/// (klawiatura: strzałki + Spacja) otwiera kartę przedmiotu z główną akcją zakładki (Sprzedaj / PSZOK) i Załóż.
/// </summary>
public sealed class BackpackPage : PhonePage
{
    private readonly CoreGame _g;
    private int _sel;

    public BackpackPage(CoreGame g)
    {
        _g = g;
        Sheet = new ItemSheet(g) { Shop = true, Done = _ => Done?.Invoke(false) };
    }

    public ItemSheet Sheet { get; }

    public BagMode Mode { get; init; }

    /// <summary>Strona w Hurtowni (pasek zakładek, zakładka Tab).</summary>
    public bool Shop { get; init; } = true;

    public int Tab { get; init; } = HurtTabs.Sell;

    /// <summary>Po akcji: true = zużyła turę (tu nigdy – Hurtownia jest poza czasem gry).</summary>
    public Action<bool> Done { get; set; }

    /// <summary>Dotknięcie zakładki Hurtowni.</summary>
    public Action<int> TabTo { get; set; }

    public int Sel
    {
        get => _sel;
        set => _sel = Math.Clamp(value, 0, Math.Max(0, _g.BackpackSlots() - 1));
    }

    public bool MenuOpen => Sheet.IsOpen;

    public override string Title => Mode == BagMode.Recycle ? Loc.T("hurt_tab_pszok") : Loc.T("hurt_tab_skup");

    public override string Sub => Loc.F("plecak_sub", _g.BackpackCount(), _g.BackpackSlots());

    public override string Hint => Sheet.IsOpen ? Loc.T("karta_hint_2") : Mode == BagMode.Recycle ? Loc.T("hurt_hint_pszok") : Loc.T("hurt_hint_skup");

    public override PageAction[] Actions => Sheet.IsOpen ? [] : [new(Loc.T("dalej"), GameAction.B, PageRole.Back)];

    public override void Enter()
    {
        Sheet.Primary = Mode == BagMode.Recycle ? SheetAct.Recycle : SheetAct.Sell;
        Sheet.Close();
        Sheet.ClearNote();
    }

    public override bool Input(InputCmd e)
    {
        if (Sheet.IsOpen) return Sheet.Input(e);
        var n = _g.BackpackSlots();
        var d = e.HDir != 0 ? e.HDir : e.VDir * 3;
        if (d != 0)
        {
            _sel = (_sel + d + n) % n;
            Sfx.Play("menu");
            return true;
        }
        if (e.IsConfirm)
        {
            Confirm();
            return true;
        }
        return false;
    }

    public override bool TapRow(int index)
    {
        if (Sheet.IsOpen) return Sheet.Tap(index);
        if (index >= HurtTabs.HitBase)
        {
            TabTo?.Invoke(index - HurtTabs.HitBase);
            return true;
        }
        _sel = index;
        Confirm();
        return true;
    }

    /// <summary>A / dotknięcie: karta zaznaczonego przedmiotu (pusty – nic).</summary>
    public void Confirm()
    {
        Sheet.Primary = Mode == BagMode.Recycle ? SheetAct.Recycle : SheetAct.Sell;
        Sheet.ClearNote();
        Sheet.Show(SheetKind.Bag, _sel);
    }

    /// <summary>Skup: „2 Stal + 1 Cem.”.</summary>
    public static string SellText(CoreGame g, BackpackItem it)
    {
        var n = g.SellCount(it);
        var d = g.D;
        var s = $"{(n + 1) / 2} {d.Materials[g.SellMat(it)].Short}";
        if (n / 2 > 0) s += $"+{n / 2} {d.Materials[d.BackpackAltMat].Short}";
        return s;
    }

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var y = HurtTabs.Draw(p, p.Top, Tab);
        var note = Sheet.Note.Length > 0;
        var head = note ? Sheet.Note : Mode == BagMode.Recycle ? Loc.F("pszok_opis", d.PszokHp) : Loc.T("skup_opis");
        var lines = p.F.Wrap(head, (int)(p.Width - 24));
        var c0 = p.Card(y, Math.Min(2, lines.Count));
        for (var k = 0; k < c0.Size.Y / PhonePainter.RowH - 0.1f && k < lines.Count && k < 2; k++)
        {
            p.Stripe(c0, k, note ? Sheet.NoteInk.Fill : Pal.Done);
            p.Text(p.TextX(c0), p.RowY(c0, k), lines[k], note ? Sheet.NoteInk : Ink.Dim, TextAlign.Left, c0.End.X - 6 - p.TextX(c0));
        }
        y = c0.End.Y + 10;
        var n = _g.BackpackSlots();
        var gap = 8f;
        var tw = Mathf.Floor((p.Width - 2 * gap) / 3);
        var th = 52f;
        var priceH = 18f;
        for (var i = 0; i < n; i++)
        {
            var r = new Rect2(Mathf.Round(p.Left + (i % 3) * (tw + gap)), Mathf.Round(y + (i / 3) * (th + priceH + gap)), tw, th);
            var it = _g.Backpack[i];
            ItemTile.Draw(p, d, r, it, 0, !Layout.Touch && !Sheet.IsOpen && i == _sel);
            if (!it.Empty)
            {
                var price = Mode == BagMode.Recycle ? Loc.F("pszok_cena", _g.PszokXpFor(it)) : SellText(_g, it);
                p.F.Draw(p.C, new Vector2(r.GetCenter().X, r.End.Y + 1), p.F.Fit(price, (int)tw), Ink.Brand, TextAlign.Center);
            }
            if (!Sheet.IsOpen) p.Hit(r.Grow(2), i);
        }
        y += ((n + 2) / 3) * (th + priceH + gap) + 2;
        // materiały (Skup) albo HP (PSZOK)
        var info = p.Card(y, 1);
        var x = p.TextX(info);
        var ry = p.RowY(info, 0);
        if (Mode == BagMode.Recycle) p.Text(x, ry, $"HP {_g.Hero.Hp}/{_g.Hero.MaxHp}", Ink.Dark);
        else
        {
            for (var m = 0; m < d.Materials.Length; m++)
            {
                MaterialIcon.Draw(p.C, m, new Vector2(x, ry + (PhonePainter.RowH - MaterialIcon.Size) / 2));
                x += MaterialIcon.Size + 3;
                x += p.Text(x, ry, $"{_g.Mats[m]}/{d.MaterialMax}", _g.Mats[m] >= d.MaterialMax ? Ink.Late : Ink.Dark) + 12;
            }
        }
        if (_g.BackpackCount() == 0) p.Text(p.Left + 4, info.End.Y + 6, Loc.T("plecak_pusty"), Ink.Dim);
        Sheet.Draw(p);
    }
}
