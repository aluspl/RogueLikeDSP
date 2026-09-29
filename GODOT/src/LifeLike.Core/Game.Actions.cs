using LifeLike.Core.Data;

namespace LifeLike.Core;

// Akcje gracza, walka, AI problemów budowy i koniec tury (port core::game z core.h).
public sealed partial class Game
{
    /// <summary>Obrażenia = rzut broni + stat/2 + premie - obrona/2, min 1.</summary>
    public void HeroAttack(int ei)
    {
        if (ei == Boss && BossWakeDamage < 0) BossEngaged(); // walka z bossem trwa
        int ex = Enemies[ei].X, ey = Enemies[ei].Y;
        bool wasWet = EnemyWet(ei), wasDusty = EnemyDusty(ei), wasFrozen = EnemyFrozen(ei);
        var melee = Cheb(Hero.X, Hero.Y, ex, ey) <= 1;
        var w = Weapon; // ulepszenie (#31): +obrażeń, Wyważenie (rzut), Przebicie (OBR)
        var dmg = R.Range(Math.Min(w.MaxDamage, w.MinDamage + ToolTraitValue(ToolTraitEffect.Steady)), w.MaxDamage)
                  + HeroStat(w.ScalesWith) / 2 + DmgBonus + GearBonus(GearStat.Dmg) + BoonSum(BoonEffect.Dmg) + UpgradeDmg() + EventDmg
                  - Math.Max(0, EnemyDefense(ei) - ToolTraitValue(ToolTraitEffect.Pierce)) / 2;
        if (dmg < 1) dmg = 1;
        dmg += Pct.Part(dmg, Bonus.DmgPct + BoonSum(BoonEffect.DmgPct), ref DmgCarry); // Kurs fachowy, Respekt, premie: +%
        var crit = R.Range(1, 100) <= CritPct();
        if (crit) dmg *= D.CritMultiplier;
        DamageEnemy(ei, dmg, crit, Weapon.Name);
        // kombinacje stanów: stan celu sprzed ciosu + żywioł ciosu
        if (St == GameStatus.Playing && HitPower() && wasWet) ComboShock(ei, ex, ey);
        if (St == GameStatus.Playing && HitSpark() && wasDusty) ComboDust(ex, ey);
        if (St == GameStatus.Playing && melee && wasFrozen && Enemies[ei].Alive && EnemyFrozen(ei)) ComboCrack(ei, dmg);
        if (Enemies[ei].Alive && BoonSum(BoonEffect.WetHits) > 0) Enemies[ei].Wet = (sbyte)Math.Max(Enemies[ei].Wet, BoonSum(BoonEffect.WetHits)); // Wąż ogrodowy
        if (Enemies[ei].Alive && BoonSum(BoonEffect.FrostHits) > 0 && ei != Boss) Enemies[ei].Flags = (byte)(Enemies[ei].Flags | ActorFlag.Frozen); // Suchy lód
        // Operator koparki: cios wręcz czasem odpycha problem o pole (bossa nie)
        var e = Enemies[ei];
        if (HasPassive(ClassPassive.Push) && e.Alive && ei != Boss && Cheb(Hero.X, Hero.Y, e.X, e.Y) == 1
            && R.Range(1, 100) <= D.PushChancePct)
        {
            Shove(ei, Math.Sign(e.X - Hero.X), Math.Sign(e.Y - Hero.Y), 1);
        }
    }

    /// <summary>Odepchnięcie problemu o n pól w kierunku (dx, dy), dopóki pole wolne; zwraca, o ile przesunięto.</summary>
    public int Shove(int ei, int dx, int dy, int n)
    {
        var moved = 0;
        for (var k = 0; k < n; ++k)
        {
            int nx = Enemies[ei].X + dx, ny = Enemies[ei].Y + dy;
            if (Lv.At(nx, ny) != Tile.Floor || Occupied(nx, ny)) break;
            Enemies[ei].X = (sbyte)nx;
            Enemies[ei].Y = (sbyte)ny;
            ++moved;
        }
        return moved;
    }

