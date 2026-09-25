namespace LifeLike.Game.Session;

/// <summary>Co wynikło z tury gracza (GameSession.Resolve): ekran, który ma się pokazać dalej.</summary>
public enum TurnOutcome
{
    /// <summary>Gra toczy się dalej.</summary>
    None,
    /// <summary>Etap zaliczony - harmonogram (i ewentualnie Hurtownia).</summary>
    StageCleared,
    /// <summary>Koniec budowy (odbiór albo wstrzymanie) - SMS i plansza końcowa.</summary>
    RunEnded,
    /// <summary>Paczka sprzętu czeka na decyzję (zajęty slot).</summary>
    Offer,
}
