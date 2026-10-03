using LifeLike.Core.Data;

namespace LifeLike.Core;

// v0.21.50 cz. 3: wydarzenia z wyborem, ulepszanie narzędzia, ukryte pomieszczenia – port 1:1 z core.h.
public sealed partial class Game
{
    /// <summary>Oferta premii z wydarzenia (inna niż po etapie).</summary>
    public byte BoonSalt;
    /// <summary>Wydarzenie czeka na odpowiedź (GameData.ChoiceEvents), -1 = brak.</summary>
    public sbyte PendingEvent = -1;
    /// <summary>Wydarzenie etapu i wybrana odpowiedź (telefon: Zadania).</summary>
    public sbyte StageChoice = -1, StageChoicePick = -1;
    /// <summary>Bity skutków ostatniej odpowiedzi, które zaszły (szansa).</summary>
    public byte ChoiceDone;
    /// <summary>Wydarzenia już wylosowane w tej budowie (bez powtórek).</summary>
    public ushort EventsSeen;
    /// <summary>Z wydarzenia: ciosy / OBR do końca etapu.</summary>
    public sbyte EventDmg, EventDef;
    /// <summary>Ulepszenie narzędzia (+obrażeń za poziom), przepada przy zmianie narzędzia.</summary>
    public sbyte WeaponLvl;
    /// <summary>Cecha ulepszenia (GameData.ToolTraits), -1 = brak.</summary>
    public sbyte WeaponTrait = -1;
    /// <summary>Poziom z cechą: czeka na wybór cechy.</summary>
    public bool TraitPending;
    /// <summary>Narzędzie na polu czeka na decyzję (ulepszenia by przepadły): indeks narzędzia i znajdźki.</summary>
    public sbyte ToolOffer = -1, ToolOfferPickup = -1;
    /// <summary>Ukryte pomieszczenie: pole pękniętej ściany / drzwi (-1 = brak na etapie).</summary>
    public sbyte SecretX = -1, SecretY = -1;
    /// <summary>Rodzaj (GameData.SecretKinds) i kierunek od ściany do wnętrza (GustVec).</summary>
    public sbyte SecretKind, SecretDir;
    /// <summary>Wnętrze magazynu.</summary>
    public sbyte SecretRx, SecretRy, SecretRw, SecretRh;
    public bool SecretOpen;
    /// <summary>Problem z kluczem do magazynu (indeks w Enemies), -1 = brak.</summary>
    public sbyte KeyHolder = -1;
    /// <summary>Klucze do magazynu (na etap).</summary>
    public byte Keys;
    /// <summary>Otwarte magazyny w budowie.</summary>
    public byte SecretsFound;

    // ------------------------------------------------------------------ wspólny generator etapu
    /// <summary>Osobny generator z seeda budowy, etapu i soli (wydarzenia, magazyn) – bez wpływu na RNG gry.</summary>
    public Rng SideRng(uint salt)
    {
        var s = new Rng();
        s.Seed((RunSeed ^ ((uint)(Stage + 1 + Tier * 16) * 2654435761u) ^ (salt * 40503u)) * 2246822519u);
        return s;
    }

    /// <summary>Wolne pole podłogi w losowym pokoju (bez pierwszego): bez postaci, znajdziek i schodów.</summary>
    public bool SideFreeCell(ref Rng sr, out int ox, out int oy)
    {
        for (var t = 0; t < 60; ++t)
        {
            var rm = Lv.Rooms[sr.Range(1, Lv.RoomsCount - 1)];
            int x = sr.Range(rm.X, rm.X + rm.W - 1), y = sr.Range(rm.Y, rm.Y + rm.H - 1);
            if (Lv.At(x, y) == Tile.Floor && !Occupied(x, y) && !PickupAt(x, y))
            {
                ox = x;
                oy = y;
                return true;
            }
        }
        ox = oy = 0;
        return false;
    }

