using System.Linq;
using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.ProfileTabs;

/// <summary>
/// Koszty (zakładka 4 profilu na GBA) ze stronami przełączanymi Tab (SELECT na GBA) albo przyciskiem strony na dotyku:
/// Szkolenia - „Pozostało” doświadczenia, lista ulepszeń, zablokowanych zawodów, narzędzi, fachowców brygady
/// i najwyższej trudności; Drzewko (v0.21.52 cz. c) - 3 gałęzie (Fach, BHP, Logistyka): pień = Szkolenia gałęzi,
/// w węzłach wybór 1 z 2 (strzałki, Spacja / stuknięcie wybiera, zmiana za opłatą); Respekt - stałe premie z rangami za
/// Respekt z ukończonych etapów; Nagrody - nagrody za odbiór (każda wygrana odblokowuje kolejną); v0.23.0: Gablota artefaktów.
/// Spacja/Enter kupuje.
/// </summary>
public sealed class TrainingTab : PhonePage
{
    private const int Window = 7;

    private readonly GameData _d;
    private readonly Profile _p;
    private readonly Action _saved;
    private readonly ListState _list = new();
    private readonly List<(TrainingKind K, int I)> _entries = new();
    private string _note = "";
    private static string[] Pages => [Loc.T("szkolenia"), Loc.T("drzewko_2"), Loc.T("respekt"), Loc.T("nagrody"), Loc.T("gablota")];
    public const int TreePage = 1;
    public const int RespectPage = 2;
    public const int RewardsPage = 3;
    public const int CasePage = 4;
    private readonly ArtifactCaseView _case;

    /// <summary>Gablota (zaznaczenie kafla w scenach zrzutów).</summary>
    public ArtifactCaseView Case => _case;
    private int _page;
    private int _tcol, _trow; // drzewko: gałąź i wiersz (węzeł I / II, opcja A / B)

    public TrainingTab(GameData d, Profile p, Action saved)
    {
        _d = d;
        _p = p;
        _saved = saved;
        _case = new ArtifactCaseView(d, p);
        Rebuild();
    }

    public override string Title => Pages[_page];
    public override string Sub => _page == CasePage ? _case.Sub : _page == RewardsPage ? Loc.F("wygrane_4", _p.Wins) : Loc.F("dosw_5", _p.Xp); // v0.22.0 (UX A): doświadczenie zamiast dubla „Koszty”
    public override string Hint => _page >= RewardsPage ? Loc.F("tab_q_e_zakladki_esc_wroc", Pages[(_page + 1) % Pages.Length])
                                   : _page == TreePage ? Loc.F("strzalki_wezel_spacja_wybierz", Pages[_page + 1])
                                   : Loc.F("spacja_kup_tab_q_e_zakladki", Pages[_page + 1]);
    public override PageAction[] Actions => _page >= RewardsPage ? [new(Pages[(_page + 1) % Pages.Length] + " >", GameAction.Select)]
                                            : _page == TreePage ? [new(Loc.T("wybierz"), GameAction.A), new(Pages[_page + 1] + " >", GameAction.Select)]
                                            : [new(Loc.T("kup"), GameAction.A), new(Pages[_page + 1] + " >", GameAction.Select)];

    public override bool TapRow(int index)
    {
        if (_page == CasePage) return _case.Tap(index);
        if (_page == TreePage) // drzewko: stuknięcie zaznacza węzeł, drugie wybiera
        {
            if (index >= BranchHit) // v0.22.1 (#70): przełącznik gałęzi
            {
                _tcol = index - BranchHit;
                _note = "";
                return true;
            }
            int col = index / 4, row = index % 4;
            if (col == _tcol && row == _trow) Choose();
            else
            {
                _tcol = col;
                _trow = row;
                _note = "";
            }
            return true;
        }
        if (index == _list.Sel && _page < TreePage) Buy();
        else if (index == _list.Sel && _page == RespectPage) BuyRespect();
        else _note = "";
        _list.Sel = index;
        return true;
    }

    /// <summary>Liczba wierszy bieżącej strony.</summary>
    public int Count => _page == CasePage ? 0 : _page == RewardsPage ? _d.Rewards.Length : _page == RespectPage ? _d.Respect.Length : _page == TreePage ? 0 : _entries.Count;

