using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Efekty na mapie (refresh() i ability_fx w GBA/src/main.cpp): liczby trafień, iskry, błyski, szturchnięcia,
/// wstrząs i dźwięk trafień; gwiazdki awansu, konfetti odbioru i efekty mocy zawodów z błyskiem ekranu.
/// </summary>
public sealed class WorldFx
{
    private static readonly Vector2[] Dir8 = [new(2, 0), new(1, 1), new(0, 2), new(-1, 1), new(-2, 0), new(-1, -1), new(0, -2), new(1, -1)];
    private readonly WorldView _w;

    public WorldFx(WorldView w) => _w = w;

    private CoreGame G => _w.Game;
    private FxLayer Fx => _w.Fx;
    private ActorSprite Hero => _w.HeroSprite;

    /// <summary>Trafienia z rdzenia: liczby, iskry, błyski, szturchnięcia, wstrząs i dźwięk (hit / hurt).</summary>
    public void TakeHits()
    {
        var g = G;
        var heroHitEnemy = false;
        for (var i = 0; i < g.HitsCount; i++)
        {
            var h = g.Hits[i];
            var pos = _w.GridToScreen(h.X, h.Y);
            Fx.AddFloater(HitFloater(h, pos + new Vector2(0, -26 - 15 * (i % 3))));
            if (h.Kind != HitKind.Dodge) Fx.Burst(pos, h.OnHero ? 3 : 5, Assets.PSpark, 2, 1.5f, 16);
            if (!h.OnHero && h.Kind != HitKind.Dodge)
            {
                var ei = g.EnemyAt(h.X, h.Y);
                var target = ei >= 0 ? _w.EnemySprite(ei) : _w.FindDying(pos);
                target?.Flash(Colors.White);
                if (!heroHitEnemy)
                {
                    heroHitEnemy = true;
                    if (CoreGame.Cheb(g.Hero.X, g.Hero.Y, h.X, h.Y) <= 1) Hero.Bump(pos - Hero.Position);
                    else Hero.Bump(new Vector2(Mathf.Sign(pos.X - Hero.Position.X), Mathf.Sign(pos.Y - Hero.Position.Y)) * 0.6f);
                }
            }
            if (i == 0 || h.OnHero != g.Hits[i - 1].OnHero) Sfx.Play(h.OnHero && h.Kind != HitKind.Dodge ? "hurt" : "hit");
        }
        if (g.HeroHit)
        {
            Hero.Flash(Pal.HurtFlash);
            Hero.Blink(0.27f);
            _w.Camera.Shake(0.14f);
            _w.Flash(Pal.HurtTint, 0.3f);
            for (var i = 0; i < g.EnemiesCount; i++) // problemy obok bohatera „uderzają”
            {
                var e = g.Enemies[i];
                if (e.Alive && e.Awake && CoreGame.Cheb(e.X, e.Y, g.Hero.X, g.Hero.Y) <= 1)
                    _w.EnemySprite(i).Bump(Hero.Position - _w.EnemySprite(i).Position);
            }
        }
        g.HitsCount = 0;
        g.TurnEvents = 0;
        g.HeroHit = false;
        Combos();
    }

    /// <summary>
    /// Kombinacje stanów z rdzenia (Game.ComboEvents, v0.21.50 cz. 2): napis „Mokry + prąd!” / „Pył + iskra!” /
    /// „Zamróz + uderzenie!” nad celem (albo bohaterem, gdy dotyczy jego), błyskawice, pył albo odłamki i błysk ekranu.
    /// </summary>
    private void Combos()
    {
        var g = G;
        if (g.ComboEvents == 0) return;
        var t = g.LastTarget >= 0 && g.LastTarget < g.EnemiesCount ? g.LastTarget : -1;
        var at = t >= 0 ? _w.GridToScreen(g.Enemies[t].X, g.Enemies[t].Y) : Hero.Position;
        for (var c = 0; c < g.D.Combos.Length && c < 3; c++)
        {
            var onEnemy = (g.ComboEvents & (1 << c)) != 0;
            var onHero = (g.ComboEvents & (8 << c)) != 0;
            if (!onEnemy && !onHero) continue;
            var pos = onEnemy ? at : Hero.Position;
            var cd = g.D.Combos[c];
            var ink = (ComboEffect)c switch
            {
                ComboEffect.ShockArea => Ink.MapWet,
                ComboEffect.DustBlast => Ink.MapBrand,
                _ => Ink.Map,
            };
            // napis nad bohaterem, wyżej niż liczby trafień (nie nachodzi na „-3”)
            Fx.AddFloater(new Floater { Pos = Hero.Position + new Vector2(0, -60 - 16 * c), Text = cd.Short, Ink = onHero ? Ink.MapBad : ink, Life = 1.5f });
            switch ((ComboEffect)c)
            {
                case ComboEffect.ShockArea:
                    Fx.Burst(pos, 10, Assets.PBolt, 1, 2.2f, 20);
                    _w.Flash(Pal.FlashChain, 0.3f);
                    break;
                case ComboEffect.DustBlast:
                    Fx.Burst(pos, 14, Assets.PDust, 3, 2.6f, 26);
                    Fx.Burst(pos, 8, Assets.PSpark, 2, 2.2f, 18);
                    _w.Flash(new Color(1f, 0.6f, 0.2f), 0.35f);
                    _w.Camera.Shake(0.2f);
                    break;
                default:
                    Fx.Burst(pos, 8, Assets.PBrick, 1, 2f, 22);
                    _w.Flash(Pal.FrostCyan, 0.25f);
                    break;
            }
        }
        g.ComboEvents = 0;
    }

