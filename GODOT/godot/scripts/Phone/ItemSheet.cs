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
/// Nazwa i rzadkość, podgląd trzech liczb „teraz → po” (Atak, Atak specjalny, Obrona; HP przy kamizelce) z + zielonym
/// i − czerwonym, cecha / profity jednym zdaniem, „Szczegóły” rozwijają tryby broni. Plecak: Załóż / Kabura / Na PSZOK
/// (w Hurtowni też Sprzedaj); noszone: Zmień (lista z plecaka z podglądem), Do plecaka, Na PSZOK; broń w ręku – Rozpiska,
/// kabura – Zamień. Broni zawodu nie zdejmuje się ani nie oddaje. Na placu każda zmiana to 1 tura.
/// Zamknij zawsze w lewym dolnym rogu, główna akcja w prawym. Strzałki wybierają przycisk, A / Enter zatwierdza,
/// B / Esc i dotknięcie obok zamykają (w liście Zmień – wracają do karty).
/// </summary>
public sealed class ItemSheet
{
    /// <summary>Indeks dotknięcia przycisku k = HitBase + k; HitBase + Outside = obok karty (zamyka).</summary>
    public const int HitBase = 300, Outside = 99, PickBase = 350;

    private const int ButtonH = 34, Gap = 6, Pad = 12;
    private readonly CoreGame _g;
    private int _sel;
    private bool _details;
    private bool _picking;
    private readonly List<int> _candidates = new();

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
        _picking = false;
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

    /// <summary>W liście „Zmień” (wybór z plecaka na noszone miejsce).</summary>
    public bool Picking => IsOpen && _picking;

    /// <summary>Slot dla Game.Unequip: sprzęt – numer slotu, broń w ręku / kabura – stałe Game.</summary>
    private int UnequipSlot => Kind switch
    {
        SheetKind.Hand => CoreGame.UnequipHand,
        SheetKind.Holster => CoreGame.UnequipHolster,
        _ => Index,
    };

    private bool ClassWeapon(BackpackItem it) => it.Kind == BackpackKind.Weapon && it.Id == _g.CDef.Weapon;

    /// <summary>Przedmioty z plecaka, które pasują na noszone miejsce (sprzęt tego slotu, broń do ręki / kabury).</summary>
    private List<int> Candidates()
    {
        _candidates.Clear();
        for (var i = 0; i < _g.BackpackSlots(); i++)
        {
            var it = _g.Backpack[i];
            if (it.Empty) continue;
            if (Kind == SheetKind.Gear ? it.Kind == BackpackKind.Gear && it.Id == Index : it.Kind == BackpackKind.Weapon) _candidates.Add(i);
        }
        return _candidates;
    }

    /// <summary>Przycisk wyszarzony (np. „Do plecaka” przy pełnym plecaku).</summary>
    private bool Enabled(SheetAct a) => a switch
    {
        SheetAct.Unequip => _g.CanUnequip(UnequipSlot, false),
        SheetAct.Change => Candidates().Count > 0 && _g.BackpackUsable,
        _ => true,
    };

    private bool Site => _g.St == GameStatus.Playing;