    // ------------------------------------------------------------------ wydarzenia z wyborem
    /// <summary>0–1 pole wydarzenia na etapie (nie pierwszym budowy i nie z bossem); wydarzenie bez powtórek w budowie.</summary>
    public void PlaceEvent()
    {
        if (D.ChoiceEvents.Length == 0 || SDef().Boss >= 0 || Stage == FirstStage || Lv.RoomsCount < 2 || PickupsCount >= MaxPickups) return;
        var er = SideRng(101);
        if (er.Range(1, 100) > D.ChoiceEventChancePct) return;
        var n = 0;
        for (var e = 0; e < D.ChoiceEvents.Length; ++e)
        {
            if (((EventsSeen >> e) & 1) == 0) n++;
        }
        if (n == 0) return;
        int k = er.Range(0, n - 1), ev = 0;
        for (var e = 0; e < D.ChoiceEvents.Length; ++e)
        {
            if (((EventsSeen >> e) & 1) == 0 && k-- == 0)
            {
                ev = e;
                break;
            }
        }
        if (!SideFreeCell(ref er, out var x, out var y)) return;
        EventsSeen = (ushort)(EventsSeen | (1 << ev));
        Pickups[PickupsCount++] = new Pickup(x, y, PickupType.EventTile, true, ev);
    }

    public ChoiceEventDef PendingDef => D.ChoiceEvents[PendingEvent];

    /// <summary>
    /// Odpowiedź k na wydarzenie: skutki po kolei (z szansą – osobny generator); premia 1 z 3 – oferta czeka na wybór,
    /// ulepszenie z cechą – wybór cechy.
    /// </summary>
    public bool ChooseEvent(int k)
    {
        if (PendingEvent < 0) return false;
        var ev = PendingDef;
        if (k < 0 || k >= ev.Choices.Length) return false;
        var c = ev.Choices[k];
        var er = SideRng(111 + (uint)k);
        StageEventLog[Stage] = (byte)(PendingEvent * 4 + k); // podsumowanie: wydarzenie i odpowiedź
        StageChoice = PendingEvent;
        StageChoicePick = (sbyte)k;
        PendingEvent = -1;
        ChoiceDone = 0;
        Push(Msg("Odpowiedź: ").Add(c.Label));
        for (var i = 0; i < c.Outs.Length; ++i)
        {
            var o = c.Outs[i];
            if (o.Chance < 100 && er.Range(1, 100) > o.Chance) continue;
            ChoiceDone = (byte)(ChoiceDone | (1 << i));
            ApplyChoice(o);
        }
        if (c.Result.Length > 0) Push(Msg(c.Result).As(LogKind.Good));
        return true;
    }

    public void ApplyChoice(ChoiceOut o)
    {
        var v = o.Value;
        switch (o.Effect)
        {
            case ChoiceEffect.Cash:
                Cash = Math.Max(0, Cash + (v > 0 ? Income(v) : v));
                Push(Msg(v > 0 ? "Budżet +" : "Budżet ").Add(v > 0 ? Income(v) : v).Add(" zł").As(v > 0 ? LogKind.Good : LogKind.Bad));
                break;
            case ChoiceEffect.Xp:
                GainXp(v);
                Push(Msg("+").Add(v).Add(" dośw.").As(LogKind.Good));
                break;
            case ChoiceEffect.Hp:
                Hero.Hp = (short)(v > 0 ? Math.Min(Hero.MaxHp, Hero.Hp + v) : Math.Max(1, Hero.Hp + v));
                Push(Msg(v > 0 ? "+" : "").Add(v).Add(" HP").As(v > 0 ? LogKind.Good : LogKind.Bad));
                break;
            case ChoiceEffect.MaxHp:
                Hero.MaxHp = (short)Math.Max(1, Hero.MaxHp + v);
                Hero.Hp = (short)Math.Min(Hero.MaxHp, Math.Max(1, Hero.Hp + v));
                break;
            case ChoiceEffect.Mats:
                for (var m = 0; m < D.Materials.Length; ++m)
                {
                    if (o.Arg < 0 || o.Arg == m) AddMaterial(m, v);
                }
                break;
            case ChoiceEffect.StageDmg: EventDmg = (sbyte)(EventDmg + v); break;
            case ChoiceEffect.StageDef: EventDef = (sbyte)(EventDef + v); break;
            case ChoiceEffect.Boon: RollBoons(50); break;
            case ChoiceEffect.Gear:
            {
                var slot = o.Arg >= 0 ? o.Arg : RandomSlot();
                TakeGear(slot, Math.Max(v, 0), R.Range(0, D.GearTraitsCount - 1));
                break;
            }
            case ChoiceEffect.Respect:
                Respect += v;
                Push(Msg("Respekt +").Add(v).As(LogKind.Loot));
                break;
            case ChoiceEffect.Coffee:
                Thermos = Math.Min(ThermosCap(), Thermos + v);
                Push(Msg("Kawa do termosu (").Add(Thermos).Add("/").Add(ThermosCap()).Add(")").As(LogKind.Good));
                break;
            case ChoiceEffect.Spawn:
                for (var k = 0; k < v; ++k)
                {
                    if (!FreeAround(Hero.X, Hero.Y, Hero.X, Hero.Y, out var x, out var y)) break;
                    var slot = FreeSlot();
                    if (slot < 0) break;
                    ref var a = ref Enemies[slot];
                    a = new Actor
                    {
                        X = (sbyte)x,
                        Y = (sbyte)y,
                        DefId = (sbyte)o.Arg,
                    };
                    a.Hp = a.MaxHp = (short)Math.Max(1, D.Enemies[o.Arg].MaxHealth * EnemyHpPct() / 100);
                    a.Alive = true;
                    a.Awake = true;
                    a.Stun = 1;
                    if (DustSight() > 0) a.Flags = (byte)(a.Flags | ActorFlag.Dusty);
                    Push(Msg(D.Enemies[o.Arg].Name).Add(" wyłazi!").As(LogKind.Bad));
                }
                break;
            case ChoiceEffect.Status: ApplyStatus((StatusEffect)o.Arg, v); break;
            case ChoiceEffect.Upgrade:
                for (var k = 0; k < v && CanUpgradeWeapon(); ++k) UpgradeWeapon();
                break;
            case ChoiceEffect.Power:
                AbilityCd = 0;
                Push(Msg("Moc gotowa: ").Add(PDef.AbilityName).As(LogKind.Good));
                break;
        }
    }

