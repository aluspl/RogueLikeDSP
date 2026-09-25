using Godot;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Screens.Play;

/// <summary>
/// Celowanie pod A jak na GBA: wciśnięcie wybiera najbliższy cel w zasięgu, po chwili trzymania widać ramki
/// zasięgu i celownik, strzałki przełączają cel, puszczenie A atakuje. Krótkie A = atak najbliższego;
/// bez celu w zasięgu zasięg tylko mignie.
/// </summary>
public sealed class Aiming
{
    /// <summary>Po tylu sekundach trzymania A pokazuje się zasięg i celownik (8 klatek na GBA).</summary>
    public const float RevealTime = 8 / 60f;

    private readonly App _app;
    private readonly sbyte[] _targets = new sbyte[CoreGame.MaxEnemies];
    private int _count, _sel;
    private float _time;
    private bool _revealed;

    public Aiming(App app) => _app = app;

    public bool Active { get; private set; }

    /// <summary>Wybrany cel (indeks wroga) albo -1.</summary>
    public int Target => Active && _count > 0 ? _targets[_sel] : -1;

    public int Count => _count;

    private SceneNodes N => _app.Nodes;

    public void Begin()
    {
        Active = true;
        _time = 0;
        _sel = 0;
        _revealed = false;
        _count = _app.Session.Game.TargetsInRange(_targets);
        N.World.Mark(Target);
    }

    public void Process(double delta)
    {
        if (!Active) return;
        _time += (float)delta;
        if (_revealed || _time < RevealTime) return;
        _revealed = true;
        N.World.ShowRange(true);
        Show();
    }

    /// <summary>Wejście w trakcie celowania (wszystko inne jest połykane, jak pętla aiming na GBA).</summary>
    public bool HandleInput(InputCmd e)
    {
        if (e.IsReleased(GameAction.A))
        {
            Commit();
            return true;
        }
        if (_count > 1 && e.Is(GameAction.Right | GameAction.Down, true)) Cycle(1);
        else if (_count > 1 && e.Is(GameAction.Left | GameAction.Up, true)) Cycle(-1);
        return true;
    }

    /// <summary>Następny / poprzedni cel (odsłania celownik od razu).</summary>
    public void Cycle(int d)
    {
        if (_count == 0) return;
        _sel = (_sel + d + _count) % _count;
        if (!_revealed)
        {
            _revealed = true;
            N.World.ShowRange(true);
        }
        Show();
    }

    /// <summary>Dotyk: palec nad polem - cel najbliższy temu polu (odsłania celownik).</summary>
    public void PointAt(Vector2I cell)
    {
        if (!Active || _count == 0) return;
        var g = _app.Session.Game;
        int best = _sel, bestD = int.MaxValue;
        for (var i = 0; i < _count; i++)
        {
            var e = g.Enemies[_targets[i]];
            var d = CoreGame.Cheb(e.X, e.Y, cell.X, cell.Y);
            if (d >= bestD) continue;
            bestD = d;
            best = i;
        }
        if (best == _sel && _revealed) return;
        _sel = best;
        if (!_revealed)
        {
            _revealed = true;
            N.World.ShowRange(true);
        }
        Show();
    }

    private void Show()
    {
        N.World.Mark(Target);
        N.World.Reticle = Target;
        var g = _app.Session.Game;
        if (_count == 0)
        {
            N.Hud.ShowHint($"Celowanie: brak celu w zasięgu (z{g.WeaponRange()})", ButtonNames.Pick("Puść Spację: wróć", "Puść Atak: wróć"));
            return;
        }
        var e = g.Enemies[Target];
        var name = g.D.Enemies[e.DefId].Name;
        N.Hud.ShowHint($"Cel {_sel + 1}/{_count}: {name}, HP {e.Hp}/{e.MaxHp}",
            ButtonNames.Pick("Strzałki: zmiana celu   Puść Spację: atak", "Przesuń palec: cel   Puść Atak: atak"));
    }

    /// <summary>Puszczenie A: atak wybranego celu (bez celu - miga zasięg jak range_flash na GBA).</summary>
    public void Commit()
    {
        var target = Target;
        Cancel();
        var g = _app.Session.Game;
        var acted = target >= 0 ? g.PlayerAttack(target) : PlayCommands.AttackNearest(g, N.World);
        _app.AfterAction(acted);
    }

    /// <summary>Koniec celowania bez ataku (np. wyjście z ekranu).</summary>
    public void Cancel()
    {
        if (!Active) return;
        Active = false;
        N.World.Reticle = -1;
        N.World.ShowRange(false);
        N.Hud.ShowHint(null);
    }
}
