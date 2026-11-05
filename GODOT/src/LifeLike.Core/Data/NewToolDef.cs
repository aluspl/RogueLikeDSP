namespace LifeLike.Core.Data;

/// <summary>v0.23.0: źródła nowej broni (meta.newTools): Shop = Szkolenia, Drop = może wypaść na budowie,
/// Hurtownia = w puli „Nowe narzędzie”, Secret = sekret. Poza GameData.Tools (maska uint8 w profilu) do czasu integracji.</summary>
public sealed record NewToolDef(int Weapon, bool Shop, bool Drop, bool Hurtownia, bool Secret);