    /// <summary>Strona: 0 Szkolenia, 1 Drzewko, 2 Respekt, 3 Nagrody (test dymny, sceny zrzutów).</summary>
    public int Page
    {
        get => _page;
        set
        {
            _page = ((value % Pages.Length) + Pages.Length) % Pages.Length;
            _list.Reset();
            _note = "";
        }
    }

    /// <summary>Zaznacz wiersz (sceny zrzutów).</summary>
    public void Select(int index)
    {
        _list.Sel = Math.Clamp(index, 0, Math.Max(0, Count - 1));
        _list.Clamp(Count, Window);
    }

    private void Rebuild()
    {
        _entries.Clear();
        for (var i = 0; i < _d.Upgrades.Length; i++) _entries.Add((TrainingKind.Upgrade, i));
        for (var i = 0; i < _d.Classes.Length; i++)   // zawody i narzędzia z nagród za odbiór nie są na sprzedaż
        {
            if (!Meta.ClassUnlocked(_d, _p, i) && !Meta.ClassReward(_d, i)) _entries.Add((TrainingKind.Class, i));
        }
        for (var i = 0; i < _d.Tools.Length; i++)
        {
            if (!Meta.ToolUnlocked(_d, _p, i) && _d.Tools[i].Shop) _entries.Add((TrainingKind.Tool, i));
        }
        for (var i = 0; i < _d.Brigade.Length; i++)
        {
            if (!Meta.HelperUnlocked(_d, _p, i)) _entries.Add((TrainingKind.Helper, i));
        }
        if (!Meta.DifficultyUnlocked(_d, _p, _d.Difficulties.Length - 1)) _entries.Add((TrainingKind.Hard, 0));
    }

    private int Cost((TrainingKind K, int I) e) => e.K switch
    {
        TrainingKind.Upgrade => Meta.UpgradeCost(_d, _p, e.I),
        TrainingKind.Class => Meta.ClassCost(_d, _p), // v0.21.52: każdy kolejny drożej
        TrainingKind.Tool => Meta.ToolCost(_d, _p),
        TrainingKind.Helper => _d.Brigade[e.I].Cost,
        _ => _d.HardCost,
    };

    private string Name((TrainingKind K, int I) e) => e.K switch
    {
        TrainingKind.Upgrade => $"{_d.Upgrades[e.I].Name} {_p.Levels[e.I]}/{_d.Upgrades[e.I].Levels}",
        TrainingKind.Class => Loc.T("zawod_3") + _d.Classes[e.I].Name,
        TrainingKind.Tool => _d.Weapons[_d.Tools[e.I].Weapon].Name,
        TrainingKind.Helper => Loc.T("brygada_2") + _d.Brigade[e.I].Name,
        _ => Loc.T("trudnosc") + _d.Difficulties[^1].Name,
    };

    private string Desc((TrainingKind K, int I) e)
    {
        if (e.K == TrainingKind.Upgrade) return UpgradeDesc(e.I);
        if (e.K == TrainingKind.Class) return Loc.T("nowy_zawod_do_wyboru") + _d.Classes[e.I].AbilityName + Loc.T("kazdy_kolejny_drozej");
        if (e.K == TrainingKind.Tool)
        {
            var w = _d.Weapons[_d.Tools[e.I].Weapon];
            return Loc.F("narzedzie_z", w.MinDamage, w.MaxDamage, w.Range, UiText.StatShort(w.ScalesWith));
        }
        if (e.K == TrainingKind.Helper) return Loc.F("wezwanie_zl", _d.Brigade[e.I].Desc, _d.Brigade[e.I].Price);
        return Loc.T("najwyzsza_trudnosc");
    }

    /// <summary>
    /// v0.21.52: Szkolenie z poziomami – co da kolejny poziom („Poziom III: +1 HP na start”), ile już daje i opis;
    /// na maksimum – razem.
    /// </summary>
    private string UpgradeDesc(int i)
    {
        var u = _d.Upgrades[i];
        var lv = _p.Levels[i];
        if (lv >= u.Levels) return Loc.T("razem") + Meta.UpgradeSummary(_d, i, lv);
        var next = Loc.F("poziom_4", UiText.Roman(lv), RunMods.UpgradeLabel(u.Steps[lv].Effect, u.Steps[lv].Value));
        return lv > 0 ? Loc.F("teraz_4", next, Meta.UpgradeSummary(_d, i, lv)) : $"{next} – {u.Desc}";
    }

