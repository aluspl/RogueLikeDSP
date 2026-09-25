namespace LifeLike.Core.Data;

/// <summary>
/// Ścieżka (wariant kolejnego etapu) wybierana na harmonogramie: +/- problemów i znajdziek, budżet od razu, materiały
/// na start, pogoda tylko niekorzystna, bez wydarzenia na placu.
/// </summary>
public sealed record PathDef(string Id, string Name, string Short, string Desc, int Enemies, int Pickups, int Cash, int Materials,
    bool BadWeather, bool NoEvent);
