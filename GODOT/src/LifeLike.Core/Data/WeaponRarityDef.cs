namespace LifeLike.Core.Data;

/// <summary>v0.23.0: rzadkość broni (Zwykła, Solidna, Markowa, Mistrzowska): premia do obrażeń, zmiana odnowienia trybu B,
/// próg rzutu k100 (Mistrzowska jest stała – RollFrom &gt; 100 = nie losowana).</summary>
public sealed record WeaponRarityDef(string Name, string Color, int DmgBonus, int CooldownDelta, int RollFrom);