    /// <summary>Obrażenia dla bohatera po obronie: Szkolenie BHP i Respekt zmniejszają je o %, najmniej 1.</summary>
    public int TakenDamage(int dmg)
    {
        if (dmg < 1) dmg = 1;
        dmg -= Pct.Part(dmg, Bonus.TakenPct, ref TakenCarry);
        return dmg < 1 ? 1 : dmg;
    }

    /// <summary>Bohater bez HP: Druga szansa (Respekt) raz na budowę zostawia 1 HP; inaczej koniec budowy.</summary>
    public void HeroDown()
    {
        if (Bonus.SecondChance > 0 && !SecondUsed)
        {
            SecondUsed = true;
            Hero.Hp = 1;
            Push(Msg("Druga szansa! Zostaje 1 HP").As(LogKind.Good));
            return;
        }
        Hero.Hp = 0;
        Hero.Alive = false;
        St = GameStatus.Dead;
        Push(Msg("Budowa wstrzymana...").As(LogKind.Bad));
    }

    /// <summary>
    /// Druga faza bossa (Decyzja odmowna: Odwołanie): raz, gdy HP spadnie do PhasePct% (także ciosem, który by go usunął) –
    /// odzyskuje PhaseHeal% max HP i od razu wzywa PhaseSummon problemów.
    /// </summary>
    public void BossPhase(int ei)
    {
        ref var e = ref Enemies[ei];
        var ed = D.Enemies[e.DefId];
        e.Flags = (byte)(e.Flags | ActorFlag.Phase);
        var heal = e.MaxHp * ed.PhaseHeal / 100;
        e.Hp = (short)Math.Min(e.MaxHp, Math.Max(1, (int)e.Hp) + heal);
        Push(Msg(ed.PhaseName).Add("! ").Add(ed.Name).Add(" +").Add(heal).Add(" HP").As(LogKind.Bad));
        int bx = e.X, by = e.Y;
        for (var k = 0; k < ed.PhaseSummon && SummonsUsed < ed.SummonMax; ++k) SummonNear(bx, by);
    }