    /// <summary>Przyciski w kolejności rysowania: drugorzędne, potem Zamknij i główny (ostatni, prawy dolny róg).</summary>
    public List<SheetAct> Buttons()
    {
        var list = new List<SheetAct>();
        var it = Item;
        if (it.Empty || _picking) return [SheetAct.Close];
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
                if (_g.CanRecycle && !ClassWeapon(it)) list.Add(SheetAct.Recycle);
                if (usable) list.Add(SheetAct.Equip);
                main = list.Contains(Primary) ? Primary : usable ? SheetAct.Equip : SheetAct.Close;
                break;
            }
            default: // noszone: broń w ręku, kabura, slot sprzętu
            {
                if (weapon) list.Add(SheetAct.Details);
                if (Kind == SheetKind.Hand && Breakdown is not null) list.Add(SheetAct.Breakdown);
                if (Kind == SheetKind.Holster && _g.St == GameStatus.Playing) list.Add(SheetAct.Swap);
                if (_g.CanUnequip(UnequipSlot, true)) list.Add(SheetAct.Recycle);
                if (_g.CanUnequip(UnequipSlot, false) || (_g.BackpackFull() && _g.BackpackUsable && !(Kind == SheetKind.Hand && ClassWeapon(it)))) list.Add(SheetAct.Unequip);
                if (_g.BackpackUsable) list.Add(SheetAct.Change);
                main = list.Contains(SheetAct.Change) && Enabled(SheetAct.Change) ? SheetAct.Change
                    : Kind == SheetKind.Hand && Breakdown is not null ? SheetAct.Breakdown : SheetAct.Close;
                break;
            }
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
            SheetAct.Equip => Loc.T("bp_zaloz"),
            SheetAct.Holster => Loc.T("bp_kabura"),
            SheetAct.Change => Loc.T("karta_zmien"),
            SheetAct.Unequip => _g.BackpackFull() ? Loc.T("karta_plecak_pelny_btn") : Loc.T("karta_zdejmij"),
            SheetAct.Sell => Loc.T("skup_btn"),
            SheetAct.Recycle => Loc.T("pszok_btn"),
            SheetAct.Details => _details ? Loc.T("karta_mniej") : Loc.T("karta_szczegoly"),
            SheetAct.Breakdown => Loc.T("karta_rozpiska"),
            SheetAct.Swap => Loc.T("karta_zamien"),
            _ => Loc.T("karta_zamknij"),
        };
    }

    public bool Input(InputCmd e)
    {
        if (!IsOpen) return false;
        if (_picking)
        {
            var c = Candidates();
            if (e.VDir != 0 || e.HDir != 0)
            {
                _sel = Math.Clamp(_sel + (e.VDir != 0 ? e.VDir : e.HDir), 0, c.Count);
                Sfx.Play("menu");
            }
            else if (e.IsConfirm)
            {
                if (_sel < c.Count) Pick(c[_sel]);
                else _picking = false;
            }
            else if (e.IsBack)
            {
                _picking = false;
                _sel = Math.Max(0, Buttons().IndexOf(SheetAct.Change));
            }
            return true;
        }
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
            var act = Buttons()[Math.Clamp(_sel, 0, n - 1)];
            if (Enabled(act)) Run(act);
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
        if (_picking)
        {
            var c = Candidates();
            if (k >= PickBase - HitBase && k - (PickBase - HitBase) < c.Count) Pick(c[k - (PickBase - HitBase)]);
            else if (k == 0) _picking = false;
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
            case SheetAct.Change:
                if (!Enabled(SheetAct.Change)) return;
                _picking = true;
                _sel = 0;
                Sfx.Play("menu");
                return;
            case SheetAct.Unequip:
                turn = Site;
                ok = _g.Unequip(UnequipSlot, false);
                if (ok) SetNote(_g.Log[CoreGame.LogLines - 1].Text, Ink.Brand);
                break;
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
            case SheetAct.Sell:
                ok = _g.BackpackSell(Index);
                SetNote(ok ? Loc.F("skup_sprzedane", name) : Loc.F("skup_maks", _g.D.MaterialMax), ok ? Ink.Done : Ink.Late);
                break;
            default: // PSZOK: z plecaka albo noszone
                turn = Site;
                ok = Kind == SheetKind.Bag ? _g.BackpackRecycle(Index) : _g.Unequip(UnequipSlot, true);
                if (ok) SetNote(Loc.F("pszok_oddane", name), Ink.Done);
                break;
        }
        if (!ok) return;
        Sfx.Play(a switch { SheetAct.Sell => "buy", SheetAct.Recycle => "level", SheetAct.Unequip => "menu", _ => "pickup" }, a == SheetAct.Recycle ? 0.6f : 1f);
        Close();
        Done?.Invoke(turn);
    }

    /// <summary>Lista „Zmień”: przedmiot i z plecaka na noszone miejsce (do ręki / do kabury / slot sprzętu).</summary>
    private void Pick(int i)
    {
        var turn = Site;
        var ok = Kind == SheetKind.Holster ? _g.BackpackHolster(i) : _g.BackpackEquip(i);
        if (!ok) return;
        SetNote(_g.Log[CoreGame.LogLines - 1].Text, Ink.Brand);
        Sfx.Play("pickup");
        Close();
        Done?.Invoke(turn);
    }

    private void SetNote(string s, Ink ink)
    {
        Note = s;
        NoteInk = ink;
    }

    /// <summary>
    /// Liczby przedmiotu it (skąd: kind), opis rzadkości (kindText), zdanie (cecha / profity / odnowienie) i szczegóły (tryby
    /// broni). Plecak, znalezisko i kabura: podgląd „teraz → po założeniu” – Atak (cios), Atak specjalny (Patent), Obrona,
    /// przy kamizelce HP. Noszone: co daje (bez porównania). Znalezisko broni liczy się jak oferta (ToolOfferBreakdown).
    /// </summary>
    public List<ItemFact> Facts(BackpackItem it, SheetKind kind, out string kindText, out string sentence, out List<string> details)
    {
        var d = _g.D;
        var facts = new List<ItemFact>();
        details = new List<string>();
        var lines = new List<string>();
        var compare = kind is SheetKind.Bag or SheetKind.Holster or SheetKind.Find;
        if (Shop && kind == SheetKind.Bag && _g.CanTrade) // Hurtownia: ceny Skupu i PSZOK-u na górze karty
        {
            facts.Add(new ItemFact(Loc.T("hurt_tab_skup"), BackpackPage.SellText(_g, it), "", 0));
            facts.Add(new ItemFact(Loc.T("hurt_tab_pszok"), Loc.F("pszok_cena", _g.PszokXpFor(it)), "", 0));
        }
        var now = _g.WeaponBreakdown();
        var hw = _g.Weapon;
        if (it.Kind == BackpackKind.Weapon)
        {
            var w = d.Weapons[it.Id];
            kindText = $"{ItemArt.RarityName(d, it)} · {WeaponLook.Hits(d, w)}";
            if (it.Perks != 0) lines.Add(Loc.F("karta_profity", WeaponLook.PerkNames(d, it.Perks)));
            details.Add(WeaponLook.LineA(w));
            details.Add(Loc.T("tryb_fach") + WeaponLook.Mode(w, w.ModeB, true, WeaponLook.CdDelta(d, it.Rarity)));
            details.AddRange(WeaponLook.PerkLines(d, it.Perks, w.Master));
            if (!compare)
            {
                facts.Add(new ItemFact(Loc.T("karta_atak"), Range(now.Min, now.Max), "", 0));
                facts.Add(new ItemFact(Loc.T("karta_atak_spec"), Special(now, hw), "", 0));
                facts.Add(new ItemFact(Loc.T("karta_kryt"), $"{now.CritChance()}%", "", 0));
                sentence = string.Join("\n", lines);
                return facts;
            }
            var next = kind == SheetKind.Find ? _g.ToolOfferBreakdown() : _g.WeaponBreakdown(-1, it.Id, -1, -1, 0, it.Rarity, it.Perks);
            var lvl = kind == SheetKind.Find ? 0 : (it.Extra & 15) * d.ToolUpgradeDmg;
            next.Min += lvl;
            next.Max += lvl;
            next.Avg10 += lvl * 10;
            Preview(facts, now, next, hw, w, 0, 0);
            var cdNow = Math.Max(1, hw.ModeB.Cooldown + _g.WeaponBCdDelta());
            var cdNext = Math.Max(1, w.ModeB.Cooldown + _g.WeaponBCdDelta() - WeaponLook.CdDelta(d, _g.WeaponRarity) + WeaponLook.CdDelta(d, it.Rarity));
            if (cdNext != cdNow) lines.Add(Loc.F("karta_odnowienie", cdNow, cdNext));
            sentence = string.Join("\n", lines);
            return facts;
        }
        var slot = it.Id;
        var gd = d.Gear[slot * 3 + it.Rarity];
        var cur = _g.Equipped[slot];
        kindText = $"{ItemArt.RarityName(d, it)} · {d.GearSlots[slot]}";
        var trait = d.GearTraits[Math.Max(0, (int)it.Trait)].Name;
        if (!compare)
        {
            facts.Add(new ItemFact(UiText.GearStatName(gd.Stat), $"+{gd.Value}", "", 0));
            facts.Add(new ItemFact(Loc.T("karta_cecha"), trait, "", 0));
            sentence = "";
            return facts;
        }
        var nextG = _g.WeaponBreakdown(-1, -1, slot, it.Rarity, it.Trait);
        var old = cur >= 0 ? d.Gear[slot * 3 + cur] : null;
        int Stat(LifeLike.Core.Data.GearDef g, LifeLike.Core.Data.GearStat st) => g is not null && g.Stat == st ? g.Value : 0;
        var dDef = Stat(gd, LifeLike.Core.Data.GearStat.Def) - Stat(old, LifeLike.Core.Data.GearStat.Def);
        var dHp = gd.Stat == LifeLike.Core.Data.GearStat.Hp ? gd.Value - Stat(old, LifeLike.Core.Data.GearStat.Hp) : 0;
        Preview(facts, now, nextG, hw, hw, dDef, dHp);
        if (gd.Stat is not (LifeLike.Core.Data.GearStat.Def or LifeLike.Core.Data.GearStat.Hp or LifeLike.Core.Data.GearStat.Dmg)) // unik, termos – czwarta liczba
        {
            var dv = gd.Value - Stat(old, gd.Stat);
            facts.Add(new ItemFact(UiText.GearStatName(gd.Stat), $"+{Stat(old, gd.Stat)} -> +{gd.Value}", dv != 0 ? (dv > 0 ? "+" : "") + dv : "", Math.Sign(dv)));
        }
        lines.Add(cur >= 0 && _g.EquippedTrait[slot] != it.Trait ? Loc.F("karta_cecha_zmiana", d.GearTraits[Math.Max(0, (int)_g.EquippedTrait[slot])].Name, trait) : Loc.F("karta_cecha_zdanie", trait));
        sentence = string.Join("\n", lines);
        return facts;
    }

    private static string Range(int min, int max) => $"{min}-{max}";

    /// <summary>Atak specjalny (Patent): zakres ciosu × % trybu B (× ciosy).</summary>
    private static string Special(DmgBreakdown b, LifeLike.Core.Data.WeaponDef w)
    {
        var m = w.ModeB;
        var s = Range(HitCalc.Apply(b.Min, m.DmgPct), HitCalc.Apply(b.Max, m.DmgPct));
        return m.Hits > 1 ? $"{s} x{m.Hits}" : s;
    }

    private static int SpecialAvg10(DmgBreakdown b, LifeLike.Core.Data.WeaponDef w) => b.Avg10 * w.ModeB.DmgPct / 100 * Math.Max(1, w.ModeB.Hits);

    /// <summary>Trzy liczby „teraz → po”: Atak, Atak specjalny, Obrona (+ HP), z różnicą w pastylce.</summary>
    private void Preview(List<ItemFact> facts, DmgBreakdown now, DmgBreakdown next, LifeLike.Core.Data.WeaponDef wNow, LifeLike.Core.Data.WeaponDef wNext, int dDef, int dHp)
    {
        var dA = next.Avg10 - now.Avg10;
        facts.Add(new ItemFact(Loc.T("karta_atak"), $"{Range(now.Min, now.Max)} -> {Range(next.Min, next.Max)}", Tenths(dA), Math.Sign(dA)));
        var dS = SpecialAvg10(next, wNext) - SpecialAvg10(now, wNow);
        facts.Add(new ItemFact(Loc.T("karta_atak_spec"), $"{Special(now, wNow)} -> {Special(next, wNext)}", Tenths(dS), Math.Sign(dS)));
        var def = _g.HeroDefense();
        facts.Add(new ItemFact(Loc.T("postac_obrona"), $"{def} -> {def + dDef}", dDef != 0 ? (dDef > 0 ? "+" : "") + dDef : "", Math.Sign(dDef)));
        if (dHp != 0 || facts.Count == 0) facts.Add(new ItemFact("HP", $"{_g.Hero.MaxHp} -> {_g.Hero.MaxHp + dHp}", (dHp > 0 ? "+" : "") + dHp, Math.Sign(dHp)));
    }

    private static string Tenths(int d10) => d10 == 0 ? "" : DamageHelp.AddTenths(new Message(), d10, true).Text;

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
        var labelW = 0;
        foreach (var fact in facts) labelW = Math.Max(labelW, f.Measure(fact.Label));
        var vx = x + Mathf.Clamp(labelW + 10, 60, w * 0.45f);
        foreach (var fact in facts)
        {
            f.Draw(p.C, new Vector2(x, y), f.Fit(fact.Label, (int)(vx - x - 6)), Ink.Dim);
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
        if (Site && Kind != SheetKind.Hand || Site && Buttons().Contains(SheetAct.Change)) sentence += (sentence.Length > 0 ? "\n" : "") + Loc.T("karta_tura_nota");
        var sLines = sentence.Length > 0 ? f.Wrap(sentence, tw) : new List<string>();
        var dLines = new List<string>();
        if (_details) foreach (var l in details) dLines.AddRange(f.Wrap(l, tw));
        var buttons = Buttons();
        var rows = (buttons.Count + 1) / 2;
        var buttonH = ButtonH;
        var cands = _picking ? Candidates() : new List<int>();
        const int PickRow = 46;
        var avail = p.Bottom - p.Top;
        var pickRows = cands.Count;
        int Height() => _picking
            ? Pad + 20 + pickRows * PickRow + 8 + buttonH + Pad
            : Pad + 40 + 8 + facts.Count * 22 + 6 + (sLines.Count + dLines.Count) * 17 + 8 + rows * (buttonH + Gap) + Pad - Gap;
        // za mało miejsca (telefon w poziomie): najpierw bez szczegółów i zdania, potem niższe przyciski, mniej wierszy listy
        if (Height() > avail) dLines.Clear();
        if (Height() > avail) sLines.Clear();
        if (Height() > avail) buttonH = 28;
        if (Height() > avail) facts.RemoveAll(fc => fc.Label == Loc.T("hurt_tab_skup") || fc.Label == Loc.T("hurt_tab_pszok")); // ceny są też pod kaflami
        while (_picking && Height() > avail && pickRows > 1) pickRows--;
        var h = Height();
        var top = Mathf.Max(p.Top, p.Bottom - h);
        var full = PhoneView.Full;
        var card = new Rect2(p.Content.Position.X - (full ? 8 : 2), top, p.Content.Size.X + (full ? 16 : 4), p.Bottom - top + (full ? 200 : 2)); // pionowo: od krawędzi do krawędzi, aż pod dół ekranu
        p.C.DrawStyleBox(Ui.Box(Pal.Card, 12), card);
        p.C.DrawRect(new Rect2(card.GetCenter().X - 16, top + 5, 32, 3), Pal.Border); // uchwyt arkusza
        p.Hit(new Rect2(p.Content.Position.X, p.Top - 400, p.Content.Size.X, top - p.Top + 400), HitBase + Outside);
        var x = p.Left + 4;
        var y = top + Pad;
        var focus = !Layout.Touch;
        if (_picking)
        {
            f.Draw(p.C, new Vector2(x, y), f.Fit(Loc.F("karta_zmien_na", _g.BackpackName(it)), tw), Ink.Dim);
            y += 20;
            for (var k = 0; k < pickRows; k++)
            {
                var ci = cands[k];
                var cit = _g.Backpack[ci];
                var r = new Rect2(x, y, w, PickRow - 4);
                if (focus && k == _sel) p.C.DrawStyleBox(Ui.Box(Colors.Transparent, 9, Pal.Brand), r.Grow(2));
                p.C.DrawStyleBox(Ui.Box(Pal.Bg, 8), r);
                var tile = new Rect2(r.Position.X + 4, r.Position.Y + 3, 36, 36);
                ItemTile.Draw(p, d, tile, cit, 0, false);
                var nx = tile.End.X + 8;
                f.Draw(p.C, new Vector2(nx, r.Position.Y + 3), f.Fit(_g.BackpackName(cit), (int)(r.End.X - nx - 4)), ItemArt.RarityInk(d, cit), TextAlign.Left, 1, true);
                var cf = Facts(cit, Kind == SheetKind.Holster ? SheetKind.Holster : SheetKind.Bag, out _, out _, out _);
                var px = nx;
                foreach (var fc in cf)
                {
                    if (fc.Delta.Length == 0 || fc.Label == Loc.T("hurt_tab_skup") || fc.Label == Loc.T("hurt_tab_pszok")) continue;
                    var part = $"{Short(fc.Label)} {fc.Delta}";
                    if (px + f.Measure(part) > r.End.X - 4) break;
                    px += f.Draw(p.C, new Vector2(px, r.Position.Y + 21), part, fc.Sign > 0 ? Ink.Done : fc.Sign < 0 ? Ink.Late : Ink.Dim) + 10;
                }
                if (px == nx) f.Draw(p.C, new Vector2(nx, r.Position.Y + 21), Loc.T("karta_bez_zmian"), Ink.Dim);
                p.Hit(r, PickBase + k);
                y += PickRow;
            }
            var back = new Rect2(x, card.Position.Y + h - Pad - buttonH, w, buttonH);
            if (focus && _sel >= pickRows) p.C.DrawStyleBox(Ui.Box(Colors.Transparent, 10, Pal.Brand), back.Grow(3));
            p.C.DrawStyleBox(Ui.Box(Pal.Card, 9, Pal.Border), back);
            f.Draw(p.C, new Vector2(back.GetCenter().X, Mathf.Round(back.GetCenter().Y - 8)), Loc.T("wroc"), Ink.Brand, TextAlign.Center, 1, true);
            p.Hit(back, HitBase);
            p.Reach(card.Position.Y + h);
            return;
        }
        y = DrawHeader(p, it, kind, x, y, w);
        y = DrawFacts(p, facts, x, y, w);
        y += 4;
        foreach (var l in sLines)
        {
            f.Draw(p.C, new Vector2(x, y), l, l == Loc.T("karta_tura_nota") || it.Kind == BackpackKind.Gear ? Ink.Dim : ItemArt.RarityInk(d, it));
            y += 17;
        }
        foreach (var l in dLines)
        {
            f.Draw(p.C, new Vector2(x, y), l, Ink.Dim);
            y += 17;
        }
        // przyciski: od dołu, główny w prawym dolnym rogu, Zamknij obok niego
        var bw = (w - Gap) / 2;
        for (var i = 0; i < buttons.Count; i++)
        {
            var fromEnd = buttons.Count - 1 - i;
            var row = fromEnd / 2;
            var col = 1 - fromEnd % 2;
            var wide = buttons.Count % 2 == 1 && i == 0 && buttons.Count > 1; // pojedynczy przycisk w górnym rzędzie – na całą szerokość
            var r = new Rect2(Mathf.Round(x + (wide ? 0 : col * (bw + Gap))), Mathf.Round(card.Position.Y + h - Pad - buttonH - row * (buttonH + Gap)), Mathf.Round(wide ? 2 * bw + Gap : bw), buttonH);
            var a = buttons[i];
            var on = Enabled(a);
            var primary = i == buttons.Count - 1 && a != SheetAct.Close && on;
            var danger = a == SheetAct.Recycle;
            if (focus && i == _sel) p.C.DrawStyleBox(Ui.Box(Colors.Transparent, 10, Pal.Brand), r.Grow(3));
            p.C.DrawStyleBox(Ui.Box(primary ? Pal.Brand : on ? Pal.Card : Pal.Bg, 9, primary ? Pal.Brand : Pal.Border), r);
            var label = Label(a);
            var ink = !on ? Ink.Dim : primary ? Ink.White : danger ? Ink.Late : Ink.Brand;
            f.Draw(p.C, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 8)), f.Fit(label, (int)r.Size.X - 10 - label.Length), ink, TextAlign.Center, 1, true);
            if (on) p.Hit(r, HitBase + i);
        }
        p.Reach(card.Position.Y + h);
    }

    /// <summary>Krótka etykieta liczby w liście „Zmień” (pierwsze słowo).</summary>
    private static string Short(string label)
    {
        if (label == Loc.T("karta_atak_spec")) return Loc.T("karta_spec_krotko");
        var i = label.IndexOf(' ');
        return i > 0 ? label[..i] + "." : label;
    }
}
