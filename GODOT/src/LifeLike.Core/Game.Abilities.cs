using LifeLike.Core.Data;

namespace LifeLike.Core;

// Moce zawodów (przycisk R) i Hurtownia między aktami.
public sealed partial class Game
{
    /// <summary>Ranga mocy rośnie z poziomem postaci: II od 3., III od 5. poziomu.</summary>
    public int AbilityRank() => 1 + (HeroLevel >= 3 ? 1 : 0) + (HeroLevel >= 5 ? 1 : 0);

    /// <summary>Każda ranga skraca odnowienie o 2 tury (minimum 4), cecha sprzętu dalej (minimum 3); upał wydłuża.</summary>
    public int AbilityCooldown() =>
        Math.Max(3, Math.Max(4, PDef.AbilityCooldown - 2 * (AbilityRank() - 1)) - TraitBonus(TraitEffect.Cooldown) - Bonus.Cooldown
                    - BoonSum(BoonEffect.Cooldown))
        + (WeatherIs(WeatherEffect.Heat) ? WDef.Value : 0);

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

    public bool CanPlaceWall(int x, int y) => WallsCount < MaxWalls && Lv.At(x, y) == Tile.Floor && !Occupied(x, y) && !PickupAt(x, y);

    public bool PlaceWall(int x, int y, int turnsLeft)
    {
        if (!CanPlaceWall(x, y)) return false;
        Lv[x, y] = Tile.Wall;
        Walls[WallsCount++] = new TempWall { X = (sbyte)x, Y = (sbyte)y, Turns = (sbyte)turnsLeft };
        return true;
    }

    /// <summary>
    /// Mur (2*half+1 pól) w poprzek drogi najbliższego widocznego wroga, na polu przed bohaterem (Ścianka, Załataj).
    /// Zwraca liczbę pól linii (0 = brak widocznego wroga).
    /// </summary>
    public int WallLine(int half, Span<int> xs, Span<int> ys)
    {
        var t = NearestVisibleEnemy();
        if (t < 0) return 0;
        int dx = Math.Sign(Enemies[t].X - Hero.X), dy = Math.Sign(Enemies[t].Y - Hero.Y);
        if (Math.Abs(Enemies[t].X - Hero.X) >= Math.Abs(Enemies[t].Y - Hero.Y)) dy = 0;
        else dx = 0;
        int cx = Hero.X + dx, cy = Hero.Y + dy;            // środek muru: pole przed bohaterem
        int px = dy != 0 ? 1 : 0, py = dx != 0 ? 1 : 0;    // kierunek muru: prostopadle
        var n = 0;
        for (var k = -half; k <= half; ++k)
        {
            xs[n] = cx + px * k;
            ys[n] = cy + py * k;
            ++n;
        }
        return n;
    }

    public bool WallPossible(int half)
    {
        Span<int> xs = stackalloc int[5], ys = stackalloc int[5];
        var n = WallLine(half, xs, ys);
        for (var i = 0; i < n; ++i)
        {
            if (CanPlaceWall(xs[i], ys[i])) return true;
        }
        return false;
    }

    public bool WallTowardEnemy(int half, int dur)
    {
        Span<int> xs = stackalloc int[5], ys = stackalloc int[5];
        var n = WallLine(half, xs, ys);
        var ok = false;
        for (var i = 0; i < n; ++i) ok |= PlaceWall(xs[i], ys[i], dur);
        return ok;
    }