    /// <summary>
    /// Obrażenia dla problemu (broń bohatera albo brygada; src = nazwa w dzienniku): trafienie, usunięcie, nagrody,
    /// koniec etapu po bossie.
    /// </summary>
    public void DamageEnemy(int ei, int dmg, bool crit, string src)
    {
        ref var e = ref Enemies[ei];
        var ed = D.Enemies[e.DefId];
        if (ei == Boss && BossWakeDamage < 0) BossEngaged();
        if (dmg > BestHit) // podsumowanie: najmocniejszy cios bohatera
        {
            BestHit = (short)Math.Min(32767, dmg);
            BestHitDef = e.DefId;
            BestHitCrit = crit;
        }
        e.Hp = (short)(e.Hp - dmg);
        e.Awake = true;
        LastTarget = ei;
        if (ei == Boss && ed.PhasePct > 0 && (e.Flags & ActorFlag.Phase) == 0 && e.Hp * 100 <= e.MaxHp * ed.PhasePct) BossPhase(ei);
        AddHit(e.X, e.Y, dmg, false, crit ? HitKind.Crit : HitKind.Normal);
        TurnEvents |= 1u << ei;
        if (e.Hp <= 0 && ei != Boss && (ed.Tags & Behavior.Returns) != 0 && (e.Flags & ActorFlag.Returned) == 0) // wraca raz
        {
            e.Alive = false;
            e.Hp = 0;
            e.Flags = (byte)(e.Flags | ActorFlag.Returned | ActorFlag.Reviving);
            e.Timer = (sbyte)D.BehaviorReturnTurns;
            Push(Msg(ed.Name).Add(" - wróci za ").Add(D.BehaviorReturnTurns).Add(" t.!").As(LogKind.Bad));
            return;
        }
        if (e.Hp <= 0)
        {
            e.Alive = false;
            ++Kills;
            ++StageKills;
            ++ActKills;
            Cash += Income(ed.Score / D.CashPerScore);
            if (KillsByType[e.DefId] < 255) ++KillsByType[e.DefId];
            Score += ed.Score * ScorePct() / 100;
            GainXp(D.XpPerKill);
            if (e.Elite >= 0) // elita: pewny drop, materiały, Respekt
            {
                EliteReward(ei);
                if (ElitesKilled < 255) ++ElitesKilled;
                StageFlags[Stage] |= RecapFlag.Elite;
            }
            MaybeDrop(e.X, e.Y);
            if (ei == KeyHolder) DropKey(e.X, e.Y); // klucz do magazynu (#32)
            Push(Msg(ed.Name).Add(" - usunięto!").As(LogKind.Good));
            var kh = BoonSum(BoonEffect.KillHeal); // Drożdżówka: HP za usunięty problem
            if (kh > 0 && Hero.Alive && Hero.Hp < Hero.MaxHp) Hero.Hp = (short)Math.Min(Hero.MaxHp, Hero.Hp + kh);
            if ((ed.Tags & Behavior.Explodes) != 0 || EliteIs(e, EliteEffect.Explode)) ArmBlast(e.X, e.Y, ed);
            if ((ed.Tags & Behavior.Splits) != 0 && (e.Flags & ActorFlag.Child) == 0) Split(ei);
            if (D.Materials.Length > 0)
            {
                if (ei == Boss) // boss: po kilka sztuk każdego materiału
                {
                    for (var m = 0; m < D.Materials.Length; ++m) AddMaterial(m, D.MaterialBossDrop);
                }
                else if (R.Range(1, 100) <= D.MaterialDropPct * (100 + Bonus.MatsPct + BoonSum(BoonEffect.MatsPct) + WeeklyValue(WeeklyRule.MatsPct)
                                                                  + SynergyValue(SynergyEffect.Stock)) / 100) // Respekt: Zapasy
                {
                    AddMaterial(ed.Material >= 0 ? ed.Material : R.Range(0, D.Materials.Length - 1));
                }
            }
            if (ei == Boss)
            {
                StageFlags[Stage] |= RecapFlag.Boss;
                if (StageDamage == BossWakeDamage && CleanBosses < 255) ++CleanBosses; // zlecenie Czysta robota
                Score += (500 + 100 * Math.Max(0, PatternStage() + 1)) * ScorePct() / 100;
                GainXp(D.XpBoss);
                SlamTimer = 0;
                if (ed.RewardCash > 0) // nagroda bossa (Inspekcja: Protokół bez uwag)
                {
                    Cash += Income(ed.RewardCash);
                    Push(Msg(ed.RewardTitle).Add("! +").Add(Income(ed.RewardCash)).Add(" zł").As(LogKind.Good));
                }
                FinishStage();
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
                    ActBonus = Income(ad.BonusPerStage * stagesInAct + ad.BonusPerKill * ActKills);
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
            Push(Msg(crit ? "KRYT! " : "").Add(src).Add(": -").Add(dmg).Add(" (").Add(ed.Name).Add(")").As(crit ? LogKind.Loot : LogKind.Info));
            if (EliteIs(e, EliteEffect.Summon) && (e.Flags & ActorFlag.Called) == 0 && e.Hp * 100 <= e.MaxHp * D.Elites[e.Elite].Value) EliteCall(ei);
        }
    }

    /// <summary>Ruch albo atak wroga na drodze. Zwraca true, jeśli zużył turę.</summary>
    public bool PlayerMove(int dx, int dy)
    {
        if (St != GameStatus.Playing) return false;
        if (ShockedTurn()) return true;
        int nx = Hero.X + dx, ny = Hero.Y + dy;
        var ei = EnemyAt(nx, ny);
        var stuck = false;
        if (ei >= 0)
        {
            HeroAttack(ei);
        }
        else if (SecretClosed() && SecretIs(nx, ny)) // magazyn (#32)
        {
            if (!TryOpenSecret()) return false;
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
            if (Puddle(Hero.X, Hero.Y)) SoakHero(); // kałuża moczy
            if (Mud(Hero.X, Hero.Y) && !Puddle(Hero.X, Hero.Y)) // akt I: błoto – grzęźniesz, tura przepada
            {
                stuck = true;
                Push(Msg("Błoto! Grzęźniesz - tura stracona").As(LogKind.Bad));
            }
        }
        else
        {
            return false;
        }
        EndTurn();
        if (stuck && St == GameStatus.Playing) EndTurn();
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

    /// <summary>Kawa leczy (Lepszy termos, Respekt: Mocna kawa +%).</summary>
    public int CoffeeHeal() => Pct.DivRound((D.CoffeeHeal + Bonus.Coffee + BoonSum(BoonEffect.Coffee)) * (100 + Bonus.CoffeePct), 100);

    public void DrinkCoffee()
    {
        var h = Math.Min(CoffeeHeal(), Hero.MaxHp - Hero.Hp);
        Hero.Hp = (short)(Hero.Hp + h);
        Push(Msg("Kawa z termosu: +").Add(h).Add(" HP").As(LogKind.Good));
        var es = SynergyValue(SynergyEffect.Espresso); // synergia Espresso: kawa ładuje moc
        if (es > 0 && AbilityCd > 0)
        {
            AbilityCd = Math.Max(0, AbilityCd - es);
            Push(Msg("Espresso: moc -").Add(es).Add(" t.").As(LogKind.Good));
        }
    }

    /// <summary>Picie z termosu (menu akcji): leczy, zużywa turę.</summary>
    public bool PlayerDrink()
    {
        if (St != GameStatus.Playing) return false;
        if (WeeklyHas(WeeklyRule.NoCoffee))
        {
            Push(Msg("Tydzień bez kawy!").As(LogKind.Bad));
            return false;
        }
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
        DropAt(x, y);
    }

    /// <summary>Drop w polu (typ losowany wagami); paczka sprzętu co najmniej jakości minRarity (elita: solidna).</summary>
    public void DropAt(int x, int y, int minRarity = 0)
    {
        var total = 0;
        foreach (var w in D.DropWeights) total += w;
        int roll = R.Range(1, total), type = 0;
        while (roll > D.DropWeights[type]) roll -= D.DropWeights[type++];
        if (type != (int)PickupType.Tool && Bonus.ToolPct > 0 && R.Range(1, 100) <= Bonus.ToolPct) type = (int)PickupType.Tool; // uprawnienie Kolekcjoner
        int arg = 0, trait = 0;
        if (type == (int)PickupType.GearBox) // slot losowy, jakość lepsza na późnych etapach i ze szczęściem
        {
            var q = R.Range(1, 100) + Math.Max(0, PatternStage()) * D.GearStageBonus + D.RarityPerLuck * Luck() + Bonus.GearPct;
            var rarity = Math.Max(minRarity, q >= D.GearBrandFrom ? 2 : (q >= D.GearSolidFrom ? 1 : 0));
            arg = (byte)(RandomSlot() * 3 + rarity);
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
        Thermos = Math.Min(Thermos, ThermosCap()); // słabszy pas: kawy ponad miejsca przepadają
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
            if (p.Type == PickupType.Tool && (WeaponLvl > 0 || WeaponTrait >= 0)) // ulepszone narzędzie: decyzja (ulepszenia przepadną)
            {
                if (!HasToolOffer)
                {
                    ToolOffer = (sbyte)p.Arg;
                    ToolOfferPickup = (sbyte)i;
                    Push(Msg("Narzędzie: ").Add(D.Weapons[D.Tools[p.Arg].Weapon].Name).Add(" - zamienić?").As(LogKind.Loot));
                }
                continue;
            }
            p.Active = false;
            if (p.Type == PickupType.EventTile) // wydarzenie z wyborem (#30): SMS czeka na odpowiedź
            {
                PendingEvent = (sbyte)p.Arg;
                Push(Msg("SMS: ").Add(PendingDef.Name).As(LogKind.Loot));
                continue;
            }
            if (p.Type == PickupType.StoreKey)
            {
                ++Keys;
                Push(Msg("Klucz do magazynu!").As(LogKind.Loot));
                continue;
            }
            if (p.Type == PickupType.Chest)
            {
                OpenChest();
                continue;
            }
            if (p.Type == PickupType.Coffee && WeeklyHas(WeeklyRule.NoCoffee)) // wyzwanie: bez kawy – kawa na wynos (zł)
            {
                Cash += Income(D.WeeklyCoffeeCash);
                Push(Msg("Bez kawy: na wynos +").Add(Income(D.WeeklyCoffeeCash)).Add(" zł").As(LogKind.Loot));
            }
            else if (p.Type == PickupType.Coffee)
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
            else if (p.Type == PickupType.Document) // pieczątki: komplet otwiera schody
            {
                Docs = (byte)(Docs | (1 << p.Arg));
                Push(Msg("Dokument: ").Add(D.Documents[p.Arg]).Add(" (").Add(DocsCount()).Add("/").Add(DocsNeeded()).Add(")").As(LogKind.Loot));
                if (!StairsLocked()) Push(Msg("Komplet pieczątek! Schody otwarte").As(LogKind.Good));
            }
            else if (p.Type == PickupType.GearBox)
            {
                if (D.Materials.Length > 0) AddMaterial(R.Range(0, D.Materials.Length - 1), D.MaterialGearBox); // w paczce też materiał
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
                TakeTool(p.Arg);
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
        if (EliteIs(e, EliteEffect.Regen) && e.Hp < e.MaxHp) e.Hp = (short)Math.Min(e.MaxHp, e.Hp + D.Elites[e.Elite].Value); // elita Uparta
        if (e.Stun > 0)
        {
            --e.Stun;
            return;
        }
        if (ed.Slam && i == Boss) // boss: co kilka tur zapowiada uderzenie w obszar wokół bohatera
        {
            if (SlamTimer > 0) return; // ładuje cios, stoi w miejscu
            if (++SlamCounter >= SlamEvery() && d <= 4)
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
        var tg = ed.Tags;
        if ((tg & Behavior.Grows) != 0) GrowTick(i);
        if (e.Timer > 0) --e.Timer; // odnowienie ucieczki / łatania
        var manh = Math.Abs(e.X - Hero.X) + Math.Abs(e.Y - Hero.Y);
        if ((tg & Behavior.Heals) != 0 && manh != 1 && e.Timer == 0 && HealNear(i))
        {
            e.Timer = (sbyte)D.BehaviorHealEvery;
            return;
        }
        if ((tg & Behavior.Flees) != 0 && manh == 1 && e.Timer == 0 && FleeStep(i))
        {
            e.Timer = (sbyte)D.BehaviorFleeCooldown;
            return;
        }
        if (manh == 1)
        {
            EnemyStrike(i, false);
            return;
        }
        if ((tg & Behavior.Ranged) != 0 && ShotLine(e.X, e.Y))
        {
            ShotEvents |= 1u << i;
            EnemyStrike(i, true);
            return;
        }
        if ((tg & Behavior.Stationary) != 0) return;
        if (WeatherIs(WeatherEffect.Frost) && i != Boss && Turns % WDef.Value == 0) return; // mróz: problemy stoją
        if ((tg & Behavior.Ranged) != 0 && RangedStep(i)) return;
        // elita Szybka: drugi krok, jeśli jeszcze nie stoi obok bohatera
        if (ChaseStep(i) && EliteIs(Enemies[i], EliteEffect.Fast) && Math.Abs(Enemies[i].X - Hero.X) + Math.Abs(Enemies[i].Y - Hero.Y) != 1) ChaseStep(i);
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
        ref var wet = ref HeroStatus[(int)StatusEffect.Wet];
        if (wet > 0) --wet; // mokry schnie
        for (var i = 0; i < EnemiesCount; ++i) // problemy: kałuża moczy, poza nią schną
        {
            ref var e = ref Enemies[i];
            if (!e.Alive) continue;
            if (Puddle(e.X, e.Y)) e.Wet = (sbyte)D.WetTurns;
            else if (e.Wet > 0) --e.Wet;
        }
        if (GuardTurns > 0) --GuardTurns; // ochrona BHP-owca mija
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
        if (AllyTurns > 0 && St == GameStatus.Playing) AllyAct(); // pomocnik z brygady
        if (SlamTimer > 0 && --SlamTimer == 0 && Boss >= 0 && Enemies[Boss].Alive) // cios bossa spada
        {
            var bd = D.Enemies[Enemies[Boss].DefId];
            if (SlamCellAt(Hero.X, Hero.Y))
            {
                var dmg = TakenDamage(R.Range(bd.MinDamage, bd.MaxDamage) + EnemyDmgBonus() + D.SlamDamageBonus - HeroDefense() / 2);
                Hero.Hp = (short)(Hero.Hp - dmg);
                StageDamage += dmg;
                HeroHit = true;
                LogHit(Enemies[Boss].DefId, Enemies[Boss].Elite, RecapKind.Slam, dmg);
                AddHit(Hero.X, Hero.Y, dmg, true);
                Push(Msg(bd.SlamName.Length > 0 ? bd.SlamName : "Uderzenie").Add(": -").Add(dmg).Add(" HP").As(LogKind.Bad));
                if (Hero.Hp <= 0) HeroDown();
            }
            else
            {
                Push(Msg("Unik! Cios poszedł obok").As(LogKind.Good));
            }
            SlamX = SlamY = -1;
        }
        if (BlastTimer > 0 && --BlastTimer == 0 && St == GameStatus.Playing) // wybuch po usuniętym problemie
        {
            if (Cheb(Hero.X, Hero.Y, BlastX, BlastY) <= D.BehaviorBlastRadius)
            {
                var dmg = TakenDamage(BlastDmg - HeroDefense() / 2);
                var dusty = DustSight() > 0; // pył + iskra (wybuch) na bohaterze
                if (dusty) dmg += D.Combos[(int)ComboEffect.DustBlast].HeroValue;
                Hero.Hp = (short)(Hero.Hp - dmg);
                StageDamage += dmg;
                HeroHit = true;
                LogHit(BlastSrc, -1, dusty ? RecapKind.Dust : RecapKind.Blast, dmg);
                AddHit(Hero.X, Hero.Y, dmg, true);
                if (dusty)
                {
                    ComboEvents = (byte)(ComboEvents | (8 << (int)ComboEffect.DustBlast));
                    Push(Msg(D.Combos[(int)ComboEffect.DustBlast].Short).Add(" Wybuch: -").Add(dmg).Add(" HP").As(LogKind.Bad));
                }
                else
                {
                    Push(Msg("Wybuch: -").Add(dmg).Add(" HP").As(LogKind.Bad));
                }
                if (Hero.Hp <= 0) HeroDown();
            }
            else
            {
                Push(Msg("Wybuch obok - uff!").As(LogKind.Good));
            }
            BlastSecret(BlastX, BlastY, D.BehaviorBlastRadius); // wybuch kruszy pękniętą ścianę magazynu
            BlastX = BlastY = -1;
        }
        if (St == GameStatus.Playing && ActIs(ActMechanic.Gust)) GustTick(); // akt II: porywy wiatru
        for (var i = 0; i < EnemiesCount && St == GameStatus.Playing; ++i) // „wraca raz”: powrót po kilku turach
        {
            ref var e = ref Enemies[i];
            if (e.Alive || (e.Flags & ActorFlag.Reviving) == 0 || --e.Timer > 0) continue;
            if (Occupied(e.X, e.Y))
            {
                e.Timer = 1;
                continue;
            }
            e.Alive = true;
            e.Awake = true;
            e.Hp = (short)Math.Max(1, e.MaxHp * D.BehaviorReturnHpPct / 100);
            e.Flags = (byte)(e.Flags & ~ActorFlag.Reviving);
            Push(Msg(D.Enemies[e.DefId].Name).Add(" wraca!").As(LogKind.Bad));
        }
        if (St == GameStatus.Playing)
        {
            for (var i = 0; i < EnemiesCount && St == GameStatus.Playing; ++i)
            {
                if (Enemies[i].Alive) EnemyAct(i);
            }
        }
        if (St == GameStatus.Playing && Hero.X == StairsX && Hero.Y == StairsY && StairsLocked())
        {
            Push(Msg("Schody zamknięte: dokumenty ").Add(DocsCount()).Add("/").Add(DocsNeeded()).As(LogKind.Bad));
        }
        else if (St == GameStatus.Playing && Hero.X == StairsX && Hero.Y == StairsY)
        {
            ClearStage();
        }
    }

    /// <summary>Wejście na otwarte schody: etap zaliczony (Respekt, oferta premii, wynik, doświadczenie, Inspekcja nadzoru).</summary>
    public void ClearStage()
    {
        {
            St = GameStatus.StageClear;
            FinishStage();
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
