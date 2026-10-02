namespace LifeLike.Game.Hud;

/// <summary>Jedno powiadomienie push: treść, zakładka telefonu otwierana dotknięciem, wiek i pozycja w stosie.</summary>
public sealed class PushBanner
{
    public string Title = "";
    public string Body = "";
    /// <summary>Zakładka telefonu w grze (0 Zadania .. 4 Koszty) otwierana kliknięciem; -1 = brak.</summary>
    public int Tab = -1;
    /// <summary>Ikona z ui/menu_icons zamiast ikony PB (np. koperta sekretnego zlecenia); -1 = ikona PB.</summary>
    public int Icon = -1;
    /// <summary>Złota ramka (sekretne zlecenie).</summary>
    public bool Gold;
    public float Age;
    public float Y;
}
