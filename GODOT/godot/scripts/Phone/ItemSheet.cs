using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone;

/// <summary>
/// v0.23.2 (UI 2): karta przedmiotu – dolny arkusz nad przyciemnioną stroną (dotknięcie kafla plecaka albo slotu postaci).
/// Nazwa i rzadkość, 1–3 liczby z porównaniem do noszonego (+ zielone, − czerwone), cecha / profity jednym zdaniem,
/// „Szczegóły” rozwija tryby broni. Przyciski: Załóż (1 tura na placu) / Do kabury / Wyrzuć; w Hurtowni Sprzedaj (cena)
/// i PSZOK (dośw.); broń w ręku – Rozpiska; kabura – Zamień. Zamknij zawsze w lewym dolnym rogu, główna akcja w prawym.
/// Strzałki wybierają przycisk, A / Enter zatwierdza, B / Esc i dotknięcie obok zamykają.
/// </summary>
public sealed class ItemSheet
{
    /// <summary>Indeks dotknięcia przycisku k = HitBase + k; HitBase + Outside = obok karty (zamyka).</summary>
    public const int HitBase = 300, Outside = 99;

    private const int ButtonH = 34, Gap = 6, Pad = 12;
    private readonly CoreGame _g;
    private int _sel;
    private bool _details;

    public ItemSheet(CoreGame g) => _g = g;

    public bool IsOpen { get; private set; }
    public SheetKind Kind { get; private set; }

    /// <summary>Miejsce w plecaku (Bag) albo slot sprzętu (Gear).</summary>
    public int Index { get; private set; }

    /// <summary>Hurtownia: Sprzedaj i PSZOK; Primary – główna akcja zakładki (Equip, Sell, Recycle).</summary>
    public bool Shop { get; set; }

    public SheetAct Primary { get; set; } = SheetAct.Equip;

    /// <summary>Po akcji na przedmiocie: true = zużyła turę (ekran wraca na plac).</summary>
    public Action<bool> Done { get; set; }

    /// <summary>Rozpiska obrażeń broni w ręku (StatsPage).</summary>
    public Action Breakdown { get; set; }

    /// <summary>Wynik ostatniej akcji (np. „Sprzedane: Łom”) do pokazania na stronie.</summary>
    public string Note { get; private set; } = "";

    public Ink NoteInk { get; private set; } = Ink.Brand;

    public int Sel => _sel;

    public void Show(SheetKind kind, int index)
    {
        Kind = kind;
        Index = index;
        IsOpen = !Item.Empty;
        _details = false;
        var b = Buttons();
        _sel = Math.Max(0, b.Count - 1);
        if (IsOpen) Sfx.Play("menu");
    }

    public void Close() => IsOpen = false;

    public void ClearNote() => Note = "";

    public BackpackItem Item => Kind switch
    {
        SheetKind.Bag => Index >= 0 && Index < _g.BackpackSlots() ? _g.Backpack[Index] : default,
        SheetKind.Gear => _g.Equipped[Index] >= 0 ? BackpackItem.Gear(Index, _g.Equipped[Index], _g.EquippedTrait[Index]) : default,
        SheetKind.Hand => _g.HandItem(),
        _ => _g.HasBelt() && _g.Weapon2 >= 0 ? BackpackItem.Weapon(_g.Weapon2, _g.Weapon2Rarity, _g.Weapon2Perks, _g.Weapon2Lvl) : default,
    };

    private bool Site => _g.St == GameStatus.Playing;

