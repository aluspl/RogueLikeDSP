namespace LifeLike.Core.Data;

/// <summary>Błąd w pliku danych gry (brak pola, nieznany identyfikator, złamane założenie).</summary>
public sealed class GameDataException(string message) : Exception(message);
