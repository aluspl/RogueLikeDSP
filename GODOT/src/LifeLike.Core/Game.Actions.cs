using LifeLike.Core.Data;

namespace LifeLike.Core;

// Akcje gracza, walka, AI problemów budowy i koniec tury (port core::game z core.h).
public sealed partial class Game
{
    /// <summary>Obrażenia = rzut broni + stat/2 + premie - obrona/2, min 1.</summary>
    public void HeroAttack(int ei)
    {
        ref var e = ref Enemies[ei];
        var ed = D.Enemies[e.DefId];
        if (ei == Boss && BossWakeDamage < 0) BossEngaged(); // walka z bossem trwa
        var dmg = R.Range(Weapon.MinDamage, Weapon.MaxDamage) + HeroStat(Weapon.ScalesWith) / 2 + DmgBonus
                  + GearBonus(GearStat.Dmg) - ed.Defense / 2;
        if (dmg < 1) dmg = 1;
        var crit = R.Range(1, 100) <= CritPct();
        if (crit) dmg *= D.CritMultiplier;
        e.Hp = (short)(e.Hp - dmg);
        e.Awake = true;
        LastTarget = ei;
        AddHit(e.X, e.Y, dmg, false, crit ? HitKind.Crit : HitKind.Normal);
        TurnEvents |= 1u << ei;
        if (e.Hp <= 0)
        {
            e.Alive = false;
            ++Kills;
            ++StageKills;
            ++ActKills;
            Cash += ed.Score / D.CashPerScore;
            if (KillsByType[e.DefId] < 255) ++KillsByType[e.DefId];
            Score += ed.Score * ScorePct() / 100;
            GainXp(D.XpPerKill);
            MaybeDrop(e.X, e.Y);
            Push(Msg(ed.Name).Add(" - usunięto!").As(LogKind.Good));
            if (ei == Boss)
            {
                if (StageDamage == BossWakeDamage && CleanBosses < 255) ++CleanBosses; // zlecenie Czysta robota
                Score += (500 + 100 * (Stage + 1)) * ScorePct() / 100;
                GainXp(D.XpBoss);
                SlamTimer = 0;
                if (ed.RewardCash > 0) // nagroda bossa (Inspekcja: Protokół bez uwag)
                {
                    Cash += ed.RewardCash;
                    Push(Msg(ed.RewardTitle).Add("! +").Add(ed.RewardCash).Add(" zł").As(LogKind.Good));
                }
                if (Stage == D.Stages.Length - 1)
                {
                    St = GameStatus.Won;
                    Push(Msg("Odbiór techniczny zaliczony!").As(LogKind.Good));
                }
                else if (D.Stages[Stage + 1].Act == D.Stages[Stage].Act) // boss w środku aktu: dalej bez Hurtowni
                {
                    St = GameStatus.StageClear;
                    Push(Msg("Etap zakończony: ").Add(D.Stages[Stage].Name).As(LogKind.Good));
                }
                else // boss aktu: premia za akt, potem Hurtownia
                {
                    var act = D.Stages[Stage].Act;
                    var ad = D.Acts[act];
                    var stagesInAct = 0;
                    for (var i = 0; i < D.Stages.Length; ++i)
                    {
                        if (D.Stages[i].Act == act) stagesInAct++;
                    }
                    ActBonus = ad.BonusPerStage * stagesInAct + ad.BonusPerKill * ActKills;
                    Cash += ActBonus;
                    ActKills = 0;
                    ActCleared = true;
                    St = GameStatus.StageClear;
                    Push(Msg("Akt zaliczony! Premia ").Add(ActBonus).Add(" zł").As(LogKind.Good));
                }
            }
        }
        else
        {
            Push(Msg(crit ? "KRYT! " : "").Add(Weapon.Name).Add(": -").Add(dmg).Add(" (").Add(ed.Name).Add(")").As(crit ? LogKind.Loot : LogKind.Info));
        }
    }

