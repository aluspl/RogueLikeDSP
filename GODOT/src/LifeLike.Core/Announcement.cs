namespace LifeLike.Core;

/// <summary>v0.23.0: ogłoszenie – duży napis, podpis, broń do animacji zamachu (-1 = broń bohatera) i jej rzadkość.</summary>
public sealed record Announcement(AnnounceKind Kind, string Title, string Sub, int Weapon, int Rarity);
