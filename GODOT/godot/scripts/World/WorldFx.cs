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

    /// <summary>Gwiazdki awansu dookoła bohatera.</summary>
    public void LevelUp()
    {
        for (var k = 0; k < 8; k++)
        {
            var a = k * Mathf.Tau / 8;
            Fx.Spawn(Hero.Position, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 1.1f, 0, 30, Assets.PStar);
        }
        Hero.Flash(Pal.LevelFlash);
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
            default:
                flash = Colors.White;
                foreach (var d in Dir8) Fx.Spawn(h + d * 12, new Vector2(-d.Y, d.X) * 1.6f, 0, 16, Assets.PSpark, 2);
                break;
        }
        _w.Flash(flash, 0.38f);
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