    /// <summary>Ruch albo atak wroga na drodze. Zwraca true, jeśli zużył turę.</summary>
    public bool PlayerMove(int dx, int dy)
    {
        if (St != GameStatus.Playing) return false;
        if (ShockedTurn()) return true;
        int nx = Hero.X + dx, ny = Hero.Y + dy;
        var ei = EnemyAt(nx, ny);
        if (ei >= 0)
        {
            HeroAttack(ei);
        }
        else if (Lv.Passable(nx, ny))
        {
            Hero.X = (sbyte)nx;
            Hero.Y = (sbyte)ny;
            Collect();
            ref var slip = ref HeroStatus[(int)StatusEffect.Slip];
            if (slip > 0) // poślizg: jeszcze jedno pole w tę samą stronę
            {
                --slip;
                int sx = Hero.X + dx, sy = Hero.Y + dy;
                if (Lv.Passable(sx, sy) && !Occupied(sx, sy))
                {
                    Hero.X = (sbyte)sx;
                    Hero.Y = (sbyte)sy;
                    Collect();
                }
            }
            else if (Puddle(Hero.X, Hero.Y)) // deszcz: kałuża = poślizg
            {
                ApplyStatus(StatusEffect.Slip, 2);
            }
        }
        else
        {
            return false;
        }
        EndTurn();
        return true;
    }

    public int NearestTarget()
    {
        int best = -1, bd = 99;
        for (var i = 0; i < EnemiesCount; ++i)
        {
            var e = Enemies[i];
            var d = Cheb(Hero.X, Hero.Y, e.X, e.Y);
            if (e.Alive && d <= WeaponRange() && d < bd)
            {
                bd = d;
                best = i;
            }
        }
        return best;
    }

    /// <summary>Cele w zasięgu broni (widoczni, żywi), posortowane od najbliższego. Zwraca liczbę.</summary>
    public int TargetsInRange(Span<sbyte> output)
    {
        var n = 0;
        for (var d = 1; d <= WeaponRange(); ++d)
        {
            for (var i = 0; i < EnemiesCount && n < output.Length; ++i)
            {
                var e = Enemies[i];
                if (e.Alive && Visible(e.X, e.Y) && Cheb(Hero.X, Hero.Y, e.X, e.Y) == d) output[n++] = (sbyte)i;
            }
        }
        return n;
    }

    /// <summary>Atak wybranego celu (celowanie). Cel musi być w zasięgu i widoczny.</summary>
    public bool PlayerAttack(int ei)
    {
        if (St != GameStatus.Playing || ei < 0 || ei >= EnemiesCount) return false;
        if (ShockedTurn()) return true;
        var e = Enemies[ei];
        if (!e.Alive || !Visible(e.X, e.Y) || Cheb(Hero.X, Hero.Y, e.X, e.Y) > WeaponRange()) return false;
        HeroAttack(ei);
        EndTurn();
        return true;
    }

    public bool PlayerAttackNearest()
    {
        if (St != GameStatus.Playing) return false;
        if (ShockedTurn()) return true;
        var t = NearestTarget();
        if (t < 0)
        {
            Push(Msg("Brak celu w zasięgu ").Add(WeaponRange()));
            return false;
        }
        HeroAttack(t);
        EndTurn();
        return true;
    }

    public bool PickupAt(int x, int y)
    {
        for (var i = 0; i < PickupsCount; ++i)
        {
            if (Pickups[i].Active && Pickups[i].X == x && Pickups[i].Y == y) return true;
        }
        return false;
    }

    public int CoffeeHeal() => D.CoffeeHeal + Bonus.Coffee;

    public void DrinkCoffee()
    {
        var h = Math.Min(CoffeeHeal(), Hero.MaxHp - Hero.Hp);
        Hero.Hp = (short)(Hero.Hp + h);
        Push(Msg("Kawa z termosu: +").Add(h).Add(" HP").As(LogKind.Good));
    }