    /// <summary>Przyciski w kolejności rysowania: drugorzędne, potem Zamknij i główny (ostatni, prawy dolny róg).</summary>
    public List<SheetAct> Buttons()
    {
        var list = new List<SheetAct>();
        var it = Item;
        if (it.Empty) return list;
        var weapon = it.Kind == BackpackKind.Weapon;
        var main = SheetAct.Close;
        switch (Kind)
        {
            case SheetKind.Bag:
            {
                var usable = _g.BackpackUsable;
                var shop = Shop && _g.CanTrade;
                if (weapon) list.Add(SheetAct.Details);
                if (weapon && _g.HasBelt() && usable) list.Add(SheetAct.Holster);
                if (shop) list.Add(SheetAct.Sell);
                if (shop) list.Add(SheetAct.Recycle);
                if (Site) list.Add(SheetAct.Drop);
                if (usable) list.Add(SheetAct.Equip);
                main = list.Contains(Primary) ? Primary : usable ? SheetAct.Equip : SheetAct.Close;
                break;
            }
            case SheetKind.Hand:
                list.Add(SheetAct.Details);
                main = Breakdown is null ? SheetAct.Close : SheetAct.Breakdown;
                break;
            case SheetKind.Holster:
                list.Add(SheetAct.Details);
                main = Site ? SheetAct.Swap : SheetAct.Close;
                break;
        }
        list.Remove(main);
        if (main != SheetAct.Close) list.Add(SheetAct.Close);
        list.Add(main);
        return list;
    }

    private string Label(SheetAct a)
    {
        var it = Item;
        return a switch
        {
            SheetAct.Equip => Site ? Loc.T("bp_zaloz_tura") : Loc.T("bp_zaloz"),
            SheetAct.Holster => Site ? Loc.T("bp_kabura_btn") : Loc.T("bp_kabura"),
            SheetAct.Drop => Loc.T("bp_wyrzuc_btn"),
            SheetAct.Sell => Loc.T("skup_btn"),
            SheetAct.Recycle => Loc.T("pszok_btn"),
            SheetAct.Details => _details ? Loc.T("karta_mniej") : Loc.T("karta_szczegoly"),
            SheetAct.Breakdown => Loc.T("karta_rozpiska"),
            SheetAct.Swap => Site ? Loc.T("karta_zamien_tura") : Loc.T("karta_zamien"),
            _ => Loc.T("karta_zamknij"),
        };
    }

    public bool Input(InputCmd e)
    {
        if (!IsOpen) return false;
        var n = Buttons().Count;
        var d = e.HDir != 0 ? e.HDir : e.VDir * 2;
        if (d != 0)
        {
            _sel = Math.Clamp(_sel + d, 0, n - 1);
            Sfx.Play("menu");
            return true;
        }
        if (e.IsConfirm)
        {
            Run(Buttons()[Math.Clamp(_sel, 0, n - 1)]);
            return true;
        }
        if (e.IsBack)
        {
            Close();
            return true;
        }
        return true; // karta zasłania stronę – reszta klawiszy nic nie robi
    }

    /// <summary>Dotknięcie (index z PhonePainter.Hit); false = nie dotyczy karty.</summary>
    public bool Tap(int index)
    {
        if (!IsOpen || index < HitBase || index > HitBase + Outside) return false;
        var k = index - HitBase;
        if (k == Outside)
        {
            Close();
            return true;
        }
        var b = Buttons();
        if (k < b.Count)
        {
            _sel = k;
            Run(b[k]);
        }
        return true;
    }

    private void Run(SheetAct a)
    {
        var it = Item;
        var name = _g.BackpackName(it);
        var turn = false;
        bool ok;
        switch (a)
        {
            case SheetAct.Close:
                Close();
                return;
            case SheetAct.Details:
                _details = !_details;
                return;
            case SheetAct.Breakdown:
                Close();
                Breakdown?.Invoke();
                return;
            case SheetAct.Swap:
                ok = LifeLike.Game.Screens.Play.PlayCommands.SwapOrNotice(_g);
                turn = ok && Site;
                SetNote(_g.Log[CoreGame.LogLines - 1].Text, Ink.Brand);
                break;
            case SheetAct.Equip:
                turn = Site;
                ok = _g.BackpackEquip(Index);
                if (ok) SetNote(_g.Log[CoreGame.LogLines - 1].Text, Ink.Brand);
                break;
            case SheetAct.Holster:
                turn = Site;
                ok = _g.BackpackHolster(Index);
                if (ok) SetNote(_g.Log[CoreGame.LogLines - 1].Text, Ink.Brand);
                break;
            case SheetAct.Drop:
                ok = _g.BackpackDrop(Index);
                if (ok) SetNote(_g.Log[CoreGame.LogLines - 1].Text, Ink.Dim);
                break;
            case SheetAct.Sell:
                ok = _g.BackpackSell(Index);
                SetNote(ok ? Loc.F("skup_sprzedane", name) : Loc.F("skup_maks", _g.D.MaterialMax), ok ? Ink.Done : Ink.Late);
                break;
            default:
                ok = _g.BackpackRecycle(Index);
                if (ok) SetNote(Loc.F("pszok_oddane", name), Ink.Done);
                break;
        }
        if (!ok) return;
        Sfx.Play(a switch { SheetAct.Sell => "buy", SheetAct.Recycle => "level", SheetAct.Drop => "menu", _ => "pickup" }, a == SheetAct.Recycle ? 0.6f : 1f);
        Close();
        Done?.Invoke(turn);
    }