    private Floater HitFloater(Hit h, Vector2 pos)
    {
        var f = new Floater { Pos = pos };
        switch (h.Kind)
        {
            case HitKind.Dodge:
                f.Text = "Unik!";
                f.Ink = Ink.MapGood;
                break;
            case HitKind.Crit:
                f.Text = $"KRYT! -{h.Amount}";
                f.Ink = Ink.MapLoot;
                f.Life = 1f;
                _w.Flash(Pal.CritTint, 0.3f);
                break;
            default:
                f.Text = $"-{h.Amount}";
                f.Ink = h.OnHero ? Ink.MapBad : Ink.Map;
                break;
        }
        return f;
    }

    /// <summary>
    /// Awans (wyraźny): złota poświata wokół bohatera, krąg i unoszące się gwiazdki, duży napis „AWANS! Poziom N”
    /// nad głową na ~1,5 s i pod nim, co się poprawiło (HP, obrona / obrażenia, ranga mocy).
    /// </summary>
    public void LevelUp(int level, bool abilityUp)
    {
        var g = G;
        var h = _w.GridToScreen(g.Hero.X, g.Hero.Y); // pole bohatera (sprite może jeszcze dochodzić)
        Fx.AddGlow(new Glow { Pos = h, Color = Pal.LevelGlow, Life = 1.4f, Radius = 64 });
        for (var k = 0; k < 12; k++)
        {
            var a = k * Mathf.Tau / 12;
            Fx.Spawn(h, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 1.4f, 0, 34, Assets.PStar);
        }
        for (var k = 0; k < 10; k++) // gwiazdki unoszą się z ziemi wokół bohatera
            Fx.Spawn(h + new Vector2(Fx.Rand(-22, 22), Fx.Rand(0, 14)), new Vector2(Fx.Rand(-0.2f, 0.2f), Fx.Rand(-1.6f, -0.8f)), 0, 60 + Fx.RandInt(30), Assets.PStar);
        Fx.AddFloater(new Floater { Pos = h + new Vector2(0, -40), Text = $"AWANS! Poziom {level}", Ink = Ink.MapLoot, Life = 1.6f, Scale = 2 });
        Fx.AddFloater(new Floater { Pos = h + new Vector2(0, -12), Text = LevelGains(g, level, abilityUp), Ink = Ink.MapGood, Life = 1.6f });
        Hero.Flash(Pal.LevelFlash);
        _w.Flash(Pal.LevelGlow, 0.3f);
    }

    /// <summary>Co dał awans: „+2 max HP, +1 obrona, ranga mocy II”.</summary>
    public static string LevelGains(CoreGame g, int level, bool abilityUp)
    {
        var s = $"+{g.D.HpPerLevel} max HP";
        if ((g.D.DefLevelsMask & (1 << level)) != 0) s += ", +1 obrona";
        if ((g.D.DmgLevelsMask & (1 << level)) != 0) s += ", +1 obrażenia";
        if (abilityUp) s += $", ranga mocy {UiText.Roman(g.AbilityRank() - 1)}";
        return s;
    }

    /// <summary>Konfetti na odbiór budowy (wygrana).</summary>
    public void Confetti()
    {
        var c = _w.Camera.GetScreenCenterPosition();
        for (var k = 0; k < 40; k++)
            Fx.Spawn(c + new Vector2(Fx.Rand(-300, 300), -190 - Fx.Rand(0, 60)), new Vector2(Fx.Rand(-0.6f, 0.6f), Fx.Rand(1f, 2.4f)),
                     0.02f, 150, Assets.PConfetti + Fx.RandInt(4));
    }

