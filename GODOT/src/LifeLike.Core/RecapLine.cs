namespace LifeLike.Core;

/// <summary>Wiersz podsumowania budowy (#33, core::recap_line): tekst, dopisek po prawej (dni, usunięte) i kolor.</summary>
public sealed class RecapLine
{
    public Message Text { get; } = new();
    public Message Tail { get; } = new();
    public LogKind Ink { get; set; } = LogKind.Info;
}