    private void SetNote(string s, Ink ink)
    {
        Note = s;
        NoteInk = ink;
    }

    /// <summary>
    /// Liczby przedmiotu it (skąd: kind) z porównaniem do noszonego, opis rzadkości (kindText), zdanie (profity / co noszone)
    /// i szczegóły (tryby broni). Znalezisko broni (Find) liczy się jak oferta narzędzia (ToolOfferBreakdown).
    /// </summary>
    public List<ItemFact> Facts(BackpackItem it, SheetKind kind, out string kindText, out string sentence, out List<string> details)
    {
        var d = _g.D;
        var facts = new List<ItemFact>();
        details = new List<string>();
        sentence = "";
        var compare = kind is SheetKind.Bag or SheetKind.Holster or SheetKind.Find;
        if (Shop && kind == SheetKind.Bag && _g.CanTrade) // Hurtownia: ceny Skupu i PSZOK-u na górze karty
        {
            facts.Add(new ItemFact(Loc.T("hurt_tab_skup"), BackpackPage.SellText(_g, it), "", 0));
            facts.Add(new ItemFact(Loc.T("hurt_tab_pszok"), Loc.F("pszok_cena", _g.PszokXpFor(it)), "", 0));
        }
        if (it.Kind == BackpackKind.Weapon)
        {
            var w = d.Weapons[it.Id];
            var now = _g.WeaponBreakdown();
            var hand = kind == SheetKind.Hand;
            var next = hand ? now : kind == SheetKind.Find ? _g.ToolOfferBreakdown() : _g.WeaponBreakdown(-1, it.Id, -1, -1, 0, it.Rarity, it.Perks);
            var lvl = hand || kind == SheetKind.Find ? 0 : (it.Extra & 15) * d.ToolUpgradeDmg;
            var avg = next.Avg10 + lvl * 10;
            var dAvg = avg - now.Avg10;
            facts.Add(new ItemFact(Loc.T("karta_cios"), $"{next.Min + lvl}-{next.Max + lvl}",
                compare && dAvg != 0 ? DamageHelp.AddTenths(new Message(), dAvg, true).Text : "", compare ? Math.Sign(dAvg) : 0));
            var dCrit = next.CritChance() - now.CritChance();
            facts.Add(new ItemFact(Loc.T("karta_kryt"), $"{next.CritChance()}%", compare && dCrit != 0 ? (dCrit > 0 ? "+" : "") + dCrit + "%" : "", compare ? Math.Sign(dCrit) : 0));
            facts.Add(new ItemFact(Loc.T("karta_patent"), ModeText.BName(w), "", 0));
            kindText = $"{ItemArt.RarityName(d, it)} · {WeaponLook.Hits(d, w)}";
            if (it.Perks != 0) sentence = Loc.F("karta_profity", WeaponLook.PerkNames(d, it.Perks));
            details.Add(WeaponLook.LineA(w));
            details.Add(Loc.T("tryb_fach") + WeaponLook.Mode(w, w.ModeB, true, WeaponLook.CdDelta(d, it.Rarity)));
            details.AddRange(WeaponLook.PerkLines(d, it.Perks, w.Master));
            return facts;
        }
        var slot = it.Id;
        var gd = d.Gear[slot * 3 + it.Rarity];
        var cur = _g.Equipped[slot];
        var curVal = cur >= 0 ? d.Gear[slot * 3 + cur].Value : 0;
        var dv = gd.Value - curVal;
        facts.Add(new ItemFact(UiText.GearStatName(gd.Stat), $"+{gd.Value}", compare && dv != 0 ? (dv > 0 ? "+" : "") + dv : "", compare ? Math.Sign(dv) : 0));
        facts.Add(new ItemFact(Loc.T("karta_cecha"), d.GearTraits[Math.Max(0, (int)it.Trait)].Name, "", 0));
        kindText = $"{ItemArt.RarityName(d, it)} · {d.GearSlots[slot]}";
        if (kind == SheetKind.Bag)
            sentence = cur >= 0 ? Loc.F("bp_teraz", $"{d.Gear[slot * 3 + cur].Name}, {d.GearTraits[Math.Max(0, (int)_g.EquippedTrait[slot])].Name}") : Loc.T("bp_teraz_pusto");
        return facts;
    }

