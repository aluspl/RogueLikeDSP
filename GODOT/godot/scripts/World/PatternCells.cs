using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// v0.23.0: pola wzoru trybu broni skierowanego na cel – do podświetlenia przy celowaniu (jak Game.Targets w rdzeniu:
/// promienie stają na pierwszej ścianie, strzał na pierwszym problemie, rzut 3x3 wokół celu, natrysk / łuk / krzyż nie
/// sięgają za ścianę przed bohaterem).
/// </summary>
public static class PatternCells
{
    public static void Fill(CoreGame g, WeaponMode m, int ei, HashSet<Vector2I> into)
    {
        into.Clear();
        if (m is null || ei < 0 || ei >= g.EnemiesCount || !g.Enemies[ei].Alive) return;
        int hx = g.Hero.X, hy = g.Hero.Y, tx = g.Enemies[ei].X, ty = g.Enemies[ei].Y;
        int dx = Math.Sign(tx - hx), dy = Math.Sign(ty - hy);
        var onLine = hx == tx || hy == ty || Math.Abs(tx - hx) == Math.Abs(ty - hy);
        var pat = m.Pattern;
        if (pat is PatternKind.Single or PatternKind.Push || (pat == PatternKind.Shot && !onLine))
        {
            into.Add(new Vector2I(tx, ty));
            return;
        }
        Span<int> xs = stackalloc int[HitPattern.MaxCells];
        Span<int> ys = stackalloc int[HitPattern.MaxCells];
        var n = pat == PatternKind.Throw3x3 ? HitPattern.Area3x3(tx, ty, xs, ys) : HitPattern.Cells(pat, g.ModeRange(m), hx, hy, dx, dy, xs, ys);
        var ray = HitPattern.IsRay(pat);
        var frontBlocked = !g.Lv.Passable(hx + dx, hy + dy);
        int prev = 0;
        var blocked = false;
        for (var i = 0; i < n; i++)
        {
            var d = CoreGame.Cheb(hx, hy, xs[i], ys[i]);
            if (d <= prev) blocked = false;
            prev = d;
            if (ray && blocked) continue;
            if (!g.Lv.Passable(xs[i], ys[i]))
            {
                if (ray) blocked = true;
                continue;
            }
            if (!ray && d >= 2 && pat is PatternKind.Spray or PatternKind.Arc or PatternKind.Cross && frontBlocked) continue;
            into.Add(new Vector2I(xs[i], ys[i]));
            if (pat == PatternKind.Shot && g.EnemyAt(xs[i], ys[i]) >= 0) blocked = true; // strzał: pierwszy cel w linii
        }
    }
}
