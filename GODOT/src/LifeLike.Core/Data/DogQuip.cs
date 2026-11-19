namespace LifeLike.Core.Data;

/// <summary>v0.23.0: ocena Kawki – warunek, próg i 3 linie dymka (core::dog_quip_def).</summary>
public sealed record DogQuip(DogCond When, int Value, string[] Lines);
