using System;
using System.Collections.Generic;
using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Debug;

/// <summary>Ręczne ustawienia stanu rdzenia dla scen pokazowych: walka z liczbami, boss z ciosem, wrogowie w polu widzenia.</summary>
public sealed class DemoStaging
{
    private readonly App _app;

    public DemoStaging(App app) => _app = app;

    private CoreGame G => _app.Session.Game;

    /// <summary>Pokaz liczb: trafienie krytyczne bohatera i unik przed ciosem problemu.</summary>
    public void Combat()
    {
        var g = G;
        g.EnemiesCount = 0;
        var (ex, ey) = FreeNeighbour();
        g.Spawn(0, ex, ey);
        g.Enemies[0].Hp = g.Enemies[0].MaxHp = 500;
        g.Enemies[0].Awake = true;
        var kept = new List<Hit>();
        for (var k = 0; k < 300 && kept.Count == 0; k++)
        {
            g.HitsCount = 0;
            g.Hero.Hp = g.Hero.MaxHp;
            g.PlayerAttack(0);
            for (var h = 0; h < g.HitsCount; h++)
            {
                if (g.Hits[h].Kind == HitKind.Crit) kept.Add(g.Hits[h]);
            }
        }
        for (var k = 0; k < 300; k++)
        {
            g.HitsCount = 0;
            g.Hero.Hp = g.Hero.MaxHp;
            g.PlayerWait();
            if (g.HitsCount > 0 && g.Hits[0].Kind == HitKind.Dodge) break;
        }
        foreach (var h in kept) g.AddHit(h.X, h.Y, h.Amount, h.OnHero, h.Kind);
        g.Enemies[0].Hp = 180;
        _app.AfterAction(true);
    }

    /// <summary>Boss w polu widzenia z zapowiedzianym ciosem (czerwone pola) i banerem „Przypisano Ci usterkę”.</summary>
    public void Boss()
    {
        var g = G;
        var b = g.Enemies[g.Boss];
        foreach (var (dx, dy) in new[] { (2, 0), (-2, 0), (0, 2), (0, -2), (2, 1), (-2, 1), (1, 2), (1, -2) })
        {
            int x = b.X + dx, y = b.Y + dy;
            if (g.Lv.At(x, y) != Tile.Floor || g.Occupied(x, y)) continue;
            g.Hero.X = (sbyte)x;
            g.Hero.Y = (sbyte)y;
            break;
        }
        g.Enemies[g.Boss].Awake = true;
        g.Enemies[g.Boss].Hp = (short)(g.Enemies[g.Boss].MaxHp * 2 / 3);
        g.SlamTimer = 2;
        g.SlamX = (sbyte)g.Hero.X;
        g.SlamY = (sbyte)g.Hero.Y;
        g.UpdateFov();
        _app.Nodes.World.Sync();
        _app.Refresh();
    }

    /// <summary>Pokaz: dwa problemy budowy podchodzą na 2-3 pola od bohatera (widoczne, jeden ranny i czujny).</summary>
    public void BringEnemies()
    {
        var g = G;
        var placed = 0;
        foreach (var (dx, dy) in new[] { (2, 1), (-2, 1), (2, -1), (-2, -1), (3, 0), (-3, 0), (0, 3), (0, -3), (1, 2), (-1, -2) })
        {
            int x = g.Hero.X + dx, y = g.Hero.Y + dy;
            if (placed >= 2 || g.Lv.At(x, y) != Tile.Floor || g.Occupied(x, y)) continue;
            for (var i = 0; i < g.EnemiesCount; i++)
            {
                ref var e = ref g.Enemies[i];
                if (!e.Alive || g.Visible(e.X, e.Y)) continue;
                e.X = (sbyte)x;
                e.Y = (sbyte)y;
                e.Awake = placed == 0;
                if (placed == 0) e.Hp = (short)Math.Max(1, e.MaxHp * 2 / 3);
                placed++;
                break;
            }
        }
        g.UpdateFov();
        _app.Refresh();
    }

    public bool EnemyInView()
    {
        var g = G;
        for (var i = 0; i < g.EnemiesCount; i++)
        {
            if (g.Enemies[i].Alive && g.Visible(g.Enemies[i].X, g.Enemies[i].Y)) return true;
        }
        return false;
    }

    private (int X, int Y) FreeNeighbour()
    {
        var g = G;
        foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
        {
            int x = g.Hero.X + dx, y = g.Hero.Y + dy;
            if (g.Lv.At(x, y) == Tile.Floor && !g.Occupied(x, y)) return (x, y);
        }
        return (g.Hero.X + 1, g.Hero.Y);
    }
}