    /// <summary>Drzewko: zaznacz węzeł (gałąź, wiersz 0-3: węzeł I A/B, węzeł II A/B) – test dymny, sceny zrzutów.</summary>
    public void SelectNode(int col, int row)
    {
        _tcol = Math.Clamp(col, 0, _d.TreeBranches.Length - 1);
        _trow = Math.Clamp(row, 0, 3);
        _note = "";
    }

    /// <summary>Węzeł drzewka w gałęzi b na poziomie tier (0 = I, 1 = II); -1 = brak.</summary>
    private int NodeOf(int b, int tier)
    {
        var k = 0;
        for (var n = 0; n < _d.TreeNodes.Length; ++n)
        {
            if (_d.TreeNodes[n].Branch == b && k++ == tier) return n;
        }
        return -1;
    }

    /// <summary>Drzewko: wybierz zaznaczoną opcję (pierwszy wybór albo zmiana za opłatą).</summary>
    public bool Choose()
    {
        var n = NodeOf(_tcol, _trow / 2);
        if (n < 0) return false;
        var o = _trow % 2;
        if (SkillTree.Choose(_d, _p, n, o))
        {
            Sfx.Play("buy");
            _note = Loc.T("wybrane");
            _saved?.Invoke();
            return true;
        }
        _note = !SkillTree.Open(_d, _p, n) ? Loc.T("najpierw_szkolenia_tej_galezi") : SkillTree.Cost(_d, _p, n, o) < 0 ? Loc.T("juz_wybrane") : Loc.T("za_malo_doswiadczenia");
        return false;
    }

    /// <summary>Kup zaznaczoną pozycję (także z testu dymnego).</summary>
    public bool Buy()
    {
        if (_page == TreePage) return Choose();
        if (_page == RespectPage) return BuyRespect();
        if (_page == RewardsPage || _entries.Count == 0) return false;
        var e = _entries[_list.Sel];
        var ok = e.K switch
        {
            TrainingKind.Upgrade => Meta.BuyUpgrade(_d, _p, e.I),
            TrainingKind.Class => Meta.BuyClass(_d, _p, e.I),
            TrainingKind.Tool => Meta.BuyTool(_d, _p, e.I),
            TrainingKind.Helper => Meta.BuyHelper(_d, _p, e.I),
            _ => Meta.BuyHard(_d, _p),
        };
        if (ok)
        {
            Sfx.Play("buy");
            _note = Loc.T("kupione");
            Rebuild();
            _list.Clamp(_entries.Count, Window);
            _saved?.Invoke();
        }
        else
        {
            _note = e.K == TrainingKind.Upgrade && Meta.UpgradeCost(_d, _p, e.I) < 0 ? Loc.T("maksymalny_poziom") : Loc.T("za_malo_doswiadczenia");
        }
        return ok;
    }

    /// <summary>Kup kolejną rangę zaznaczonego ulepszenia Respektu.</summary>
    public bool BuyRespect()
    {
        var i = _list.Sel;
        if (i < 0 || i >= _d.Respect.Length) return false;
        if (Meta.BuyRespect(_d, _p, i))
        {
            Sfx.Play("buy");
            _note = Loc.T("kupione");
            _saved?.Invoke();
            return true;
        }
        _note = !Meta.RespectUnlocked(_d, _p, i) ? Loc.T("najpierw_sekretne_zlecenie")
              : Meta.RespectCost(_d, _p, i) < 0 ? Loc.T("maksymalna_ranga") : Loc.T("za_malo_respektu_koncz_etapy");
        return false;
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (_page == TreePage && (v != 0 || e.HDir != 0)) // drzewko: strzałki po węzłach (Q/E dalej zmieniają zakładki)
        {
            _trow = (_trow + v + 4) % 4;
            _tcol = (_tcol + e.HDir + _d.TreeBranches.Length) % _d.TreeBranches.Length;
            _note = "";
            Sfx.Play("menu");
            return true;
        }
        if (_page == CasePage && _case.Move(e.HDir, v)) // v0.23.0: Gablota – strzałki po kaflach
        {
            Sfx.Play("menu");
            return true;
        }
        if (_page == CasePage && v != 0) return true;
        if (v != 0)
        {
            _list.Move(v, Count, Window);
            _note = "";
            return true;
        }
        if (e.Is(GameAction.Select))   // Szkolenia -> Drzewko -> Respekt -> Nagrody
        {
            Page = _page + 1;
            Sfx.Play("menu");
            return true;
        }
        if (e.Is(GameAction.A | GameAction.Start) && _page < RewardsPage)
        {
            Buy();
            return true;
        }
        return false;
    }

