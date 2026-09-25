using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Screens.Play;

/// <summary>
/// Marsz po dotknięciu pola: krok po kroku (każdy krok to zwykła tura PlayerMove), z przerwą między krokami,
/// żeby było widać ruch. Staje, gdy pojawi się nowy problem budowy, gdy problem jest obok, gdy bohater oberwie,
/// gdy cel (dotknięty problem) jest w zasięgu broni, albo gdy ekran się zmieni (paczka, harmonogram).
/// </summary>
public sealed class AutoWalk
{
    public const float StepTime = 0.14f;

    private readonly App _app;
    private List<Vector2I> _path;
    private Vector2I _goal;
    private int _i, _target = -1, _hp;
    private uint _seen;
    private float _timer;

    public AutoWalk(App app) => _app = app;

    public bool Active => _path is not null;

    /// <summary>Marsz do pola albo (target >= 0) do problemu budowy, aż będzie w zasięgu. false = brak drogi.</summary>
    public bool Start(Vector2I goal, int target = -1)
    {
        var g = _app.Session.Game;
        _goal = goal;
        _target = target;
        _path = PathFinder.Find(g, goal, target >= 0);
        if (_path is null) return false;
        _i = 0;
        _timer = 0;
        _hp = g.Hero.Hp;
        _seen = VisibleMask(g);
        return true;
    }

    public void Stop() => _path = null;

    public void Process(double delta)
    {
        if (!Active) return;
        if (_app.Flow.Current != _app.Flow.Game) // paczka, harmonogram, telefon - koniec marszu
        {
            Stop();
            return;
        }
        _timer -= (float)delta;
        if (_timer > 0) return;
        _timer = StepTime;
        var g = _app.Session.Game;
        if (g.St != GameStatus.Playing || ShouldStop(g))
        {
            Stop();
            return;
        }
        var hero = new Vector2I(g.Hero.X, g.Hero.Y);
        var stale = _i >= _path.Count;
        if (!stale)
        {
            var d = (_path[_i] - hero).Abs();
            stale = d.X + d.Y != 1;
        }
        if (stale) // poślizg przesunął bohatera: nowa droga
        {
            _path = PathFinder.Find(g, _goal, _target >= 0);
            _i = 0;
            if (_path is null) return;
        }
        var next = _path[_i++];
        var acted = g.PlayerMove(next.X - hero.X, next.Y - hero.Y);
        _app.AfterAction(acted);
        if (!acted || _i >= _path.Count) Stop();
    }

    private bool ShouldStop(CoreGame g)
    {
        if (g.Hero.Hp < _hp) return true;
        var mask = VisibleMask(g);
        if ((mask & ~_seen) != 0) return true; // nowy problem w polu widzenia
        for (var i = 0; i < g.EnemiesCount; i++)
        {
            if ((mask & (1u << i)) == 0) continue;
            var e = g.Enemies[i];
            var dist = CoreGame.Cheb(g.Hero.X, g.Hero.Y, e.X, e.Y);
            if (dist <= 1) return true;
            if (i == _target && dist <= g.WeaponRange()) return true;
        }
        return false;
    }

    private static uint VisibleMask(CoreGame g)
    {
        uint m = 0;
        for (var i = 0; i < g.EnemiesCount && i < 32; i++)
        {
            var e = g.Enemies[i];
            if (e.Alive && g.Visible(e.X, e.Y)) m |= 1u << i;
        }
        return m;
    }
}
