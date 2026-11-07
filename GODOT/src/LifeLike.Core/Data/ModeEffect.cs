namespace LifeLike.Core.Data;

/// <summary>v0.23.0: efekt trybu ataku na trafiony cel (Value/Turns w WeaponMode zależą od efektu). Wet (Myjka): cel mokry; Ignite (Szlifierka): iskry wybuchają pył na trafionych.
/// v0.23.0: Burn = podpalenie (zawsze ogień, także bronią tnącą), Spread = ogień z trafionych przeskakuje na Value sąsiadów (artefakty).</summary>
public enum ModeEffect : byte { None, Stun, Root, Push, Pull, DefDown, Dot, Mark, Reveal, OpenWall, Swap, Chain, Dry, CritNext, Wet, Ignite, Burn, Spread }
