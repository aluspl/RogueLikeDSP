namespace LifeLike.Core.Data;

/// <summary>Materiał budowy (cement, stal, drewno): wypada z problemów i paczek, płaci w Hurtowni i za naprawy.</summary>
public sealed record MaterialDef(string Id, string Name, string Short);
