using LifeLike.Core.Data;

namespace LifeLike.Core;

// Moce zawodów (przycisk R) i Hurtownia między aktami.
public sealed partial class Game
{
    /// <summary>Ranga mocy rośnie z poziomem postaci: II od 3., III od 5. poziomu.</summary>
    public int AbilityRank() => 1 + (HeroLevel >= 3 ? 1 : 0) + (HeroLevel >= 5 ? 1 : 0);

    /// <summary>Każda ranga skraca odnowienie o 2 tury (minimum 4), cecha sprzętu dalej (minimum 3).</summary>
    public int AbilityCooldown() =>
        Math.Max(3, Math.Max(4, CDef.AbilityCooldown - 2 * (AbilityRank() - 1)) - TraitBonus(TraitEffect.Cooldown));

    public int NearestVisibleEnemy()
    {
        int best = -1, bd = 99;
        for (var i = 0; i < EnemiesCount; ++i)
        {
            var e = Enemies[i];
            var d = Cheb(Hero.X, Hero.Y, e.X, e.Y);
            if (e.Alive && Visible(e.X, e.Y) && d < bd)
            {
                bd = d;
                best = i;
            }
        }
        return best;
    }

    public bool PlaceWall(int x, int y, int turnsLeft)
    {
        if (WallsCount >= MaxWalls || Lv.At(x, y) != Tile.Floor || Occupied(x, y) || PickupAt(x, y)) return false;
        Lv[x, y] = Tile.Wall;
        Walls[WallsCount++] = new TempWall { X = (sbyte)x, Y = (sbyte)y, Turns = (sbyte)turnsLeft };
        return true;
    }

    /// <summary>Moc zawodu (R). Zwraca true, jeśli zużyła turę; bez celu nic się nie dzieje.</summary>
    public bool PlayerAbility()
    {
        if (St != GameStatus.Playing || AbilityCd > 0) return false;
        var c = CDef;
        var rank = AbilityRank();
        var ok = false;
        switch (c.Ability)
        {
            case AbilityEffect.Stun: // Odprawa: ogłusza widocznych na 2/3/4 tury
                for (var i = 0; i < EnemiesCount; ++i)
                {
                    if (Enemies[i].Alive && Visible(Enemies[i].X, Enemies[i].Y))
                    {
                        Enemies[i].Stun = (sbyte)(1 + rank);
                        Enemies[i].Awake = true;
                        ok = true;
                    }
                }
                if (ok) Push(Msg(c.AbilityName).Add(": problemy wstrzymane"));
                break;
            case AbilityEffect.Wall: // Ścianka: mur w poprzek drogi najbliższego wroga (nigdy wokół bohatera)
            {
                var t = NearestVisibleEnemy();
                if (t < 0) break;
                int dx = Math.Sign(Enemies[t].X - Hero.X), dy = Math.Sign(Enemies[t].Y - Hero.Y);
                if (Math.Abs(Enemies[t].X - Hero.X) >= Math.Abs(Enemies[t].Y - Hero.Y)) dy = 0;
                else dx = 0;
                int cx = Hero.X + dx, cy = Hero.Y + dy;            // środek muru: pole przed bohaterem
                int px = dy != 0 ? 1 : 0, py = dx != 0 ? 1 : 0;    // kierunek muru: prostopadle
                int half = rank >= 3 ? 2 : 1, dur = 4 + 2 * rank;
                for (var k = -half; k <= half; ++k) ok |= PlaceWall(cx + px * k, cy + py * k, dur);
                if (ok) Push(Msg(c.AbilityName).Add(" postawiona!"));
                break;
            }
            case AbilityEffect.Volley: // Seria: wszyscy widoczni w zasięgu (+1 obrażeń od II, +1 zasięgu na III)
            {
                var range = Weapon.Range + (rank >= 3 ? 1 : 0);
                if (rank >= 2) ++DmgBonus;
                for (var i = 0; i < EnemiesCount && St == GameStatus.Playing; ++i)
                {
                    var e = Enemies[i];
                    if (e.Alive && Visible(e.X, e.Y) && Cheb(Hero.X, Hero.Y, e.X, e.Y) <= range)
                    {
                        HeroAttack(i);
                        ok = true;
                    }
                }
                if (rank >= 2) --DmgBonus;
                break;
            }
            case AbilityEffect.Chain: // Łańcuch: 3/4/5 celów, skoki do 2 pól
            {
                uint done = 0;
                var t = NearestTarget();
                for (var k = 0; k < 2 + rank && t >= 0 && St == GameStatus.Playing; ++k)
                {
                    int px = Enemies[t].X, py = Enemies[t].Y;
                    HeroAttack(t);
                    done |= 1u << t;
                    ok = true;
                    t = -1;
                    for (var i = 0; i < EnemiesCount; ++i)
                    {
                        var e = Enemies[i];
                        if (e.Alive && (done & (1u << i)) == 0 && Visible(e.X, e.Y) && Cheb(px, py, e.X, e.Y) <= 2)
                        {
                            t = i;
                            break;
                        }
                    }
                }
                break;
            }
            case AbilityEffect.Flush: // Zawór: strumień odpycha sąsiadów o 1/2 pola (tracą turę) i leczy 6/8/10 HP
            {
                var pushBy = rank >= 2 ? 2 : 1;
                for (var i = 0; i < EnemiesCount; ++i)
                {
                    ref var e = ref Enemies[i];
                    if (!e.Alive || Cheb(Hero.X, Hero.Y, e.X, e.Y) != 1) continue;
                    int dx = Math.Sign(e.X - Hero.X), dy = Math.Sign(e.Y - Hero.Y);
                    for (var k = 0; k < pushBy; ++k)
                    {
                        int nx = e.X + dx, ny = e.Y + dy;
                        if (Lv.At(nx, ny) != Tile.Floor || Occupied(nx, ny)) break;
                        e.X = (sbyte)nx;
                        e.Y = (sbyte)ny;
                    }
                    e.Awake = true;
                    e.Stun = (sbyte)Math.Max((int)e.Stun, 1); // zalany traci turę, inaczej od razu by wrócił
                    ok = true;
                }
                var h = Math.Min(4 + 2 * rank, Hero.MaxHp - Hero.Hp);
                if (h > 0)
                {
                    Hero.Hp = (short)(Hero.Hp + h);
                    ok = true;
                }
                if (ok) Push(Msg(c.AbilityName).Add(": strumień! +").Add(Math.Max(0, h)).Add(" HP"));
                break;
            }
            case AbilityEffect.Spin: // Wirówka: wszyscy obok (zasięg 2 na III), od II ogłusza na 1 turę
            {
                var reach = rank >= 3 ? 2 : 1;
                for (var i = 0; i < EnemiesCount && St == GameStatus.Playing; ++i)
                {
                    var d = Cheb(Hero.X, Hero.Y, Enemies[i].X, Enemies[i].Y);
                    if (Enemies[i].Alive && d >= 1 && d <= reach)
                    {
                        HeroAttack(i);
                        ok = true;
                        if (rank >= 2 && Enemies[i].Alive) Enemies[i].Stun = (sbyte)Math.Max((int)Enemies[i].Stun, 1);
                    }
                }
                break;
            }
        }
        if (!ok)
        {
            Push(Msg(c.AbilityName).Add(": nie teraz"));
            return false;
        }
        EndTurn();
        AbilityCd = AbilityCooldown();
        return true;
    }

