namespace LifeLike.Core.Data;

/// <summary>v0.23.0: tryb ataku broni (A – zwykły, B – specjalny z odnowieniem). Range = zasięg wzoru (domyślnie zasięg broni),
/// DmgPct = % obrażeń broni, Effect/Value/Turns = efekt na celu, Cooldown = tury odnowienia (tryb B zawsze &gt; 0).
/// Effect2/Value2/Turns2 = drugi, dodatkowy efekt (np. Łom: odrzut + otwarcie pękniętej ściany). SteelPct = obrażenia efektu Dot w problem ze stali w % (0 = bez zmian, Spawarka: 150).
/// v0.23.0 (#84): Hits = ile ciosów w każdy trafiony cel (Nóż majstra seniora B: 2), Name = własna nazwa trybu dla gracza (opcjonalna).</summary>
public sealed record WeaponMode(PatternKind Pattern, int Range, int DmgPct, ModeEffect Effect, int Value, int Turns, int Cooldown,
    ModeEffect Effect2 = ModeEffect.None, int Value2 = 0, int Turns2 = 0, int SteelPct = 0, int Hits = 1, string Name = "");
