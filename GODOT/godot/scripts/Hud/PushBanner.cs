namespace LifeLike.Game.Hud;

/// <summary>Jedno powiadomienie push: treść, zakładka telefonu otwierana dotknięciem, wiek i pozycja w stosie.</summary>
public sealed class PushBanner
{
    public string Title = "";
    public string Body = "";
    /// <summary>Zakładka telefonu w grze (0 Zadania .. 4 Koszty) otwierana kliknięciem; -1 = brak.</summary>
    public int Tab = -1;
    public float Age;
    public float Y;
}
