using Godot;

namespace LifeLike.Game;

/// <summary>
/// Paleta gry: tokeny AppColors aplikacji PlanBudowlany (jak PHONE_PAL w GBA/tools/make_assets.py)
/// i kolory mapy / HUD z wersji GBA.
/// </summary>
public static class Pal
{
    public static readonly Color Brand = new("6b4eff");
    public static readonly Color Accent = new("ff7a3d");
    public static readonly Color Todo = new("94a3b8");
    public static readonly Color Prog = new("f59e0b");
    public static readonly Color Done = new("10b981");
    public static readonly Color Late = new("ef4444");
    public static readonly Color Bg = new("f6f5f8");
    public static readonly Color Card = new("ffffff");
    public static readonly Color Text = new("0b0b0f");
    public static readonly Color Dim = new("636366");
    public static readonly Color Group = new("efeaf7");
    public static readonly Color Border = new("e5e7eb");
    public static readonly Color ProgBg = new("fef3de");
    public static readonly Color LateBg = new("fee2e2");
    public static readonly Color DoneBg = new("d1fae5");

    /// <summary>Ciemny fiolet marki: tła ekranów tekstowych (bn::color(3, 2, 8) na GBA).</summary>
    public static readonly Color Navy = new("1b1440");
    public static readonly Color ScreenBg = new("181040");
    /// <summary>Tło poza mapą (bn::color(1, 1, 3)).</summary>
    public static readonly Color Void = new("0c0c14");
    /// <summary>Kolor mgły: pola zapamiętane i skraj pola widzenia (paleta światła 1-3 na GBA).</summary>
    public static readonly Color Fog = new("0e0b23");

    /// <summary>Paski HP (HP_PAL): obrys, tło, połysk, zielony/żółty/czerwony z cieniem.</summary>
    public static readonly Color HpEdge = new("101018");
    public static readonly Color HpBack = new("363a48");
    public static readonly Color HpShine = new("fafafa");
    public static readonly Color[] HpMain = [new("4caf50"), new("f5d33d"), new("d63c3c")];
    public static readonly Color[] HpShade = [new("2e6b30"), new("be961e"), new("8c1e1e")];

    /// <summary>Kolor paska HP wg progu jak na GBA: &gt;50% zielony, &gt;25% żółty, reszta czerwony.</summary>
    public static int HpColor(int hp, int max) => hp * 2 > max ? 0 : hp * 4 > max ? 1 : 2;
}
