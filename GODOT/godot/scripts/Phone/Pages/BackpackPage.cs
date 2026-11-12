using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// v0.23.2: plecak – 6 miejsc z ikoną, nazwą w kolorze rzadkości i strzałką ▲ lepsze / ▼ słabsze / = równe względem
/// noszonego; pod listą porównanie zaznaczonego. Tryb Bag: A otwiera akcje (Załóż – 1 tura na placu, w Hurtowni bez tury;
/// Do kabury; Wyrzuć na ziemię). Tryb Sell (Skup) i Recycle (PSZOK) – A sprzedaje / oddaje zaznaczony przedmiot.
/// W Hurtowni nad treścią pasek zakładek (HurtTabs).
/// </summary>
public sealed class BackpackPage : PhonePage
{
    private const int MenuBase = 100;
    private readonly CoreGame _g;
    private int _sel;
    private bool _menu;
    private int _msel;
    private string _note = "";
    private Ink _noteInk = Ink.Brand;

    private enum Act
    {
        Equip,
        Holster,
        Drop,
        Cancel,
    }

    public BackpackPage(CoreGame g) => _g = g;

    public BagMode Mode { get; init; }

    /// <summary>Strona w Hurtowni (pasek zakładek, zakładka Tab).</summary>
    public bool Shop { get; init; }

    public int Tab { get; init; } = HurtTabs.Bag;

    /// <summary>Po akcji: true = zużyła turę (ekran wraca na plac).</summary>
    public Action<bool> Done { get; set; }

    /// <summary>Dotknięcie zakładki Hurtowni.</summary>
    public Action<int> TabTo { get; set; }

    public int Sel
    {
        get => _sel;
        set => _sel = Math.Clamp(value, 0, Math.Max(0, _g.BackpackSlots() - 1));
    }

    public bool MenuOpen => _menu;

    public override string Title => Mode switch
    {
        BagMode.Sell => Loc.T("hurt_tab_skup"),
        BagMode.Recycle => Loc.T("hurt_tab_pszok"),
        _ => Loc.T("plecak"),
    };

    public override string Sub => Loc.F("plecak_sub", _g.BackpackCount(), _g.BackpackSlots());

    public override string Hint => _menu ? Loc.T("karta_hint")
        : Mode == BagMode.Sell ? Loc.T("hurt_hint_skup")
        : Mode == BagMode.Recycle ? Loc.T("hurt_hint_pszok")
        : Shop ? Loc.T("hurt_hint_plecak") : Loc.T("plecak_hint");

    public override PageAction[] Actions => _menu
        ? [new(Loc.T("bp_anuluj"), GameAction.B), new(Loc.T("wybierz"), GameAction.A)]
        : Mode switch
        {
            BagMode.Sell => [new(Loc.T("dalej"), GameAction.B, PageRole.Back), new(Loc.T("skup_sprzedaj"), GameAction.A)],
            BagMode.Recycle => [new(Loc.T("dalej"), GameAction.B, PageRole.Back), new(Loc.T("pszok_oddaj"), GameAction.A)],
            _ => [new(Shop ? Loc.T("dalej") : Loc.T("wroc"), GameAction.B, PageRole.Back), new(Loc.T("plecak_akcje"), GameAction.A)],
        };

    public override void Enter()
    {
        _menu = false;
        _note = "";
    }

    private BackpackItem Item => _g.Backpack[_sel];

    private List<Act> MenuItems()
    {
        var list = new List<Act> { Act.Equip };
        if (Item.Kind == BackpackKind.Weapon && _g.HasBelt()) list.Add(Act.Holster);
        if (_g.St == GameStatus.Playing) list.Add(Act.Drop);
        list.Add(Act.Cancel);
        return list;
    }

