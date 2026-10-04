namespace LifeLike.Core.Data;

/// <summary>
/// Komplet kolekcji (v0.21.52 cz. c,, core::collection_def): każdy problem z maski Enemies pokonany Count razy (akty),
/// każdy boss (karty bossów) albo wszystkie ozdoby Osiedla (album); nagroda Reward: stała premia (Perk – Bonus), tytuł
/// albo kolor kasku.
/// </summary>
public sealed record CollectionDef(string Id, string Name, string Desc, CollectionKind Kind, ulong Enemies, int Count, ProgressLevel Reward, Perk Bonus);
