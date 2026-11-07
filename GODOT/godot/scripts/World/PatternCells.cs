using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// v0.23.0: pola wzoru trybu broni skierowanego na cel – do podświetlenia przy celowaniu. Liczy je rdzeń
/// (Game.PatternCells – to samo, czego używa atak), więc podświetlenie zawsze zgadza się z ciosem.
/// </summary>
public static class PatternCells
{
    public static void Fill(CoreGame g, WeaponMode m, int ei, HashSet<Vector2I> into)
    {
        into.Clear();
        if (m is null) return;
        Span<int> xs = stackalloc int[HitPattern.MaxCells];
        Span<int> ys = stackalloc int[HitPattern.MaxCells];
        var n = g.PatternCells(m, ei, xs, ys);
        for (var i = 0; i < n; i++) into.Add(new Vector2I(xs[i], ys[i]));
    }
}
