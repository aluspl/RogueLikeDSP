using System;

namespace LifeLike.Game.Session;

/// <summary>
/// Zdarzenia budowy dla warstwy prezentacji (obserwatorzy: banery push, dźwięki, efekty mapy). Odpowiednik
/// detect_events i przejść scen w GBA/src/main.cpp. Zgłasza je GameSession (TurnWatcher po każdej turze).
/// </summary>
public sealed class SessionEvents
{
    /// <summary>Nowa budowa (albo NG+): czyste powiadomienia.</summary>
    public event Action RunStarted;
    /// <summary>Zebrana znajdźka.</summary>
    public event Action PickedUp;
    /// <summary>Awans bohatera: nowy poziom, czy moc zawodu weszła na wyższą rangę.</summary>
    public event Action<int, bool> LevelUp;
    /// <summary>Nowe narzędzie w ręku.</summary>
    public event Action ToolFound;
    /// <summary>Coś wypadło z usuniętego problemu.</summary>
    public event Action Dropped;
    /// <summary>Założony sprzęt w slocie.</summary>
    public event Action<int> GearEquipped;
    /// <summary>Moc zawodu gotowa (koniec ładowania).</summary>
    public event Action AbilityReady;
    /// <summary>Boss etapu pierwszy raz w polu widzenia (indeks definicji wroga).</summary>
    public event Action<int> BossSpotted;
    /// <summary>Etap zaliczony (przed odznakami i bankowaniem).</summary>
    public event Action StageCleared;
    /// <summary>Koniec budowy: true = odbiór (wygrana), false = budowa wstrzymana.</summary>
    public event Action<bool> RunEnded;
    /// <summary>Nowe odznaki i wykonane zlecenia (maski bitowe).</summary>
    public event Action<int, int> Achievements;

    public void RaiseRunStarted() => RunStarted?.Invoke();
    public void RaisePickedUp() => PickedUp?.Invoke();
    public void RaiseLevelUp(int level, bool abilityUp) => LevelUp?.Invoke(level, abilityUp);
    public void RaiseToolFound() => ToolFound?.Invoke();
    public void RaiseDropped() => Dropped?.Invoke();
    public void RaiseGearEquipped(int slot) => GearEquipped?.Invoke(slot);
    public void RaiseAbilityReady() => AbilityReady?.Invoke();
    public void RaiseBossSpotted(int def) => BossSpotted?.Invoke(def);
    public void RaiseStageCleared() => StageCleared?.Invoke();
    public void RaiseRunEnded(bool won) => RunEnded?.Invoke(won);
    public void RaiseAchievements(int badges, int contracts) => Achievements?.Invoke(badges, contracts);
}
