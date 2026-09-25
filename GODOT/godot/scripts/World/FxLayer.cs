using System.Collections.Generic;
using Godot;

namespace LifeLike.Game;

/// <summary>
/// Cząsteczki i unoszące się liczby nad mapą (particle_pool i floatery z GBA/src/main.cpp). Symulacja w stałym
/// kroku 60 Hz jak na GBA, prędkości w pikselach świata (2x względem GBA).
/// </summary>
public partial class FxLayer : Node2D
{
    private const int MaxParticles = 160;
    private readonly List<Particle> _particles = new();
    private readonly List<Floater> _floaters = new();
    private readonly RandomNumberGenerator _rnd = new();
    private double _acc;

    public int ParticleCount => _particles.Count;

    public float Rand(float lo, float hi) => _rnd.RandfRange(lo, hi);

    public int RandInt(int n) => _rnd.RandiRange(0, n - 1);

    public void Spawn(Vector2 pos, Vector2 vel, float gravity, int life, int frame, int frames = 1)
    {
        if (_particles.Count >= MaxParticles) _particles.RemoveAt(0);
        _particles.Add(new Particle { Pos = pos, Vel = vel, Gravity = gravity, Life = life, Frame = frame, Frames = frames });
    }

    /// <summary>Rozprysk: count cząsteczek w losowych kierunkach (burst na GBA; speed w px/klatkę).</summary>
    public void Burst(Vector2 pos, int count, int frame, int frames, float speed, int life)
    {
        for (var i = 0; i < count; i++)
            Spawn(pos, new Vector2(Rand(-speed, speed), Rand(-speed, speed / 2)), 0.16f, life, frame, frames);
    }

    public void AddFloater(Floater f) => _floaters.Add(f);

    public void Clear()
    {
        _particles.Clear();
        _floaters.Clear();
    }

    public override void _Process(double delta)
    {
        _acc += delta;
        var steps = 0;
        while (_acc >= 1.0 / 60 && steps < 4)
        {
            _acc -= 1.0 / 60;
            steps++;
            for (var i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                if (++p.Age >= p.Life)
                {
                    _particles.RemoveAt(i);
                    continue;
                }
                p.Vel.Y += p.Gravity;
                p.Pos += p.Vel;
            }
        }
        if (_acc > 0.2) _acc = 0;
        for (var i = _floaters.Count - 1; i >= 0; i--)
        {
            _floaters[i].Age += (float)delta;
            if (_floaters[i].Age >= _floaters[i].Life) _floaters.RemoveAt(i);
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        const int s = Assets.Particle;
        var tex = Assets.Particles;
        foreach (var p in _particles)
        {
            var f = p.Frame + Mathf.Min(p.Frames - 1, p.Age * p.Frames / p.Life);
            var fade = p.Life - p.Age < 6 ? (p.Life - p.Age) / 6f : 1f;
            DrawTextureRectRegion(tex, new Rect2(Mathf.Round(p.Pos.X - s / 2), Mathf.Round(p.Pos.Y - s / 2), s, s), Assets.Frame(f, s), new Color(1, 1, 1, fade));
        }
        var font = PixelFont.I;
        foreach (var f in _floaters)
        {
            var t = f.Age / f.Life;
            var rise = Mathf.Round(Mathf.Min(t * 1.6f, 1f) * 20f);
            var alpha = t > 0.7f ? 1f - (t - 0.7f) / 0.3f : 1f;
            var pop = t < 0.08f && f.Scale > 1 ? 1 : 0;
            font.Draw(this, f.Pos + new Vector2(0, -rise - 8 * f.Scale - pop), f.Text, f.Ink.WithAlpha(alpha), TextAlign.Center, f.Scale);
        }
    }
}
