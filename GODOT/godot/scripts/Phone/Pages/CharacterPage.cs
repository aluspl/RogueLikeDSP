using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.World;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// v0.23.2 (UI 2): ekran Postać – bohater na środku, dookoła sloty (kask, kamizelka, rękawice | broń, kabura, buty; pas pod
/// bohaterem), pod spodem 4 kluczowe liczby (HP, Obrona, Obrażenia, Kryt), niżej plecak – siatka 3×2 dużych kafli z ramką
/// rzadkości i ▲/▼. Dotknięcie kafla albo slotu (klawiatura: strzałki + Spacja) otwiera kartę przedmiotu (ItemSheet).
/// Na placu: Q – premie, E – opis statystyk (dotyk: przyciski na dole). W Hurtowni (Shop) – zakładka Plecak z paskiem
/// zakładek, zamiana bez tury. Gdy brak miejsca (telefon w poziomie, Hurtownia), układ zwarty: mniejsze kafle bez podpisów.
/// </summary>
public sealed class CharacterPage : PhonePage
{
    /// <summary>Indeksy dotknięcia i fokusu: sloty sprzętu 0..5, broń w ręku, kabura, plecak BagBase + i.</summary>
    public const int HandId = 20, HolsterId = 21, BagBase = 10;

    private readonly CoreGame _g;
    private readonly Profile _profile;
    private readonly HeroDress _dress = new();
    private readonly List<(int Id, Rect2 Rect)> _focusable = new();
    private int _focus = -1;

    public CharacterPage(CoreGame g, Profile profile)
    {
        _g = g;
        _profile = profile;
        Sheet = new ItemSheet(g) { Done = Acted };
    }

    public ItemSheet Sheet { get; }

    /// <summary>W Hurtowni: pasek zakładek, zamiana bez tury.</summary>
    public bool Shop { get; init; }

    /// <summary>Po akcji na przedmiocie: true = zużyła turę.</summary>
    public Action<bool> Done { get; set; }

    /// <summary>Q / „Premie”: lista premii; E / „Statystyki”: opis statystyk (tylko na placu).</summary>
    public Action Boons { get; set; }

    public Action Stats { get; set; }

    /// <summary>Rozpiska obrażeń broni w ręku (karta broni → Rozpiska).</summary>
    public Action Breakdown
    {
        get => Sheet.Breakdown;
        set => Sheet.Breakdown = value;
    }

    /// <summary>Dotknięcie zakładki Hurtowni.</summary>
    public Action<int> TabTo { get; set; }

    public int Focus
    {
        get => _focus;
        set => _focus = value;
    }

    public override string Title => Shop ? Loc.T("plecak") : Loc.T("postac");
    public override string Sub => Shop ? Loc.T("bp_bez_tury_sub") : Loc.F("postac_sub", _g.CDef.Name, _g.HeroLevel);

    public override string Hint => Sheet.IsOpen ? Loc.T("karta_hint_2") : Shop ? Loc.T("hurt_hint_plecak") : Loc.T("postac_hint");

    public override PageAction[] Actions => Sheet.IsOpen || Shop
        ? (Shop && !Sheet.IsOpen ? [new(Loc.T("dalej"), GameAction.B, PageRole.Back)] : [])
        : [new(Loc.T("wroc"), GameAction.B, PageRole.Back), new(Loc.F("postac_premie", _g.BoonsOwned()), GameAction.TabPrev, PageRole.Other), new(Loc.T("postac_statystyki"), GameAction.TabNext, PageRole.Primary)];

    public override void Enter()
    {
        Sheet.Close();
        Sheet.ClearNote();
        if (_focus < 0) _focus = _g.BackpackCount() > 0 ? BagBase : HandId;
    }

    private void Acted(bool turn)
    {
        Redraw();
        Done?.Invoke(turn);
    }

    /// <summary>Otwiera kartę elementu id (slot, broń, kabura, plecak); puste – nic.</summary>
    public void OpenSheet(int id)
    {
        _focus = id;
        Sheet.ClearNote();
        if (id >= BagBase && id < BagBase + CoreGame.MaxBackpack) Sheet.Show(SheetKind.Bag, id - BagBase);
        else if (id == HandId) Sheet.Show(SheetKind.Hand, 0);
        else if (id == HolsterId) Sheet.Show(SheetKind.Holster, 0);
        else if (id >= 0 && id < _g.D.GearSlotsCount) Sheet.Show(SheetKind.Gear, id);
        Redraw();
    }

    public override bool Input(InputCmd e)
    {
        if (Sheet.IsOpen) return Sheet.Input(e);
        if (e.VDir != 0 || e.HDir != 0)
        {
            MoveFocus(new Vector2(e.HDir, e.VDir));
            return true;
        }
        if (e.IsConfirm)
        {
            OpenSheet(_focus);
            return true;
        }
        if (!Shop && Boons is not null && e.Is(GameAction.TabPrev))
        {
            Boons();
            return true;
        }
        if (!Shop && Stats is not null && e.Is(GameAction.TabNext))
        {
            Stats();
            return true;
        }
        return false;
    }