    /// <summary>Picie z termosu (menu akcji): leczy, zużywa turę.</summary>
    public bool PlayerDrink()
    {
        if (St != GameStatus.Playing) return false;
        if (Thermos <= 0)
        {
            Push(Msg("Termos pusty"));
            return false;
        }
        if (Hero.Hp >= Hero.MaxHp)
        {
            Push(Msg("HP pełne - kawa poczeka"));
            return false;
        }
        if (ShockedTurn()) return true;
        --Thermos;
        DrinkCoffee();
        EndTurn();
        return true;
    }

    public bool PlayerWait()
    {
        if (St != GameStatus.Playing) return false;
        if (Hero.Hp < Hero.MaxHp && Turns % 4 == 0) ++Hero.Hp; // krótki odpoczynek
        EndTurn();
        return true;
    }

    /// <summary>Drop z pokonanego wroga: szansa DropChancePct, typ losowany wagami.</summary>
    public void MaybeDrop(int x, int y)
    {
        if (R.Range(1, 100) > D.DropChancePct + D.DropPerLuckPct * Luck() || PickupsCount >= MaxPickups) return;
        for (var i = 0; i < PickupsCount; ++i)
        {
            if (Pickups[i].Active && Pickups[i].X == x && Pickups[i].Y == y) return;
        }
        var total = 0;
        foreach (var w in D.DropWeights) total += w;
        int roll = R.Range(1, total), type = 0;
        while (roll > D.DropWeights[type]) roll -= D.DropWeights[type++];
        if (type != (int)PickupType.Tool && Bonus.ToolPct > 0 && R.Range(1, 100) <= Bonus.ToolPct) type = (int)PickupType.Tool; // uprawnienie Kolekcjoner
        int arg = 0, trait = 0;
        if (type == (int)PickupType.GearBox) // slot losowy, jakość lepsza na późnych etapach i ze szczęściem
        {
            var q = R.Range(1, 100) + Stage * D.GearStageBonus + D.RarityPerLuck * Luck();
            var rarity = q >= D.GearBrandFrom ? 2 : (q >= D.GearSolidFrom ? 1 : 0);
            arg = (byte)(R.Range(0, D.GearSlotsCount - 1) * 3 + rarity);
            trait = (byte)R.Range(0, D.GearTraitsCount - 1);
        }
        if (type == (int)PickupType.Tool)
        {
            var n = 0;
            for (var i = 0; i < D.Tools.Length; ++i) n += (Bonus.Tools >> i) & 1;
            if (n == 0)
            {
                type = (int)PickupType.Coffee;
            }
            else
            {
                var k = R.Range(0, n - 1);
                for (var i = 0; i < D.Tools.Length; ++i)
                {
                    if (((Bonus.Tools >> i) & 1) != 0 && k-- == 0)
                    {
                        arg = (byte)i;
                        break;
                    }
                }
            }
        }
        Pickups[PickupsCount++] = new Pickup(x, y, (PickupType)type, true, arg, trait);
    }

    /// <summary>Zakłada przedmiot w slocie (zastępuje obecny; kamizelka od razu zmienia max HP).</summary>
    public void Equip(int slot, int rarity, int trait)
    {
        var nw = D.Gear[slot * 3 + rarity];
        if (nw.Stat == GearStat.Hp)
        {
            var diff = nw.Value - (Equipped[slot] >= 0 ? D.Gear[slot * 3 + Equipped[slot]].Value : 0);
            Hero.MaxHp = (short)(Hero.MaxHp + diff);
            Hero.Hp = (short)Math.Max(1, Hero.Hp + diff);
        }
        Equipped[slot] = (sbyte)rarity;
        EquippedTrait[slot] = (sbyte)trait;
        if (rarity == 2 && BrandFound < 255) ++BrandFound; // zlecenie Markowy styl
        UpdateFov(); // cecha Widzenie zmienia pole widzenia
        Push(Msg("Sprzęt: ").Add(nw.Name).Add(" +").Add(nw.Value).As(LogKind.Loot));
    }

