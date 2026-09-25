namespace LifeLike.Core.Data;

/// <summary>
/// Fachowiec z brygady (sekcja "brigade"): wzywany raz na etap z telefonu za Price zł z budżetu budowy.
/// Value: pompa – obrażenia, BHP-owiec – obrona, pomocnik – cios; Turns: czas ochrony / pomocy; Reach: zasięg pompy;
/// Cost: odblokowanie w Szkoleniach (0 = od początku); Frame: klatka postaci pomocnika.
/// </summary>
public sealed record HelperDef(string Id, string Name, string Desc, HelperEffect Effect, int Value, int Turns, int Reach, int Price, int Cost, int Frame);
