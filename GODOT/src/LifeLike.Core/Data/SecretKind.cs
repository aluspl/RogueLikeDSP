namespace LifeLike.Core.Data;

/// <summary>Warunek sekretnego zlecenia (core::secret_kind, v0.21.51 cz. 2). v0.23.0 (#84): NoSwapWin – wygrana zawodem Value
/// bez wymiany broni (tylko Godot do zadania 10). v0.23.0: PatentWalls – Value ścian otwartych Patentem w jednej budowie.</summary>
public enum SecretKind : byte { NoCoffeeWin, HelperBoss, Storerooms, ClassWins, PaperClean, ShockCombos, LowHpWin, FastWin, NoSwapWin, PatentWalls }
