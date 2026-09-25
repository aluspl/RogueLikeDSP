using Godot;

namespace LifeLike.Game.Gfx;

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

    /// <summary>Gradient fioletu marki na planszach (make_gradient na GBA): góra i dół.</summary>
    public static readonly Color GradientTop = new(124 / 255f, 98 / 255f, 1f);
    public static readonly Color GradientBottom = new(44 / 255f, 30 / 255f, 150 / 255f);
    /// <summary>Obudowa telefonu (obrys) i jaśniejszy fiolet pastylki grupy.</summary>
    public static readonly Color PhoneBezel = new("3a3550");
    public static readonly Color PillGroup = new("e2dafd");
    /// <summary>Pasek zbudowanych domów na Osiedlu.</summary>
    public static readonly Color EstateBar = new("8bc34a");

    /// <summary>Mapa: poświata schodów, szara ikona (moc się ładuje), bohater po przegranej.</summary>
    public static readonly Color StairsGlow = new(1f, 0.92f, 0.5f);
    public static readonly Color Grayed = new(0.55f, 0.55f, 0.6f);
    public static readonly Color DeadHero = new(0.5f, 0.5f, 0.55f);

    /// <summary>Błyski: bohater trafiony, ekran po obrażeniach, kryt, awans, pulsowanie przy niskim HP.</summary>
    public static readonly Color HurtFlash = new(1f, 0.25f, 0.25f);
    public static readonly Color HurtTint = new(1f, 0.13f, 0.13f);
    public static readonly Color CritTint = new(1f, 0.87f, 0.25f);
    public static readonly Color LevelFlash = new(1f, 0.95f, 0.5f);
    public static readonly Color LowHpTint = new(0.9f, 0.05f, 0.05f);

    /// <summary>Błysk ekranu w kolorze mocy zawodu (ability_fx na GBA).</summary>
    public static readonly Color FlashStun = new(0.65f, 0.52f, 1f);
    public static readonly Color FlashWall = new(1f, 0.52f, 0.2f);
    public static readonly Color FlashVolley = new(1f, 0.9f, 0.26f);
    public static readonly Color FlashChain = new(0.4f, 0.9f, 1f);
    public static readonly Color FlashFlush = new(0.26f, 0.97f, 0.52f);

    /// <summary>Kolor paska HP wg progu jak na GBA: &gt;50% zielony, &gt;25% żółty, reszta czerwony.</summary>
    public static int HpColor(int hp, int max) => hp * 2 > max ? 0 : hp * 4 > max ? 1 : 2;
}