    /// <summary>Hurtownia między aktami: zakup za budżet budowy.</summary>
    public bool HurtowniaBuy(int i)
    {
        var it = D.Hurtownia[i];
        if (Cash < it.Price) return false;
        switch (it.Effect)
        {
            case ShopEffect.Heal:
                Hero.Hp = Hero.MaxHp;
                break;
            case ShopEffect.MaxHp:
                Hero.MaxHp = (short)(Hero.MaxHp + 3);
                Hero.Hp = (short)(Hero.Hp + 3);
                break;
            case ShopEffect.Ability:
                AbilityCd = 0;
                break;
            case ShopEffect.Gear:
            {
                var slot = R.Range(0, D.GearSlotsCount - 1);
                var rarity = R.Range(1, 2);
                if (rarity <= Equipped[slot]) rarity = Math.Min(2, Equipped[slot] + 1);
                if (rarity > Equipped[slot]) Equip(slot, rarity, R.Range(0, D.GearTraitsCount - 1));
                else GainXp(3);
                break;
            }
            case ShopEffect.Tool:
            {
                var n = 0;
                for (var t = 0; t < D.Tools.Length; ++t) n += (Bonus.Tools >> t) & 1;
                var k = R.Range(0, Math.Max(0, n - 1));
                for (var t = 0; t < D.Tools.Length; ++t)
                {
                    if (((Bonus.Tools >> t) & 1) != 0 && k-- == 0)
                    {
                        WeaponOverride = D.Tools[t].Weapon;
                        ToolsFound = (byte)(ToolsFound | (1u << t));
                        break;
                    }
                }
                break;
            }
        }
        Cash -= it.Price;
        Push(Msg("Hurtownia: ").Add(it.Name).As(LogKind.Loot));
        return true;
    }
}
