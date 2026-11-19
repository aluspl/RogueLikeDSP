namespace LifeLike.Core.Data;

/// <summary>Cecha ulepszonego narzędzia (#31): Przebicie, Ostrze, Wyważenie (core::tool_trait_effect).
/// v0.23.0: też skutek profitu broni – MaxDmg (+najsilniejszy rzut), CooldownB (-odnowienie trybu B, min. 1).</summary>
public enum ToolTraitEffect : byte { Pierce, Crit, Steady, MaxDmg, CooldownB }
