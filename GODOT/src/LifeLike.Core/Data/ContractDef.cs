namespace LifeLike.Core.Data;

/// <summary>Rodzaj zlecenia (core::contract_kind) – który licznik profilu je napędza.</summary>
public enum ContractKind : byte { Kills, Powers, Brand, CleanBoss, ClassWins, Wins }

/// <summary>Zlecenie: długofalowy cel z licznikiem w profilu; Xp = nagroda, Keepsake = odblokowana pamiątka (-1 = brak);
/// v0.21.52: Title = tytuł do wyboru w profilu, Cosmetic = odblokowany wygląd (kolor kasku, -1 = brak).</summary>
public sealed record ContractDef(string Id, string Name, string Desc, ContractKind Kind, int Target, int Xp, int Keepsake, string Title = "", int Cosmetic = -1);
