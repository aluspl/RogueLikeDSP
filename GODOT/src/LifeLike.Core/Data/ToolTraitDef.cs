namespace LifeLike.Core.Data;

/// <summary>Cecha ulepszonego narzędzia: nazwa, skrót do rozpiski, opis, skutek (core::tool_trait_def).</summary>
public sealed record ToolTraitDef(string Id, string Name, string Short, string Desc, ToolTraitEffect Effect, int Value);
