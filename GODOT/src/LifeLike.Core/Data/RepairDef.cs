namespace LifeLike.Core.Data;

/// <summary>
/// Naprawa za materiał (telefon: Brygada i naprawy). Value: Załataj – tury muru; Kładka – zasięg (pola).
/// </summary>
public sealed record RepairDef(string Id, string Name, string Desc, string Info, RepairEffect Effect, int Material, int Cost, int Value);