    private string MenuLabel(Act a)
    {
        var site = _g.St == GameStatus.Playing;
        return a switch
        {
            Act.Equip => site ? Loc.T("bp_zaloz_tura") : Loc.T("bp_zaloz"),
            Act.Holster => site ? Loc.T("bp_kabura_tura") : Loc.T("bp_kabura"),
            Act.Drop => Loc.T("bp_wyrzuc"),
            _ => Loc.T("bp_anuluj"),
        };
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            if (_menu) _msel = (_msel + v + MenuItems().Count) % MenuItems().Count;
            else
            {
                _sel = (_sel + v + _g.BackpackSlots()) % _g.BackpackSlots();
                _note = "";
            }
            Sfx.Play("menu");
            return true;
        }
        if (e.IsConfirm)
        {
            Confirm();
            return true;
        }
        if (e.IsBack && _menu)
        {
            _menu = false;
            return true;
        }
        return false;
    }

    public override bool TapRow(int index)
    {
        if (index >= HurtTabs.HitBase)
        {
            TabTo?.Invoke(index - HurtTabs.HitBase);
            return true;
        }
        if (index >= MenuBase)
        {
            _msel = index - MenuBase;
            Execute(MenuItems()[_msel]);
            return true;
        }
        if (index == _sel) Confirm();
        else
        {
            _sel = index;
            _menu = false;
            _note = "";
        }
        return true;
    }

    /// <summary>A: akcje (plecak), sprzedaż (Skup), oddanie (PSZOK); w menu – wybrana akcja.</summary>
    public void Confirm()
    {
        if (_menu)
        {
            Execute(MenuItems()[_msel]);
            return;
        }
        if (Item.Empty) return;
        switch (Mode)
        {
            case BagMode.Sell:
            {
                var name = _g.BackpackName(Item);
                if (_g.BackpackSell(_sel))
                {
                    Sfx.Play("buy");
                    Note(Loc.F("skup_sprzedane", name), Ink.Done);
                    Done?.Invoke(false);
                }
                break;
            }
            case BagMode.Recycle:
            {
                var name = _g.BackpackName(Item);
                if (_g.BackpackRecycle(_sel))
                {
                    Sfx.Play("level", 0.6f);
                    Note(Loc.F("pszok_oddane", name), Ink.Done);
                    Done?.Invoke(false);
                }
                break;
            }
            default:
                _menu = true;
                _msel = 0;
                Sfx.Play("menu");
                break;
        }
    }

    private void Note(string s, Ink ink)
    {
        _note = s;
        _noteInk = ink;
    }

    private void Execute(Act a)
    {
        _menu = false;
        var site = _g.St == GameStatus.Playing;
        var ok = a switch
        {
            Act.Equip => _g.BackpackEquip(_sel),
            Act.Holster => _g.BackpackHolster(_sel),
            Act.Drop => _g.BackpackDrop(_sel),
            _ => false,
        };
        if (!ok) return;
        Sfx.Play(a == Act.Drop ? "menu" : "pickup");
        Note(_g.Log[CoreGame.LogLines - 1].Text, Ink.Brand);
        Done?.Invoke(site && a != Act.Drop);
    }

    /// <summary>Strzałka porównania (▲ zielona, ▼ czerwona, = szara) wyśrodkowana w wierszu przy x; zwraca szerokość.</summary>
    private static float Arrow(PhonePainter p, float x, float rowY, int cmp)
    {
        var cy = rowY + PhonePainter.RowH / 2f;
        if (cmp == 0)
        {
            p.C.DrawRect(new Rect2(x, cy - 3, 8, 2), Pal.Dim);
            p.C.DrawRect(new Rect2(x, cy + 1, 8, 2), Pal.Dim);
            return 10;
        }
        var up = cmp > 0;
        Vector2[] pts = up
            ? [new(x, cy + 4), new(x + 9, cy + 4), new(x + 4.5f, cy - 4)]
            : [new(x, cy - 4), new(x + 9, cy - 4), new(x + 4.5f, cy + 4)];
        p.C.DrawColoredPolygon(pts, up ? Ink.Done.Fill : Ink.Late.Fill);
        return 11;
    }

    private string RarityName(BackpackItem it) =>
        it.Kind == BackpackKind.Weapon ? WeaponLook.RarityName(_g.D, it.Rarity) : _g.D.GearRarities[Math.Clamp((int)it.Rarity, 0, 2)];

    private Ink RarityInk(BackpackItem it) =>
        it.Kind == BackpackKind.Weapon ? WeaponLook.PhoneInk(_g.D, it.Rarity) : it.Rarity == 2 ? Ink.Prog : it.Rarity == 1 ? Ink.Brand : Ink.Dark;

    /// <summary>Ikona przedmiotu 16x16 (broń w kolorze rzadkości, paczka sprzętu); zwraca przesunięcie tekstu.</summary>
    private float ItemIcon(PhonePainter p, float x, float rowY, BackpackItem it)
    {
        if (it.Kind == BackpackKind.Weapon) return p.WeaponIcon(x, rowY, _g.D.Weapons[it.Id], it.Rarity);
        var y = rowY + (PhonePainter.RowH - Assets.Icon) / 2f;
        p.C.DrawTextureRectRegion(Assets.Actors, new Rect2(x, y, Assets.Icon, Assets.Icon), Assets.Frame(Assets.FrameGear + it.Rarity, Assets.Actor));
        return Assets.Icon + 3;
    }

    /// <summary>Skup: „2x Stal + 1x Cem.”.</summary>
    public static string SellText(CoreGame g, BackpackItem it)
    {
        var n = g.SellCount(it);
        var d = g.D;
        var s = $"{(n + 1) / 2} {d.Materials[g.SellMat(it)].Short}";
        if (n / 2 > 0) s += $"+{n / 2} {d.Materials[d.BackpackAltMat].Short}";
        return s;
    }

    private string RightPill(BackpackItem it) => Mode switch
    {
        BagMode.Sell => SellText(_g, it),
        BagMode.Recycle => Loc.F("pszok_cena", _g.PszokXpFor(it)),
        _ => RarityName(it),
    };

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var y = p.Top;
        if (Shop) y = HurtTabs.Draw(p, y, Tab);
        var head = _note.Length > 0 ? _note
            : Mode == BagMode.Sell ? Loc.T("skup_opis")
            : Mode == BagMode.Recycle ? Loc.F("pszok_opis", d.PszokHp)
            : Shop ? Loc.T("bp_bez_tury") : "";
        if (head.Length > 0)
        {
            var headLines = p.F.Wrap(head, (int)(p.Width - 24));
            var rows = Math.Min(2, headLines.Count);
            var c0 = p.Card(y, rows);
            for (var k = 0; k < rows; k++)
            {
                p.Stripe(c0, k, _note.Length > 0 ? _noteInk.Fill : Pal.Done);
                p.Text(p.TextX(c0), p.RowY(c0, k), headLines[k], _note.Length > 0 ? _noteInk : Ink.Dim, TextAlign.Left, c0.End.X - 6 - p.TextX(c0));
            }
            y = c0.End.Y + 4;
        }
        var n = _g.BackpackSlots();
        var card = p.Card(y, n);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var r = 0; r < n; r++)
        {
            var it = _g.Backpack[r];
            var ry = p.RowY(card, r);
            if (r == _sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            p.HitRow(card, r, r);
            if (it.Empty)
            {
                p.Text(tx, ry, "-", Ink.Dim);
                continue;
            }
            var pill = RightPill(it);
            var pw = p.Pill(right, ry, pill, Mode == BagMode.Bag ? PillKind.Gray : PillKind.Group);
            var aw = 0f;
            if (Mode == BagMode.Bag) aw = Arrow(p, right - pw - 14, ry, _g.BackpackCompare(r)) + 6;
            var x = tx + ItemIcon(p, tx, ry, it);
            p.Text(x, ry, _g.BackpackName(it), RarityInk(it), TextAlign.Left, right - pw - aw - 6 - x);
        }
        y = card.End.Y + 4;
        if (_g.BackpackCount() == 0)
        {
            var ce = p.Card(y, 3);
            p.Text(p.TextX(ce), p.RowY(ce, 0), Loc.T("plecak_pusty"), Ink.Dark, TextAlign.Left, ce.End.X - 6 - p.TextX(ce));
            p.Text(p.TextX(ce), p.RowY(ce, 1), Loc.T("plecak_pusty_2"), Ink.Dim, TextAlign.Left, ce.End.X - 6 - p.TextX(ce));
            p.Text(p.TextX(ce), p.RowY(ce, 2), Loc.T("plecak_pusty_3"), Ink.Dim, TextAlign.Left, ce.End.X - 6 - p.TextX(ce));
            return;
        }
        if (_menu)
        {
            var items = MenuItems();
            var cm = p.Card(y, items.Count);
            for (var k = 0; k < items.Count; k++)
            {
                if (k == _msel) p.Selected(cm, k);
                else if (k > 0) p.Divider(cm, k);
                p.HitRow(cm, k, MenuBase + k);
                var ink = items[k] == Act.Drop ? Ink.Late : k == _msel ? Ink.Brand : Ink.Dark;
                p.Text(p.TextX(cm), p.RowY(cm, k), MenuLabel(items[k]), ink, TextAlign.Left, cm.End.X - 6 - p.TextX(cm));
            }
            return;
        }
        if (Item.Empty) return;
        var lines = Detail();
        var cd = p.Card(y, lines.Count);
        for (var k = 0; k < lines.Count; k++)
        {
            if (k > 0) p.Divider(cd, k);
            p.Text(p.TextX(cd), p.RowY(cd, k), lines[k].Text, lines[k].Ink, TextAlign.Left, cd.End.X - 6 - p.TextX(cd));
        }
    }

    /// <summary>Porównanie zaznaczonego z noszonym (2 wiersze); w Skupie / PSZOK-u – materiały albo HP.</summary>
    private List<(string Text, Ink Ink)> Detail()
    {
        var d = _g.D;
        var it = Item;
        var list = new List<(string, Ink)>();
        if (it.Kind == BackpackKind.Weapon)
        {
            var w = d.Weapons[it.Id];
            list.Add(($"{RarityName(it)} · {WeaponLook.Hits(d, w)}", RarityInk(it)));
            if (Mode == BagMode.Bag)
            {
                var now = _g.WeaponBreakdown();
                var next = _g.WeaponBreakdown(-1, it.Id, -1, -1, 0, it.Rarity, it.Perks);
                var cmp = _g.BackpackCompare(_sel);
                list.Add((Loc.T("cios_2") + DamageHelp.CompareLine(new Message(), now, next, true).Text, cmp > 0 ? Ink.Done : cmp < 0 ? Ink.Late : Ink.Dim));
            }
        }
        else
        {
            var slot = it.Id;
            var gd = d.Gear[slot * 3 + it.Rarity];
            list.Add(($"{UiText.GearStatName(gd.Stat)} +{gd.Value}, {d.GearTraits[Math.Max(0, (int)it.Trait)].Name}", RarityInk(it)));
            if (Mode == BagMode.Bag)
            {
                var now = _g.Equipped[slot];
                var cmp = _g.BackpackCompare(_sel);
                list.Add((now >= 0 ? Loc.F("bp_teraz", d.Gear[slot * 3 + now].Name) : Loc.T("bp_teraz_pusto"), cmp > 0 ? Ink.Done : cmp < 0 ? Ink.Late : Ink.Dim));
            }
        }
        if (Mode == BagMode.Sell)
        {
            var info = "";
            for (var m = 0; m < d.Materials.Length; m++) info += $"{d.Materials[m].Short} {_g.Mats[m]}  ";
            list.Add((info.TrimEnd(), Ink.Dim));
            var full = _g.Mats[_g.SellMat(it)] >= d.MaterialMax;
            if (full) list.Add((Loc.F("skup_maks", d.MaterialMax), Ink.Late));
        }
        else if (Mode == BagMode.Recycle)
        {
            list.Add(($"HP {_g.Hero.Hp}/{_g.Hero.MaxHp}", Ink.Dim));
        }
        return list;
    }
}
