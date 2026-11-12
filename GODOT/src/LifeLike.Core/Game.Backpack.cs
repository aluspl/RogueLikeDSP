namespace LifeLike.Core;

/// <summary>
/// v0.23.2: plecak – 6 miejsc na bronie i sprzęt z mapy (na jedną budowę). Zbieranie z karty paczki / broni („Do plecaka”),
/// zamiana z noszonym (1 tura, jak kabura; w Hurtowni bez tury), wyrzucenie na ziemię, po bossie aktu Skup (materiały)
/// i PSZOK (dośw. + HP). Port 1:1 w GBA/include/core.h (bp_*).
/// </summary>
public sealed partial class Game
{
    /// <summary>Miejsca w tablicy plecaka (dane: GameData.BackpackSlots ≤ MaxBackpack).</summary>
    public const int MaxBackpack = 6;

    /// <summary>Pickup.Extra paczki sprzętu odłożonej przez gracza: bez materiału i bez dośw. za „Zostaw” przy kolejnym wejściu.</summary>
    public const ushort GearBoxUsed = 1;

    public readonly BackpackItem[] Backpack = new BackpackItem[MaxBackpack];

    /// <summary>Znajdźka paczki sprzętu, z której pochodzi oferta (Zostaw kładzie ją z powrotem), -1 = oferta bez znajdźki.</summary>
    public sbyte OfferPickup = -1;

    public int BackpackSlots() => Math.Min(MaxBackpack, D.BackpackSlots);

    public int BackpackCount()
    {
        var n = 0;
        for (var i = 0; i < BackpackSlots(); ++i)
        {
            if (!Backpack[i].Empty) ++n;
        }
        return n;
    }

    public bool BackpackFull() => BackpackCount() >= BackpackSlots();

    /// <summary>Wkłada przedmiot do pierwszego wolnego miejsca; false = pełny (albo plecak wyłączony w danych).</summary>
    public bool BackpackAdd(BackpackItem it)
    {
        for (var i = 0; i < BackpackSlots(); ++i)
        {
            if (!Backpack[i].Empty) continue;
            Backpack[i] = it;
            return true;
        }
        return false;
    }

    /// <summary>Nazwa przedmiotu (broń z poziomem ulepszenia: „Kielnia+2”).</summary>
    public string BackpackName(BackpackItem it)
    {
        if (it.Kind == BackpackKind.Gear) return D.Gear[it.Id * 3 + it.Rarity].Name;
        if (it.Kind != BackpackKind.Weapon) return "";
        var lvl = it.Extra & 15;
        return lvl > 0 ? $"{D.Weapons[it.Id].Name}+{lvl}" : D.Weapons[it.Id].Name;
    }

    /// <summary>Poziom przedmiotu do Skupu i PSZOK-u: 0 Zwykła, 1 Solidna, 2 Markowa, 3 Mistrzowska / artefakt.</summary>
    public int BackpackTier(BackpackItem it)
    {
        if (it.Kind == BackpackKind.Gear) return Math.Clamp((int)it.Rarity, 0, 2);
        if (it.Kind != BackpackKind.Weapon) return 0;
        return D.Weapons[it.Id].Master || it.Rarity >= 3 ? 3 : Math.Clamp((int)it.Rarity, 0, 2);
    }

    /// <summary>Broń w ręku jako przedmiot plecaka (z rzadkością, profitami i ulepszeniami).</summary>
    public BackpackItem HandItem() =>
        BackpackItem.Weapon(HandWeapon(), WeaponRarity, WeaponPerks, PackWeaponExtra(WeaponLvl, WeaponTrait, WeaponCharges, TraitPending));

    /// <summary>Broń z plecaka do ręki (bez wpisu i tury).</summary>
    private void SetHand(BackpackItem it)
    {
        ResetUpgrade();
        ResetProc();
        WeaponOverride = it.Id == CDef.Weapon ? -1 : it.Id;
        WeaponRarity = it.Rarity;
        WeaponPerks = it.Perks;
        ApplyWeaponExtra(it.Extra);
        MarkArtifact(it.Id);
        SecretFlags |= SecretToolSwapFlag;
    }