    /// <summary>Nagłówek karty: kafel z obrazkiem (icon px), nazwa pogrubiona i rzadkość pod nią; zwraca y pod nagłówkiem.</summary>
    public float DrawHeader(PhonePainter p, BackpackItem it, string kindText, float x, float y, float w, float icon = 40)
    {
        var d = _g.D;
        var f = p.F;
        var r = new Rect2(x, y, icon, icon);
        p.C.DrawStyleBox(Ui.Box(ItemArt.Rarity(d, it), 7), r);
        p.C.DrawStyleBox(Ui.Box(Pal.Card, 5), r.Grow(-2));
        ItemArt.Draw(p.C, d, it, r.Grow(-4));
        var nx = r.End.X + 8;
        var ty = y + Mathf.Round((icon - 38) / 2);
        f.Draw(p.C, new Vector2(nx, ty + 2), f.Fit(_g.BackpackName(it), (int)(x + w - nx - 8)), Ink.Dark, TextAlign.Left, 1, true);
        f.Draw(p.C, new Vector2(nx, ty + 21), f.Fit(kindText, (int)(x + w - nx)), ItemArt.RarityInk(d, it));
        p.Reach(y + icon);
        return y + icon + 8;
    }

    /// <summary>Wiersze liczb (etykieta, wartość pogrubiona, zmiana w pastylce przy prawej krawędzi); zwraca y pod nimi.</summary>
    public static float DrawFacts(PhonePainter p, List<ItemFact> facts, float x, float y, float w)
    {
        var f = p.F;
        foreach (var fact in facts)
        {
            f.Draw(p.C, new Vector2(x, y), fact.Label, Ink.Dim);
            var vx = x + Mathf.Max(70, w * 0.32f);
            var dw = fact.Delta.Length > 0 ? f.Measure(fact.Delta, 1, true) + 12 : 0;
            f.Draw(p.C, new Vector2(vx, y), f.Fit(fact.Value, (int)(x + w - vx - dw)), Ink.Dark, TextAlign.Left, 1, true);
            if (fact.Delta.Length > 0)
            {
                var (bg, ink) = fact.Sign > 0 ? (Pal.DoneBg, Ink.Done) : fact.Sign < 0 ? (Pal.LateBg, Ink.Late) : (Pal.Border, Ink.Dim);
                var pr = new Rect2(x + w - dw, y - 1, dw, 18);
                p.C.DrawStyleBox(Ui.Box(bg, 8), pr);
                f.Draw(p.C, new Vector2(pr.GetCenter().X, y), fact.Delta, ink, TextAlign.Center, 1, true);
            }
            y += 22;
            p.Reach(y);
        }
        return y;
    }