    /// <summary>Wybór bota (testy balansu, test złoty): prosta ocena skutków (wartość x szansa), remis – pierwsza odpowiedź.</summary>
    public int BotEventChoice()
    {
        var ev = PendingDef;
        int best = 0, bv = -1000000;
        for (var k = 0; k < ev.Choices.Length; ++k)
        {
            var v = 0;
            foreach (var o in ev.Choices[k].Outs)
            {
                var w = o.Effect switch
                {
                    ChoiceEffect.Cash => o.Value,
                    ChoiceEffect.Xp => o.Value,
                    ChoiceEffect.Hp => o.Value * (Hero.Hp * 2 < Hero.MaxHp ? 6 : 3),
                    ChoiceEffect.MaxHp => o.Value * 5,
                    ChoiceEffect.Mats => o.Value * (o.Arg < 0 ? 9 : 3),
                    ChoiceEffect.StageDmg => o.Value * 12,
                    ChoiceEffect.StageDef => o.Value * 10,
                    ChoiceEffect.Boon => 25,
                    ChoiceEffect.Gear => 15,
                    ChoiceEffect.Respect => o.Value * 6,
                    ChoiceEffect.Coffee => o.Value * 8,
                    ChoiceEffect.Spawn => -10 * o.Value,
                    ChoiceEffect.Status => -8,
                    ChoiceEffect.Upgrade => CanUpgradeWeapon() ? 20 : 0,
                    ChoiceEffect.Power => 4,
                    _ => 0,
                };
                v += w * o.Chance;
            }
            if (v > bv)
            {
                bv = v;
                best = k;
            }
        }
        return best;
    }

    // ------------------------------------------------------------------ ulepszanie narzędzia
    public bool CanUpgradeWeapon() => WeaponLvl < D.ToolUpgradeMax && !TraitPending;

    /// <summary>Cena kolejnego poziomu w zł (Rabat z Respektu i premie jak w Hurtowni).</summary>
    public int UpgradePrice() =>
        D.ToolLevels.Length == 0 ? 0
        : D.ToolLevels[Math.Min(WeaponLvl, D.ToolUpgradeMax - 1)].Cash * (100 - Math.Min(90, Bonus.ShopPct + BoonSum(BoonEffect.ShopPct))) / 100;

    /// <summary>Czy stać na kolejny poziom (zł + materiał).</summary>
    public bool UpgradeAffordable()
    {
        if (!CanUpgradeWeapon()) return false;
        var t = D.ToolLevels[WeaponLvl];
        return Cash >= UpgradePrice() && Mats[t.Material] >= t.Count;
    }

    /// <summary>+1 poziom (za darmo – płaci Hurtownia albo wydarzenie); na poziomie ToolTraitAt czeka wybór cechy.</summary>
    public void UpgradeWeapon()
    {
        if (WeaponLvl >= D.ToolUpgradeMax) return;
        ++WeaponLvl;
        StageFlags[Stage] |= RecapFlag.Upgrade;
        if (WeaponLvl >= D.ToolTraitAt && WeaponTrait < 0) TraitPending = true;
        Push(Msg("Ulepszenie: ").Add(Weapon.Name).Add("+").Add(WeaponLvl).As(LogKind.Loot));
    }

