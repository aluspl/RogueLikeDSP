using System;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Tryb inwestora (run_investor na GBA): modyfikatory trudności po pierwszej wygranej - lista z pastylką stawki
/// (zielona „WŁ” = włączony), opis zaznaczonego, suma: stawka, premia doświadczenia i rekord stawki zawodu.
/// Spacja / dotknięcie zaznaczonego włącza i wyłącza (zapis profilu od razu), Esc wraca do wyboru zawodu.
/// v0.21.51 cz. 2: za modyfikatorami wiersz wyglądu „Kask w paski” (po sekretnym zleceniu Na styk; tylko wygląd);
/// v0.21.52: wiersz koloru kasku (odznaki i zlecenia) – stuknięcie / Spacja zmienia na kolejny; przed pierwszą wygraną
/// strona „Wygląd” bez modyfikatorów. v0.21.52 cz. b: wiersz wariantu mocy (mistrzostwo zawodu 3, wł./wył.) i drugiej
/// pamiątki (poziom inspektora, ranga I); w podsumowaniu nagroda za kolejny stopień inwestora (#48). v0.21.53: wiersz filtra
/// ekranu (zawsze; stuknięcie / Spacja – kolejny odblokowany), lista przewija się, gdy wiersze się nie mieszczą.
/// </summary>
public sealed class InvestorPage : PhonePage
{
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly int _cls;
    private readonly Action _saved;
    private readonly ListState _list = new();

    public InvestorPage(GameData d, Profile p, int cls, Action saved)
    {
        _d = d;
        _p = p;
        _cls = cls;
        _saved = saved;
    }

    /// <summary>Strona dostępna: tryb inwestora (po pierwszej wygranej), kolor kasku, wariant mocy, druga pamiątka, a od
    /// v0.21.53 zawsze – wiersz filtra ekranu (tryby dla daltonistów dostępne od pierwszego uruchomienia).</summary>
    public static bool Available(GameData d, Profile p, int cls) => true;

    private int Inv => Meta.InvestorUnlocked(_p) ? _d.Investor.Length : 0;

    public override string Title => Inv > 0 ? "Tryb inwestora" : "Wygląd";
    public override string Sub => Inv > 0 ? $"Stawka {Investor.Stake(_d, Meta.InvestorMask(_d, _p))}" : "kask i ekran";
    public override string Hint => "Spacja: wł./wył. / zmień  Esc: wróć";
    public override PageAction[] Actions => [new(_list.Sel == HelmetRow || _list.Sel == Keep2Row || _list.Sel == FilterRow ? "Zmień" : "Wł. / wył.", GameAction.A), new("Gotowe", GameAction.Cancel)];
    public override bool Closable => true;

    public int Sel
    {
        get => _list.Sel;
        set => _list.Sel = value;
    }

    /// <summary>Wiersz wyglądu „Kask w paski” (odblokowany sekretnym zleceniem).</summary>
    private bool Stripes => _d.CosmeticStripes >= 0 && Secrets.CosmeticUnlocked(_d, _p, _d.CosmeticStripes);

    /// <summary>v0.21.52: wiersz koloru kasku (z odznak i zleceń).</summary>
    private bool Helmets => Secrets.HelmetsUnlocked(_d, _p) > 0;

    private int StripesRow => Stripes ? Inv : -1;

    private int HelmetRow => Helmets ? Inv + (Stripes ? 1 : 0) : -1;

    /// <summary>v0.21.52 cz. b: wariant mocy (mistrzostwo zawodu) i druga pamiątka (poziom inspektora).</summary>
    private bool Power => Progress.MasteryHas(_d, _p, _cls, ProgressReward.Power);

    private bool Keep2 => Progress.KeepsakeSlot2(_d, _p);

    private int PowerRow => Power ? Inv + (Stripes ? 1 : 0) + (Helmets ? 1 : 0) : -1;

    private int Keep2Row => Keep2 ? Inv + (Stripes ? 1 : 0) + (Helmets ? 1 : 0) + (Power ? 1 : 0) : -1;

    /// <summary>v0.21.53: filtr ekranu – zawsze ostatni wiersz.</summary>
    private int FilterRow => Inv + (Stripes ? 1 : 0) + (Helmets ? 1 : 0) + (Power ? 1 : 0) + (Keep2 ? 1 : 0);

    private int Rows => FilterRow + 1;

    /// <summary>Wiersze widoczne naraz (lista przewija się, gdy wszystkie się nie mieszczą).</summary>
    private int _window = 99;

    /// <summary>Zaznacza wiersz filtra (sceny zrzutów).</summary>
    public void SelectFilterRow()
    {
        _list.Sel = FilterRow;
        Redraw();
    }

