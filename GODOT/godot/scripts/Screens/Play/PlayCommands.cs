using System;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.World;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Screens.Play;

/// <summary>Akcje gracza na mapie wspólne dla sterowania, menu akcji i myszy (każda zwraca: czy zużyła turę).</summary>
public static class PlayCommands
{
    /// <summary>A: atak najbliższego widocznego celu w zasięgu (bez celu miga zasięg).</summary>
    public static bool AttackNearest(CoreGame g, WorldView view)
    {
        Span<sbyte> t = stackalloc sbyte[CoreGame.MaxEnemies];
        var acted = g.TargetsInRange(t) > 0 ? g.PlayerAttack(t[0]) : g.PlayerAttackNearest();
        if (!acted) view.FlashRange();
        return acted;
    }

    /// <summary>Moc zawodu z dźwiękiem i efektami (ability_fx na GBA) - przed odświeżeniem, póki są trafienia tury.</summary>
    public static bool UseAbility(CoreGame g, WorldView view)
    {
        var acted = g.PlayerAbility();
        if (acted)
        {
            Sfx.Play("ability");
            view.Effects.Ability();
        }
        return acted;
    }

    /// <summary>
    /// v0.23.0: Cios B – tryb B broni w cel; bez celu otwiera pękniętą ścianę obok (Przecinak, Łom), inaczej miga zasięg wzoru B
    /// (bez tury). Bez celu wybiera najbliższy cel trybu B.
    /// </summary>
    public static bool WeaponB(CoreGame g, WorldView view, int target)
    {
        if (target < 0)
        {
            Span<sbyte> t = stackalloc sbyte[CoreGame.MaxEnemies];
            if (Aiming.TargetsB(g, t) > 0) target = t[0];
        }
        var acted = target >= 0 ? g.PlayerWeaponB(target) : g.CanWeaponBOpenWall() && g.PlayerWeaponBWall();
        if (acted) Sfx.Play("ability", 0.8f);
        else view.FlashRange(g.Weapon.ModeB);
        return acted;
    }

    /// <summary>v0.23.0: zamiana broni z kabury (pas monterski) – tura; bez pasa / z pustą kaburą wpis w dzienniku, bez tury.</summary>
    public static bool SwapOrNotice(CoreGame g)
    {
        var acted = g.PlayerSwapWeapon();
        if (acted) Sfx.Play("pickup");
        else Notice(g, g.HasBelt() ? Loc.T("kabura_pusta") : Loc.T("zamiana_tylko_z_pasem"));
        return acted;
    }

    /// <summary>v0.23.0: krótka informacja w dzienniku budowy (bez tury), np. „Cios B: jeszcze 3 t.”.</summary>
    public static void Notice(CoreGame g, string text) => g.Push(new Message().Add(text));

    /// <summary>Termos: kawa leczy i zużywa turę.</summary>
    public static bool Drink(CoreGame g)
    {
        var acted = g.PlayerDrink();
        if (acted) Sfx.Play("pickup");
        return acted;
    }

    /// <summary>Mysz: lewy klik na wroga w zasięgu = atak, gdzie indziej = krok w tę stronę; prawy klik = czekaj.</summary>
    public static bool Mouse(CoreGame g, WorldView view, MouseButton button)
    {
        if (button == MouseButton.Right) return g.PlayerWait();
        if (button != MouseButton.Left) return false;
        var cell = view.ScreenToGrid(view.GetGlobalMousePosition());
        var ei = g.EnemyAt(cell.X, cell.Y);
        if (ei >= 0 && g.Visible(cell.X, cell.Y) && CoreGame.Cheb(g.Hero.X, g.Hero.Y, cell.X, cell.Y) <= g.WeaponRange())
            return g.PlayerAttack(ei);
        int dx = cell.X - g.Hero.X, dy = cell.Y - g.Hero.Y;
        if (dx == 0 && dy == 0) return g.PlayerWait();
        return Math.Abs(dx) >= Math.Abs(dy) ? g.PlayerMove(Math.Sign(dx), 0) : g.PlayerMove(0, Math.Sign(dy));
    }
}
