namespace LifeLike.Core.Data;

/// <summary>Cecha przedmiotu sprzętu, losowana do każdej paczki.</summary>
public sealed record TraitDef(string Id, string Name, string Short, TraitEffect Effect, int Value);