    public bool ChooseTrait(int t)
    {
        if (!TraitPending || t < 0 || t >= D.ToolTraits.Length) return false;
        TraitPending = false;
        WeaponTrait = (sbyte)t;
        Push(Msg("Cecha narzędzia: ").Add(D.ToolTraits[t].Name).As(LogKind.Loot));
        return true;
    }

    /// <summary>Bot: zawsze pierwsza cecha (Przebicie).</summary>
    public int BotTraitChoice() => 0;

    public int UpgradeDmg() => WeaponLvl * D.ToolUpgradeDmg;

    public int ToolTraitValue(ToolTraitEffect e) =>
        WeaponTrait >= 0 && D.ToolTraits[WeaponTrait].Effect == e ? D.ToolTraits[WeaponTrait].Value : 0;

    public void ResetUpgrade()
    {
        WeaponLvl = 0;
        WeaponTrait = -1;
        TraitPending = false;
    }

    /// <summary>Nazwa narzędzia z poziomem ulepszenia („Kielnia+2”).</summary>
    public Message WeaponTitle(Message m)
    {
        m.Add(Weapon.Name);
        if (WeaponLvl > 0) m.Add("+").Add(WeaponLvl);
        return m;
    }

    public string WeaponTitle() => WeaponTitle(new Message()).Text;

    /// <summary>Narzędzie na polu przy ulepszonym: decyzja gracza (ulepszenia przepadną). Nie zużywa tury.</summary>
    public bool HasToolOffer => ToolOffer >= 0;

    public void AcceptTool()
    {
        if (!HasToolOffer) return;
        Pickups[ToolOfferPickup].Active = false;
        int t = ToolOffer;
        ToolOffer = ToolOfferPickup = -1;
        TakeTool(t);
    }

    public void DeclineTool()
    {
        if (!HasToolOffer) return;
        ToolOffer = ToolOfferPickup = -1;
        Push(Msg("Zostajesz przy ulepszonym narzędziu"));
    }

    public void TakeTool(int t)
    {
        ResetUpgrade();
        WeaponOverride = D.Tools[t].Weapon;
        ToolsFound = (byte)(ToolsFound | (1u << t));
        Push(Msg("Narzędzie: ").Add(Weapon.Name).Add(" ").Add(Weapon.MinDamage).Add("-").Add(Weapon.MaxDamage).As(LogKind.Loot));
    }

    /// <summary>Bot bierze nowe narzędzie, gdy średni cios (bez problemu) jest wyższy niż ulepszonym obecnym.</summary>
    public bool BotToolAccept() => WeaponBreakdown(-1, D.Tools[ToolOffer].Weapon).Avg10 > WeaponBreakdown().Avg10;

