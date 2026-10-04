namespace LifeLike.Core;

/// <summary>Dlaczego naprawy nie można teraz zrobić (Game.RepairBlocked) – odpowiednik game::repair_block w GBA.</summary>
public enum RepairBlock : byte { Ok, Busy, Material, NoTarget, NoRoom, NoPuddle }
