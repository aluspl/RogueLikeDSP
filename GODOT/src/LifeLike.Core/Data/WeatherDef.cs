namespace LifeLike.Core.Data;

/// <summary>
/// Pogoda dnia (sekcja "weather"): losowana na starcie etapu wagą Weight spośród dozwolonych na etapie (StagesMask).
/// Value: upał – tury mocy więcej; mróz – co ile tur problemy stoją; wiatr – o ile krótszy zasięg; deszcz – 1 kałuża na tyle pól.
/// Bad = niekorzystna (z niekorzystnym wydarzeniem na placu się nie łączy).
/// </summary>
public sealed record WeatherDef(string Id, string Name, string Short, string Info, WeatherEffect Effect, int Value, int Weight, bool Bad, int StagesMask);
