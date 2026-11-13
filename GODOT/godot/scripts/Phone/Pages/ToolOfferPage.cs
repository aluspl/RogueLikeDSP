using System;
using System.Collections.Generic;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Nowe narzędzie na polu (v0.21.50 cz. 3, tool_offer_dialog na GBA). v0.23.2 (UI 2): karta znaleziska (FindCard) – nowa broń
/// z porównaniem ciosu i krytu do noszonej, wiersz „Noszone”, co stanie się z noszoną (do plecaka / zostaje / przepada),
/// przyciski Do plecaka (n/6), Zostaw i Weź. Domyślnie zaznaczone „Do plecaka” (pełny plecak – „Zostaw”).
/// </summary>
public sealed class ToolOfferPage : PhonePage
{
    private readonly CoreGame _g;

    public ToolOfferPage(CoreGame g) => _g = g;

    public override string Title => Loc.T("nowe_narzedzie");
    public override string Sub => _g.ToolOffer >= 0 ? _g.D.Weapons[_g.D.Tools[_g.ToolOffer].Weapon].Name : "";

    /// <summary>v0.23.2: zaznaczony wybór – OfferPage.Take (Weź), Store (Do plecaka, domyślnie), Leave (Zostaw).</summary>
    public int Sel { get; set; }

    /// <summary>Zatwierdzenie wyboru (ToolOfferScreen.Decide).</summary>
    public Action<int> Decided;

    public override string Hint => Loc.T("karta_hint");

    public override void Enter() => Sel = _g.BackpackFull() ? OfferPage.Leave : OfferPage.Store;

    public override bool Input(InputCmd e) => OfferPage.Move(e, _g.BackpackFull(), () => Sel, v => Sel = v);

    public override bool TapRow(int index)
    {
        var k = index - FindCard.HitBase;
        if (k is < 0 or > 2 || (k == OfferPage.Store && _g.BackpackFull())) return false;
        Sel = k;
        Decided?.Invoke(k);
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        if (_g.ToolOffer < 0) return;
        var d = _g.D;
        var w = d.Tools[_g.ToolOffer].Weapon;
        var nr = d.Weapons[w].Master ? d.WeaponRarities.Length - 1 : Math.Max(0, (int)_g.ToolOfferRarity); // unikat – Mistrzowska
        var found = BackpackItem.Weapon(w, nr, _g.ToolOfferBreakdown().Perks, 0);
        var worn = _g.HandItem();
        var full = _g.BackpackFull();
        var title = _g.WeaponTitle();
        var fate = _g.ToolTakeFate();
        var notes = new List<(string, Ink)>
        {
            fate == 0 ? (Loc.F("karta_los_plecak", title), Ink.Dim) : fate == 1 ? (Loc.F("karta_los_ziemia", title), Ink.Dim) : (Loc.F("karta_los_przepada", title), Ink.Late),
        };
        string[] labels =
        [
            Loc.T("karta_wez"),
            full ? Loc.F("karta_plecak_pelny", _g.BackpackCount(), _g.BackpackSlots()) : Loc.F("karta_do_plecaka", _g.BackpackCount(), _g.BackpackSlots()),
            Loc.T("zostaw"),
        ];
        FindCard.Draw(p, _g, found, worn, notes, labels, full, Sel);
    }
}
