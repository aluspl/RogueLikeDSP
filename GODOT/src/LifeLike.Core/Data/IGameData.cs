namespace LifeLike.Core.Data;

/// <summary>Wspólny kontrakt dla wszystkich definicji danych gry (JSON).</summary>
public interface IGameData
{
    string Id { get; }
    /// <summary>Zwraca listę błędów walidacji (pusta = poprawne).</summary>
    IEnumerable<string> Validate();
}