    /// <summary>
    /// Porównanie z noszonym w tym slocie (▲ 1 lepsze, ▼ -1 słabsze, = 0): broń – średni cios (z ulepszeniem), sprzęt – jakość
    /// (pusty slot = lepsze).
    /// </summary>
    public int BackpackCompare(int i)
    {
        var it = Backpack[i];
        if (it.Kind == BackpackKind.Gear)
        {
            var now = Equipped[it.Id];
            return it.Rarity > now ? 1 : it.Rarity < now ? -1 : 0;
        }
        if (it.Kind != BackpackKind.Weapon) return 0;
        var mine = WeaponBreakdown().Avg10;
        var other = ItemAvg10(it);
        return other > mine ? 1 : other < mine ? -1 : 0;
    }

    /// <summary>Średni cios ×10 broni z plecaka (ulepszenie: + poziom × obrażenia za poziom).</summary>
    public int ItemAvg10(BackpackItem it) =>
        WeaponBreakdown(-1, it.Id, -1, -1, 0, it.Rarity, it.Perks).Avg10 + (it.Extra & 15) * D.ToolUpgradeDmg * 10;

    /// <summary>Plecak w Hurtowni (między aktami): zamiana bez tury, Skup i PSZOK.</summary>
    public bool InHurtownia => St == GameStatus.StageClear && ActCleared;

    /// <summary>Skup i PSZOK: Hurtownia otwarta (wyzwanie / inwestor „bez Hurtowni” zamyka też Skup).</summary>
    public bool CanTrade => InHurtownia && !ShopClosed;

    /// <summary>Zamiana zużywa turę na placu (jak kabura), w Hurtowni nie. Zwraca true = tura zużyta.</summary>
    private bool BackpackTurn()
    {
        if (St != GameStatus.Playing) return false;
        EndTurn();
        return true;
    }

    /// <summary>Czy z plecaka można teraz coś założyć (plac w trakcie albo Hurtownia).</summary>
    public bool BackpackUsable => St == GameStatus.Playing || InHurtownia;

    /// <summary>
    /// Załóż: zamiana miejscami z noszonym (sprzęt w slocie albo broń w ręku). Na placu kosztuje turę, w Hurtowni nie.
    /// Zwraca true, gdy coś się zmieniło.
    /// </summary>
    public bool BackpackEquip(int i)
    {
        if (i < 0 || i >= BackpackSlots() || Backpack[i].Empty || !BackpackUsable) return false;
        if (St == GameStatus.Playing && ShockedTurn()) return true;
        var it = Backpack[i];
        if (it.Kind == BackpackKind.Gear)
        {
            var slot = it.Id;
            Backpack[i] = Equipped[slot] >= 0 ? BackpackItem.Gear(slot, Equipped[slot], EquippedTrait[slot]) : default;
            Equip(slot, it.Rarity, it.Trait);
        }
        else
        {
            Backpack[i] = HandItem();
            SetHand(it);
            Push(Msg(Loc.T("z_plecaka")).Add(WeaponTitle()).As(LogKind.Loot));
            PushPerks(WeaponPerks, WeaponRarity);
        }
        BackpackTurn();
        return true;
    }

    /// <summary>Broń z plecaka do kabury (pas z kaburą): zamiana z bronią w kaburze; tura jak przy zakładaniu.</summary>
    public bool BackpackHolster(int i)
    {
        if (i < 0 || i >= BackpackSlots() || Backpack[i].Kind != BackpackKind.Weapon || !HasBelt() || !BackpackUsable) return false;
        if (St == GameStatus.Playing && ShockedTurn()) return true;
        var it = Backpack[i];
        Backpack[i] = Weapon2 >= 0
            ? BackpackItem.Weapon(Weapon2, Weapon2Rarity, Weapon2Perks, PackWeaponExtra(Weapon2Lvl, Weapon2Trait, Weapon2Charges, Weapon2TraitPending))
            : default;
        Weapon2 = it.Id;
        Weapon2Rarity = it.Rarity;
        Weapon2Perks = it.Perks;
        Weapon2Lvl = (sbyte)(it.Extra & 15);
        Weapon2Trait = (sbyte)(((it.Extra >> 4) & 15) - 1);
        Weapon2Charges = (sbyte)((it.Extra >> 8) & 15);
        Weapon2TraitPending = ((it.Extra >> 12) & 1) != 0;
        MarkArtifact(it.Id);
        Push(Msg(Loc.T("bron_do_kabury")).Add(D.Weapons[Weapon2].Name).As(LogKind.Loot));
        BackpackTurn();
        return true;
    }

