using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Phone.Pages;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone;

/// <summary>
/// v0.23.2 (UI 2): karta znaleziska (paczka sprzętu, broń na polu) w tym samym układzie co karta przedmiotu: nowy przedmiot
/// z liczbami i porównaniem do noszonego (+ / −), pod nim mały wiersz „Noszone”, ostrzeżenia, a na dole duże przyciski:
/// Do plecaka (n/6) na całą szerokość, pod nim Zostaw (lewo) i Załóż / Weź (prawo, główny). Wspólne dla OfferPage i ToolOfferPage.
/// </summary>
public static class FindCard
{
    /// <summary>Indeks dotknięcia przycisku k (OfferPage.Take / Store / Leave) = HitBase + k.</summary>
    public const int HitBase = 50;

    private const int ButtonH = 36, Gap = 8;

    public static void Draw(PhonePainter p, CoreGame g, BackpackItem found, BackpackItem worn, List<(string Text, Ink Ink)> notes, string[] labels, bool storeOff, int sel)
    {
        var f = p.F;
        var sheet = new ItemSheet(g);
        var facts = sheet.Facts(found, SheetKind.Find, out var kind, out var sentence, out _);
        var x = p.Left + 4;
        var w = p.Width - 8;
        var sLines = sentence.Length > 0 ? f.Wrap(sentence, (int)w - 16) : new List<string>();
        var nLines = new List<(string, Ink)>();
        foreach (var (t, ink) in notes) foreach (var l in f.Wrap(t, (int)w)) nLines.Add((l, ink));
        var buttonsTop = p.Bottom - 2 * ButtonH - Gap;
        var showWorn = !worn.Empty;
        float CardH() => 12 + 48 + 8 + facts.Count * 22 + sLines.Count * 17 + 8;
        float Need() => 18 + CardH() + 8 + (showWorn ? 18 + 44 + 8 : 0) + nLines.Count * 17 + 8;
        if (Need() > buttonsTop - p.Top) sLines.Clear();
        if (Need() > buttonsTop - p.Top) showWorn = false;
        if (Need() > buttonsTop - p.Top) // telefon w poziomie: każda uwaga w jednym wierszu
        {
            nLines.Clear();
            foreach (var (t, ink) in notes) nLines.Add((f.Fit(t, (int)w), ink));
        }
        var y = p.Section(p.Top, Loc.T("karta_znalezisko").ToUpperInvariant()) - 4;
        var card = p.CardH(y, CardH());
        var cx = card.Position.X + 10;
        var cw = card.Size.X - 20;
        var cy = sheet.DrawHeader(p, found, kind, cx, card.Position.Y + 10, cw, 48);
        cy = ItemSheet.DrawFacts(p, facts, cx, cy, cw);
        foreach (var l in sLines)
        {
            f.Draw(p.C, new Vector2(cx, cy), l, found.Kind == BackpackKind.Gear ? Ink.Dim : ItemArt.RarityInk(g.D, found));
            cy += 17;
        }
        y = card.End.Y + 8;
        if (showWorn)
        {
            y = p.Section(y, Loc.T("karta_noszone").ToUpperInvariant()) - 4;
            var wc = p.CardH(y, 44);
            var tile = new Rect2(wc.Position.X + 6, wc.Position.Y + 6, 32, 32);
            ItemTile.Draw(p, g.D, tile, worn, 0, false);
            var nx = tile.End.X + 8;
            f.Draw(p.C, new Vector2(nx, wc.Position.Y + 4), f.Fit(g.BackpackName(worn), (int)(wc.End.X - 8 - nx)), Ink.Dark);
            f.Draw(p.C, new Vector2(nx, wc.Position.Y + 22), f.Fit(WornLine(g, worn), (int)(wc.End.X - 8 - nx)), Ink.Dim);
            y = wc.End.Y + 8;
        }
        foreach (var (l, ink) in nLines)
        {
            f.Draw(p.C, new Vector2(x, y), l, ink);
            y += 17;
            p.Reach(y);
        }
        // przyciski: Do plecaka na całą szerokość, pod nim Zostaw | Załóż
        var focus = !Layout.Touch;
        var bw = (w - Gap) / 2;
        Rect2[] rects =
        [
            new(Mathf.Round(x + bw + Gap), buttonsTop + ButtonH + Gap, Mathf.Round(bw), ButtonH),
            new(x, buttonsTop, w, ButtonH),
            new(x, buttonsTop + ButtonH + Gap, Mathf.Round(bw), ButtonH),
        ];
        for (var k = 0; k < 3; k++)
        {
            var r = rects[k];
            var off = k == OfferPage.Store && storeOff;
            var primary = k == OfferPage.Take;
            if (focus && k == sel) p.C.DrawStyleBox(Ui.Box(Colors.Transparent, 11, Pal.Brand), r.Grow(3));
            p.C.DrawStyleBox(Ui.Box(primary ? Pal.Brand : off ? Pal.Bg : Pal.Card, 10, primary ? Pal.Brand : Pal.Border), r);
            var ink = primary ? Ink.White : off ? Ink.Dim : Ink.Brand;
            f.Draw(p.C, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 8)), f.Fit(labels[k], (int)r.Size.X - 10 - labels[k].Length), ink, TextAlign.Center, 1, true);
            if (!off) p.Hit(r, HitBase + k);
            p.Reach(r.End.Y);
        }
    }

    /// <summary>Noszone: broń – zakres ciosu i rzadkość, sprzęt – rzadkość, premia i cecha.</summary>
    private static string WornLine(CoreGame g, BackpackItem worn)
    {
        if (worn.Kind == BackpackKind.Weapon)
        {
            var b = g.WeaponBreakdown();
            return $"{ItemArt.RarityName(g.D, worn)} · {Loc.T("karta_cios")} {b.Min}-{b.Max}";
        }
        var gd = g.D.Gear[worn.Id * 3 + worn.Rarity];
        return $"{ItemArt.RarityName(g.D, worn)} · {UiText.GearStatName(gd.Stat)} +{gd.Value} · {g.D.GearTraits[System.Math.Max(0, (int)worn.Trait)].Name}";
    }
}
