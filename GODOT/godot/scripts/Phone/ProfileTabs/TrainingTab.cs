using System;
using System.Collections.Generic;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.ProfileTabs;

/// <summary>
/// Koszty = sklep Szkolenia (zakładka 4 profilu na GBA): „Pozostało” doświadczenia, lista ulepszeń, zablokowanych
/// zawodów, narzędzi i najwyższej trudności z pastylkami ceny; Spacja/Enter kupuje (jak A na GBA).
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

    public TrainingTab(GameData d, Profile p, Action saved)
    {
        _d = d;
        _p = p;
        _saved = saved;
        Rebuild();
    }

    public override string Title => "Koszty";
    public override string Sub => "Szkolenia";
    public override string Hint => "Spacja: kup  Q/E: zakładki  Esc: wróć";

    public int Count => _entries.Count;

    private void Rebuild()
    {
        _entries.Clear();
        for (var i = 0; i < _d.Upgrades.Length; i++) _entries.Add((TrainingKind.Upgrade, i));
        for (var i = 0; i < _d.Classes.Length; i++)
        {
            if (!Meta.ClassUnlocked(_p, i)) _entries.Add((TrainingKind.Class, i));
        }
        for (var i = 0; i < _d.Tools.Length; i++)
        {
            if (!Meta.ToolUnlocked(_d, _p, i)) _entries.Add((TrainingKind.Tool, i));
        }
        if (!Meta.DifficultyUnlocked(_d, _p, _d.Difficulties.Length - 1)) _entries.Add((TrainingKind.Hard, 0));
    }

    private int Cost((TrainingKind K, int I) e) => e.K switch
    {
        TrainingKind.Upgrade => Meta.UpgradeCost(_d, _p, e.I),
        TrainingKind.Class => _d.ClassCost,
        TrainingKind.Tool => _d.Tools[e.I].Cost,
        _ => _d.HardCost,
    };

    private string Name((TrainingKind K, int I) e) => e.K switch
    {
        TrainingKind.Upgrade => $"{_d.Upgrades[e.I].Name} {_p.Levels[e.I]}/{_d.Upgrades[e.I].Levels}",
        TrainingKind.Class => "Zawód: " + _d.Classes[e.I].Name,
        TrainingKind.Tool => _d.Weapons[_d.Tools[e.I].Weapon].Name,
        _ => "Trudność: " + _d.Difficulties[^1].Name,
    };

    private string Desc((TrainingKind K, int I) e)
    {
        if (e.K == TrainingKind.Upgrade) return _d.Upgrades[e.I].Desc;
        if (e.K == TrainingKind.Class) return "Nowy zawód do wyboru: " + _d.Classes[e.I].AbilityName;
        if (e.K == TrainingKind.Tool)
        {
            var w = _d.Weapons[_d.Tools[e.I].Weapon];
            return $"Narzędzie {w.MinDamage}-{w.MaxDamage} z{w.Range}, {UiText.StatShort(w.ScalesWith)}";
        }
        return "Najwyższa trudność";
    }

    /// <summary>Kup zaznaczoną pozycję (także z testu dymnego).</summary>
    public bool Buy()
    {
        if (_entries.Count == 0) return false;
        var e = _entries[_list.Sel];
        var ok = e.K switch
        {
            TrainingKind.Upgrade => Meta.BuyUpgrade(_d, _p, e.I),
            TrainingKind.Class => Meta.BuyClass(_d, _p, e.I),
            TrainingKind.Tool => Meta.BuyTool(_d, _p, e.I),
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

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, _entries.Count, Window);
            _note = "";
            return true;
        }
        if (e.Is(GameAction.A | GameAction.Start))
        {
            Buy();
            return true;
        }
        return false;
    }

    public override void Draw(PhonePainter p)
    {
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
}
