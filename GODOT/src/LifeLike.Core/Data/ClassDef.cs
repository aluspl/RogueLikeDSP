namespace LifeLike.Core.Data;

/// <summary>Zawód budowlany; Weapon = indeks w GameData.Weapons.</summary>
public sealed record ClassDef(
    string Id, string Name, string Desc, int MaxHealth, int Strength, int Agility, int Intelligence, int Defense,
    int Weapon, int Frame, string AbilityName, string AbilityDesc, AbilityEffect Ability, int AbilityCooldown);
