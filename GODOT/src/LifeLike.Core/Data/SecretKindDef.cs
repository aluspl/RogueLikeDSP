namespace LifeLike.Core.Data;

/// <summary>Ukryte pomieszczenie (#32): pęknięta ściana (Breakable) albo drzwi magazynu (core::secret_kind_def).</summary>
public sealed record SecretKindDef(string Id, string Name, string Info, bool Breakable);
