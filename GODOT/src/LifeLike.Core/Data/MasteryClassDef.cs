namespace LifeLike.Core.Data;

/// <summary>
/// Mistrzostwo zawodu (core::mastery_class_def): wariant mocy (siła i tury odnowienia), broń mistrza (mała cecha – kryt,
/// złoty błysk przy krycie) i premia mistrzostwa w ofercie po etapie (indeks GameData.Boons).
/// </summary>
public sealed record MasteryClassDef(string PowerName, string PowerDesc, int Power, int Cooldown, string WeaponName, Perk WeaponPerk, int Boon);
