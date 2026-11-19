namespace LifeLike.Core.Data;

/// <summary>
/// v0.23.0: mechanika artefaktu „co N-ty cios” (JSON weapons[].proc): Every = co który cios (1 = każdy), Name = nazwa w dzienniku i HUD,
/// Desc = opis dla gracza (Gablota). Efekt na trafionych jak w trybie ataku (Effect/Value/Turns, Effect2/Value2/Turns2), DmgPct = % obrażeń
/// tego ciosu (100 = bez zmian), Elem = żywioł ciosu (prąd: mokry cel = porażenie).
/// </summary>
public sealed record ProcDef(int Every, string Name, string Desc, ModeEffect Effect, int Value, int Turns, int DmgPct, Element Elem,
    ModeEffect Effect2, int Value2, int Turns2);
