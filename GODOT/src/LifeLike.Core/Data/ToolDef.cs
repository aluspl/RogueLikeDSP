namespace LifeLike.Core.Data;

/// <summary>Narzędzie do znalezienia (drop); Weapon = indeks broni; Shop (v0.21.52) = na sprzedaż w Szkoleniach (cena
/// z GameData.ToolCosts, rośnie z każdym zakupem), bez flag = dostępne od początku; Reward = z nagrody za odbiór;
/// Secret (v0.21.51 cz. 2) = z sekretnego zlecenia; v0.23.0: Drop = może wypaść na budowie bez kupna, Hurtownia = w puli
/// „Nowe narzędzie” bez kupna (17 nowych broni z meta.newTools dołączonych za dawnymi 10; maska narzędzi ma 64 bity).</summary>
public sealed record ToolDef(int Weapon, bool Shop, bool Reward = false, bool Secret = false, bool Drop = false, bool Hurtownia = false);