    public bool HasOffer => OfferSlot >= 0;

    public bool OfferIsBetter => HasOffer && OfferRarity > Equipped[OfferSlot];

    /// <summary>Paczka sprzętu przy zajętym slocie: gracz porównuje (A zakładam, B zostawiam). Nie zużywa tury.</summary>
    public void AcceptOffer()
    {
        if (!HasOffer) return;
        int slot = OfferSlot;
        OfferSlot = -1;
        Equip(slot, OfferRarity, OfferTrait);
    }

    public void DeclineOffer()
    {
        if (!HasOffer) return;
        var xp = D.GearDeclineXp + OfferRarity;
        OfferSlot = -1;
        GainXp(xp);
        Push(Msg("Zostawiasz stary sprzęt: +").Add(xp).Add(" dośw.").As(LogKind.Loot));
    }

    public void Collect()
    {
        for (var i = 0; i < PickupsCount; ++i)
        {
            ref var p = ref Pickups[i];
            if (!p.Active || p.X != Hero.X || p.Y != Hero.Y) continue;
            if (p.Type == PickupType.GearBox && HasOffer) continue; // najpierw decyzja o poprzedniej paczce
            p.Active = false;
            if (p.Type == PickupType.Coffee)
            {
                if (Thermos < ThermosCap()) // kawa do termosu; pełny termos – pije od razu
                {
                    ++Thermos;
                    Push(Msg("Kawa do termosu (").Add(Thermos).Add("/").Add(ThermosCap()).Add(")").As(LogKind.Good));
                }
                else
                {
                    DrinkCoffee();
                }
            }
            else if (p.Type == PickupType.Helmet)
            {
                ++DefBonus;
                Push(Msg("Nowy kask: obrona +1").As(LogKind.Loot));
            }
            else if (p.Type == PickupType.Plan)
            {
                ++DmgBonus;
                Push(Msg("Projekt wykonawczy: obrażenia +1").As(LogKind.Loot));
            }
            else if (p.Type == PickupType.GearBox)
            {
                var slot = p.Arg / 3;
                if (Equipped[slot] < 0)
                {
                    Equip(slot, p.Arg % 3, p.Trait); // pusty slot: zakłada od razu
                }
                else
                {
                    OfferSlot = (sbyte)slot;
                    OfferRarity = (sbyte)(p.Arg % 3);
                    OfferTrait = (sbyte)p.Trait;
                    Push(Msg("Paczka: ").Add(D.Gear[p.Arg].Name).As(LogKind.Loot));
                }
            }
            else
            {
                WeaponOverride = D.Tools[p.Arg].Weapon;
                ToolsFound = (byte)(ToolsFound | (1u << p.Arg));
                Push(Msg("Narzędzie: ").Add(Weapon.Name).Add(" ").Add(Weapon.MinDamage).Add("-").Add(Weapon.MaxDamage).As(LogKind.Loot));
            }
        }
    }