    // ------------------------------------------------------------------ ukryte pomieszczenia
    /// <summary>
    /// Magazyn 3x3 za ścianą przy krawędzi pokoju: ściana E graniczy z podłogą pokoju, wnętrze i jego obrys to same mury
    /// (poza E). Klucz ma problem (najpierw elita), czasem w środku śpi elita-strażnik; na środku skrzynia.
    /// </summary>
    public void PlaceSecret()
    {
        if (D.SecretKinds.Length == 0 || SDef().Boss >= 0 || Lv.RoomsCount < 2) return;
        var sr = SideRng(202);
        if (sr.Range(1, 100) > D.SecretChancePct) return;
        for (var a = 0; a < 80 && SecretX < 0; ++a)
        {
            var rm = Lv.Rooms[sr.Range(0, Lv.RoomsCount - 1)];
            var d = sr.Range(0, 3);
            int dx = GustVec[d, 0], dy = GustVec[d, 1];
            int fx, fy;
            if (dx != 0)
            {
                fx = dx > 0 ? rm.X + rm.W - 1 : rm.X;
                fy = sr.Range(rm.Y, rm.Y + rm.H - 1);
            }
            else
            {
                fy = dy > 0 ? rm.Y + rm.H - 1 : rm.Y;
                fx = sr.Range(rm.X, rm.X + rm.W - 1);
            }
            int ex = fx + dx, ey = fy + dy;
            int ix = dx != 0 ? (dx > 0 ? ex + 1 : ex - 3) : ex - 1, iy = dy != 0 ? (dy > 0 ? ey + 1 : ey - 3) : ey - 1;
            if (Lv.At(fx, fy) != Tile.Floor || ix - 1 < 0 || iy - 1 < 0 || ix + 3 >= Level.W || iy + 3 >= Level.H) continue;
            var ok = true;
            for (var y = iy - 1; y <= iy + 3 && ok; ++y)
            {
                for (var x = ix - 1; x <= ix + 3; ++x)
                {
                    if (Lv[x, y] != Tile.Wall)
                    {
                        ok = false;
                        break;
                    }
                }
            }
            if (!ok) continue;
            for (var y = iy; y < iy + 3; ++y)
            {
                for (var x = ix; x < ix + 3; ++x) Lv[x, y] = Tile.Floor;
            }
            SecretX = (sbyte)ex;
            SecretY = (sbyte)ey;
            SecretDir = (sbyte)d;
            SecretRx = (sbyte)ix;
            SecretRy = (sbyte)iy;
            SecretRw = 3;
            SecretRh = 3;
            SecretKind = (sbyte)sr.Range(0, D.SecretKinds.Length - 1);
        }
        if (SecretX < 0) return;
        int cx = SecretRx + 1, cy = SecretRy + 1;
        if (PickupsCount < MaxPickups) Pickups[PickupsCount++] = new Pickup(cx, cy, PickupType.Chest, true);
        var sd = SDef();
        if (sr.Range(1, 100) <= D.SecretGuardPct && EnemiesCount < MaxEnemies) // strażnik: elita, śpi w kącie
        {
            var gx = SecretRx + 2 * sr.Range(0, 1);
            var gy = SecretRy + 2 * sr.Range(0, 1);
            Spawn(sd.Pool[sr.Range(0, sd.Pool.Length - 1)], gx, gy);
            MakeElite(EnemiesCount - 1, sr.Range(0, D.Elites.Length - 1));
        }
        // klucz: pierwsza elita poza magazynem, inaczej losowy problem (bez bossa i strażnika)
        var outside = 0;
        for (var i = 0; i < EnemiesCount; ++i)
        {
            if (!InSecret(Enemies[i].X, Enemies[i].Y)) outside++;
        }
        for (var i = 0; i < EnemiesCount && KeyHolder < 0; ++i)
        {
            if (Enemies[i].Elite >= 0 && !InSecret(Enemies[i].X, Enemies[i].Y)) KeyHolder = (sbyte)i;
        }
        if (KeyHolder < 0 && outside > 0)
        {
            var k = sr.Range(0, outside - 1);
            for (var i = 0; i < EnemiesCount; ++i)
            {
                if (!InSecret(Enemies[i].X, Enemies[i].Y) && k-- == 0)
                {
                    KeyHolder = (sbyte)i;
                    break;
                }
            }
        }
    }

    public bool HasSecret => SecretX >= 0;

    public bool SecretClosed() => SecretX >= 0 && !SecretOpen;

    public bool SecretIs(int x, int y) => SecretX >= 0 && x == SecretX && y == SecretY;

    public bool InSecret(int x, int y) =>
        SecretX >= 0 && x >= SecretRx && x < SecretRx + SecretRw && y >= SecretRy && y < SecretRy + SecretRh;

    public SecretKindDef SecretDef => D.SecretKinds[SecretKind];

    /// <summary>Pole przed ścianą magazynu (od strony pokoju) – tam trzeba stanąć, żeby otworzyć.</summary>
    public int SecretFrontX() => SecretX - GustVec[SecretDir, 0];

    public int SecretFrontY() => SecretY - GustVec[SecretDir, 1];

    public bool CanOpenSecret() => SecretClosed() && (Keys > 0 || (SecretDef.Breakable && HasPassive(ClassPassive.Push)));

    public void OpenSecret(string how)
    {
        if (!SecretClosed()) return;
        SecretOpen = true;
        Lv[SecretX, SecretY] = Tile.Floor;
        if (SecretsFound < 255) ++SecretsFound;
        StageFlags[Stage] |= RecapFlag.Secret;
        Push(Msg(how).Add(" Magazyn otwarty!").As(LogKind.Good));
        UpdateFov();
    }

    /// <summary>Wejście w ścianę magazynu: klucz, łyżka Operatora koparki (pęknięta ściana); inaczej podpowiedź, bez tury.</summary>
    public bool TryOpenSecret()
    {
        if (Keys > 0)
        {
            --Keys;
            OpenSecret("Klucz pasuje!");
            return true;
        }
        if (SecretDef.Breakable && HasPassive(ClassPassive.Push))
        {
            OpenSecret("Łyżka kruszy ścianę!");
            return true;
        }
        Push(Msg(SecretDef.Name).Add(": ").Add(SecretDef.Info));
        return false;
    }