    /// <summary>Fokus klawiatury do najbliższego elementu w kierunku dir (nawigacja przestrzenna).</summary>
    private void MoveFocus(Vector2 dir)
    {
        var from = _focusable.Find(f => f.Id == _focus);
        if (from.Rect.Size.X <= 0)
        {
            if (_focusable.Count > 0) _focus = _focusable[0].Id;
            return;
        }
        var c = from.Rect.GetCenter();
        var best = -1;
        var bestScore = float.MaxValue;
        foreach (var (id, r) in _focusable)
        {
            if (id == _focus) continue;
            var v = r.GetCenter() - c;
            var along = v.Dot(dir);
            if (along <= 2) continue;
            var across = Mathf.Abs(dir.X != 0 ? v.Y : v.X);
            var score = along + across * 2.5f;
            if (score >= bestScore) continue;
            bestScore = score;
            best = id;
        }
        if (best < 0) return;
        _focus = best;
        Sfx.Play("menu");
    }

    public override bool TapRow(int index)
    {
        if (Sheet.IsOpen) return Sheet.Tap(index);
        if (index >= HurtTabs.HitBase)
        {
            TabTo?.Invoke(index - HurtTabs.HitBase);
            return true;
        }
        OpenSheet(index);
        return true;
    }

    private bool SlotOpen(int i) => ((_g.Bonus.GearSlots >> i) & 1) != 0 || _g.Equipped[i] >= 0;

    private BackpackItem SlotItem(int id) => id switch
    {
        HandId => _g.HandItem(),
        HolsterId => _g.HasBelt() && _g.Weapon2 >= 0 ? BackpackItem.Weapon(_g.Weapon2, _g.Weapon2Rarity, _g.Weapon2Perks, _g.Weapon2Lvl) : default,
        _ => id < _g.D.GearSlotsCount && _g.Equipped[id] >= 0 ? BackpackItem.Gear(id, _g.Equipped[id], _g.EquippedTrait[id]) : default,
    };

    private string SlotName(int id) => id switch
    {
        HandId => Loc.T("slot_bron"),
        HolsterId => Loc.T("slot_kabura"),
        _ => id < _g.D.GearSlotsCount ? _g.D.GearSlots[id] : "",
    };

    private bool SlotLocked(int id) => id == HolsterId ? !_g.HasBelt() : id != HandId && (id >= _g.D.GearSlotsCount || !SlotOpen(id));

    public override void Draw(PhonePainter p)
    {
        _focusable.Clear();
        var d = _g.D;
        var y = p.Top;
        if (Shop) y = HurtTabs.Draw(p, y, HurtTabs.Bag);
        if (Sheet.Note.Length > 0) y = Line(p, y, Sheet.Note, Sheet.NoteInk, Sheet.NoteInk.Fill);
        var room = p.Bottom - y;
        var big = room >= 365; // duże kafle z podpisami (potrzeba ~385, kafle plecaka niższe przy 365-385)
        var th = big ? Mathf.Clamp(room - 329, 46, 56) : 40f;
        var tile = big ? 38f : 30f;
        var label = big ? 15f : 0f;
        var step = tile + label + (big ? 6 : 4);
        var colW = big ? 72f : tile;
        // sloty: lewa kolumna kask, kamizelka, rękawice; prawa broń, kabura, buty; pas pod bohaterem
        int[] left = [0, 2, 1];
        int[] right = [HandId, HolsterId, 3];
        var top = y + 2;
        for (var k = 0; k < 3; k++)
        {
            Slot(p, left[k], new Vector2(p.Left + 4 + colW / 2, top + k * step), tile, label);
            Slot(p, right[k], new Vector2(p.Right - 4 - colW / 2, top + k * step), tile, label);
        }
        var cx = Mathf.Round((p.Left + p.Right) / 2);
        var heroScale = big ? 3 : 2;
        var heroSize = 32 * heroScale;
        var beltTop = top + 2 * step;
        var heroTop = Mathf.Round(top + (beltTop - top - heroSize) / 2 + (big ? 4 : 0));
        Hero(p, new Vector2(cx - heroSize / 2f, heroTop), heroScale);
        if (d.GearSlotsCount > 4) Slot(p, 4, new Vector2(cx, beltTop), tile, label);
        y = top + 3 * step + (big ? 2 : 0);
        y = KeyStats(p, y, big) + (big ? 8 : 4);
        // plecak
        var n = _g.BackpackSlots();
        y = p.Section(y, Loc.T("plecak").ToUpperInvariant(), Loc.F("plecak_sub", _g.BackpackCount(), n));
        var gap = 8f;
        var tw = Mathf.Floor((p.Width - 2 * gap) / 3);
        for (var i = 0; i < n; i++)
        {
            var r = new Rect2(Mathf.Round(p.Left + (i % 3) * (tw + gap)), Mathf.Round(y + (i / 3) * (th + gap)), tw, th);
            var it = _g.Backpack[i];
            ItemTile.Draw(p, d, r, it, it.Empty ? 0 : _g.BackpackCompare(i), Focused(BagBase + i));
            Register(p, BagBase + i, r);
        }
        y += ((n + 2) / 3) * (th + gap);
        if (_g.BackpackCount() == 0 && y + PhonePainter.RowH <= p.Bottom)
        {
            foreach (var line in p.F.Wrap(Loc.T("plecak_pusty_karta"), (int)p.Width - 8))
            {
                if (y + PhonePainter.RowH > p.Bottom) break;
                p.Text(p.Left + 4, y, line, Ink.Dim);
                y += 18;
            }
        }
        Sheet.Draw(p);
    }

