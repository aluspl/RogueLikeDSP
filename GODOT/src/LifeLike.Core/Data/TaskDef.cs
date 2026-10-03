namespace LifeLike.Core.Data;

/// <summary>Zadanie dnia albo tygodnia (v0.21.52 cz. c,, core::task_def): licznik Kind do Target, nagroda w Respekcie.</summary>
public sealed record TaskDef(string Id, string Name, TaskKind Kind, int Target, int Respect);