    /// <summary>Pole na znajdźkę przy bohaterze: jego pole (gdy wolne od znajdziek), inaczej najbliższe wolne. False = brak miejsca.</summary>
    private bool DropCell(out int x, out int y)
    {
        x = Hero.X;
        y = Hero.Y;
        if (PickupsCount >= MaxPickups) return false;
        return !PickupAt(x, y) || FreeAround(Hero.X, Hero.Y, Hero.X, Hero.Y, out x, out y);
    }

    /// <summary>Przedmiot jako znajdźka na polu (broń: tylko narzędzie – broń zawodu nie leży na ziemi). False = przepada.</summary>
    private bool PlaceItem(BackpackItem it, int x, int y, int slot = -1)
    {
        Pickup p;
        if (it.Kind == BackpackKind.Gear)
        {
            p = new Pickup(x, y, PickupType.GearBox, true, it.Id * 3 + it.Rarity, Math.Max(0, (int)it.Trait)) { Extra = GearBoxUsed };
        }
        else
        {
            var id = it.Id;
            var t = Array.FindIndex(D.Tools, tl => tl.Weapon == id);
            if (t < 0) return false;
            p = new Pickup(x, y, PickupType.Tool, true, t, it.Rarity) { Extra = it.Extra, Perks = it.Perks };
        }
        if (slot >= 0) Pickups[slot] = p;
        else if (PickupsCount < MaxPickups) Pickups[PickupsCount++] = p;
        else return false;
        return true;
    }

    /// <summary>Do plecaka, a gdy pełny – na ziemię (pole bohatera / obok, albo znajdźka slot); zwraca false, gdy przepadł.</summary>
    private bool StoreOrDrop(BackpackItem it, int slot = -1)
    {
        if (BackpackAdd(it))
        {
            Push(Msg(Loc.T("do_plecaka")).Add(BackpackName(it)).As(LogKind.Loot));
            return true;
        }
        int x = Hero.X, y = Hero.Y;
        var ok = slot >= 0 || DropCell(out x, out y);
        ok = ok && PlaceItem(it, x, y, slot);
        Push(Msg(ok ? Loc.T("plecak_pelny_ziemia") : Loc.T("przedmiot_przepada")).Add(BackpackName(it)).As(LogKind.Loot));
        return ok;
    }

    /// <summary>Wyrzuć: przedmiot na ziemię pod bohaterem (albo najbliższe wolne pole); bez tury. Broń zawodu przepada.</summary>
    public bool BackpackDrop(int i)
    {
        if (i < 0 || i >= BackpackSlots() || Backpack[i].Empty || St != GameStatus.Playing) return false;
        var it = Backpack[i];
        Backpack[i] = default;
        var ok = DropCell(out var x, out var y) && PlaceItem(it, x, y);
        Push(Msg(ok ? Loc.T("wyrzucasz") : Loc.T("przedmiot_przepada")).Add(BackpackName(it)).As(LogKind.Loot));
        return true;
    }

    /// <summary>Skup: sztuki materiału za przedmiot (GameData.BackpackSell wg poziomu).</summary>
    public int SellCount(BackpackItem it) => it.Empty || D.BackpackSell.Length == 0 ? 0 : D.BackpackSell[Math.Min(BackpackTier(it), D.BackpackSell.Length - 1)];

    /// <summary>Materiał Skupu: broń – stal, sprzęt – drewno; co druga sztuka cement (główny = (n+1)/2, cement = n/2).</summary>
    public int SellMat(BackpackItem it) => it.Kind == BackpackKind.Weapon ? D.BackpackWeaponMat : D.BackpackGearMat;

    /// <summary>Skup (Hurtownia): sprzedaż przedmiotu za materiały.</summary>
    public bool BackpackSell(int i)
    {
        if (i < 0 || i >= BackpackSlots() || Backpack[i].Empty || !CanTrade || D.Materials.Length == 0) return false;
        var it = Backpack[i];
        var n = SellCount(it);
        Backpack[i] = default;
        Push(Msg(Loc.T("skup")).Add(BackpackName(it)).As(LogKind.Loot));
        var main = SellMat(it);
        if (main >= 0) AddMaterial(main, (n + 1) / 2);
        if (D.BackpackAltMat >= 0 && n / 2 > 0) AddMaterial(D.BackpackAltMat, n / 2);
        return true;
    }

