namespace LifeLike.Core.Data;

/// <summary>
/// Działanie poziomu Szkolenia (core::upgrade_effect); v0.21.52: kryt, unik, termos, materiały, sprzęt, zł; cz. c – węzły
/// drzewka: Hurtownia i brygada taniej, moc szybciej, pierwszy cios (Siła rozpędu).
/// </summary>
public enum UpgradeEffect : byte
{
    Hp, Def, Dmg, Coffee, Pickups, Luck, Craft, DmgPct, TakenPct, Crit, Dodge, Thermos, MatsPct, GearPct, Cash,
    ShopPct, BrigadePct, Cooldown, FirstHit,
    /// <summary>v0.23.0: startowa rzadkość broni domyślnej (Szkolenie Narzędzia: 1 = Solidna, 2 = Markowa).</summary>
    StartRarity, Unknown = 255,
}
