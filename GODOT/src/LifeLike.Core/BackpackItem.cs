namespace LifeLike.Core;

/// <summary>
/// v0.23.2: przedmiot w plecaku (core::bp_item). Broń: Id = indeks GameData.Weapons, Rarity = rzadkość broni, Perks = profity,
/// Extra = ulepszenia spakowane jak Pickup.Extra (poziom, cecha, ładunki, wybór cechy). Sprzęt: Id = slot, Rarity = jakość 0..2,
/// Trait = cecha sprzętu.
/// </summary>
public struct BackpackItem
{
    public BackpackKind Kind;
    public sbyte Id;
    public byte Rarity;
    public sbyte Trait;
    public byte Perks;
    public ushort Extra;

    public readonly bool Empty => Kind == BackpackKind.Empty;

    public static BackpackItem Weapon(int weapon, int rarity, int perks, int extra) =>
        new() { Kind = BackpackKind.Weapon, Id = (sbyte)weapon, Rarity = (byte)rarity, Trait = -1, Perks = (byte)perks, Extra = (ushort)extra };

    public static BackpackItem Gear(int slot, int rarity, int trait) =>
        new() { Kind = BackpackKind.Gear, Id = (sbyte)slot, Rarity = (byte)rarity, Trait = (sbyte)trait };
}
