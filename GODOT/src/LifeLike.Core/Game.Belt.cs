namespace LifeLike.Core;

/// <summary>v0.23.0: pas monterski z kaburą – druga broń pod ręką i zamiana za turę.</summary>
public sealed partial class Game
{
    /// <summary>Jakość (0..2) przedmiotu „Pas monterski z kaburą” w slocie pasa: środkowy z trzech (Zwykły / z kaburą / Markowy).</summary>
    public const int BeltTier = 1;

    /// <summary>Druga broń w kaburze: indeks broni (nie nadpisanie – zawsze prawdziwa broń), -1 = pusta.</summary>
    public int Weapon2 = -1;

    /// <summary>Ulepszenia broni w kaburze (należą do broni, nie do bohatera): poziom, cecha (-1 brak), oczekujący wybór cechy.</summary>
    public sbyte Weapon2Lvl;
    public sbyte Weapon2Trait = -1;
    public bool Weapon2TraitPending;

    /// <summary>Ulepszenia upuszczonej broni oczekujące w ofercie wymiany narzędzia (Pickup.Extra).</summary>
    public ushort ToolOfferExtra;

    /// <summary>Rzadkość broni w kaburze (jak WeaponRarity).</summary>
    public byte Weapon2Rarity;

    /// <summary>Ładunki broni w kaburze (Przecinak liczy je razem z bronią, nie z ręką).</summary>
    public sbyte Weapon2Charges;

    /// <summary>Założony pas monterski z kaburą (slot Pas, przedmiot jakości BeltTier; nadal daje termos).</summary>
    public bool HasBelt() => D.BeltSlot >= 0 && Equipped[D.BeltSlot] == BeltTier;

    /// <summary>
    /// Pas z kaburą jest rzadki: paczka / Hurtownia dająca Pas jakości BeltTier trafia w pas z kaburą tylko ze szansą
    /// BeltChancePct, inaczej dostaje jakość zastępczą (fallback). Losuje tylko dla tego jednego przedmiotu.
    /// </summary>
    private int BeltFilter(int slot, int rarity, int fallback) =>
        slot == D.BeltSlot && rarity == BeltTier && R.Range(1, 100) > D.BeltChancePct ? fallback : rarity;

    /// <summary>Broń w ręku jako prawdziwy indeks broni (WeaponOverride -1 = broń zawodu).</summary>
    private int HandWeapon() => WeaponOverride >= 0 ? WeaponOverride : CDef.Weapon;

    /// <summary>
    /// Zamiana broni z kabury: wymaga pasa i broni w drugim miejscu; zamienia broń, rzadkość, ładunki i ulepszenia
    /// (poziom, cecha, oczekujący wybór cechy), kosztuje turę. Odnowienie trybu B zostaje przy bohaterze. Zwraca true = tura zużyta.
    /// </summary>
    public bool PlayerSwapWeapon()
    {
        if (St != GameStatus.Playing || !HasBelt() || Weapon2 < 0) return false;
        if (ShockedTurn()) return true;
        SwapHands();
        SecretFlags |= SecretToolSwapFlag; // v0.23.0 (#84): zlecenie „ta sama broń”
        Push(Msg(Loc.T("zamiana_broni")).Add(Weapon.Name).As(LogKind.Loot));
        EndTurn();
        return true;
    }

    /// <summary>Broń z ręki do kabury i z kabury do ręki – z rzadkością, ładunkami, ulepszeniami i profitami (bez tury i wpisu).</summary>
    private void SwapHands()
    {
        var hand = HandWeapon();
        var rarity = WeaponRarity;
        var charges = WeaponCharges;
        var lvl = WeaponLvl;
        var trait = WeaponTrait;
        var pending = TraitPending;
        var perks = WeaponPerks;
        WeaponOverride = Weapon2 == CDef.Weapon ? -1 : Weapon2;
        ResetProc(); // v0.23.0: licznik „co N-ty cios” od nowa
        WeaponRarity = Weapon2Rarity;
        WeaponCharges = Weapon2Charges;
        Weapon2 = hand;
        Weapon2Rarity = rarity;
        Weapon2Charges = charges;
        // ulepszenia (poziom, cecha, oczekujący wybór cechy) należą do broni i jadą razem z nią; odnowienie trybu B zostaje przy bohaterze
        WeaponLvl = Weapon2Lvl;
        WeaponTrait = Weapon2Trait;
        TraitPending = Weapon2TraitPending;
        Weapon2Lvl = lvl;
        Weapon2Trait = trait;
        Weapon2TraitPending = pending;
        WeaponPerks = Weapon2Perks; // v0.23.0: profity należą do broni
        Weapon2Perks = perks;
    }

