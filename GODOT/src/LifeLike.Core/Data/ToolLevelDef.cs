namespace LifeLike.Core.Data;

/// <summary>Koszt kolejnego poziomu ulepszenia narzędzia: zł + materiał (core::tool_level_def).</summary>
public sealed record ToolLevelDef(int Cash, int Material, int Count);
