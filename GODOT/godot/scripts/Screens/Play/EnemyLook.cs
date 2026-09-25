using System.Collections.Generic;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Screens.Play;

/// <summary>
/// B jak na GBA: krótko = czekaj turę; przytrzymane = podgląd problemów w polu widzenia bez zużycia tury -
/// karta wroga (nazwa, HP, obrażenia, opis) w miejscu dziennika, celownik nad nim, strzałki przełączają.
/// </summary>
public sealed class EnemyLook
{
    /// <summary>Po tylu sekundach trzymania B zamiast czekania jest podgląd (10 klatek na GBA).</summary>
    public const float HoldTime = 10 / 60f;

    private readonly App _app;
    private readonly List<int> _list = new();
    private int _sel;
    private float _time;
    private bool _listed;

    public EnemyLook(App app) => _app = app;

    public bool Active { get; private set; }

    /// <summary>Oglądany wróg albo -1.</summary>
    public int Target => Active && _listed && _list.Count > 0 ? _list[_sel] : -1;

    public int Count => _list.Count;

    private SceneNodes N => _app.Nodes;

    public void Begin()
    {
        Active = true;
        _time = 0;
        _sel = 0;
        _listed = false;
    }

    public void Process(double delta)
    {
        if (!Active || _listed) return;
        _time += (float)delta;
        if (_time >= HoldTime) Reveal();
    }

    /// <summary>Lista widocznych problemów od najbliższego (look_list na GBA) i pierwsza karta.</summary>
    public void Reveal()
    {
        var g = _app.Session.Game;
        _listed = true;
        _list.Clear();
        for (var d = 1; d <= g.SightRadius() + 1; d++)
        {
            for (var i = 0; i < g.EnemiesCount; i++)
            {
                var e = g.Enemies[i];
                if (e.Alive && g.Visible(e.X, e.Y) && CoreGame.Cheb(g.Hero.X, g.Hero.Y, e.X, e.Y) == d) _list.Add(i);
            }
        }
        Show();
    }

    public bool HandleInput(InputCmd e)
    {
        if (e.IsReleased(GameAction.B))
        {
            End();
            return true;
        }
        if (!_listed || _list.Count == 0) return true;
        if (e.Is(GameAction.Right | GameAction.Down, true)) Cycle(1);
        else if (e.Is(GameAction.Left | GameAction.Up, true)) Cycle(-1);
        return true;
    }

    public void Cycle(int d)
    {
        if (_list.Count == 0) return;
        _sel = (_sel + d + _list.Count) % _list.Count;
        Show();
    }

    private void Show()
    {
        N.World.Reticle = Target;
        N.Hud.ShowEnemyCard(_app.Session.Game, Target, _sel, _list.Count);
    }

    /// <summary>Puszczenie B: krótko - czekaj turę; po podglądzie - powrót bez zużycia tury.</summary>
    public void End()
    {
        var wait = Active && !_listed;
        Cancel();
        if (wait) _app.AfterAction(_app.Session.Game.PlayerWait());
    }

    public void Cancel()
    {
        if (!Active) return;
        Active = false;
        N.World.Reticle = -1;
        if (!_listed) return;
        N.Hud.HideEnemyCard();
        N.Hud.SilenceLog();
    }
}