    /// <summary>Znaleziona broń z założonym pasem i pustą kaburą trafia do drugiego miejsca (zamiast pytania o wymianę).</summary>
    private bool StashTool(int t, int rarity, int extra = 0, int perks = 0)
    {
        if (!HasBelt() || Weapon2 >= 0) return false;
        Weapon2 = D.Tools[t].Weapon;
        Weapon2Rarity = (byte)ResolveRarity(Weapon2, rarity);
        Weapon2Charges = 0;
        Weapon2Lvl = 0;
        Weapon2Trait = -1;
        Weapon2TraitPending = false;
        Weapon2Perks = ResolvePerks(Weapon2, Weapon2Rarity, perks); // v0.23.0
        MarkArtifact(Weapon2);
        if (extra != 0)
        {
            Weapon2Lvl = (sbyte)(extra & 15);
            Weapon2Trait = (sbyte)(((extra >> 4) & 15) - 1);
            Weapon2Charges = (sbyte)((extra >> 8) & 15);
            Weapon2TraitPending = ((extra >> 12) & 1) != 0;
        }
        ToolsFound |= 1UL << t;
        Push(Msg(Loc.T("bron_do_kabury")).Add(D.Weapons[Weapon2].Name).As(LogKind.Loot));
        PushPerks(Weapon2Perks, Weapon2Rarity);
        return true;
    }

    /// <summary>Ulepszenia broni spakowane do Pickup.Extra: poziom (4 b), cecha+1 (4 b), ładunki (4 b), wybór cechy (1 b).</summary>
    private static ushort PackWeaponExtra(int lvl, int trait, int charges, bool pending) =>
        (ushort)((lvl & 15) | (((trait + 1) & 15) << 4) | ((charges & 15) << 8) | (pending ? 1 << 12 : 0));

    /// <summary>Nałożenie spakowanych ulepszeń na broń w ręku (po ResetUpgrade).</summary>
    private void ApplyWeaponExtra(int extra)
    {
        if (extra == 0) return;
        WeaponLvl = (sbyte)(extra & 15);
        WeaponTrait = (sbyte)(((extra >> 4) & 15) - 1);
        WeaponCharges = (sbyte)((extra >> 8) & 15);
        TraitPending = ((extra >> 12) & 1) != 0;
    }

    /// <summary>
    /// Utrata pasa z kaburą (inny przedmiot w slocie Pas): broń z drugiego miejsca wypada jako znajdźka na polu bohatera
    /// (albo obok), z rzadkością, ładunkami i ulepszeniami; kabura pusta. Gdy broń nie jest narzędziem albo brak miejsca – przepada.
    /// Broń zawodu w kaburze nie przepada: wraca do ręki (z ulepszeniami), a wypada narzędzie, które było w ręku.
    /// </summary>
    private void DropHolster()
    {
        if (Weapon2 < 0) return;
        if (Weapon2 == CDef.Weapon && HandWeapon() != CDef.Weapon)
        {
            SwapHands();
            Push(Msg(Loc.T("bron_z_kabury_domyslna")).Add(Weapon.Name).As(LogKind.Info));
        }
        var w = Weapon2;
        if (BackpackAdd(BackpackItem.Weapon(w, Weapon2Rarity, Weapon2Perks, PackWeaponExtra(Weapon2Lvl, Weapon2Trait, Weapon2Charges, Weapon2TraitPending))))
        {
            Push(Msg(Loc.T("bron_z_kabury_do_plecaka")).Add(D.Weapons[w].Name).As(LogKind.Loot)); // v0.23.2: najpierw do plecaka
            ClearHolster();
            return;
        }
        var t = Array.FindIndex(D.Tools, tl => tl.Weapon == w);
        int x = Hero.X, y = Hero.Y;
        var ok = t >= 0 && PickupsCount < MaxPickups && (!PickupAt(x, y) || FreeAround(x, y, Hero.X, Hero.Y, out x, out y));
        if (ok)
        {
            Pickups[PickupsCount++] = new Pickup(x, y, PickupType.Tool, true, t, Weapon2Rarity)
            {
                Extra = PackWeaponExtra(Weapon2Lvl, Weapon2Trait, Weapon2Charges, Weapon2TraitPending),
                Perks = Weapon2Perks, // v0.23.0: profity wypadają razem z bronią
            };
        }
        if (ok) Push(Msg(Loc.T("bron_z_kabury_wypada")).Add(D.Weapons[w].Name).As(LogKind.Loot));
        else Push(Msg(Loc.T("bron_z_kabury_przepada")).Add(D.Weapons[w].Name).As(LogKind.Loot));
        ClearHolster();
    }

    /// <summary>Pusta kabura (bez wpisu).</summary>
    private void ClearHolster()
    {
        Weapon2 = -1;
        Weapon2Rarity = 0;
        Weapon2Charges = 0;
        Weapon2Lvl = 0;
        Weapon2Trait = -1;
        Weapon2TraitPending = false;
        Weapon2Perks = 0;
    }
}