    public override void Draw(PhonePainter p)
    {
        if (_page == TreePage)
        {
            DrawTree(p);
            return;
        }
        if (_page == RespectPage)
        {
            DrawRespect(p);
            return;
        }
        if (_page == RewardsPage)
        {
            DrawRewards(p);
            return;
        }
        if (_page == CasePage)
        {
            _case.Draw(p);
            return;
        }
        var c0 = p.Card(p.Top, 1);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        p.Text(tx, p.RowY(c0, 0), Loc.T("pozostalo"), Ink.Dim);
        p.Text(right, p.RowY(c0, 0), Loc.F("dosw_5", _p.Xp), Ink.Done, TextAlign.Right);

        _list.Clamp(_entries.Count, Window);
        var rows = Math.Min(Window, _entries.Count - _list.Top);
        var card = p.Card(c0.End.Y + 6, Math.Max(rows, 1));
        for (var r = 0; r < rows; r++)
        {
            var i = _list.Top + r;
            var e = _entries[i];
            var y = p.RowY(card, r);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            p.HitRow(card, r, i);
            var cost = Cost(e);
            var pw = p.Pill(right, y, cost < 0 ? "MAX" : cost.ToString(), cost < 0 ? PillKind.Done : cost <= _p.Xp ? PillKind.Group : PillKind.Gray);
            p.Text(tx, y, Name(e), sel ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        }
        var dc = p.Card(card.End.Y + 6, 2);
        var desc = _entries.Count > 0 ? Desc(_entries[_list.Sel]) : Loc.T("wszystko_kupione");
        var lines = p.F.Wrap(desc, (int)(right - tx));
        p.Text(tx, p.RowY(dc, 0), lines.Count > 0 ? lines[0] : "", Ink.Dim);
        if (_note.Length > 0) p.Text(tx, p.RowY(dc, 1), _note, _note == Loc.T("kupione") ? Ink.Done : Ink.Late);
        else if (lines.Count > 1) p.Text(tx, p.RowY(dc, 1), lines[1], Ink.Dim);
        else p.Text(tx, p.RowY(dc, 1), Loc.F("wydano", Meta.ShopSpent(_d, _p), Meta.ShopTotalCost(_d)), Ink.Brand);
    }

    private const int BranchHit = 1000;

    /// <summary>Indeks celu stuknięcia przełącznika gałęzi b (test dymny).</summary>
    public static int BranchTap(int b) => BranchHit + b;

    /// <summary>Wybrana gałąź drzewka (test dymny).</summary>
    public int Branch => _tcol;

    /// <summary>
    /// v0.22.1 (#70): drzewko – u góry przełącznik gałęzi (nazwa i pień), pod nim karta wybranej gałęzi: dwa węzły, każdy
    /// z dwiema opcjami obok siebie (nazwa, efekt, cena; wybrana – zielona, zaznaczona – fioletowa ramka, zamknięta –
    /// szara) i stanem węzła („wybierz 1 z 2”, „wybrane”, „od pnia N”); na dole opis zaznaczonej opcji i pień gałęzi.
    /// </summary>
    private void DrawTree(PhonePainter p)
    {
        var f = p.F;
        var nb = _d.TreeBranches.Length;
        var rowH = PhonePainter.RowH;
        var full = PhoneView.Full;

        // przełącznik gałęzi
        var sw = p.CardH(p.Top, rowH + 10);
        var seg = (sw.Size.X - 8) / nb;
        for (var b = 0; b < nb; ++b)
        {
            var r = new Rect2(sw.Position.X + 4 + b * seg, sw.Position.Y + 3, seg - 4, rowH + 4);
            int lv = SkillTree.BranchLevels(_d, _p, b), max = SkillTree.BranchMax(_d, b);
            var on = b == _tcol;
            if (on) p.C.DrawStyleBox(Ui.Box(Pal.Brand, 6), r);
            var label = $"{_d.TreeBranches[b].Name} {lv}/{max}";
            if (f.Measure(label) > r.Size.X - 8) label = _d.TreeBranches[b].Name;
            f.Draw(p.C, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 9)), f.Fit(label, (int)r.Size.X - 8), on ? Ink.OnBrand : lv >= max ? Ink.Done : Ink.Dark, TextAlign.Center, 1, on);
            var bar = new Rect2(r.Position.X + 6, r.End.Y - 3, r.Size.X - 12, 2);
            p.C.DrawRect(bar, on ? new Color(Pal.Card, 0.35f) : Pal.Border);
            p.C.DrawRect(new Rect2(bar.Position, new Vector2(Mathf.Round(bar.Size.X * (max > 0 ? lv / (float)max : 0)), 2)), on ? Pal.Card : lv >= max ? Pal.Done : Pal.Brand);
            p.Hit(r, BranchHit + b);
        }

