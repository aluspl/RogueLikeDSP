namespace LifeLike.Core.Data;

/// <summary>v0.23.0: tryb ataku broni (A – zwykły, B – specjalny z odnowieniem). Range = zasięg wzoru (domyślnie zasięg broni),
/// DmgPct = % obrażeń broni, Effect/Value/Turns = efekt na celu, Cooldown = tury odnowienia (tryb B zawsze &gt; 0).</summary>
public sealed record WeaponMode(PatternKind Pattern, int Range, int DmgPct, ModeEffect Effect, int Value, int Turns, int Cooldown);
