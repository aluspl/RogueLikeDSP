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
    /// <summary>Nowe narzędzie w ręku; argument: poprzednia broń (WeaponOverride, -1 = broń zawodu).</summary>
    public event Action<int> ToolFound;
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
    public event Action<int, int, int> Achievements;
    /// <summary>v0.21.52 cz. c: koniec etapu – zadania wykonane teraz (bity), komplety kolekcji (bity), zadania łącznie przed.</summary>
    public event Action<int, int, int> Goals;
    /// <summary>Respekt za ukończony etap (zdobyty, razem w profilu) - już w profilu.</summary>
    public event Action<int, int> RespectGained;
    /// <summary>Wygrana odblokowała nagrodę za odbiór (indeks w GameData.Rewards).</summary>
    public event Action<int> RewardUnlocked;
    /// <summary>Druga szansa (Respekt): bohater przeżył z 1 HP.</summary>
    public event Action SecondChance;
    /// <summary>Akt 0 (pieczątki): zebrany dokument (indeks w GameData.Documents); drugi argument = komplet (schody otwarte).</summary>
    public event Action<int, bool> DocumentFound;
    /// <summary>Boss wszedł w drugą fazę (np. Decyzja odmowna: Odwołanie) - indeks definicji wroga.</summary>
    public event Action<int> BossPhase;

    public void RaiseRunStarted() => RunStarted?.Invoke();
    public void RaisePickedUp() => PickedUp?.Invoke();
    public void RaiseLevelUp(int level, bool abilityUp) => LevelUp?.Invoke(level, abilityUp);
    public void RaiseToolFound(int previous) => ToolFound?.Invoke(previous);
    public void RaiseDropped() => Dropped?.Invoke();
    public void RaiseGearEquipped(int slot) => GearEquipped?.Invoke(slot);
    public void RaiseAbilityReady() => AbilityReady?.Invoke();
    public void RaiseBossSpotted(int def) => BossSpotted?.Invoke(def);
    public void RaiseStageCleared() => StageCleared?.Invoke();
    public void RaiseRunEnded(bool won) => RunEnded?.Invoke(won);
    public void RaiseAchievements(int badges, int contracts, int secrets = 0) => Achievements?.Invoke(badges, contracts, secrets);
    public void RaiseGoals(int tasks, int collections, int tasksBefore) => Goals?.Invoke(tasks, collections, tasksBefore);
    public void RaiseRespectGained(int gained, int total) => RespectGained?.Invoke(gained, total);
    public void RaiseRewardUnlocked(int index) => RewardUnlocked?.Invoke(index);
    public void RaiseSecondChance() => SecondChance?.Invoke();
    public void RaiseDocumentFound(int doc, bool complete) => DocumentFound?.Invoke(doc, complete);
    public void RaiseBossPhase(int def) => BossPhase?.Invoke(def);
}
