using System.Collections.Generic;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Paczka sprzętu przy zajętym slocie (gear_offer_dialog na GBA). v0.23.2 (UI 2): karta znaleziska (FindCard) – nowy
/// przedmiot z porównaniem do noszonego, wiersz „Noszone”, przyciski Do plecaka (n/6), Zostaw (+dośw.) i Załóż.
/// Strzałki / dotknięcie wybierają, A / Enter zatwierdza, B / Esc = Zostaw.
/// </summary>
public sealed class OfferPage : PhonePage
{
    /// <summary>v0.23.2: wybory karty – Załóż, Do plecaka, Zostaw.</summary>
    public const int Take = 0, Store = 1, Leave = 2;

    private readonly CoreGame _g;

    public OfferPage(CoreGame g) => _g = g;

    /// <summary>Zaznaczony wybór (Take / Store / Leave).</summary>
    public int Sel { get; set; }

    /// <summary>Zatwierdzenie wyboru (OfferScreen.Decide).</summary>
    public System.Action<int> Decided { get; set; }

    public override string Title => Loc.T("paczka_sprzetu");
    public override string Sub => _g.OfferSlot >= 0 ? _g.D.GearSlots[_g.OfferSlot] : "";
    public override string Hint => Loc.T("karta_hint");

    public override void Enter() => Sel = _g.OfferIsBetter ? Take : !_g.BackpackFull() ? Store : Leave;

    public override bool Input(InputCmd e) => Move(e, _g.BackpackFull(), () => Sel, v => Sel = v);

    /// <summary>Strzałki: kolejny wybór (pełny plecak – pomija „Do plecaka”); wspólne z kartą broni.</summary>
    public static bool Move(InputCmd e, bool storeOff, System.Func<int> get, System.Action<int> set)
    {
        var d = e.VDir != 0 ? e.VDir : e.HDir;
        if (d == 0) return false;
        var sel = (get() + d + 3) % 3;
        if (sel == Store && storeOff) sel = (sel + d + 3) % 3;
        set(sel);
        Audio.Sfx.Play("menu");
        return true;
    }

    /// <summary>Dotknięcie przycisku: od razu wybór (v0.23.2 – duże przyciski, blokada wejścia chroni przed przypadkiem).</summary>
    public override bool TapRow(int index)
    {
        var k = index - FindCard.HitBase;
        if (k is < 0 or > 2 || (k == Store && _g.BackpackFull())) return false;
        Sel = k;
        Decided?.Invoke(k);
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        var g = _g;
        var d = g.D;
        var slot = g.OfferSlot;
        if (slot < 0) return;
        var found = BackpackItem.Gear(slot, g.OfferRarity, g.OfferTrait);
        var worn = g.Equipped[slot] >= 0 ? BackpackItem.Gear(slot, g.Equipped[slot], g.EquippedTrait[slot]) : default;
        var full = g.BackpackFull();
        var notes = new List<(string, Ink)>();
        if (!worn.Empty) notes.Add((full ? Loc.F("karta_los_ziemia", d.Gear[slot * 3 + worn.Rarity].Name) : Loc.F("karta_los_plecak", d.Gear[slot * 3 + worn.Rarity].Name), Ink.Dim));
        var xp = g.OfferDeclineXp();
        if (xp > 0) notes.Add((Loc.F("karta_zostaw_nota", xp), Ink.Done));
        if (g.OfferDropsHolster) notes.Add((g.Weapon2 >= 0 ? Loc.F("uwaga_kabura", d.Weapons[g.Weapon2].Name) : Loc.T("uwaga_kabura_pusta"), Ink.Late));
        string[] labels =
        [
            Loc.T("bp_zaloz"),
            full ? Loc.F("karta_plecak_pelny", g.BackpackCount(), g.BackpackSlots()) : Loc.F("karta_do_plecaka", g.BackpackCount(), g.BackpackSlots()),
            Loc.T("zostaw"),
        ];
        FindCard.Draw(p, g, found, worn, notes, labels, full, Sel);
    }
}