        // karta gałęzi: dwa węzły, opcje obok siebie
        var cellH = full ? 3 * 16 + 12 : 2 * 16 + 8;
        var nodeH = rowH + cellH + 8;
        var top = sw.End.Y + 6;
        var bc = p.CardH(top, 2 * nodeH + 4);
        var x0 = bc.Position.X + 6;
        var cw = (bc.Size.X - 18) / 2;
        var y = bc.Position.Y + 4;
        for (var tier = 0; tier < 2; ++tier)
        {
            var n = NodeOf(_tcol, tier);
            if (n < 0) continue;
            var node = _d.TreeNodes[n];
            var open = SkillTree.Open(_d, _p, n);
            var pick = SkillTree.Pick(_p, n);
            if (tier > 0) p.C.DrawRect(new Rect2(bc.Position.X + 6, y - 4, bc.Size.X - 12, 1), Pal.Border);
            p.Bold(x0 + 2, y + PhonePainter.TextDy, Loc.F("wezel", UiText.Roman(tier)), open ? Ink.Dark : Ink.Dim);
            var state = !open ? Loc.F("od_pnia", node.Depth) : pick > 0 ? Loc.T("wybrane") : Loc.T("wybierz_1_z_2");
            p.Pill(bc.End.X - 8, y, state, !open ? PillKind.Gray : pick > 0 ? PillKind.Done : PillKind.Group);
            y += rowH;
            for (var o = 0; o < 2; ++o)
            {
                var r = tier * 2 + o;
                var cell = new Rect2(x0 + o * (cw + 6), y, cw, cellH);
                var picked = pick == o + 1;
                var cur = r == _trow;
                p.C.DrawStyleBox(Ui.Box(picked ? Pal.DoneBg : open ? Pal.Group : Pal.Card, 8, cur ? Pal.Brand : picked ? Pal.Done : Pal.Border), cell);
                if (cur) p.C.DrawStyleBox(Ui.Box(Colors.Transparent, 8, Pal.Brand), cell.Grow(1));
                p.Hit(cell, _tcol * 4 + r);
                var opt = node.Options[o];
                var ink = picked ? Ink.Done : !open ? Ink.Dim : cur ? Ink.Brand : Ink.Dark;
                var tw = (int)cell.Size.X - 12;
                var name = f.Measure(opt.Name, 1, true) <= tw ? opt.Name : opt.Short;
                f.Draw(p.C, new Vector2(cell.Position.X + 6, cell.Position.Y + 4), f.Fit(name, tw), ink, TextAlign.Left, 1, true);
                var oc = SkillTree.Cost(_d, _p, n, o);
                var price = oc < 0 ? Loc.T("masz_3") : oc.ToString();
                var opw = f.Measure(price) + 4;
                f.Draw(p.C, new Vector2(cell.Position.X + 6, cell.Position.Y + 20), f.Fit(RunMods.UpgradeLabel(opt.Effect, opt.Value), tw - (int)opw - 4), open ? Ink.Brand : Ink.Dim);
                f.Draw(p.C, new Vector2(cell.End.X - 6, cell.Position.Y + 20), price, oc < 0 ? Ink.Done : open && oc <= _p.Xp ? Ink.Dark : Ink.Dim, TextAlign.Right);
                if (full) f.Draw(p.C, new Vector2(cell.Position.X + 6, cell.Position.Y + 36), f.Fit(opt.Desc, tw), Ink.Dim);
            }
            y += cellH + 8;
        }
        top = bc.End.Y;
        var colH = 0f;
        // opis zaznaczonej opcji i pień gałęzi (Szkolenia z poziomami)
        var dc = p.Card(top + colH + 6, 3);
        var tx = p.TextX(dc);
        var right = dc.End.X - 6;
        var sn = NodeOf(_tcol, _trow / 2);
        if (sn < 0) return;
        var so = _trow % 2;
        var sop = _d.TreeNodes[sn].Options[so];
        var cost = SkillTree.Cost(_d, _p, sn, so);
        var spick = SkillTree.Pick(_p, sn);
        var sopen = SkillTree.Open(_d, _p, sn);
        var pill = !sopen ? Loc.F("pien_3", _d.TreeNodes[sn].Depth) : cost < 0 ? Loc.T("masz_3") : spick > 0 ? Loc.F("zmiana_2", cost) : cost.ToString();
        var pw = p.Pill(right, p.RowY(dc, 0), pill, !sopen ? PillKind.Gray : cost < 0 ? PillKind.Done : cost <= _p.Xp ? PillKind.Group : PillKind.Gray);
        p.Text(tx, p.RowY(dc, 0), $"{sop.Name}: {RunMods.UpgradeLabel(sop.Effect, sop.Value)}", Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        var info = _note.Length > 0 ? _note : $"{sop.Desc}. {(spick > 0 && cost >= 0 ? Loc.F("zmiana_wyboru_dosw", _d.TreeRespecCost) : Loc.T("druga_opcje_zmienisz_za_oplata"))}";
        p.Text(tx, p.RowY(dc, 1), info, _note.Length > 0 ? (_note == Loc.T("wybrane") ? Ink.Done : Ink.Late) : Ink.Dim, TextAlign.Left, right - tx);
        var trunk = string.Join(", ", Enumerable.Range(0, _d.Upgrades.Length).Where(i => ((_d.TreeBranches[_tcol].Upgrades >> i) & 1) != 0)
            .Select(i => $"{_d.Upgrades[i].Name} {_p.Levels[i]}/{_d.Upgrades[i].Levels}"));
        p.Divider(dc, 2);
        p.Text(tx, p.RowY(dc, 2), Loc.T("pien_4") + trunk, Ink.Brand, TextAlign.Left, right - tx);
    }