    /// <summary>PSZOK: dośw. za oddanie (bazowe + za poziom przedmiotu).</summary>
    public int PszokXpFor(BackpackItem it) => it.Empty ? 0 : D.PszokXp + D.PszokXpPerTier * BackpackTier(it);

    /// <summary>PSZOK (Hurtownia): oddanie za darmo – dośw. i trochę HP.</summary>
    public bool BackpackRecycle(int i)
    {
        if (i < 0 || i >= BackpackSlots() || Backpack[i].Empty || !CanTrade) return false;
        var it = Backpack[i];
        var xp = PszokXpFor(it);
        Backpack[i] = default;
        GainXp(xp);
        Hero.Hp = (short)Math.Min(Hero.MaxHp, Hero.Hp + D.PszokHp);
        Push(Msg(Loc.T("pszok_segregacja")).Add(xp).Add(Loc.T("dosw")).As(LogKind.Good));
        return true;
    }

    /// <summary>Dośw. za „Zostaw” na karcie paczki (0, gdy paczka była już odłożona).</summary>
    public int OfferDeclineXp() =>
        !HasOffer || (OfferPickup >= 0 && (Pickups[OfferPickup].Extra & GearBoxUsed) != 0) ? 0 : D.GearDeclineXp + OfferRarity;

    /// <summary>Co stanie się ze starą bronią po „Weź”: 0 do plecaka, 1 na ziemię, 2 przepada (broń zawodu przy pełnym plecaku).</summary>
    public int ToolTakeFate() => !BackpackFull() ? 0 : Array.FindIndex(D.Tools, tl => tl.Weapon == HandWeapon()) >= 0 ? 1 : 2;

    /// <summary>Paczka sprzętu z karty do plecaka (bez tury); false = plecak pełny.</summary>
    public bool OfferToBackpack()
    {
        if (!HasOffer || BackpackFull()) return false;
        var it = BackpackItem.Gear(OfferSlot, OfferRarity, OfferTrait);
        OfferSlot = -1;
        OfferPickup = -1;
        BackpackAdd(it);
        Push(Msg(Loc.T("do_plecaka")).Add(BackpackName(it)).As(LogKind.Loot));
        return true;
    }

    /// <summary>Broń z karty do plecaka (bez tury); false = plecak pełny.</summary>
    public bool ToolToBackpack()
    {
        if (!HasToolOffer || BackpackFull()) return false;
        int t = ToolOffer, w = D.Tools[t].Weapon;
        var rarity = ResolveRarity(w, ToolOfferRarity);
        var it = BackpackItem.Weapon(w, rarity, ResolvePerks(w, rarity, ToolOfferPerks), ToolOfferExtra);
        Pickups[ToolOfferPickup].Active = false;
        ToolOffer = ToolOfferPickup = -1;
        ToolOfferExtra = 0;
        ToolOfferPerks = 0;
        ToolsFound |= 1UL << t;
        MarkArtifact(w);
        BackpackAdd(it);
        Push(Msg(Loc.T("do_plecaka")).Add(BackpackName(it)).As(LogKind.Loot));
        return true;
    }

    /// <summary>
    /// Bot w Skupie (po bossie aktu): sprzedaje wszystko oprócz najlepszej zapasowej broni (średni cios), przedmioty
    /// po kolei od pierwszego miejsca.
    /// </summary>
    public void BotSkup()
    {
        if (!CanTrade) return;
        int keep = -1, best = -1;
        for (var i = 0; i < BackpackSlots(); ++i)
        {
            if (Backpack[i].Kind != BackpackKind.Weapon) continue;
            var v = ItemAvg10(Backpack[i]);
            if (v > best)
            {
                best = v;
                keep = i;
            }
        }
        for (var i = 0; i < BackpackSlots(); ++i)
        {
            if (i != keep && !Backpack[i].Empty) BackpackSell(i);
        }
    }

    /// <summary>Bot: paczka sprzętu – lepszą zakłada (stary do plecaka), tej samej jakości chowa do plecaka, gorszą zostawia.</summary>
    public void BotGearOffer(bool acceptSame)
    {
        if (!HasOffer) return;
        var holster = OfferDropsHolster && Weapon2 >= 0; // pas z bronią w kaburze zostaje
        var now = Equipped[OfferSlot];
        if (!holster && (OfferRarity > now || (acceptSame && OfferRarity == now))) AcceptOffer();
        else if (OfferRarity < now || !OfferToBackpack()) DeclineOffer();
    }
}
