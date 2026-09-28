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

    /// <summary>
    /// Pokaz nowego zawodu (scenariusze 25-27 na GBA): podłoga wokół bohatera i problemy ustawione pod moc -
    /// Rynna: trzy w linii w prawo, Narzut: grupa 2 pola w prawo, Taran: wytrzymały problem 3 pola w prawo.
    /// </summary>
    public void NewClassShowcase(LifeLike.Core.Data.AbilityEffect effect)
    {
        var g = G;
        var d = g.D;
        for (var y = g.Hero.Y - 2; y <= g.Hero.Y + 2; y++)
        {
            for (var x = g.Hero.X - 1; x <= g.Hero.X + 6; x++)
            {
                if (x >= 1 && y >= 1 && x < Level.W - 1 && y < Level.H - 1 && g.Lv[x, y] == Tile.Wall) g.Lv[x, y] = Tile.Floor;
            }
        }
        g.EnemiesCount = 0;
        int Def(string id) => Math.Max(0, Array.FindIndex(d.Enemies, e => e.Id == id));
        void Put(string id, int dx, int dy, int hp)
        {
            int x = g.Hero.X + dx, y = g.Hero.Y + dy;
            if (g.Lv.At(x, y) != Tile.Floor || g.Occupied(x, y)) return;
            g.Spawn(Def(id), x, y);
            ref var e = ref g.Enemies[g.EnemiesCount - 1];
            e.Awake = true;
            e.Stun = 30;
            if (hp > 0) e.Hp = e.MaxHp = (short)hp;
        }
        switch (effect)
        {
            case LifeLike.Core.Data.AbilityEffect.Line:
                Put("przeciek", 2, 0, 40);
                Put("kornik", 3, 0, 40);
                Put("przeciek", 4, 0, 40);
                break;
            case LifeLike.Core.Data.AbilityEffect.Splash:
                Put("kornik", 2, 0, 40);
                Put("przeciek", 3, 1, 40);
                Put("plesn", 3, -1, 40);
                break;
            default:
                Put("budzet", 3, 0, 80);
                break;
        }
        g.PickupsCount = 0;
        g.UpdateFov();
        _app.Nodes.World.Sync();
        _app.Refresh();
    }

    /// <summary>Podłoga w prostokącie wokół bohatera (pokazy potrzebują miejsca).</summary>
    private void ClearAround(int dx0, int dy0, int dx1, int dy1)
    {
        var g = G;
        for (var y = g.Hero.Y + dy0; y <= g.Hero.Y + dy1; y++)
        {
            for (var x = g.Hero.X + dx0; x <= g.Hero.X + dx1; x++)
            {
                if (x >= 1 && y >= 1 && x < Level.W - 1 && y < Level.H - 1 && g.Lv[x, y] == Tile.Wall) g.Lv[x, y] = Tile.Floor;
            }
        }
    }

    /// <summary>Problem o identyfikatorze id na polu (bohater + dx, dy), przebudzony; hp > 0 = ustawione HP.</summary>
    private void Place(string id, int dx, int dy, int hp, int stun)
    {
        var g = G;
        int x = g.Hero.X + dx, y = g.Hero.Y + dy;
        if (g.EnemiesCount >= CoreGame.MaxEnemies || g.Lv.At(x, y) != Tile.Floor || g.Occupied(x, y)) return;
        g.Spawn(Math.Max(0, Array.FindIndex(g.D.Enemies, e => e.Id == id)), x, y);
        ref var e = ref g.Enemies[g.EnemiesCount - 1];
        e.Awake = true;
        e.Stun = (sbyte)stun;
        if (hp > 0) e.Hp = (short)hp;
    }

    /// <summary>
    /// Pokaz zachowań (scenariusze 29-30 na GBA): Mostek termiczny strzela z 3 pól, Woda gruntowa (1 HP) dzieli się po
    /// ciosie, Pęknięty pustak (1 HP) wybucha - czerwone pola i tura na zejście.
    /// </summary>
    public void BehaviorShowcase()
    {
        var g = G;
        ClearAround(-3, -3, 3, 3);
        g.EnemiesCount = 0;
        g.PickupsCount = 0;
        Place("mostek", 0, 3, 0, 0);
        Place("woda", 1, 0, 1, 4);
        Place("pustak", -1, 0, 1, 4);
        g.UpdateFov();
        _app.Nodes.World.Sync();
        _app.AfterAction(g.PlayerMove(1, 0));    // podział
        _app.AfterAction(g.PlayerMove(-1, 0));   // wybuch za turę + strzał Mostka
    }

    /// <summary>Etap s aktu z mechaniką (błoto / porywy / pył): bohater obok błota albo poryw w tej turze.</summary>
    public void ActShowcase(int stage)
    {
        var g = G;
        g.StartStage(stage);
        for (var i = 0; i < g.EnemiesCount; i++) g.Enemies[i].Stun = 60;
        if (g.ActIs(LifeLike.Core.Data.ActMechanic.Mud)) // najbliższe błoto z wolnym polem po lewej: krok w nie
        {
            int best = 999, hx = g.Hero.X, hy = g.Hero.Y, bx = hx, by = hy;
            for (var y = 1; y < Level.H - 1; y++)
            {
                for (var x = 2; x < Level.W - 1; x++)
                {
                    if (!g.Mud(x, y) || g.Lv.At(x - 1, y) != Tile.Floor || g.Mud(x - 1, y) || g.Occupied(x - 1, y)) continue;
                    var d = CoreGame.Cheb(x, y, hx, hy);
                    if (d < best)
                    {
                        best = d;
                        bx = x - 1;
                        by = y;
                    }
                }
            }
            g.Hero.X = (sbyte)bx;
            g.Hero.Y = (sbyte)by;
            g.UpdateFov();
            _app.Nodes.World.Sync();
            _app.AfterAction(g.PlayerMove(1, 0));
            return;
        }
        if (g.ActIs(LifeLike.Core.Data.ActMechanic.Gust)) // poryw właśnie spycha
        {
            g.StageStartTurn = g.Turns - (g.ADef.MechValue - 1);
            g.UpdateFov();
            _app.Nodes.World.Sync();
            _app.AfterAction(g.PlayerWait());
            return;
        }
        g.UpdateFov();
        _app.Nodes.World.Sync();
        _app.Refresh();
    }
}
