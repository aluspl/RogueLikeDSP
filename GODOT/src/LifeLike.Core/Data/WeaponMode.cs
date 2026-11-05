namespace LifeLike.Core.Data;

/// <summary>v0.23.0: tryb ataku broni (A – zwykły, B – specjalny z odnowieniem). Range = zasięg wzoru (domyślnie zasięg broni),
/// DmgPct = % obrażeń broni, Effect/Value/Turns = efekt na celu, Cooldown = tury odnowienia (tryb B zawsze &gt; 0).
/// Effect2/Value2/Turns2 = drugi, dodatkowy efekt (np. Łom: odrzut + otwarcie pękniętej ściany).</summary>
public sealed record WeaponMode(PatternKind Pattern, int Range, int DmgPct, ModeEffect Effect, int Value, int Turns, int Cooldown,
    ModeEffect Effect2 = ModeEffect.None, int Value2 = 0, int Turns2 = 0);
