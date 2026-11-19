using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Phone.Pages;

/// <summary>v0.23.2: zakładki Hurtowni nad treścią strony – Hurtownia, Skup, PSZOK, Plecak (dotknięcie, Q/E).</summary>
public static class HurtTabs
{
    /// <summary>Indeks dotknięcia zakładki k = HitBase + k.</summary>
    public const int HitBase = 200;

    public const int Shop = 0, Sell = 1, Recycle = 2, Bag = 3, Count = 4;

    public static string[] Labels => [Loc.T("hurt_tab_hurtownia"), Loc.T("hurt_tab_skup"), Loc.T("hurt_tab_pszok"), Loc.T("hurt_tab_plecak")];

    /// <summary>Pasek zakładek od y; zwraca y pod nim.</summary>
    public static float Draw(PhonePainter p, float y, int sel)
    {
        var labels = Labels;
        var w = p.Width / labels.Length;
        for (var k = 0; k < labels.Length; k++)
        {
            var r = new Rect2(Mathf.Round(p.Left + k * w + 1), y, Mathf.Round(w - 2), PhonePainter.RowH);
            p.C.DrawStyleBox(Ui.Box(k == sel ? Pal.Brand : Pal.Group, 6), r);
            p.Text(r.Position.X + r.Size.X / 2, y, p.F.Fit(labels[k], (int)r.Size.X - 4), k == sel ? Ink.White : Ink.Brand, TextAlign.Center);
            p.Hit(r, HitBase + k);
        }
        return y + PhonePainter.RowH + 4;
    }
}
