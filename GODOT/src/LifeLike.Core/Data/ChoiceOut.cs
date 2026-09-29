namespace LifeLike.Core.Data;

/// <summary>
/// Skutek odpowiedzi (core::choice_out): Chance w % (100 = zawsze); Arg – materiał (-1 = każdy), problem (Spawn),
/// stan (Status), slot sprzętu (Gear, -1 = losowy).
/// </summary>
public sealed record ChoiceOut(ChoiceEffect Effect, int Value, int Chance, int Arg);