    /// <summary>Wybuch w promieniu rad od (x, y) kruszy pękniętą ścianę.</summary>
    public void BlastSecret(int x, int y, int rad)
    {
        if (SecretClosed() && SecretDef.Breakable && Cheb(x, y, SecretX, SecretY) <= rad) OpenSecret("Wybuch kruszy ścianę!");
    }

    /// <summary>Klucz z problemu: na polu usunięcia (albo obok), bez miejsca na znajdźkę – od razu do kieszeni.</summary>
    public void DropKey(int x, int y)
    {
        KeyHolder = -1;
        int kx = x, ky = y;
        if ((PickupAt(x, y) && !FreeAround(x, y, Hero.X, Hero.Y, out kx, out ky)) || PickupsCount >= MaxPickups)
        {
            ++Keys;
            Push(Msg("Klucz do magazynu!").As(LogKind.Loot));
            return;
        }
        Pickups[PickupsCount++] = new Pickup(kx, ky, PickupType.StoreKey, true);
        Push(Msg("Wypadł klucz do magazynu!").As(LogKind.Loot));
    }

    public void OpenChest()
    {
        Respect += D.ChestRespect;
        Cash += Income(D.ChestCash);
        for (var m = 0; m < D.Materials.Length; ++m) AddMaterial(m, D.ChestMats);
        Push(Msg("Skrzynia! Respekt +").Add(D.ChestRespect).Add(", +").Add(Income(D.ChestCash)).Add(" zł").As(LogKind.Loot));
        TakeGear(RandomSlot(), D.ChestGearMin, R.Range(0, D.GearTraitsCount - 1));
    }

    /// <summary>Cel bota zamiast schodów: klucz, pole przed magazynem (gdy da się otworzyć), skrzynia, wydarzenie.</summary>
    public bool BotGoal(out int gx, out int gy)
    {
        for (var i = 0; i < PickupsCount; ++i)
        {
            if (Pickups[i].Active && Pickups[i].Type == PickupType.StoreKey)
            {
                gx = Pickups[i].X;
                gy = Pickups[i].Y;
                return true;
            }
        }
        if (CanOpenSecret())
        {
            gx = SecretFrontX();
            gy = SecretFrontY();
            return true;
        }
        for (var i = 0; i < PickupsCount; ++i)
        {
            var p = Pickups[i];
            if (p.Active && (p.Type == PickupType.Chest ? SecretOpen : p.Type == PickupType.EventTile))
            {
                gx = p.X;
                gy = p.Y;
                return true;
            }
        }
        gx = gy = -1;
        return false;
    }

    /// <summary>Bot: rozstrzyga oczekujące decyzje (wydarzenie, premia z wydarzenia, cecha narzędzia, narzędzie). true = coś zrobił.</summary>
    public bool BotPending()
    {
        if (PendingEvent >= 0)
        {
            ChooseEvent(BotEventChoice());
            return true;
        }
        if (HasBoonOffer && St == GameStatus.Playing)
        {
            PickBoon(BotBoonChoice());
            return true;
        }
        if (TraitPending)
        {
            ChooseTrait(BotTraitChoice());
            return true;
        }
        if (HasToolOffer)
        {
            if (BotToolAccept()) AcceptTool();
            else DeclineTool();
            return true;
        }
        return false;
    }

    /// <summary>Bot w Hurtowni (po bossie aktu): ulepsza narzędzie o 1 poziom, jeśli stać (cecha – pierwsza).</summary>
    public void BotUpgrade()
    {
        if (!ActCleared || ShopClosed) return;
        for (var i = 0; i < D.Hurtownia.Length; ++i)
        {
            if (D.Hurtownia[i].Effect == ShopEffect.Upgrade && HurtowniaBuy(i)) BotPending();
        }
    }

    /// <summary>Sprzęt z wydarzenia / skrzyni: pusty slot – zakłada, zajęty – porównanie (jak paczka).</summary>
    public void TakeGear(int slot, int rarity, int trait)
    {
        if (Equipped[slot] < 0)
        {
            Equip(slot, rarity, trait);
        }
        else if (!HasOffer)
        {
            OfferSlot = (sbyte)slot;
            OfferRarity = (sbyte)rarity;
            OfferTrait = (sbyte)trait;
            Push(Msg("Paczka: ").Add(D.Gear[slot * 3 + rarity].Name).As(LogKind.Loot));
        }
    }
}
