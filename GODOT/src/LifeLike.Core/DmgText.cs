namespace LifeLike.Core;

/// <summary>Wiersz rozpiski obrażeń broni (port core::dmg_text). v0.23.0: HitMult = mnożnik ciosu (typ x materiał / żywioł), Perks = profity broni.</summary>
public enum DmgText : byte { Weapon, Stat, StatParts, Profile, Run, Gear, Pct, Enemy, Total, Crit, CritParts, CritExtra, Power, Boon, Upgrade, HitMult, Perks }