    /// <summary>Przyciemnienie strony i karta przy dolnej krawędzi treści.</summary>
    public void Draw(PhonePainter p)
    {
        if (!IsOpen) return;
        var it = Item;
        if (it.Empty)
        {
            Close();
            return;
        }
        var d = _g.D;
        var f = p.F;
        p.C.DrawRect(new Rect2(p.Content.Position.X - 16, p.Top - 400, p.Content.Size.X + 32, p.Bottom - p.Top + 800), new Color(0.05f, 0.04f, 0.12f, 0.5f));
        var facts = Facts(it, Kind, out var kind, out var sentence, out var details);
        var w = p.Width - 8;
        var tw = (int)w;
        var sLines = sentence.Length > 0 ? f.Wrap(sentence, tw) : new List<string>();
        var dLines = new List<string>();
        if (_details) foreach (var l in details) dLines.AddRange(f.Wrap(l, tw));
        var buttons = Buttons();
        var rows = (buttons.Count + 1) / 2;
        var buttonH = ButtonH;
        int Height() => Pad + 40 + 8 + facts.Count * 22 + 6 + (sLines.Count + dLines.Count) * 17 + 8 + rows * (buttonH + Gap) + Pad - Gap;
        var avail = p.Bottom - p.Top;
        // za mało miejsca (telefon w poziomie): najpierw bez szczegółów i zdania, potem bez Patentu, niższe przyciski
        if (Height() > avail) dLines.Clear();
        if (Height() > avail) sLines.Clear();
        if (Height() > avail) facts.RemoveAll(fc => fc.Label == Loc.T("karta_patent"));
        if (Height() > avail) buttonH = 28;
        var h = Height();
        var top = Mathf.Max(p.Top, p.Bottom - h);
        var full = PhoneView.Full;
        var card = new Rect2(p.Content.Position.X - (full ? 8 : 2), top, p.Content.Size.X + (full ? 16 : 4), p.Bottom - top + (full ? 200 : 2)); // pionowo: od krawędzi do krawędzi, aż pod dół ekranu
        p.C.DrawStyleBox(Ui.Box(Pal.Card, 12), card);
        p.C.DrawRect(new Rect2(card.GetCenter().X - 16, top + 5, 32, 3), Pal.Border); // uchwyt arkusza
        p.Hit(new Rect2(p.Content.Position.X, p.Top - 400, p.Content.Size.X, top - p.Top + 400), HitBase + Outside);
        var x = p.Left + 4;
        var y = top + Pad;
        y = DrawHeader(p, it, kind, x, y, w);
        y = DrawFacts(p, facts, x, y, w);
        y += 4;
        foreach (var l in sLines)
        {
            f.Draw(p.C, new Vector2(x, y), l, Kind == SheetKind.Bag && Item.Kind == BackpackKind.Gear ? Ink.Dim : ItemArt.RarityInk(d, it));
            y += 17;
        }
        foreach (var l in dLines)
        {
            f.Draw(p.C, new Vector2(x, y), l, Ink.Dim);
            y += 17;
        }
        // przyciski: od dołu, główny w prawym dolnym rogu, Zamknij obok niego
        var bw = (w - Gap) / 2;
        var focus = !Layout.Touch;
        for (var i = 0; i < buttons.Count; i++)
        {
            var fromEnd = buttons.Count - 1 - i;
            var row = fromEnd / 2;
            var col = 1 - fromEnd % 2;
            var wide = buttons.Count % 2 == 1 && i == 0 && buttons.Count > 1; // pojedynczy przycisk w górnym rzędzie – na całą szerokość
            var r = new Rect2(Mathf.Round(x + (wide ? 0 : col * (bw + Gap))), Mathf.Round(card.Position.Y + h - Pad - buttonH - row * (buttonH + Gap)), Mathf.Round(wide ? 2 * bw + Gap : bw), buttonH);
            var a = buttons[i];
            var primary = i == buttons.Count - 1 && a != SheetAct.Close;
            var danger = a == SheetAct.Drop;
            if (focus && i == _sel) p.C.DrawStyleBox(Ui.Box(Colors.Transparent, 10, Pal.Brand), r.Grow(3));
            p.C.DrawStyleBox(Ui.Box(primary ? Pal.Brand : Pal.Card, 9, primary ? Pal.Brand : Pal.Border), r);
            var label = Label(a);
            f.Draw(p.C, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 8)), f.Fit(label, (int)r.Size.X - 10 - label.Length), primary ? Ink.White : danger ? Ink.Late : Ink.Brand, TextAlign.Center, 1, true);
            p.Hit(r, HitBase + i);
        }
        p.Reach(card.Position.Y + h);
    }
}
