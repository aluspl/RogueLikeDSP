namespace LifeLike.Core.Data;

/// <summary>Cecha elity: przedrostek nazwy wg rodzaju (m, ż, n / l.mn.), opis, skutek (core::elite_def).</summary>
public sealed record EliteDef(string Id, string Name, string[] Prefix, string Info, EliteEffect Effect, int Value);