    private bool Focused(int id) => !Layout.Touch && !Sheet.IsOpen && id == _focus;

    private void Register(PhonePainter p, int id, Rect2 r)
    {
        _focusable.Add((id, r));
        if (!Sheet.IsOpen) p.Hit(r.Grow(2), id);
    }

    /// <summary>Wiersz notatki (wynik akcji / informacja) z paskiem koloru; zwraca y pod nim.</summary>
    private static float Line(PhonePainter p, float y, string text, Ink ink, Color stripe)
    {
        var c = p.Card(y, 1);
        p.Stripe(c, 0, stripe);
        p.Text(p.TextX(c), p.RowY(c, 0), text, ink, TextAlign.Left, c.End.X - 6 - p.TextX(c));
        return c.End.Y + 6;
    }

    /// <summary>Slot postaci: kafel (środek u góry w center) i podpis pod nim.</summary>
    private void Slot(PhonePainter p, int id, Vector2 center, float tile, float label)
    {
        var r = new Rect2(Mathf.Round(center.X - tile / 2), Mathf.Round(center.Y), tile, tile);
        var it = SlotItem(id);
        var locked = SlotLocked(id);
        ItemTile.Draw(p, _g.D, r, it, 0, Focused(id), locked && it.Empty);
        if (label > 0) p.F.Draw(p.C, new Vector2(center.X, r.End.Y), p.F.Fit(SlotName(id), (int)(tile + 32)), locked ? Ink.Dim.WithAlpha(0.6f) : Ink.Dim, TextAlign.Center);
        if (!locked || !it.Empty) Register(p, id, r);
    }

    /// <summary>Bohater w sprzęcie z bronią w ręku, powiększony całkowicie (scale), lewy górny róg at.</summary>
    private void Hero(PhonePainter p, Vector2 at, int scale)
    {
        _dress.Sync(_g);
        _dress.Dir = WeaponPose.Down; // broń przed sobą – nie wchodzi na sloty obok
        var xf = new Transform2D(0f, new Vector2(scale, scale), 0f, at);
        p.C.DrawStyleBox(Ui.Box(Pal.Group, 32 * scale / 2), new Rect2(at + new Vector2(4, 4) * scale, new Vector2(24, 24) * scale)); // tło – krąg
        p.C.DrawSetTransformMatrix(xf);
        var dst = new Rect2(0, 0, 32, 32);
        var frame = Assets.HeroFrame(_g.D, _profile, _g.Cls);
        p.C.DrawTextureRectRegion(Assets.ActorsNoTool, dst, Assets.Frame(frame, Assets.Actor));
        _dress.DrawGear(p.C, dst, 0, Colors.White);
        _dress.DrawWeapon(p.C, xf, dst, Colors.White);
        p.C.DrawSetTransformMatrix(Transform2D.Identity);
    }

    /// <summary>4 kluczowe liczby w kolumnach: etykieta drobno, wartość pogrubiona; zwraca y pod nimi.</summary>
    private float KeyStats(PhonePainter p, float y, bool big)
    {
        var b = _g.WeaponBreakdown();
        (string Label, string Value, Ink Ink)[] cols =
        [
            ("HP", $"{_g.Hero.Hp}/{_g.Hero.MaxHp}", _g.Hero.Hp * 4 <= _g.Hero.MaxHp ? Ink.Late : Ink.Dark),
            (Loc.T("postac_obrona"), $"{_g.HeroDefense()}", Ink.Dark),
            (Loc.T("postac_obrazenia"), $"{b.Min}-{b.Max}", Ink.Dark),
            (Loc.T("karta_kryt"), $"{_g.CritPct()}%", Ink.Dark),
        ];
        var h = big ? 42f : 36f;
        var card = p.CardH(y, h);
        var w = card.Size.X / cols.Length;
        for (var i = 0; i < cols.Length; i++)
        {
            var cx = Mathf.Round(card.Position.X + w * i + w / 2);
            if (i > 0) p.C.DrawRect(new Rect2(card.Position.X + w * i, card.Position.Y + 8, 1, h - 16), Pal.Border);
            p.F.Draw(p.C, new Vector2(cx, card.Position.Y + (big ? 4 : 2)), p.F.Fit(cols[i].Label, (int)w - 4), Ink.Dim, TextAlign.Center);
            p.F.Draw(p.C, new Vector2(cx, card.Position.Y + (big ? 21 : 18)), p.F.Fit(cols[i].Value, (int)w - 4), cols[i].Ink, TextAlign.Center, 1, true);
        }
        return card.End.Y;
    }
}
