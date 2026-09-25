namespace LifeLike.Core.Data;

/// <summary>Akt budowy: kilka etapów zakończonych bossem, potem Hurtownia.</summary>
public sealed record ActDef(string Name, int BonusPerStage, int BonusPerKill);