    private void DrawRespect(PhonePainter p)
    {
        var c0 = p.Card(p.Top, 1);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        p.Text(tx, p.RowY(c0, 0), Loc.T("masz_3"), Ink.Dim);
        p.Text(right, p.RowY(c0, 0), Loc.F("respektu_3", _p.Respect), Ink.Done, TextAlign.Right);
        var n = _d.Respect.Length;
        _list.Clamp(n, Window);
        var rows = Math.Min(Window, n - _list.Top);
        var card = p.Card(c0.End.Y + 6, rows);
        for (var r = 0; r < rows; r++)
        {
            var i = _list.Top + r;
            var y = p.RowY(card, r);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            p.HitRow(card, r, i);
            var cost = Meta.RespectCost(_d, _p, i);
            var rd = _d.Respect[i];
            if (!Meta.RespectUnlocked(_d, _p, i)) // v0.21.51 cz. 2: ranga z sekretnego zlecenia (Zaprawiony w boju)
            {
                var lw = p.Pill(right, y, Loc.T("sekret_2"), PillKind.Gray);
                p.Icon(Assets.UiMenu, Assets.MenuSecret, Assets.Icon, new Vector2(tx - 2, y + (PhonePainter.RowH - 16) / 2f));
                p.Text(tx + 18, y, "???", sel ? Ink.Brand : Ink.Dim, TextAlign.Left, right - lw - 4 - tx - 18);
                continue;
            }
            var pw = p.Pill(right, y, cost < 0 ? "MAX" : cost.ToString(), cost < 0 ? PillKind.Done : cost <= _p.Respect ? PillKind.Group : PillKind.Gray);
            p.Text(tx, y, $"{rd.Name} {Meta.RespectRank(_d, _p, i)}/{rd.Ranks}", sel ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        }
        var dc = p.Card(card.End.Y + 6, 2);
        var s = _list.Sel;
        var def = _d.Respect[s];
        var rank = Meta.RespectRank(_d, _p, s);
        if (!Meta.RespectUnlocked(_d, _p, s))
        {
            p.Text(tx, p.RowY(dc, 0), Loc.T("sekret_3") + _d.Secrets[def.Secret].Hint, Ink.Dim, TextAlign.Left, right - tx);
            p.Text(tx, p.RowY(dc, 1), _note.Length > 0 ? _note : Loc.T("odblokujesz_sekretnym"), _note.Length > 0 ? Ink.Late : Ink.Brand, TextAlign.Left, right - tx);
            return;
        }
        var now = rank > 0 ? RunMods.RespectLabel(def.Effect, Meta.RespectValue(_d, _p, s)) : def.Desc + Loc.T("brak_3");
        p.Text(tx, p.RowY(dc, 0), now, Ink.Dark, TextAlign.Left, right - tx);
        if (_note.Length > 0) p.Text(tx, p.RowY(dc, 1), _note, _note == Loc.T("kupione") ? Ink.Done : Ink.Late, TextAlign.Left, right - tx);
        else if (rank < def.Ranks) p.Text(tx, p.RowY(dc, 1), Loc.T("dalej_4") + RunMods.RespectLabel(def.Effect, def.Values[rank]), Ink.Brand, TextAlign.Left, right - tx);
        else p.Text(tx, p.RowY(dc, 1), Loc.F("wydano", Meta.RespectSpent(_d, _p), Meta.RespectTotalCost(_d)), Ink.Dim);
    }

    private void DrawRewards(PhonePainter p)
    {
        var next = _p.Rewards;
        var c0 = p.Card(p.Top, 1);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        var head = next < Meta.RewardsAvailable(_d) ? Loc.T("za_wygrana") + _d.Rewards[next].Name : Loc.T("wszystko_odebrane_wiecej");
        p.Text(tx, p.RowY(c0, 0), head, Ink.Brand, TextAlign.Left, right - tx);
        var n = _d.Rewards.Length;
        _list.Clamp(n, Window);
        var rows = Math.Min(Window, n - _list.Top);
        var card = p.Card(c0.End.Y + 6, rows);
        for (var r = 0; r < rows; r++)
        {
            var i = _list.Top + r;
            var rw = _d.Rewards[i];
            var y = p.RowY(card, r);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            p.HitRow(card, r, i);
            var got = Meta.RewardOwned(_p, i);
            var pill = got ? Loc.T("odebrana") : rw.Kind == RewardKind.Soon ? Loc.T("wkrotce_2") : i == next ? Loc.T("nastepna") : Loc.F("wygr_3", Meta.RewardWin(_d, _p, i));
            var pw = p.Pill(right, y, pill, got ? PillKind.Done : i == next ? PillKind.Prog : PillKind.Gray);
            var icon = new Rect2(tx, y + (PhonePainter.RowH - 16) / 2f, 16, 16);
            if (rw.Kind == RewardKind.Cls)
                p.C.DrawTextureRectRegion(Assets.Actors, icon, Assets.Frame(got ? _d.Classes[rw.Index].Frame : Assets.Silhouette(rw.Index), Assets.Actor));
            else p.C.DrawTextureRectRegion(Assets.UiMenu, icon, Assets.Frame(Assets.RewardIcon(_d, rw), Assets.Icon));
            p.Text(tx + 20, y, rw.Name, sel ? Ink.Brand : got ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 24 - tx);
        }
        var dc = p.Card(card.End.Y + 6, 1);
        p.Text(tx, p.RowY(dc, 0), RewardDesc(_d.Rewards[_list.Sel]), Ink.Dim, TextAlign.Left, right - tx);
    }

    private string RewardDesc(RewardDef r)
    {
        switch (r.Kind)
        {
            case RewardKind.Tool:
            {
                var w = _d.Weapons[_d.Tools[r.Index].Weapon];
                return $"{w.Name} {w.MinDamage}-{w.MaxDamage} z{w.Range}, {UiText.StatShort(w.ScalesWith)}";
            }
            case RewardKind.Gear:
            {
                var g = _d.Gear[r.Index * 3 + 2];
                return $"{_d.GearSlots[r.Index]}: {UiText.GearStatName(g.Stat)} do +{g.Value}";
            }
            case RewardKind.Cls:
                return $"{_d.Classes[r.Index].AbilityName}: {_d.Classes[r.Index].AbilityDesc}";
            default:
                return r.Desc;
        }
    }
}