    /// <summary>Moc zawodu (R). Zwraca true, jeśli zużyła turę; bez celu nic się nie dzieje.</summary>
    public bool PlayerAbility()
    {
        if (St != GameStatus.Playing || AbilityCd > 0) return false;
        var c = PDef; // Majster: moc pożyczona na ten etap
        var rank = AbilityRank();
        var ok = false;
        switch (c.Ability)
        {
            case AbilityEffect.Stun: // Odprawa: ogłusza widocznych na 2/3/4 tury
                for (var i = 0; i < EnemiesCount; ++i)
                {
                    if (Enemies[i].Alive && Visible(Enemies[i].X, Enemies[i].Y))
                    {
                        Enemies[i].Stun = (sbyte)(1 + rank + BoonPower());
                        Enemies[i].Awake = true;
                        ok = true;
                    }
                }
                if (ok) Push(Msg(c.AbilityName).Add(": problemy wstrzymane"));
                break;
            case AbilityEffect.Wall: // Ścianka: mur w poprzek drogi najbliższego wroga (nigdy wokół bohatera)
                ok = WallTowardEnemy(rank >= 3 ? 2 : 1, 4 + 2 * rank + BoonPower());
                if (ok) Push(Msg(c.AbilityName).Add(" postawiona!"));
                break;
            case AbilityEffect.Volley: // Seria: wszyscy widoczni w zasięgu (+1 obrażeń od II, +1 zasięgu na III)
            {
                var range = WeaponRange() + (rank >= 3 ? 1 : 0) + BoonPower();
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
                HitCtx = 1; // Łańcuch: prąd (mokry + prąd = porażenie)
                for (var k = 0; k < 2 + rank + BoonPower() && t >= 0 && St == GameStatus.Playing; ++k)
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
                HitCtx = 0;
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
                    e.Wet = (sbyte)Math.Max((int)e.Wet, D.WetTurns); // i jest mokry
                    ok = true;
                }
                var h = Math.Min(4 + 2 * rank + BoonPower(), Hero.MaxHp - Hero.Hp);
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
                HitCtx = 2; // Wirówka: iskry (pył + iskra = wybuch)
                DmgBonus += BoonPower();
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
                DmgBonus -= BoonPower();
                HitCtx = 0;
                break;
            }
            case AbilityEffect.Line: // Rynna (Dekarz): dachówki lecą linią przez najbliższy widoczny problem (4/5/6 pól)
            {
                var t = NearestVisibleEnemy();
                if (t < 0) break;
                int dx = Enemies[t].X - Hero.X, dy = Enemies[t].Y - Hero.Y, len = Math.Max(Math.Abs(dx), Math.Abs(dy));
                if (rank >= 2) ++DmgBonus;
                uint done = 0;
                for (var k = 1; k <= 3 + rank + BoonPower() && St == GameStatus.Playing; ++k)
                {
                    int x = Hero.X + Pct.DivRound(dx * k, len), y = Hero.Y + Pct.DivRound(dy * k, len);
                    if (!Lv.Passable(x, y)) break; // mur zatrzymuje dachówki
                    var ei = EnemyAt(x, y);
                    if (ei >= 0 && (done & (1u << ei)) == 0)
                    {
                        HeroAttack(ei);
                        done |= 1u << ei;
                        ok = true;
                    }
                }
                if (rank >= 2) --DmgBonus;
                if (ok) Push(Msg(c.AbilityName).Add(": dachówki w linii!"));
                break;
            }
            case AbilityEffect.Splash: // Narzut (Tynkarz): tynk na obszar wokół celu w zasięgu (3x3, 5x5 na III), od II ogłusza
            {
                var t = NearestTarget();
                if (t < 0) break;
                int cx = Enemies[t].X, cy = Enemies[t].Y, rad = rank >= 3 ? 2 : 1;
                for (var i = 0; i < EnemiesCount && St == GameStatus.Playing; ++i)
                {
                    if (Enemies[i].Alive && Cheb(cx, cy, Enemies[i].X, Enemies[i].Y) <= rad)
                    {
                        HeroAttack(i);
                        ok = true;
                        var stun = (rank >= 2 ? 1 : 0) + BoonPower(); // premia Gęsty tynk: dłużej
                        if (stun > 0 && Enemies[i].Alive) Enemies[i].Stun = (sbyte)Math.Max((int)Enemies[i].Stun, stun);
                    }
                }
                break;
            }
            case AbilityEffect.Ram: // Taran (Operator koparki): szarża 3/4/5 pól do problemu, cios +ranga, odepchnięcie o 2
            {
                var t = NearestVisibleEnemy();
                if (t < 0) break;
                int ex = Enemies[t].X - Hero.X, ey = Enemies[t].Y - Hero.Y;
                int dx = Math.Abs(ey) >= 2 * Math.Abs(ex) ? 0 : Math.Sign(ex), dy = Math.Abs(ex) >= 2 * Math.Abs(ey) ? 0 : Math.Sign(ey);
                var moved = false;
                for (var k = 0; k < 2 + rank; ++k)
                {
                    int nx = Hero.X + dx, ny = Hero.Y + dy;
                    var ei = EnemyAt(nx, ny);
                    if (ei >= 0)
                    {
                        DmgBonus += rank + BoonPower();
                        HeroAttack(ei);
                        DmgBonus -= rank + BoonPower();
                        if (Enemies[ei].Alive && St == GameStatus.Playing)
                        {
                            if (ei != Boss) Shove(ei, dx, dy, 2);
                            Enemies[ei].Stun = (sbyte)Math.Max((int)Enemies[ei].Stun, 1);
                        }
                        ok = true;
                        break;
                    }
                    if (SecretClosed() && SecretIs(nx, ny) && SecretDef.Breakable)
                    {
                        OpenSecret("Taran kruszy ścianę!");
                        ok = true;
                        break;
                    }
                    if (!Lv.Passable(nx, ny) || Occupied(nx, ny)) break;
                    Hero.X = (sbyte)nx;
                    Hero.Y = (sbyte)ny;
                    moved = ok = true;
                }
                if (moved) Collect();
                if (ok) Push(Msg(c.AbilityName).Add("!"));
                break;
            }
            case AbilityEffect.Weld: // Spaw (Spawacz): iskry linią (3/4/5 pól), trafieni w dymie – kolejna iskra = wybuch pyłu
                ok = AbilityWeld(c, rank);
                break;
            case AbilityEffect.Mark: // Tyczenie (Geodeta): najbliższy widoczny problem oznaczony na kilka tur, ogłuszony na turę
                ok = AbilityMark(c);
                break;
        }
        if (!ok)
        {
            Push(Msg(c.AbilityName).Add(": nie teraz"));
            return false;
        }
        if (PowersUsed < 65535) ++PowersUsed;
        EndTurn();
        AbilityCd = AbilityCooldown();
        return true;
    }

    /// <summary>Czy stać na towar z Hurtowni (zł albo materiał).</summary>
    public bool HurtowniaCan(int i)
    {
        var it = D.Hurtownia[i];
        if (it.Effect == ShopEffect.Upgrade) return UpgradeAffordable(); // ulepszenie narzędzia: zł + materiał
        return it.Material >= 0 ? Mats[it.Material] >= it.MatCost : Cash >= HurtowniaPrice(i);
    }

    /// <summary>Cena towaru w zł po rabacie z Respektu (ulepszenie narzędzia: kolejny poziom).</summary>
    public int HurtowniaPrice(int i) =>
        D.Hurtownia[i].Effect == ShopEffect.Upgrade ? UpgradePrice() : D.Hurtownia[i].Price * (100 - Math.Min(90, Bonus.ShopPct + BoonSum(BoonEffect.ShopPct))) / 100;

    /// <summary>Losowy slot sprzętu spośród dostępnych (nagrody dokładają buty i pas); przy 3 slotach jak dawniej.</summary>
    public int RandomSlot()
    {
        var n = 0;
        for (var i = 0; i < D.GearSlotsCount; ++i) n += (Bonus.GearSlots >> i) & 1;
        var k = R.Range(0, Math.Max(1, n) - 1);
        for (var i = 0; i < D.GearSlotsCount; ++i)
        {
            if (((Bonus.GearSlots >> i) & 1) != 0 && k-- == 0) return i;
        }
        return 0;
    }

    /// <summary>Hurtownia między aktami: zakup za budżet budowy albo materiał.</summary>
    public bool HurtowniaBuy(int i)
    {
        var it = D.Hurtownia[i];
        if (!HurtowniaCan(i)) return false;
        if (it.Effect == ShopEffect.Upgrade) // ulepszenie narzędzia (#31): zł i materiał, potem +1 poziom
        {
            var t = D.ToolLevels[WeaponLvl];
            Cash -= UpgradePrice();
            Mats[t.Material] = (byte)(Mats[t.Material] - t.Count);
            UpgradeWeapon();
            return true;
        }
        switch (it.Effect)
        {
            case ShopEffect.Heal:
                Hero.Hp = Hero.MaxHp;
                if (CoffeeDrunk < 255) ++CoffeeDrunk; // kawa z ekspresu
                break;
            case ShopEffect.MaxHp:
                Hero.MaxHp = (short)(Hero.MaxHp + 3);
                Hero.Hp = (short)(Hero.Hp + 3);
                break;
            case ShopEffect.Ability:
                AbilityCd = 0;
                break;
            case ShopEffect.Def:
                ++DefBonus;
                break;
            case ShopEffect.Thermos:
                Thermos = Math.Min(ThermosCap(), Thermos + 2);
                break;
            case ShopEffect.Gear:
            {
                var slot = RandomSlot();
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
                        ResetUpgrade();
                        WeaponOverride = D.Tools[t].Weapon;
                        ToolsFound = (byte)(ToolsFound | (1u << t));
                        break;
                    }
                }
                break;
            }
        }
        if (it.Material >= 0) Mats[it.Material] = (byte)(Mats[it.Material] - it.MatCost);
        else Cash -= HurtowniaPrice(i);
        Push(Msg("Hurtownia: ").Add(it.Name).As(LogKind.Loot));
        return true;
    }
}
