namespace LifeLike.Core.Data;

/// <summary>Opis stanu (komunikat przy nałożeniu, HUD): nazwa, skrót, skutek np. „-1 HP/turę”.</summary>
public sealed record StatusDef(string Name, string Short, string Effect);
