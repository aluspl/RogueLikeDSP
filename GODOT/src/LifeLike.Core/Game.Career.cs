using LifeLike.Core.Data;

namespace LifeLike.Core;

// v0.21.52 cz. d (#47): mapa kariery – kontrakt budowy (budynek z własnymi etapami) i bliźniak – port 1:1 z core.h.
public sealed partial class Game
{
    /// <summary>Kontrakt mapy kariery (GameData.Career; 0 = Dom jednorodzinny).</summary>
    public sbyte Contract;
    /// <summary>Bliźniak: problemy przeniesione z pierwszej połowy na bieżący etap (do GameData.CareerTwinCarryMax).</summary>
    public byte TwinCarry;

    public CareerDef KDef => D.Career[Contract];

    /// <summary>Etapy kontraktu (z Aktem 0); Stage = 0..RouteCount()-1.</summary>
    public int RouteCount() => KDef.Count;

    /// <summary>Etapy Aktu 0 na początku (tylko Dom jednorodzinny).</summary>
    public int PreludeCount() => KDef.Prelude;

    /// <summary>Indeks etapu s kontraktu w GameData.Stages.</summary>
    public int StageId(int s) => KDef.First + s;

    public StageDef SDef(int s) => D.Stages[StageId(s)];

    public StageDef SDef() => SDef(Stage);

    public bool LastStage() => Stage == RouteCount() - 1;

    /// <summary>Wartość mechaniki aktu; kontrakt może mieć silniejsze porywy (Dom z poddaszem: co 4 tury).</summary>
    public int MechValue() => ADef.Mechanic == ActMechanic.Gust && KDef.Gust > 0 ? KDef.Gust : ADef.MechValue;
}
