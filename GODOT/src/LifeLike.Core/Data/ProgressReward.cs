namespace LifeLike.Core.Data;

/// <summary>
/// Nagroda za poziom (v0.21.52 cz. b, core::progress_reward): poziom inspektora (#44), mistrzostwo zawodu (#45),
/// stopnie inwestora (#48); cz. c: pamiątka (seria dni), stała premia (kolekcje).
/// </summary>
public enum ProgressReward : byte { Respect, Title, Helmet, Story, Decor, KeepsakeSlot, Power, Weapon, Boon, Keepsake, Perk }
