using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Session;

/// <summary>
/// Budowa i profil gracza: opakowuje LifeLike.Core (port 1:1 z GBA) - start budowy, kolejny etap, NG+, odznaki
/// i zlecenia, bankowanie doświadczenia, zapis profilu. Zdarzenia (SessionEvents) zamiast grzebania w stanie
/// rdzenia przez ekrany: banery, dźwięki i efekty mapy tylko je obserwują.
/// </summary>
public sealed class GameSession
{
    private readonly TurnWatcher _watcher;

    public GameSession(GameData d, Profile profile, bool persist, uint seed)
    {
        Data = d;
        Profile = profile;
        Persist = persist;
        Seed = seed;
        Difficulty = d.DefaultDifficulty;
        Game = new CoreGame(d);
        _watcher = new TurnWatcher(Events);
    }

    public GameData Data { get; }
    public CoreGame Game { get; }
    public SessionEvents Events { get; } = new();
    public Profile Profile { get; set; }
    /// <summary>false = profil tylko w pamięci (test dymny, zrzuty ekranu).</summary>
    public bool Persist { get; set; }
    /// <summary>Stały seed budowy (0 = losowy przy każdej budowie).</summary>
    public uint Seed { get; set; }
    public int ClassId { get; set; } = 1;
    public int Difficulty { get; set; }
    /// <summary>Notatka o odznakach / zleceniach z ostatniego etapu albo budowy.</summary>
    public string Note { get; set; } = "";
    /// <summary>Doświadczenie zabankowane na koniec budowy.</summary>
    public int LastGained { get; private set; }
    /// <summary>Pierwszy etap tej budowy jeszcze nie wystartował (podpowiedzi na start).</summary>
    public bool FirstStage { get; set; }

    public void Save()
    {
        if (Persist) GodotDataSource.SaveProfile(Profile);
    }

    /// <summary>Nowa budowa wybranym zawodem i trudnością (jak wybór zawodu -> gra na GBA).</summary>
    public void StartRun()
    {
        var seed = Seed != 0 ? Seed : GD.Randi() | 1u;
        Game.NewRun(ClassId, seed, Difficulty, Meta.Mods(Data, Profile)); // Mods przed StartRun: ranga pamiątki z budów przed tą
        Meta.StartRun(Data, Profile);
        Save();
        Note = "";
        FirstStage = true;
        Events.RaiseRunStarted();
        _watcher.Reset(Game);
        GD.Print($"Nowa budowa: {Game.CDef.Name}, {Data.Difficulties[Difficulty].Name}, seed {seed}");
    }

    /// <summary>Kolejny etap (po harmonogramie albo Hurtowni).</summary>
    public void NextStage()
    {
        Note = "";
        Game.NextStage();
        _watcher.Reset(Game);
    }

    /// <summary>Kolejna budowa po odbiorze (NG+), te same statystyki.</summary>
    public void NewGamePlus()
    {
        Game.NewGamePlus();
        Note = "";
        _watcher.Reset(Game);
    }

    /// <summary>Stan odniesienia zdarzeń tury bez powiadomień (po ręcznej zmianie stanu).</summary>
    public void ResetWatch() => _watcher.Reset(Game);

    /// <summary>Zdarzenia tury po odświeżeniu widoku (awans, drop, moc gotowa...).</summary>
    public void Watch() => _watcher.Check(Game);

    /// <summary>
    /// Po akcji gracza: zaliczony etap albo koniec budowy (odznaki, zlecenia, rekord, bankowanie doświadczenia,
    /// zapis) jak main.cpp na GBA, albo paczka sprzętu czekająca na decyzję.
    /// </summary>
    public TurnOutcome Resolve()
    {
        var g = Game;
        if (g.St == GameStatus.StageClear)
        {
            Events.RaiseStageCleared();
            Note = CheckProgress();
            if (g.Score > Profile.Best) Profile.Best = g.Score;
            Meta.BankXp(Profile, g);
            Save();
            return TurnOutcome.StageCleared;
        }
        if (g.St is GameStatus.Won or GameStatus.Dead)
        {
            var won = g.St == GameStatus.Won;
            if (g.Score > Profile.Best) Profile.Best = g.Score;
            if (won)
            {
                ++Profile.Wins;
                Meta.AddHouse(Profile, g);
            }
            Events.RaiseRunEnded(won);
            Note = CheckProgress();
            LastGained = Meta.BankXp(Profile, g);
            Save();
            return TurnOutcome.RunEnded;
        }
        return g.St == GameStatus.Playing && g.HasOffer ? TurnOutcome.Offer : TurnOutcome.None;
    }

    /// <summary>Odznaki (z bankowaniem liczników zleceń) i zlecenia - jak main.cpp na GBA: check_badges, potem check_contracts.</summary>
    private string CheckProgress()
    {
        var got = Meta.CheckBadges(Data, Profile, Game);
        var done = Meta.CheckContracts(Data, Profile);
        Events.RaiseAchievements(got, done);
        return ProgressNotes.Join(Data, got, done);
    }
}
