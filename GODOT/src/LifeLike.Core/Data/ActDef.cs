namespace LifeLike.Core.Data;

/// <summary>
/// Akt budowy: kilka etapów zakończonych bossem, potem Hurtownia. Mechanika aktu (v0.21.49): błoto (wejście kosztuje
/// turę, MechValue = 1 pole na tyle), porywy wiatru (co MechValue tur spychają o pole), pył (widzenie -MechValue).
/// </summary>
public sealed record ActDef(string Name, int BonusPerStage, int BonusPerKill, ActMechanic Mechanic = ActMechanic.None, int MechValue = 0,
    string MechName = "", string MechShort = "", string MechInfo = "");
