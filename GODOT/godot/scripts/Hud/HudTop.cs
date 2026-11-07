using LifeLike.Core;
using Godot;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Hud;

/// <summary>
/// Górny pasek HUD jak na GBA (półprzezroczysty ciemny pas): HP z paskiem, poziom z paskiem doświadczenia, etap
/// (w wąskim pionie numer etapu z trudnością w 1. rzędzie, pełna nazwa etapu w 2. rzędzie po prawej);
/// w drugim rzędzie stany z liczbą tur, termos, pogoda dnia, mechanika aktu (porywy: tury do kolejnego), materiały, wydarzenie na placu, ostrzeżenie o ciosie bossa, termos i ikona mocy
/// (szara z odliczaniem, gdy się ładuje; „R” i podskakiwanie, gdy gotowa); v0.23.0: broń w kolorze rzadkości z odnowieniem trybu B.
/// </summary>
public partial class HudTop : Control
{
    private CoreGame _g;
    private float _clock;

    public const int Height = 38;

    /// <summary>v0.23.0: pole broni i Patentu w drugim rzędzie (współrzędne paska) – najechanie myszą pokazuje opis ataku.</summary>
    public Rect2 WeaponRect { get; private set; }
    private const int HpBarW = 64, XpBarW = 40;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public void SetGame(CoreGame g)
    {
        _g = g;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        if (_g is not null) QueueRedraw();
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("HudTop", ex);
        }
    }

    private void DrawContent()
    {
        var g = _g;
        if (g is null) return;
        var f = PixelFont.I;
        var w = Size.X;
        if (OffsetTop > 0) DrawRect(new Rect2(-OffsetLeft, -OffsetTop, w + OffsetLeft - OffsetRight, OffsetTop), new Color(Pal.Text, 0.9f)); // pod wyspą
        DrawRect(new Rect2(-OffsetLeft, 0, w + OffsetLeft - OffsetRight, Height), new Color(Pal.Text, 0.62f));
        DrawRect(new Rect2(0, Height, w, 1), new Color(Pal.Brand, 0.55f));

        // rząd 1: HP, pasek, liczby, poziom z paskiem doświadczenia (szerokości od tekstu - HUD bywa wąski)
        var low = g.Hero.Hp * 4 <= g.Hero.MaxHp;
        f.Draw(this, new Vector2(6, 2), "HP", Ink.Map);
        var blinkOff = low && ((int)(_clock * 4) & 1) == 1;
        if (!blinkOff) DrawHpBar(new Rect2(26, 6, HpBarW, 8), g.Hero.Hp, g.Hero.MaxHp);
        var x = 32f + HpBarW;
        x += f.Draw(this, new Vector2(x, 2), $"{g.Hero.Hp}/{g.Hero.MaxHp}", low ? Ink.MapBad : Ink.Map) + 10;
        x += f.Draw(this, new Vector2(x, 2), Loc.F("poz_2", g.HeroLevel), Ink.MapLoot) + 5;
        var prev = g.HeroLevel >= 2 ? g.D.LevelThresholds[g.HeroLevel - 2] : 0;
        var fill = g.XpToNext() < 0 ? 1f : (g.RunXp - prev) / (float)Mathf.Max(1, g.D.LevelThresholds[g.HeroLevel - 1] - prev);
        var xr = new Rect2(x, 8, XpBarW, 4);
        DrawRect(xr.Grow(1), Pal.HpEdge);
        DrawRect(xr, Pal.HpBack);
        DrawRect(new Rect2(xr.Position, new Vector2(Mathf.Round(xr.Size.X * Mathf.Clamp(fill, 0, 1)), 4)), Pal.Brand);

        // etap i poziom trudności (prawa strona, jak „Etap 1/8 N” na GBA)
        var sd = g.SDef();
        var ng = g.Tier > 0 ? $" +{g.Tier}" : "";
        // v0.21.51: gdy „Etap 2/10: nazwa” nie mieści się w 1. rzędzie (wąski pion na telefonie), 1. rząd = „Etap 2/10,
        // Normalny”, a pełna nazwa etapu idzie do 2. rzędu po prawej (mierzona po ikonach - bez ucinania „Izolacja fu..”)
        var stageNo = Loc.F("etap_2", g.StageNumber(), g.StagesInRun());
        var right = $"{stageNo}: {sd.Name}";
        var re = w - 6 - Mathf.Ceil((SettingsButton.Side + 4) / Layout.HudScale); // miejsce na klucz ustawień
        var room1 = (int)(re - xr.End.X - 12);
        var nameBelow = f.Measure(right) > room1;
        if (nameBelow) // v0.22.0 (#69): gdy „Etap 3/10, Normalny” się nie mieści – skrót trudności (N/Ł/T, EN: N/E/H) zamiast „Normal..”
        {
            var full = $"{stageNo}, {g.DDef.Name}{ng}";
            var tag = $"{stageNo} {DifficultyTag(g.DDef.Name)}{ng}";
            var bare = $"{g.StageNumber()}/{g.StagesInRun()} {DifficultyTag(g.DDef.Name)}{ng}";   // najwęższy: „3/10 N”
            var row1 = f.Measure(full) <= room1 ? full : f.Measure(tag) <= room1 ? tag : f.Fit(bare, room1);
            f.Draw(this, new Vector2(re, 2), row1, Ink.Map, TextAlign.Right);
        }
        else
        {
            f.Draw(this, new Vector2(re, 2), right, Ink.Map, TextAlign.Right);
            f.Draw(this, new Vector2(re, 19), $"{g.DDef.Name}{ng}", Ink.MapDim, TextAlign.Right);
        }

        // rząd 2 (v0.22.0, UX A): ikony z krótkimi podpisami („Termos 0/4”, „Deszcz”, „Błoto”, „Moc R”), gdy mieszczą się
        // obok nazwy etapu i wydarzenia; inaczej same ikony jak dotąd
        var reserve = (nameBelow ? Mathf.Min(f.Measure(sd.Name), 160) : f.Measure($"{g.DDef.Name}{ng}")) + 8f; // prawa strona rzędu 2
        if (g.SlamTimer > 0) reserve += f.Measure(Loc.F("cios_za", g.SlamTimer)) + 8;   // zapowiedź ciosu bossa
        else if (g.CurrentEvent is { } cev) reserve += f.Measure(cev.Short) + 18;       // pastylka wydarzenia na placu
        var labels = Row2(f, false, true) <= re - 6 - reserve;
        x = Row2(f, true, labels);

        // nazwa etapu w 2. rzędzie (gdy nie zmieściła się w 1.): pierwszeństwo przed pastylką wydarzenia
        var nameRoom = re - x - 6;
        if (nameBelow)
        {
            var nw = Mathf.Min(f.Measure(sd.Name), (int)nameRoom);
            f.Draw(this, new Vector2(re, 19), f.Fit(sd.Name, (int)nameRoom), Ink.Map, TextAlign.Right);
            nameRoom -= nw + 8;
        }

        // wydarzenie na placu / zapowiedź ciosu bossa (gdy jest miejsce obok nazwy etapu)
        if (g.SlamTimer > 0)
        {
            var pulse = ((int)(_clock * 6) & 1) == 1;
            var warn = Loc.F("uwaga_cios_za", g.SlamTimer);
            if (f.Measure(warn) + 4 > nameRoom) warn = Loc.F("cios_za", g.SlamTimer);
            f.Draw(this, new Vector2(x + 4, 19), warn, pulse ? Ink.MapBad : Ink.MapLoot);
        }
        else if (g.CurrentEvent is { } ev && f.Measure(ev.Short) + 14 <= nameRoom)
        {
            var pill = f.Measure(ev.Short) + 10;
            var r = new Rect2(x + 4, 21, pill, 13);
            DrawRect(r, ev.Good ? new Color(Pal.Done, 0.85f) : new Color(Pal.Late, 0.85f));
            f.Draw(this, new Vector2(r.Position.X + pill / 2, 19), ev.Short, Ink.White, TextAlign.Center);
        }
    }

    /// <summary>Rząd 2 HUD: stany, termos, pogoda, mechanika aktu, moc, materiały; draw = false tylko mierzy. Zwraca prawą krawędź.</summary>
    private float Row2(PixelFont f, bool draw, bool labels)
    {
        var g = _g;
        var x = 6f;
        float Text(float tx, string t, Ink ink) => draw ? f.Draw(this, new Vector2(tx, 19), t, ink) : f.Measure(t);
        void Icon(Texture2D tex, int frame, int size, float ix, float iy)
        {
            if (draw) Assets.DrawFrame(this, tex, frame, size, new Vector2(ix, iy));
        }
        for (var k = 0; k < 3; k++)
        {
            var t = g.StatusTurns(UiText.HudStatuses[k]);
            if (t <= 0) continue;
            Icon(Assets.Particles, Assets.PStatus + k, Assets.Particle, x, 20);
            x += 16;
            x += Text(x, t.ToString(), Ink.MapBad) + 6;
        }
        var wet = g.StatusTurns(StatusEffect.Wet); // v0.21.50: mokry (prąd boli bardziej)
        if (wet > 0)
        {
            Icon(Assets.Particles, Assets.PDrop, Assets.Particle, x, 20);
            x += 16;
            x += Text(x, wet.ToString(), Ink.MapWet) + 6;
        }
        if (x > 6) x += 4;

        // termos
        Icon(Assets.UiMenu, 1, Assets.Icon, x, 20);
        x += 17;
        x += Text(x, (labels ? Loc.T("hud_termos") + " " : "") + $"{g.Thermos}/{g.ThermosCap()}", g.Thermos > 0 ? Ink.Map : Ink.MapDim) + 8;

        // pogoda dnia (menu_icons 6-10 jak na GBA)
        Icon(Assets.UiMenu, Assets.MenuWeather + (int)g.WDef.Effect, Assets.Icon, x, 20);
        x += labels ? 18 + Text(x + 18, g.WDef.Name, g.WDef.Bad ? Ink.MapBad : Ink.MapDim) + 8 : 20;

        // mechanika aktu (menu_icons 20-22: błoto, porywy, pył; 24: pieczątki Aktu 0); przy porywach tury do kolejnego (ostatnia na czerwono)
        var mech = (int)g.ADef.Mechanic;
        if (mech > 0)
        {
            Icon(Assets.UiMenu, Assets.ActIcon(g.ADef.Mechanic), Assets.Icon, x, 20);
            x += 17;
            if (labels) x += Text(x, g.ADef.MechShort, Ink.MapDim) + 4;
            var gin = g.GustIn();
            if (g.DocsNeeded() > 0) x += Text(x, $"{g.DocsCount()}/{g.DocsNeeded()}", g.StairsLocked() ? Ink.Map : Ink.MapGood) + 6;   // pieczątki
            else if (gin > 0) x += Text(x, gin.ToString(), gin == 1 ? Ink.MapBad : Ink.Map) + 6;
            else x += labels ? 6 : 3;
        }

        // moc zawodu: ikona szara z odliczaniem albo pulsująca z „R”
        var ready = g.AbilityCd == 0;
        var bob = ready && ((int)(_clock * 4) & 1) == 1 ? -1 : 0;
        Icon(ready ? Assets.UiAbility : Assets.UiAbilityGray, g.PowerCls(), Assets.Icon, x, 20 + bob);
        x += 18;
        if (labels) x += Text(x, Loc.T("hud_moc"), ready ? Ink.MapGood : Ink.MapDim) + 4;
        if (!ready) x += Text(x, g.AbilityCd.ToString(), Ink.MapDim) + 8;
        else if (((int)(_clock * 2) & 1) == 0) x += Text(x, "R", Ink.MapGood) + 8;
        else x += f.Measure("R") + 8;

        // v0.23.0: broń w kolorze rzadkości i odnowienie Patentu („P” – gotowy, liczba – tury)
        var wx = x;
        if (draw && !WeaponSheet.DrawIcon(this, g.Weapon, WeaponSheet.Rarity(g), new Rect2(x, 20, Assets.Icon, Assets.Icon)))
            Assets.DrawFrame(this, Assets.UiMenu, 0, Assets.Icon, new Vector2(x, 20), 1, WeaponLook.IconTint(g.D, g.WeaponRarity));
        x += 18;
        if (labels) x += Text(x, Loc.T("hud_cios_b"), g.WeaponBCd == 0 ? Ink.MapGood : Ink.MapDim) + 4;
        if (g.WeaponBCd > 0) x += Text(x, g.WeaponBCd.ToString(), Ink.MapDim) + 8;
        else x += Text(x, Loc.T("fach_znak"), Ink.MapGood) + 8;
        if (draw) WeaponRect = new Rect2(wx - 2, 16, x - wx - 4, 22); // v0.23.0: najechanie myszą – opis Patentu

        // materiały: ikona + liczba (tylko posiadane)
        for (var m = 0; m < g.D.Materials.Length; m++)
        {
            if (g.Mats[m] == 0) continue;
            if (draw) MaterialIcon.Draw(this, m, new Vector2(x, 22));
            x += MaterialIcon.Size + 2;
            x += Text(x, g.Mats[m].ToString(), Ink.Map) + 6;
        }
        return x;
    }

    /// <summary>v0.22.0 (#69): skrót poziomu trudności – pierwsza litera nazwy z danych (Łatwy → Ł, Normal → N, Hard → H).</summary>
    public static string DifficultyTag(string name) => string.IsNullOrEmpty(name) ? "" : char.ToUpperInvariant(name[0]).ToString();

    /// <summary>Pasek HP jak hp_bar.bmp (obrys, tło, połysk w 1. wierszu, cień w ostatnim), kolor wg progu.</summary>
    private void DrawHpBar(Rect2 r, int hp, int max)
    {
        var c = Pal.HpColor(hp, max);
        DrawRect(r.Grow(1), Pal.HpEdge);
        DrawRect(r, Pal.HpBack);
        var fill = max > 0 ? Mathf.Clamp(hp / (float)max, 0f, 1f) : 0f;
        var w = Mathf.Round(r.Size.X * fill);
        if (hp > 0 && w < 1) w = 1;
        if (w <= 0) return;
        DrawRect(new Rect2(r.Position, new Vector2(w, r.Size.Y)), Pal.HpMain[c]);
        DrawRect(new Rect2(r.Position, new Vector2(w, 1)), Pal.HpShine);
        DrawRect(new Rect2(r.Position + new Vector2(0, r.Size.Y - 1), new Vector2(w, 1)), Pal.HpShade[c]);
    }
}
