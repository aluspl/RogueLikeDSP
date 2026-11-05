namespace LifeLike.Core.Data;

/// <summary>v0.23.0: efekt trybu ataku na trafiony cel (Value/Turns w WeaponMode zależą od efektu).</summary>
public enum ModeEffect : byte { None, Stun, Root, Push, Pull, DefDown, Dot, Mark, Reveal, OpenWall, Swap, Chain, Dry, CritNext }
