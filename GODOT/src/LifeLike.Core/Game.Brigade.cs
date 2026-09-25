using LifeLike.Core.Data;

namespace LifeLike.Core;

// Brygada: najemny fachowiec raz na etap z telefonu (port core::game::call_helper z core.h).
public sealed partial class Game
{
    /// <summary>Wolne pole podłogi obok (x, y), najbliższe (nx, ny); false, gdy brak.</summary>
    public bool FreeAround(int x, int y, int nx, int ny, out int ox, out int oy)
    {
        var bd = 99;
        ox = oy = -1;
        for (var k = 0; k < 8; ++k)
        {
            int cx = x + Around[k, 0], cy = y + Around[k, 1];
            if (Lv.At(cx, cy) != Tile.Floor || Occupied(cx, cy) || PickupAt(cx, cy)) continue;
            var d = Cheb(cx, cy, nx, ny);
            if (d < bd)
            {
                bd = d;
                ox = cx;
                oy = cy;
            }
        }
        return bd < 99;
    }

    /// <summary>Czy fachowca h można teraz wezwać (bez skutków ubocznych – telefon i bot).</summary>
    public HelperBlock HelperBlocked(int h)
    {
        var hd = D.Brigade[h];
        if (St != GameStatus.Playing) return HelperBlock.Busy;
        if (HelperCalled >= 0) return HelperBlock.Used;
        if (((Bonus.Helpers >> h) & 1) == 0) return HelperBlock.Locked;
        if (Cash < hd.Price) return HelperBlock.Cash;
        if (hd.Effect == HelperEffect.Pump)
        {
            for (var i = 0; i < EnemiesCount; ++i)
            {
                if (Enemies[i].Alive && Cheb(Hero.X, Hero.Y, Enemies[i].X, Enemies[i].Y) <= hd.Reach) return HelperBlock.Ok;
            }
            return HelperBlock.NoTarget;
        }
        if (hd.Effect == HelperEffect.Ally && !FreeAround(Hero.X, Hero.Y, Hero.X, Hero.Y, out _, out _)) return HelperBlock.NoRoom;
        return HelperBlock.Ok;
    }

    /// <summary>
    /// Wezwanie fachowca (zużywa turę i budżet). Geodeta: mapa etapu; pompa: beton na problemy wokół;
    /// BHP-owiec: zdejmuje stany i daje obronę; pomocnik: idzie za bohaterem i bije sąsiadów przez kilka tur.
    /// </summary>
    public bool CallHelper(int h)
    {
        var hd = D.Brigade[h];
        switch (HelperBlocked(h))
        {
            case HelperBlock.Ok:
                break;
            case HelperBlock.Used:
                Push(Msg("Brygada już była na tym etapie"));
                return false;
            case HelperBlock.Cash:
                Push(Msg("Brygada: za mały budżet (").Add(hd.Price).Add(" zł)"));
                return false;
            case HelperBlock.NoTarget:
                Push(Msg(hd.Name).Add(": nikogo w zasięgu"));
                return false;
            case HelperBlock.NoRoom:
                Push(Msg(hd.Name).Add(": brak miejsca obok"));
                return false;
            default:
                return false;
        }
        if (ShockedTurn()) return true;
        Cash -= hd.Price;
        HelperCalled = (sbyte)h;
        Push(Msg("Brygada: ").Add(hd.Name).As(LogKind.Good));
        switch (hd.Effect)
        {
            case HelperEffect.Reveal: // podłoga, schody i mury przy nich
                for (var y = 0; y < Level.H; ++y)
                {
                    for (var x = 0; x < Level.W; ++x)
                    {
                        if (Fov[y * Level.W + x] != Sight.Unknown) continue;
                        var near = false;
                        for (var dy = -1; dy <= 1 && !near; ++dy)
                        {
                            for (var dx = -1; dx <= 1; ++dx)
                            {
                                if (!Lv.Passable(x + dx, y + dy)) continue;
                                near = true;
                                break;
                            }
                        }
                        if (near) Fov[y * Level.W + x] = Sight.Remembered;
                    }
                }
                break;
            case HelperEffect.Pump:
                for (var i = 0; i < EnemiesCount && St == GameStatus.Playing; ++i)
                {
                    if (Enemies[i].Alive && Cheb(Hero.X, Hero.Y, Enemies[i].X, Enemies[i].Y) <= hd.Reach) DamageEnemy(i, hd.Value, false, hd.Name);
                }
                break;
            case HelperEffect.Safety:
                HeroStatus[(int)StatusEffect.Poison] = HeroStatus[(int)StatusEffect.Shock] = HeroStatus[(int)StatusEffect.Slip] = 0;
                GuardTurns = (sbyte)(hd.Turns + 1); // + tura wezwania
                break;
            case HelperEffect.Ally:
                FreeAround(Hero.X, Hero.Y, Hero.X, Hero.Y, out var ax, out var ay);
                AllyX = (sbyte)ax;
                AllyY = (sbyte)ay;
                AllyTurns = (sbyte)hd.Turns;
                break;
        }
        EndTurn();
        return true;
    }

    /// <summary>Pomocnik: trzyma się obok bohatera i bije problem obok siebie (bez rzutu – stałe obrażenia).</summary>
    public void AllyAct()
    {
        var hd = D.Brigade[HelperCalled];
        if (Cheb(AllyX, AllyY, Hero.X, Hero.Y) != 1 && FreeAround(Hero.X, Hero.Y, AllyX, AllyY, out var x, out var y))
        {
            AllyX = (sbyte)x;
            AllyY = (sbyte)y;
        }
        for (var i = 0; i < EnemiesCount; ++i)
        {
            if (!Enemies[i].Alive || Cheb(AllyX, AllyY, Enemies[i].X, Enemies[i].Y) != 1) continue;
            DamageEnemy(i, hd.Value, false, hd.Name);
            break;
        }
        if (--AllyTurns == 0)
        {
            AllyX = AllyY = -1;
            Push(Msg(hd.Name).Add(": koniec pomocy"));
        }
    }
}
