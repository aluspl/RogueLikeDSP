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
/// Respekt z ukończonych etapów; Nagrody - nagrody za odbiór (każda wygrana odblokowuje kolejną). Spacja/Enter kupuje.
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
    private static readonly string[] Pages = ["Szkolenia", "Drzewko", "Respekt", "Nagrody"];
    public const int TreePage = 1;
    public const int RespectPage = 2;
    public const int RewardsPage = 3;
    private int _page;
    private int _tcol, _trow; // drzewko: gałąź i wiersz (węzeł I / II, opcja A / B)

    public TrainingTab(GameData d, Profile p, Action saved)
    {
        _d = d;
        _p = p;
        _saved = saved;
        Rebuild();
    }

    public override string Title => Pages[_page];
    public override string Sub => _page == RewardsPage ? $"Wygrane {_p.Wins}" : _page == TreePage ? $"{_p.Xp} dośw." : "Koszty";
    public override string Hint => _page == RewardsPage ? $"Tab: {Pages[0]}  Q/E: zakładki  Esc: wróć"
                                   : _page == TreePage ? $"Strzałki: węzeł  Spacja: wybierz  Tab: {Pages[_page + 1]}"
                                   : $"Spacja: kup  Tab: {Pages[_page + 1]}  Q/E: zakładki";
    public override PageAction[] Actions => _page == RewardsPage ? [new(Pages[0] + " >", GameAction.Select)]
                                            : _page == TreePage ? [new("Wybierz", GameAction.A), new(Pages[_page + 1] + " >", GameAction.Select)]
                                            : [new("Kup", GameAction.A), new(Pages[_page + 1] + " >", GameAction.Select)];

    public override bool TapRow(int index)
    {
        if (_page == TreePage) // drzewko: stuknięcie zaznacza węzeł, drugie wybiera
        {
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
    public int Count => _page == RewardsPage ? _d.Rewards.Length : _page == RespectPage ? _d.Respect.Length : _page == TreePage ? 0 : _entries.Count;

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
        TrainingKind.Class => "Zawód: " + _d.Classes[e.I].Name,
        TrainingKind.Tool => _d.Weapons[_d.Tools[e.I].Weapon].Name,
        TrainingKind.Helper => "Brygada: " + _d.Brigade[e.I].Name,
        _ => "Trudność: " + _d.Difficulties[^1].Name,
    };

    private string Desc((TrainingKind K, int I) e)
    {
        if (e.K == TrainingKind.Upgrade) return UpgradeDesc(e.I);
        if (e.K == TrainingKind.Class) return "Nowy zawód do wyboru: " + _d.Classes[e.I].AbilityName + " (każdy kolejny drożej)";
        if (e.K == TrainingKind.Tool)
        {
            var w = _d.Weapons[_d.Tools[e.I].Weapon];
            return $"Narzędzie {w.MinDamage}-{w.MaxDamage} z{w.Range}, {UiText.StatShort(w.ScalesWith)}";
        }
        if (e.K == TrainingKind.Helper) return $"{_d.Brigade[e.I].Desc} (wezwanie {_d.Brigade[e.I].Price} zł)";
        return "Najwyższa trudność";
    }

    /// <summary>
    /// v0.21.52: Szkolenie z poziomami – co da kolejny poziom („Poziom III: +1 HP na start”), ile już daje i opis;
    /// na maksimum – razem.
    /// </summary>
    private string UpgradeDesc(int i)
    {
        var u = _d.Upgrades[i];
        var lv = _p.Levels[i];
        if (lv >= u.Levels) return "Razem: " + Meta.UpgradeSummary(_d, i, lv);
        var next = $"Poziom {UiText.Roman(lv)}: {RunMods.UpgradeLabel(u.Steps[lv].Effect, u.Steps[lv].Value)}";
        return lv > 0 ? $"{next} (teraz: {Meta.UpgradeSummary(_d, i, lv)})" : $"{next} – {u.Desc}";
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
            _note = "Wybrane!";
            _saved?.Invoke();
            return true;
        }
        _note = !SkillTree.Open(_d, _p, n) ? "Najpierw Szkolenia tej gałęzi" : SkillTree.Cost(_d, _p, n, o) < 0 ? "Już wybrane" : "Za mało doświadczenia";
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
            _note = "Kupione!";
            Rebuild();
            _list.Clamp(_entries.Count, Window);
            _saved?.Invoke();
        }
        else
        {
            _note = e.K == TrainingKind.Upgrade && Meta.UpgradeCost(_d, _p, e.I) < 0 ? "Maksymalny poziom" : "Za mało doświadczenia";
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
            _note = "Kupione!";
            _saved?.Invoke();
            return true;
        }
        _note = !Meta.RespectUnlocked(_d, _p, i) ? "Najpierw sekretne zlecenie"
              : Meta.RespectCost(_d, _p, i) < 0 ? "Maksymalna ranga" : "Za mało Respektu - kończ etapy";
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
        var c0 = p.Card(p.Top, 1);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        p.Text(tx, p.RowY(c0, 0), "Pozostało", Ink.Dim);
        p.Text(right, p.RowY(c0, 0), $"{_p.Xp} dośw.", Ink.Done, TextAlign.Right);

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
        var desc = _entries.Count > 0 ? Desc(_entries[_list.Sel]) : "Wszystko kupione!";
        var lines = p.F.Wrap(desc, (int)(right - tx));
        p.Text(tx, p.RowY(dc, 0), lines.Count > 0 ? lines[0] : "", Ink.Dim);
        if (_note.Length > 0) p.Text(tx, p.RowY(dc, 1), _note, _note == "Kupione!" ? Ink.Done : Ink.Late);
        else if (lines.Count > 1) p.Text(tx, p.RowY(dc, 1), lines[1], Ink.Dim);
        else p.Text(tx, p.RowY(dc, 1), $"Wydano {Meta.ShopSpent(_d, _p)}/{Meta.ShopTotalCost(_d)}", Ink.Brand);
    }

    /// <summary>
    /// v0.21.52 cz. c: drzewko – 3 kolumny (gałęzie): nazwa i pień (Szkolenia gałęzi z poziomami), potem dwa węzły
    /// z opcjami A / B (wybrana – zielona, zaznaczona – fioletowa ramka, zamknięta – szara z progiem pnia); na dole opis
    /// zaznaczonej opcji, koszt albo opłata za zmianę.
    /// </summary>
    private void DrawTree(PhonePainter p)
    {
        var nb = _d.TreeBranches.Length;
        var rowH = PhonePainter.RowH;
        var head = p.CardH(p.Top, 2 * rowH + 8);
        var gap = 4f;
        var colW = (head.Size.X - gap * (nb - 1)) / nb;
        var nodeRows = 6; // etykieta węzła + 2 opcje, x2
        var colH = nodeRows * rowH + 14;
        var top = p.Top;
        for (var b = 0; b < nb; ++b)
        {
            var x = head.Position.X + b * (colW + gap);
            var col = new Rect2(x, top, colW, colH + 2 * rowH);
            p.C.DrawStyleBox(Ui.Box(Pal.Card, 6, b == _tcol ? Pal.Brand : Pal.Border), col);
            var cx = x + colW / 2;
            var y = top + 4;
            p.Bold(cx, y, _d.TreeBranches[b].Name, b == _tcol ? Ink.Brand : Ink.Dark, TextAlign.Center);
            y += rowH;
            int lv = SkillTree.BranchLevels(_d, _p, b), max = SkillTree.BranchMax(_d, b);
            p.Text(cx, y, $"Pień {lv}/{max}", lv >= max ? Ink.Done : Ink.Dim, TextAlign.Center);
            p.Bar(x + 8, y + rowH - 3, colW - 16, lv, max, lv >= max ? Pal.Done : Pal.Brand, 3);
            y += rowH + 2;
            for (var tier = 0; tier < 2; ++tier)
            {
                var n = NodeOf(b, tier);
                if (n < 0) continue;
                var node = _d.TreeNodes[n];
                var open = SkillTree.Open(_d, _p, n);
                var pick = SkillTree.Pick(_p, n);
                y += 2;
                p.Text(cx, y, open ? $"Węzeł {UiText.Roman(tier)}" : $"od pnia {node.Depth}", open ? Ink.Dim : Ink.Late, TextAlign.Center, colW - 6);
                y += rowH;
                for (var o = 0; o < 2; ++o)
                {
                    var r = tier * 2 + o;
                    var cell = new Rect2(x + 4, y, colW - 8, rowH);
                    var picked = pick == o + 1;
                    var cur = b == _tcol && r == _trow;
                    if (picked || cur)
                        p.C.DrawStyleBox(Ui.Box(picked ? Pal.DoneBg : Pal.Group, 5, cur ? Pal.Brand : Colors.Transparent), cell);
                    p.Hit(cell, b * 4 + r);
                    var ink = picked ? Ink.Done : cur ? Ink.Brand : open ? Ink.Dark : Ink.Dim;
                    var opt = node.Options[o];
                    var name = p.F.Measure(opt.Name) <= colW - 12 ? opt.Name : opt.Short;
                    p.Text(cx, y + PhonePainter.TextDy, name, ink, TextAlign.Center, colW - 12);
                    y += rowH;
                }
            }
        }
        // opis zaznaczonej opcji i pień gałęzi (Szkolenia z poziomami)
        var dc = p.Card(top + colH + 2 * rowH + 6, 3);
        var tx = p.TextX(dc);
        var right = dc.End.X - 6;
        var sn = NodeOf(_tcol, _trow / 2);
        if (sn < 0) return;
        var so = _trow % 2;
        var sop = _d.TreeNodes[sn].Options[so];
        var cost = SkillTree.Cost(_d, _p, sn, so);
        var spick = SkillTree.Pick(_p, sn);
        var sopen = SkillTree.Open(_d, _p, sn);
        var pill = !sopen ? $"Pień {_d.TreeNodes[sn].Depth}" : cost < 0 ? "Masz" : spick > 0 ? $"Zmiana {cost}" : cost.ToString();
        var pw = p.Pill(right, p.RowY(dc, 0), pill, !sopen ? PillKind.Gray : cost < 0 ? PillKind.Done : cost <= _p.Xp ? PillKind.Group : PillKind.Gray);
        p.Text(tx, p.RowY(dc, 0), $"{sop.Name}: {RunMods.UpgradeLabel(sop.Effect, sop.Value)}", Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        var info = _note.Length > 0 ? _note : $"{sop.Desc}. {(spick > 0 && cost >= 0 ? $"Zmiana wyboru: {_d.TreeRespecCost} dośw." : "Drugą opcję zmienisz za opłatą.")}";
        p.Text(tx, p.RowY(dc, 1), info, _note.Length > 0 ? (_note == "Wybrane!" ? Ink.Done : Ink.Late) : Ink.Dim, TextAlign.Left, right - tx);
        var trunk = string.Join(", ", Enumerable.Range(0, _d.Upgrades.Length).Where(i => ((_d.TreeBranches[_tcol].Upgrades >> i) & 1) != 0)
            .Select(i => $"{_d.Upgrades[i].Name} {_p.Levels[i]}/{_d.Upgrades[i].Levels}"));
        p.Divider(dc, 2);
        p.Text(tx, p.RowY(dc, 2), "Pień: " + trunk, Ink.Brand, TextAlign.Left, right - tx);
    }

    private void DrawRespect(PhonePainter p)
    {
        var c0 = p.Card(p.Top, 1);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        p.Text(tx, p.RowY(c0, 0), "Masz", Ink.Dim);
        p.Text(right, p.RowY(c0, 0), $"{_p.Respect} Respektu", Ink.Done, TextAlign.Right);
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
                var lw = p.Pill(right, y, "Sekret", PillKind.Gray);
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
            p.Text(tx, p.RowY(dc, 0), "Sekret: " + _d.Secrets[def.Secret].Hint, Ink.Dim, TextAlign.Left, right - tx);
            p.Text(tx, p.RowY(dc, 1), _note.Length > 0 ? _note : "Odblokujesz sekretnym zleceniem", _note.Length > 0 ? Ink.Late : Ink.Brand, TextAlign.Left, right - tx);
            return;
        }
        var now = rank > 0 ? RunMods.RespectLabel(def.Effect, Meta.RespectValue(_d, _p, s)) : def.Desc + ": brak";
        p.Text(tx, p.RowY(dc, 0), now, Ink.Dark, TextAlign.Left, right - tx);
        if (_note.Length > 0) p.Text(tx, p.RowY(dc, 1), _note, _note == "Kupione!" ? Ink.Done : Ink.Late, TextAlign.Left, right - tx);
        else if (rank < def.Ranks) p.Text(tx, p.RowY(dc, 1), "Dalej: " + RunMods.RespectLabel(def.Effect, def.Values[rank]), Ink.Brand, TextAlign.Left, right - tx);
        else p.Text(tx, p.RowY(dc, 1), $"Wydano {Meta.RespectSpent(_d, _p)}/{Meta.RespectTotalCost(_d)}", Ink.Dim);
    }

    private void DrawRewards(PhonePainter p)
    {
        var next = _p.Rewards;
        var c0 = p.Card(p.Top, 1);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        var head = next < Meta.RewardsAvailable(_d) ? "Za wygraną: " + _d.Rewards[next].Name : "Wszystko odebrane - więcej wkrótce";
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
            var pill = got ? "Odebrana" : rw.Kind == RewardKind.Soon ? "Wkrótce" : i == next ? "Następna" : $"{Meta.RewardWin(_d, _p, i)}. wygr.";
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
