using Godot;
using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Górny pasek HUD jak na GBA (półprzezroczysty ciemny pas): HP z paskiem, poziom z paskiem doświadczenia, etap;
/// w drugim rzędzie stany z liczbą tur, wydarzenie na placu, ostrzeżenie o ciosie bossa, termos i ikona mocy
/// (szara z odliczaniem, gdy się ładuje; „R” i podskakiwanie, gdy gotowa).
/// </summary>
public partial class HudTop : Control
{
    private CoreGame _g;
    private float _clock;

    public const int Height = 38;

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
        DrawRect(new Rect2(0, 0, w, Height), new Color(Pal.Text, 0.62f));
        DrawRect(new Rect2(0, Height, w, 1), new Color(Pal.Brand, 0.55f));

        // rząd 1: HP, pasek, liczby
        var low = g.Hero.Hp * 4 <= g.Hero.MaxHp;
        f.Draw(this, new Vector2(6, 2), "HP", Ink.Map);
        var blinkOff = low && ((int)(_clock * 4) & 1) == 1;
        if (!blinkOff) DrawHpBar(new Rect2(26, 6, 104, 8), g.Hero.Hp, g.Hero.MaxHp);
        f.Draw(this, new Vector2(136, 2), $"{g.Hero.Hp}/{g.Hero.MaxHp}", low ? Ink.MapBad : Ink.Map);

        // poziom i doświadczenie (Start w telefonie ma szczegóły)
        var lvX = 196;
        f.Draw(this, new Vector2(lvX, 2), $"Poz. {g.HeroLevel}", Ink.MapLoot);
        var prev = g.HeroLevel >= 2 ? g.D.LevelThresholds[g.HeroLevel - 2] : 0;
        var fill = g.XpToNext() < 0 ? 1f : (g.RunXp - prev) / (float)Mathf.Max(1, g.D.LevelThresholds[g.HeroLevel - 1] - prev);
        var xr = new Rect2(lvX + 44, 8, 56, 4);
        DrawRect(xr.Grow(1), Pal.HpEdge);
        DrawRect(xr, Pal.HpBack);
        DrawRect(new Rect2(xr.Position, new Vector2(Mathf.Round(xr.Size.X * Mathf.Clamp(fill, 0, 1)), 4)), Pal.Brand);

        // etap i poziom trudności (prawa strona, jak „Etap 1/8 N” na GBA)
        var sd = g.D.Stages[g.Stage];
        var ng = g.Tier > 0 ? $" +{g.Tier}" : "";
        var right = $"Etap {g.Stage + 1}/{g.D.Stages.Length}: {sd.Name}";
        f.Draw(this, new Vector2(w - 6, 2), f.Fit(right, 250), Ink.Map, TextAlign.Right);
        f.Draw(this, new Vector2(w - 6, 19), $"{g.DDef.Name}{ng}", Ink.MapDim, TextAlign.Right);

        // rząd 2: stany (ikona + tury)
        var x = 6f;
        for (var k = 0; k < 3; k++)
        {
            var t = g.StatusTurns(UiText.HudStatuses[k]);
            if (t <= 0) continue;
            Assets.DrawFrame(this, Assets.Particles, Assets.PStatus + k, Assets.Particle, new Vector2(x, 20));
            x += 16;
            x += f.Draw(this, new Vector2(x, 19), t.ToString(), Ink.MapBad) + 6;
        }
        if (x > 6) x += 4;

        // termos
        Assets.DrawFrame(this, Assets.UiMenu, 1, Assets.Icon, new Vector2(x, 20));
        x += 17;
        x += f.Draw(this, new Vector2(x, 19), $"{g.Thermos}/{g.ThermosCap()}", g.Thermos > 0 ? Ink.Map : Ink.MapDim) + 8;

        // moc zawodu: ikona szara z odliczaniem albo pulsująca z „R”
        var ready = g.AbilityCd == 0;
        var bob = ready && ((int)(_clock * 4) & 1) == 1 ? -1 : 0;
        Assets.DrawFrame(this, ready ? Assets.UiAbility : Assets.UiAbilityGray, g.Cls, Assets.Icon, new Vector2(x, 20 + bob));
        x += 18;
        if (!ready) x += f.Draw(this, new Vector2(x, 19), g.AbilityCd.ToString(), Ink.MapDim) + 8;
        else if (((int)(_clock * 2) & 1) == 0) x += f.Draw(this, new Vector2(x, 19), "R", Ink.MapGood) + 8;
        else x += f.Measure("R") + 8;

        // wydarzenie na placu / zapowiedź ciosu bossa
        if (g.SlamTimer > 0)
        {
            var pulse = ((int)(_clock * 6) & 1) == 1;
            f.Draw(this, new Vector2(x + 4, 19), $"UWAGA: cios za {g.SlamTimer}!", pulse ? Ink.MapBad : Ink.MapLoot);
        }
        else if (g.CurrentEvent is { } ev)
        {
            var pill = f.Measure(ev.Short) + 10;
            var r = new Rect2(x + 4, 21, pill, 13);
            DrawRect(r, ev.Good ? new Color(Pal.Done, 0.85f) : new Color(Pal.Late, 0.85f));
            f.Draw(this, new Vector2(r.Position.X + pill / 2, 19), ev.Short, Ink.White, TextAlign.Center);
        }
    }

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
