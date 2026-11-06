namespace LifeLike.Core;

/// <summary>v0.23.0: pas monterski z kaburą – druga broń pod ręką i zamiana za turę.</summary>
public sealed partial class Game
{
    /// <summary>Jakość (0..2) przedmiotu „Pas monterski z kaburą” w slocie pasa: środkowy z trzech (Zwykły / z kaburą / Markowy).</summary>
    public const int BeltTier = 1;

    /// <summary>Druga broń w kaburze: indeks broni (nie nadpisanie – zawsze prawdziwa broń), -1 = pusta.</summary>
    public int Weapon2 = -1;

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
    /// Zamiana broni z kabury: wymaga pasa i broni w drugim miejscu; zamienia broń, rzadkość i ładunki, kosztuje turę.
    /// Ulepszenia (poziom, cecha) i odnowienie trybu B zostają przy bohaterze. Zwraca true = tura zużyta.
    /// </summary>
    public bool PlayerSwapWeapon()
    {
        if (St != GameStatus.Playing || !HasBelt() || Weapon2 < 0) return false;
        if (ShockedTurn()) return true;
        var hand = HandWeapon();
        var rarity = WeaponRarity;
        var charges = WeaponCharges;
        WeaponOverride = Weapon2 == CDef.Weapon ? -1 : Weapon2;
        WeaponRarity = Weapon2Rarity;
        WeaponCharges = Weapon2Charges;
        Weapon2 = hand;
        Weapon2Rarity = rarity;
        Weapon2Charges = charges;
        Push(Msg(Loc.T("zamiana_broni")).Add(Weapon.Name).As(LogKind.Loot));
        EndTurn();
        return true;
    }

    /// <summary>Znaleziona broń z założonym pasem i pustą kaburą trafia do drugiego miejsca (zamiast pytania o wymianę).</summary>
    private bool StashTool(int t, int rarity)
    {
        if (!HasBelt() || Weapon2 >= 0) return false;
        Weapon2 = D.Tools[t].Weapon;
        Weapon2Rarity = (byte)ResolveRarity(Weapon2, rarity);
        Weapon2Charges = 0;
        ToolsFound |= 1UL << t;
        Push(Msg(Loc.T("bron_do_kabury")).Add(D.Weapons[Weapon2].Name).As(LogKind.Loot));
        return true;
    }
}