    private static readonly int[,] Around = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 } };

    /// <summary>Wezwanie (Inspekcja: Papierologia): budzi kolejne uśpione miejsce za bossem na wolnym polu obok niego.</summary>
    public bool SummonNear(int bx, int by)
    {
        var slot = Boss + 1 + SummonsUsed;
        if (slot >= EnemiesCount) return false;
        for (var k = 0; k < 8; ++k)
        {
            int x = bx + Around[k, 0], y = by + Around[k, 1];
            if (Lv.At(x, y) != Tile.Floor || Occupied(x, y)) continue;
            ref var m = ref Enemies[slot];
            m.X = (sbyte)x;
            m.Y = (sbyte)y;
            m.Hp = m.MaxHp;
            m.Alive = true;
            m.Awake = true;
            m.Stun = 0;
            ++SummonsUsed;
            Push(Msg("Wezwanie: ").Add(D.Enemies[m.DefId].Name).As(LogKind.Bad));
            return true;
        }
        return false;
    }

    public void EnemyAct(int i)
    {
        ref var e = ref Enemies[i];
        var ed = D.Enemies[e.DefId];
        var d = Cheb(e.X, e.Y, Hero.X, Hero.Y);
        if (!e.Awake)
        {
            if (d <= ed.Sight) e.Awake = true;
            else return;
        }
        if (i == Boss && BossWakeDamage < 0) BossEngaged();
        if (e.Stun > 0)
        {
            --e.Stun;
            return;
        }
        if (ed.Slam && i == Boss) // boss: co kilka tur zapowiada uderzenie w obszar wokół bohatera
        {
            if (SlamTimer > 0) return; // ładuje cios, stoi w miejscu
            if (++SlamCounter >= D.SlamEvery && d <= 4)
            {
                var cross = ed.Shape == SlamShape.Cross;
                SlamCounter = 0;
                SlamTimer = cross ? D.SlamCrossDelay : D.SlamDelay;
                SlamX = Hero.X;
                SlamY = Hero.Y;
                Push(Msg(ed.SlamName.Length > 0 ? ed.SlamName : "Cios bossa").Add(" za ").Add(SlamTimer).Add(" tury!").As(LogKind.Bad));
                return;
            }
        }
        if (i == Boss && ed.Summon >= 0 && SummonsUsed < ed.SummonMax && ++SummonCounter >= ed.SummonEvery && d <= 6)
        {
            SummonCounter = 0;
            if (SummonNear(e.X, e.Y)) return; // wezwanie zużywa turę bossa
        }
        // Termin: porusza się co drugą turę, poniżej połowy HP przyspiesza
        if (i == Boss && e.Hp * 2 > e.MaxHp && (Turns & 1) != 0) return;
        var manh = Math.Abs(e.X - Hero.X) + Math.Abs(e.Y - Hero.Y);
        if (manh == 1)
        {
            if (DodgePct() > 0 && R.Range(1, 100) <= DodgePct()) // szczęście: unik
            {
                AddHit(Hero.X, Hero.Y, 0, true, HitKind.Dodge);
                Push(Msg("Unik! ").Add(ed.Name).Add(" chybia").As(LogKind.Good));
                return;
            }
            var dmg = R.Range(ed.MinDamage, ed.MaxDamage) + EnemyDmgBonus() - (CDef.Defense + DefBonus + GearBonus(GearStat.Def)) / 2;
            if (dmg < 1) dmg = 1;
            Hero.Hp = (short)(Hero.Hp - dmg);
            StageDamage += dmg;
            HeroHit = true;
            AddHit(Hero.X, Hero.Y, dmg, true);
            Push(Msg(ed.Name).Add(": -").Add(dmg).Add(" HP").As(LogKind.Bad));
            if (ed.OnHit != StatusEffect.None && Hero.Hp > 0 && R.Range(1, 100) <= ed.StatusChance)
                ApplyStatus(ed.OnHit, ed.StatusTurns);
            if (EventActive(EventEffect.Rain) && Hero.Hp > 0 && R.Range(1, 100) <= D.SiteEvents[StageEvent].Value)
                ApplyStatus(StatusEffect.Slip, 2); // Ulewa w nocy: błoto na placu
            if (Hero.Hp <= 0)
            {
                Hero.Hp = 0;
                Hero.Alive = false;
                St = GameStatus.Dead;
                Push(Msg("Budowa wstrzymana...").As(LogKind.Bad));
            }
            return;
        }
        if (WeatherIs(WeatherEffect.Frost) && i != Boss && Turns % WDef.Value == 0) return; // mróz: problemy stoją
        int dx = Math.Sign(Hero.X - e.X), dy = Math.Sign(Hero.Y - e.Y);
        var xfirst = Math.Abs(Hero.X - e.X) >= Math.Abs(Hero.Y - e.Y);
        Span<int> tries = [xfirst ? dx : 0, xfirst ? 0 : dy, xfirst ? 0 : dx, xfirst ? dy : 0];
        for (var t = 0; t < 2; t++)
        {
            int tx = tries[t * 2], ty = tries[t * 2 + 1];
            if (tx == 0 && ty == 0) continue;
            int nx = e.X + tx, ny = e.Y + ty;
            if (Lv.At(nx, ny) == Tile.Floor && !Occupied(nx, ny))
            {
                e.X = (sbyte)nx;
                e.Y = (sbyte)ny;
                return;
            }
        }
    }

    public void EndTurn()
    {
        ++Turns;
        ref var poison = ref HeroStatus[(int)StatusEffect.Poison];
        if (poison > 0 && St == GameStatus.Playing) // zatrucie: -1 HP na turę, ale nie zabija
        {
            --poison;
            if (Hero.Hp > 1)
            {
                Hero.Hp = (short)(Hero.Hp - 1);
                StageDamage += 1;
                AddHit(Hero.X, Hero.Y, 1, true);
            }
        }
        if (AbilityCd > 0 && --AbilityCd == 0) Push(Msg("Moc gotowa: ").Add(CDef.AbilityName).As(LogKind.Good));
        for (var i = 0; i < WallsCount;)
        {
            if (--Walls[i].Turns <= 0)
            {
                Lv[Walls[i].X, Walls[i].Y] = Tile.Floor;
                Walls[i] = Walls[--WallsCount];
            }
            else
            {
                ++i;
            }
        }
        UpdateFov();
        if (SlamTimer > 0 && --SlamTimer == 0 && Boss >= 0 && Enemies[Boss].Alive) // cios bossa spada
        {
            var bd = D.Enemies[Enemies[Boss].DefId];
            if (SlamCellAt(Hero.X, Hero.Y))
            {
                var dmg = R.Range(bd.MinDamage, bd.MaxDamage) + EnemyDmgBonus() + D.SlamDamageBonus
                          - (CDef.Defense + DefBonus + GearBonus(GearStat.Def)) / 2;
                if (dmg < 1) dmg = 1;
                Hero.Hp = (short)(Hero.Hp - dmg);
                StageDamage += dmg;
                HeroHit = true;
                AddHit(Hero.X, Hero.Y, dmg, true);
                Push(Msg(bd.SlamName.Length > 0 ? bd.SlamName : "Uderzenie").Add(": -").Add(dmg).Add(" HP").As(LogKind.Bad));
                if (Hero.Hp <= 0)
                {
                    Hero.Hp = 0;
                    Hero.Alive = false;
                    St = GameStatus.Dead;
                    Push(Msg("Budowa wstrzymana...").As(LogKind.Bad));
                }
            }
            else
            {
                Push(Msg("Unik! Cios poszedł obok").As(LogKind.Good));
            }
            SlamX = SlamY = -1;
        }
        if (St == GameStatus.Playing)
        {
            for (var i = 0; i < EnemiesCount && St == GameStatus.Playing; ++i)
            {
                if (Enemies[i].Alive) EnemyAct(i);
            }
        }
        if (St == GameStatus.Playing && Hero.X == StairsX && Hero.Y == StairsY)
        {
            St = GameStatus.StageClear;
            Score += 100 * ScorePct() / 100;
            GainXp(D.XpPerStage);
            Push(Msg("Etap zakończony: ").Add(D.Stages[Stage].Name).As(LogKind.Good));
            if (EventActive(EventEffect.Inspection) && StageDamage == 0) // Inspekcja nadzoru: etap bez obrażeń
            {
                GainXp(D.SiteEvents[StageEvent].Value);
                Push(Msg("Inspekcja: +").Add(D.SiteEvents[StageEvent].Value).Add(" dośw.").As(LogKind.Good));
            }
        }
    }
}
