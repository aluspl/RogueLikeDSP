using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Screens.Play;

/// <summary>
/// Droga po planszy dla dotknięcia pola (BFS w 4 kierunkach jak ruch na GBA) - tylko po odkrytych, przejezdnych
/// polach, omijając widoczne problemy budowy. Bez wpływu na logikę gry: kroki robi zwykłe PlayerMove.
/// </summary>
public static class PathFinder
{
    private static readonly Vector2I[] Dirs = [new(0, -1), new(1, 0), new(0, 1), new(-1, 0)];

    /// <summary>
    /// Kroki od bohatera do celu (bez pola startowego); adjacent = wystarczy stanąć obok (cel to problem budowy).
    /// null = nie ma drogi po znanym terenie.
    /// </summary>
    public static List<Vector2I> Find(CoreGame g, Vector2I goal, bool adjacent)
    {
        var start = new Vector2I(g.Hero.X, g.Hero.Y);
        if (!Level.In(goal.X, goal.Y) || start == goal) return null;
        var prev = new int[Level.W * Level.H];
        System.Array.Fill(prev, -1);
        var q = new Queue<Vector2I>();
        q.Enqueue(start);
        prev[Idx(start)] = Idx(start);
        var end = new Vector2I(-1, -1);
        while (q.Count > 0)
        {
            var c = q.Dequeue();
            if (c == goal || (adjacent && Mathf.Abs(c.X - goal.X) + Mathf.Abs(c.Y - goal.Y) == 1))
            {
                end = c;
                break;
            }
            foreach (var d in Dirs)
            {
                var n = c + d;
                if (!Level.In(n.X, n.Y) || prev[Idx(n)] >= 0 || !Walkable(g, n, goal)) continue;
                prev[Idx(n)] = Idx(c);
                q.Enqueue(n);
            }
        }
        if (end.X < 0) return null;
        var path = new List<Vector2I>();
        for (var c = end; c != start; c = At(prev[Idx(c)])) path.Add(c);
        path.Reverse();
        return path.Count > 0 ? path : null;
    }

    private static bool Walkable(CoreGame g, Vector2I c, Vector2I goal)
    {
        if (!g.Explored(c.X, c.Y) || !g.Lv.Passable(c.X, c.Y)) return false;
        var e = g.EnemyAt(c.X, c.Y);
        return e < 0 || !g.Visible(c.X, c.Y) || c == goal;
    }

    private static int Idx(Vector2I c) => c.Y * Level.W + c.X;

    private static Vector2I At(int i) => new(i % Level.W, i / Level.W);
}
