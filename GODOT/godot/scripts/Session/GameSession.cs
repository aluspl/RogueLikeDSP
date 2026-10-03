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
    /// <summary>Nagroda za odbiór odblokowana ostatnią wygraną (indeks w GameData.Rewards), -1 = brak.</summary>
    public int LastReward { get; private set; } = -1;
    /// <summary>Rekord sprzed zakończonej budowy (do „Nowy rekord!”).</summary>
    public int PrevBest { get; private set; }
    /// <summary>Codzienna budowa pobiła najlepszy wynik dnia (Daily.Record).</summary>
    public bool DailyRecord { get; private set; }
    /// <summary>Tabela wyników codziennej budowy (na razie lokalna zaślepka).</summary>
    public ILeaderboard Leaderboard { get; set; } = new LocalLeaderboard();
    /// <summary>Stała data „dzisiaj” (zrzuty ekranu, test dymny); null = data systemowa.</summary>
    public System.Tuple<int, int, int> FixedToday { get; set; }

    /// <summary>Dzisiejsza data (system albo FixedToday).</summary>
    public (int Y, int M, int D) Today
    {
        get
        {
            if (FixedToday is not null) return (FixedToday.Item1, FixedToday.Item2, FixedToday.Item3);
            var t = Time.GetDateDictFromSystem();
            return ((int)t["year"], (int)t["month"], (int)t["day"]);
        }
    }

    /// <summary>Numer dzisiejszej codziennej budowy.</summary>
    public int TodayNumber
    {
        get
        {
            var (y, m, d) = Today;
            return Daily.Number(Data, y, m, d);
        }
    }
    /// <summary>Numer bieżącego tygodnia (wyzwanie tygodnia #34).</summary>
    public int TodayWeek
    {
        get
        {
            var (y, m, d) = Today;
            return Weekly.Number(Data, y, m, d);
        }
    }

    /// <summary>Wyzwanie tygodnia pobiło najlepszy wynik tygodnia (Weekly.Record).</summary>
    public bool WeeklyRecord { get; private set; }

    /// <summary>v0.21.52 cz. d: ostatnia budowa to pierwsza wygrana kontraktu (baner z nagrodą).</summary>
    public bool LastCareerFirst { get; private set; }

    /// <summary>v0.21.52 cz. d: kontrakty odblokowane na koniec ostatniej budowy (bity GameData.Career).</summary>
    public int LastCareerNew { get; set; }

    /// <summary>Wątki fabuły odblokowane na koniec ostatniej budowy (bity GameData.StoryArc).</summary>
    public uint LastStory { get; set; }

    /// <summary>v0.21.52 cz. b: poziom inspektora i mistrzostwo z ostatniej budowy (paski i banery końca budowy).</summary>
    public ProgressGain LastProgress { get; private set; } = new();

    /// <summary>v0.21.52 cz. b (#48): stopnie inwestora przed i po ostatniej budowie (banery nowych stopni).</summary>
    public int LastStakeBefore { get; private set; }
    public int LastStakeAfter { get; private set; }

    /// <summary>
    /// v0.21.52 cz. c: zadania dnia / tygodnia wykonane na końcu budowy (bity), komplety kolekcji (bity), wykonane zadania
    /// łącznie przed budową i najdłuższa seria dni przed / po (banery na planszy końcowej).
    /// </summary>
    public int LastTasks { get; private set; }
    public int LastCollections { get; private set; }
    public int LastTasksBefore { get; private set; }
    public int LastStreakBefore { get; private set; }
    public int LastStreakAfter { get; private set; }

    /// <summary>v0.21.52 cz. c: zadania nowego dnia / tygodnia (data z systemu); true = zmieniono (zapisz profil).</summary>
    public bool RollTasks() => DailyTasks.Roll(Profile, TodayNumber, TodayWeek);

    /// <summary>Postęp zadań z budowy (koniec etapu / budowy, porzucenie) i komplety kolekcji – jak bank_tasks + check_collections na GBA.</summary>
    private (int Tasks, int Collections, int TasksBefore) BankGoals()
    {
        var before = Profile.TasksTotal;
        var tasks = DailyTasks.Bank(Data, Profile, Game, TodayNumber, TodayWeek);
        return (tasks, CollectionBook.Check(Data, Profile), before);
    }

    /// <summary>Rady kierownika z game.json (ekran harmonogramu).</summary>
    public string[] Tips { get; set; } = [];

    /// <summary>Rada na harmonogram po bieżącym etapie: kolejna z każdym etapem, bez losowania (jak GBA).</summary>
    public string Tip => Tips.Length == 0 ? "" : Tips[(Game.Stage + Game.Tier * 3) % Tips.Length];

    /// <summary>Pierwszy etap tej budowy jeszcze nie wystartował (podpowiedzi na start).</summary>
    public bool FirstStage { get; set; }

    public void Save()
    {
        if (Persist) GodotDataSource.SaveProfile(Profile);
    }

    /// <summary>Zapis przerwanej budowy (user://run.sav, jak run_save w SRAM na GBA): na starcie etapu, przy
    /// „Zapisz i wyjdź” i gdy system usypia aplikację w trakcie gry.</summary>
    public void SaveRun()
    {
        if (Persist && Game.St == GameStatus.Playing) GodotDataSource.SaveRun(RunSave.Make(Game));
    }

    /// <summary>Czy jest przerwana budowa do wznowienia (ta sama wersja danych).</summary>
    public bool HasRun => Persist && GodotDataSource.LoadRun() is { } s && s.Valid(Data);

    /// <summary>Wznowienie przerwanej budowy (Kontynuuj budowę na tytule).</summary>
    public bool ResumeRun()
    {
        var s = Persist ? GodotDataSource.LoadRun() : null;
        if (s is null || !s.Valid(Data))
        {
            ClearRun();
            return false;
        }
        Game.FromBytes(s.Data);
        ClassId = Game.Cls;
        Note = "";
        FirstStage = false;
        _watcher.Reset(Game);
        return true;
    }

    public void ClearRun()
    {
        if (Persist) GodotDataSource.DeleteRun();
    }

    /// <summary>Porzuć budowę (jak „Porzuć budowę” w telefonie na GBA): rekord, odznaki i zlecenia z przerwanej
    /// budowy się liczą, doświadczenie trafia do profilu, zapis budowy znika.</summary>
    public void AbandonRun()
    {
        if (Game.Score > Profile.Best) Profile.Best = Game.Score;
        var progress = CheckProgress();
        LastGained = Meta.BankXp(Profile, Game);
        LastProgress = Progress.Bank(Data, Profile, Game); // v0.21.52 cz. b: inspektor i mistrzostwo
        BankGoals(); // cz. c: zadania, kolekcje
        Save();
        ClearRun();
        Note = $"Budowa porzucona: +{LastGained} dośw." + (progress.Length > 0 ? " " + progress : "");
    }

    /// <summary>Nowa budowa wybranym zawodem i trudnością (jak wybór zawodu -> gra na GBA).</summary>
    public void StartRun()
    {
        var seed = Seed != 0 ? Seed : GD.Randi() | 1u;
        Game.NewRun(ClassId, seed, Difficulty, Meta.Mods(Data, Profile, ClassId), Career.Selected(Data, Profile)); // Mods przed StartRun: ranga pamiątki; cz. b: mistrzostwo; cz. d: kontrakt
        Meta.StartRun(Data, Profile);
        Save();
        SaveRun();
        Note = "";
        LastSecrets = 0;
        FirstStage = true;
        Events.RaiseRunStarted();
        _watcher.Reset(Game);
        GD.Print($"Nowa budowa: {Game.CDef.Name}, {Data.Difficulties[Difficulty].Name}, seed {seed}");
    }

    /// <summary>Codzienna budowa dnia `day`: zawód, seed i modyfikatory dnia, bez Szkoleń i pamiątek (liczniki profilu jak zwykle).</summary>
    public void StartDaily(int day)
    {
        Daily.Start(Game, day);
        ClassId = Game.Cls;
        Meta.StartRun(Data, Profile);
        Save();
        SaveRun();
        Note = "";
        LastSecrets = 0;
        FirstStage = true;
        Events.RaiseRunStarted();
        _watcher.Reset(Game);
        GD.Print($"Codzienna budowa nr {day}: {Game.CDef.Name}, seed {Game.RunSeed}");
    }

    /// <summary>Wyzwanie tygodnia `week`: zawód, seed i zasady tygodnia, bez Szkoleń i pamiątek.</summary>
    public void StartWeekly(int week)
    {
        Weekly.Start(Game, week);
        ClassId = Game.Cls;
        Meta.StartRun(Data, Profile);
        Save();
        SaveRun();
        Note = "";
        LastSecrets = 0;
        FirstStage = true;
        Events.RaiseRunStarted();
        _watcher.Reset(Game);
        GD.Print($"Wyzwanie tygodnia nr {week}: {Data.Weekly[Game.Bonus.Weekly].Name}, {Game.CDef.Name}, seed {Game.RunSeed}");
    }

    /// <summary>Kolejny etap (po harmonogramie albo Hurtowni).</summary>
    public void NextStage()
    {
        Note = "";
        Game.NextStage();
        _watcher.Reset(Game);
        SaveRun(); // autozapis na starcie etapu (jak GBA)
    }

    /// <summary>Kolejna budowa po odbiorze (NG+), te same statystyki.</summary>
    public void NewGamePlus()
    {
        Game.NewGamePlus();
        Note = "";
        _watcher.Reset(Game);
        SaveRun();
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
            Events.RaiseRespectGained(g.StageRespect(), Profile.Respect);
            var (tasks, coll, before) = BankGoals(); // v0.21.52 cz. c: zadania dnia / tygodnia i kolekcje – na bieżąco
            Events.RaiseGoals(tasks, coll, before);
            if (g.Score > Profile.Best) Profile.Best = g.Score;
            Meta.BankXp(Profile, g);
            Save();
            return TurnOutcome.StageCleared;
        }
        if (g.St is GameStatus.Won or GameStatus.Dead)
        {
            var won = g.St == GameStatus.Won;
            PrevBest = Profile.Best;
            if (g.Score > Profile.Best) Profile.Best = g.Score;
            LastStreakBefore = Profile.StreakBest; // v0.21.52 cz. c: seria dni – nagrody za nowy rekord serii
            DailyRecord = g.Daily && Daily.Record(Data, Profile, g.DailyDay, g.Score, won);
            LastStreakAfter = Profile.StreakBest;
            LastReward = won ? Meta.RecordWin(Data, Profile) : -1;   // nagroda za odbiór: każda wygrana odblokowuje kolejną
            LastCareerFirst = won && Career.Win(Data, Profile, g); // v0.21.52 cz. d: pierwsza wygrana kontraktu
            if (won) Meta.AddHouse(Profile, g);
            LastStakeBefore = Progress.StakeRank(Data, Profile); // v0.21.52 cz. b: stopnie inwestora przed rekordem stawki
            Events.RaiseRunEnded(won);
            if (LastReward >= 0) Events.RaiseRewardUnlocked(LastReward);
            Note = CheckProgress();
            if (won) Events.RaiseRespectGained(g.StageRespect(), Profile.Respect);
            LastGained = Meta.BankXp(Profile, g);
            WeeklyRecord = g.WeeklyWeek != 0 && Weekly.Record(Data, Profile, g.WeeklyWeek, g.Score, won); // wyzwanie tygodnia (#34)
            LastProgress = Progress.Bank(Data, Profile, g); // v0.21.52 cz. b: poziom inspektora i mistrzostwo (przed fabułą)
            LastStakeAfter = Progress.StakeRank(Data, Profile);
            (LastTasks, LastCollections, LastTasksBefore) = BankGoals(); // cz. c: zadania dnia / tygodnia, kolekcje
            LastStory = Story.Check(Data, Profile, g); // fabuła (#35): nowe wątki SMS za kamienie milowe (też od inspektora)
            LastCareerNew = Career.Announce(Data, Profile); // v0.21.52 cz. d: nowe kontrakty na mapie kariery
            Save();
            ClearRun();
            return TurnOutcome.RunEnded;
        }
        return g.St == GameStatus.Playing && g.HasOffer ? TurnOutcome.Offer : TurnOutcome.None;
    }

    /// <summary>Odznaki (z bankowaniem liczników zleceń), zlecenia i sekretne zlecenia (#39) - jak main.cpp na GBA:
    /// check_badges, check_contracts, check_secrets.</summary>
    private string CheckProgress()
    {
        var got = Meta.CheckBadges(Data, Profile, Game);
        var done = Meta.CheckContracts(Data, Profile);
        var secrets = LifeLike.Core.Secrets.Check(Data, Profile, Game);
        LastSecrets |= secrets;
        Events.RaiseAchievements(got, done, secrets);
        return ProgressNotes.Join(Data, got, done, secrets);
    }

    /// <summary>Sekretne zlecenia wykonane w tej budowie (bity GameData.Secrets) – plansza końcowa.</summary>
    public int LastSecrets { get; private set; }
}