    /// <summary>Efekty mocy zawodów + błysk ekranu w kolorze mocy (ability_fx w main.cpp).</summary>
    public void Ability()
    {
        var g = G;
        var h = Hero.Position;
        Color flash;
        switch (g.CDef.Ability)
        {
            case AbilityEffect.Stun:
                flash = Pal.FlashStun;
                foreach (var d in Dir8) Fx.Spawn(h, d * 1.8f, 0, 22, Assets.PRing, 2);
                for (var i = 0; i < g.EnemiesCount; i++)
                {
                    var e = g.Enemies[i];
                    if (e.Alive && e.Stun > 0 && g.Visible(e.X, e.Y))
                        Fx.Spawn(_w.GridToScreen(e.X, e.Y) + new Vector2(8, -16), new Vector2(0.4f, -0.8f), 0, 40, Assets.PZzz);
                }
                break;
            case AbilityEffect.Wall:
                flash = Pal.FlashWall;
                for (var i = 0; i < g.WallsCount; i++)
                {
                    var w = _w.GridToScreen(g.Walls[i].X, g.Walls[i].Y);
                    Fx.Spawn(w + new Vector2(0, 8), new Vector2(Fx.Rand(-1, 1), -3.2f), 0.3f, 20, Assets.PBrick);
                    Fx.Spawn(w + new Vector2(-8, 12), new Vector2(-0.6f, -0.4f), 0, 16, Assets.PDust, 3);
                    Fx.Spawn(w + new Vector2(8, 12), new Vector2(0.6f, -0.4f), 0, 16, Assets.PDust, 3);
                }
                break;
            case AbilityEffect.Volley:
                flash = Pal.FlashVolley;
                for (var i = 0; i < g.HitsCount; i++)
                    Fx.Spawn(h, (_w.GridToScreen(g.Hits[i].X, g.Hits[i].Y) - h) / 10f, 0, 10, Assets.PNail);
                break;
            case AbilityEffect.Chain:
                flash = Pal.FlashChain;
                ChainBolts(h);
                break;
            case AbilityEffect.Flush:
                flash = Pal.FlashFlush;
                for (var k = 0; k < 10; k++)
                    Fx.Spawn(h + new Vector2(Fx.Rand(-150, 150), 8), new Vector2(0, Fx.Rand(-3f, -1f)), 0, 26, (k & 1) == 1 ? Assets.PPlus : Assets.PDrop);
                break;
            case AbilityEffect.Line:   // Rynna: dachówki lecą linią do trafionych
                flash = new Color(1f, 0.45f, 0.25f);
                for (var i = 0; i < g.HitsCount; i++)
                {
                    if (g.Hits[i].OnHero) continue;
                    var t = _w.GridToScreen(g.Hits[i].X, g.Hits[i].Y);
                    for (var k = 0; k < 2; k++) Fx.Spawn(h - new Vector2(0, 8 * k), (t - h) / 12f, 0, 12, Assets.PBrick);
                }
                break;
            case AbilityEffect.Splash:   // Narzut: tynk chlapie na obszar wokół celu
                flash = new Color(0.95f, 0.95f, 0.9f);
                for (var i = 0; i < g.HitsCount; i++)
                {
                    if (g.Hits[i].OnHero) continue;
                    Fx.Burst(_w.GridToScreen(g.Hits[i].X, g.Hits[i].Y), 6, Assets.PDust, 3, 1.3f, 18);
                }
                break;
            case AbilityEffect.Ram:   // Taran: kurz spod gąsienic i gwiazdki przy uderzeniu
            {
                flash = new Color(1f, 0.78f, 0.15f);
                var at = _w.GridToScreen(g.Hero.X, g.Hero.Y);
                for (var k = 0; k < 8; k++) Fx.Spawn(at + new Vector2(Fx.Rand(-12, 12), 12), new Vector2(Fx.Rand(-1, 1), -0.6f), 0, 20, Assets.PDust, 3);
                for (var i = 0; i < g.HitsCount; i++)
                {
                    if (g.Hits[i].OnHero) continue;
                    var t = _w.GridToScreen(g.Hits[i].X, g.Hits[i].Y);
                    for (var k = 0; k < 3; k++) Fx.Spawn(t - new Vector2(0, 16), new Vector2(Fx.Rand(-1, 1), -1.2f), 0.1f, 24, Assets.PStar);
                }
                break;
            }
            default:
                flash = Colors.White;
                foreach (var d in Dir8) Fx.Spawn(h + d * 12, new Vector2(-d.Y, d.X) * 1.6f, 0, 16, Assets.PSpark, 2);
                break;
        }
        _w.Flash(flash, 0.38f);
    }

