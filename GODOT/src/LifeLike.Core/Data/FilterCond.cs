namespace LifeLike.Core.Data;

/// <summary>v0.21.53: jeden warunek odblokowania filtra – Value: poziom / wygrane albo indeks (Collections, Career, Secrets).</summary>
public sealed record FilterCond(FilterUnlock Kind, int Value);
