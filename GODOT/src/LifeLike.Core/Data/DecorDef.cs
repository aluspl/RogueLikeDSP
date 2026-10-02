namespace LifeLike.Core.Data;

/// <summary>Ozdoba Osiedla (#35, core::decor_def): pojawia się po tylu wygranych albo (v0.21.52 cz. b, Inspector &gt; 0)
/// na tym poziomie inspektora.</summary>
public sealed record DecorDef(string Id, string Name, int Wins, int Inspector = 0);