    /// <summary>Druga szansa (Respekt): złoty błysk i gwiazdki wokół bohatera.</summary>
    public void SecondChance()
    {
        foreach (var d in Dir8) Fx.Spawn(Hero.Position, d * 2f, 0, 26, Assets.PStar);
        Hero.Flash(Pal.LevelFlash);
        _w.Flash(Pal.LevelGlow, 0.4f);
    }

    /// <summary>Druga faza bossa (Odwołanie): czerwony błysk, wstrząs i kartki wylatujące z bossa.</summary>
    public void BossPhase()
    {
        var g = G;
        if (g.Boss < 0) return;
        var b = _w.GridToScreen(g.Enemies[g.Boss].X, g.Enemies[g.Boss].Y);
        foreach (var d in Dir8) Fx.Spawn(b, d * 2.2f, 0.05f, 28, Assets.PRing, 2);
        Fx.Burst(b, 10, Assets.PSpark, 2, 2.4f, 24);
        _w.EnemySprite(g.Boss)?.Flash(Pal.HurtTint);
        _w.Camera.Shake(0.3f);
        _w.Flash(new Color(1f, 0.25f, 0.2f), 0.45f);
    }

    /// <summary>Brygada: efekt wezwania fachowca (brigade_fx w main.cpp) i błysk.</summary>
    public void Brigade(HelperEffect effect)
    {
        var g = G;
        var h = Hero.Position;
        switch (effect)
        {
            case HelperEffect.Reveal:
                foreach (var d in Dir8) Fx.Spawn(h, d * 2.4f, 0, 26, Assets.PRing, 2);
                _w.Flash(new Color(0.55f, 0.8f, 1f), 0.3f);
                break;
            case HelperEffect.Pump: // beton rozlewa się wokół bohatera
                foreach (var d in Dir8) Fx.Spawn(h + new Vector2(0, 8), d * 1.6f, 0, 26, Assets.PDust, 3);
                for (var i = 0; i < g.HitsCount; i++) Fx.Burst(_w.GridToScreen(g.Hits[i].X, g.Hits[i].Y), 6, Assets.PDust, 3, 1.4f, 20);
                _w.Flash(new Color(0.8f, 0.8f, 0.75f), 0.35f);
                break;
            case HelperEffect.Safety:
                for (var k = 0; k < 8; k++) Fx.Spawn(h + new Vector2(Fx.Rand(-40, 40), 8), new Vector2(0, Fx.Rand(-2.4f, -1f)), 0, 26, Assets.PPlus);
                _w.Flash(Pal.FlashFlush, 0.3f);
                break;
            default:
                if (g.AllyTurns > 0)
                    Fx.Burst(_w.GridToScreen(g.AllyX, g.AllyY) + new Vector2(0, 10), 8, Assets.PDust, 3, 1.2f, 18);
                _w.Flash(Pal.FlashChain, 0.3f);
                break;
        }
    }

    /// <summary>Naprawa za materiał: deski wyskakują z ziemi (Załataj) albo krąg kropel wokół (Kładka).</summary>
    public void Repair(RepairEffect effect)
    {
        var g = G;
        if (effect == RepairEffect.Patch)
        {
            for (var i = 0; i < g.WallsCount; i++)
            {
                var w = _w.GridToScreen(g.Walls[i].X, g.Walls[i].Y);
                Fx.Spawn(w + new Vector2(0, 8), new Vector2(Fx.Rand(-1, 1), -3f), 0.3f, 20, Assets.PBrick);
                Fx.Spawn(w + new Vector2(0, 12), new Vector2(Fx.Rand(-0.6f, 0.6f), -0.4f), 0, 16, Assets.PDust, 3);
            }
            _w.Flash(Pal.FlashWall, 0.25f);
            return;
        }
        foreach (var d in Dir8) Fx.Spawn(Hero.Position, d * 2f, 0, 24, Assets.PDrop);
        _w.Flash(new Color(0.6f, 0.75f, 0.95f), 0.25f);
    }

    private void ChainBolts(Vector2 from)
    {
        var g = G;
        for (var i = 0; i < g.HitsCount; i++)
        {
            if (g.Hits[i].OnHero) continue;
            var to = _w.GridToScreen(g.Hits[i].X, g.Hits[i].Y);
            for (var k = 1; k <= 3; k++) Fx.Spawn(from + (to - from) * k / 4f, Vector2.Zero, 0, 12, Assets.PBolt, 2);
            from = to;
        }
    }
}