    private int Filter => ScreenFilter.Resolve(_d, _p);

    public void Toggle()
    {
        if (Rows == 0) return;
        if (_list.Sel == StripesRow) Secrets.ToggleCosmetic(_d, _p, _d.CosmeticStripes);
        else if (_list.Sel == HelmetRow) Secrets.CycleHelmet(_d, _p, 1);
        else if (_list.Sel == PowerRow) Progress.TogglePowerVariant(_d, _p, _cls);
        else if (_list.Sel == Keep2Row) Meta.CycleKeepsake2(_d, _p, 1);
        else if (_list.Sel == FilterRow) ScreenFilter.Select(_d, ScreenFilters.Next(_d, _p, Filter, 1));
        else Meta.ToggleInvestor(_p, _list.Sel);
        _saved?.Invoke();
        Sfx.Play("buy");
        Redraw();
    }

    public override bool TapRow(int index)
    {
        _list.Sel = index;
        Toggle();
        return true;
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, Rows, _window);
            Sfx.Play("menu");
            return true;
        }
        if (!e.Is(GameAction.A)) return false;
        Toggle();
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        var n = Inv;
        var mask = Meta.InvestorMask(_d, _p);
        var y = p.Section(p.Top, n > 0 ? "MODYFIKATORY" : "WYGLĄD", n > 0 ? "za doświadczenie" : "tylko oprawa");
        _window = Math.Max(3, (int)((p.Bottom - y - 8 - 6 - (3 * PhonePainter.RowH + 8)) / PhonePainter.RowH));
        _list.Clamp(Rows, _window);
        var shown = Math.Min(Rows, _window);
        var card = p.Card(y, shown);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var k = 0; k < shown; k++)
        {
            var i = _list.Top + k;
            if (i == FilterRow) // v0.21.53: filtr ekranu (tryby dla daltonistów zawsze)
            {
                var fd = _d.ScreenFilters[Filter];
                var fsel = i == _list.Sel;
                var fy = p.RowY(card, k);
                if (fsel) p.Selected(card, k);
                else if (k > 0) p.Divider(card, k);
                p.HitRow(card, k, i);
                var fpw = p.Pill(right, fy, fd.Short, Filter > 0 ? PillKind.Done : PillKind.Gray);
                p.Text(tx, fy, _d.FilterText("lookRow") + fd.Name, fsel ? Ink.Brand : Filter > 0 ? Ink.Dark : Ink.Dim, TextAlign.Left, right - fpw - 4 - tx);
                continue;
            }
            if (i == PowerRow || i == Keep2Row) // v0.21.52 cz. b: wariant mocy / druga pamiątka
            {
                var power = i == PowerRow;
                var k2 = Meta.SelectedKeepsake2(_d, _p);
                var won = power ? Progress.PowerVariantOn(_d, _p, _cls) : k2 >= 0;
                var vsel = i == _list.Sel;
                var vy = p.RowY(card, k);
                if (vsel) p.Selected(card, k);
                else if (k > 0) p.Divider(card, k);
                if (won && !vsel) p.Stripe(card, k, Pal.Done);
                p.HitRow(card, k, i);
                var vpw = p.Pill(right, vy, power ? (won ? "Wariant" : "Zwykła") : (won ? "Ranga I" : "Wybierz"), won ? PillKind.Done : PillKind.Gray);
                var vlabel = power ? "Moc: " + (won ? _d.MasteryClasses[_cls].PowerName : _d.Classes[_cls].AbilityName)
                                   : "Pamiątka 2: " + (k2 >= 0 ? _d.Keepsakes[k2].Name : "brak");
                p.Text(tx, vy, vlabel, vsel ? Ink.Brand : won ? Ink.Dark : Ink.Dim, TextAlign.Left, right - vpw - 4 - tx);
                continue;
            }
            if (i == StripesRow || i == HelmetRow) // wygląd: kask w paski / kolor kasku
            {
                var stripes = i == StripesRow;
                var hk = Secrets.HelmetCosmetic(_d, _p, _cls);
                var son = stripes ? Secrets.CosmeticOn(_d, _p, _d.CosmeticStripes) : hk >= 0;
                var ssel = i == _list.Sel;
                var sy = p.RowY(card, k);
                if (ssel) p.Selected(card, k);
                else if (k > 0) p.Divider(card, k);
                p.HitRow(card, k, i);
                var spw = p.Pill(right, sy, stripes ? (son ? "WŁ" : "Wygląd") : "Wygląd", son ? PillKind.Done : PillKind.Gray);
                p.Icon(Assets.UiMenu, Assets.MenuStripes, Assets.Icon, new Vector2(tx - 2, sy + (PhonePainter.RowH - 16) / 2f));
                var label = stripes ? _d.Cosmetics[_d.CosmeticStripes].Name : "Kask: " + (hk >= 0 ? _d.Cosmetics[hk].Name : "zawodu");
                p.Text(tx + 18, sy, label, ssel ? Ink.Brand : son ? Ink.Dark : Ink.Dim, TextAlign.Left, right - spw - 4 - tx - 18);
                if (!stripes && hk >= 0) // próbka koloru kasku
                {
                    var c = _d.Cosmetics[hk].Helmet;
                    p.C.DrawRect(new Rect2(right - spw - 18, sy + PhonePainter.RowH / 2f - 5, 10, 10), Color.Color8((byte)(c >> 16), (byte)((c >> 8) & 255), (byte)(c & 255)));
                }
                continue;
            }
            var m = _d.Investor[i];
            var on = ((mask >> i) & 1) != 0;
            var sel = i == _list.Sel;
            var ry = p.RowY(card, k);
            if (sel) p.Selected(card, k);
            else if (k > 0) p.Divider(card, k);
            if (on && !sel) p.Stripe(card, k, Pal.Done);
            p.HitRow(card, k, i);
            var pw = p.Pill(right, ry, on ? $"WŁ +{m.Stake}" : $"+{m.Stake}", on ? PillKind.Done : PillKind.Gray);
            p.Text(tx, ry, m.Name, sel ? Ink.Brand : on ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
        }
        var dc = p.Card(card.End.Y + 6, 3);
        string desc;
        if (_list.Sel == StripesRow) desc = "Wygląd: " + _d.Cosmetics[_d.CosmeticStripes].Desc;
        else if (_list.Sel == PowerRow) desc = $"Wariant mocy: {_d.MasteryClasses[_cls].PowerDesc} (mistrzostwo zawodu)";
        else if (_list.Sel == Keep2Row)
        {
            var k2 = Meta.SelectedKeepsake2(_d, _p);
            desc = k2 >= 0 ? "Druga pamiątka: " + RunMods.PerkLabel(Meta.Keepsake2Perk(_d, k2)) : "Druga pamiątka (ranga I), inna niż pierwsza";
        }
        else if (_list.Sel == FilterRow)
        {
            var fd = _d.ScreenFilters[Filter];
            desc = $"{_d.FilterText("lookDesc")}{fd.Desc} ({_d.FilterText("count")}: {ScreenFilters.UnlockedCount(_d, _p)}/{_d.ScreenFilters.Length})";
        }
        else if (_list.Sel == HelmetRow)
        {
            var hk = Secrets.HelmetCosmetic(_d, _p, _cls);
            desc = (hk >= 0 ? "Wygląd: " + _d.Cosmetics[hk].Desc : "Kolor kasku zawodu") + $" (kolorów: {Secrets.HelmetsUnlocked(_d, _p)})";
        }
        else desc = n > 0 ? $"{_d.Investor[_list.Sel].Desc}, dośw. +{_d.Investor[_list.Sel].XpPct}%" : "";
        p.Text(tx, p.RowY(dc, 0), desc, Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(dc, 1);
        if (n == 0)
        {
            p.Text(tx, p.RowY(dc, 1), "Kolory kasku: odznaki i zlecenia", Ink.Dark, TextAlign.Left, right - tx);
            p.Divider(dc, 2);
            p.Text(tx, p.RowY(dc, 2), "Tryb inwestora: po pierwszej wygranej", Ink.Dim, TextAlign.Left, right - tx);
            return;
        }
        p.Text(tx, p.RowY(dc, 1), $"Stawka {Investor.Stake(_d, mask)}, dośw. +{Investor.Xp(_d, mask)}%, rekord {Meta.BestStake(_p, _cls)}", Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(dc, 2); // v0.21.52 cz. b (#48): nagroda za kolejny stopień inwestora (najwyższa stawka wygranej budowy)
        var rk = Progress.StakeRank(_d, _p);
        var next = rk < _d.StakeRanks.Length ? $"Stawka {_d.StakeRanks[rk].Xp}: {Progress.RewardLabel(_d, _d.StakeRanks[rk], _cls)}" : "Stopnie inwestora: wszystkie";
        var npw = p.Pill(right, p.RowY(dc, 2), rk < _d.StakeRanks.Length ? $"{rk}/{_d.StakeRanks.Length}" : "MAX", rk < _d.StakeRanks.Length ? PillKind.Prog : PillKind.Done);
        p.Stripe(dc, 2, Pal.Prog);
        p.Text(tx, p.RowY(dc, 2), next, Ink.Prog, TextAlign.Left, right - npw - 4 - tx);
    }
}
