using System;
using System.Linq;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Kompozycja gry: dane (game.json wspólny z GBA) -> LifeLike.Core.Game -> widok mapy, HUD, telefon i ekrany.
/// Cała logika siedzi w LifeLike.Core (port 1:1 z GBA); tu jest tylko prezentacja: przejścia ekranów (jak sceny
/// w GBA/src/main.cpp), wejście (klawiatura/pad/mysz), dźwięk i powiadomienia. Obraz: 640x360 skalowane 2x.
/// Pliki częściowe: Main.Screens.cs (ekrany i telefon), Main.Input.cs (wejście), Main.Debug.cs (zrzuty, test dymny).
/// </summary>
public partial class Main : Node2D
{
    private enum Screen
    {
        Title,
        ClassSelect,
        Profile,
        Playing,
        Phone,
        ActionMenu,
        GearOffer,
        StageClear,
        StageCard,
        Hurtownia,
        EndMessage,
        End,
    }

    [Export] public uint Seed { get; set; } // 0 = losowy

    private GameData _d;
    private Profile _profile;
    private CoreGame _g;
    private Screen _screen = Screen.Title;
    private int _cls, _diff;
    private int _titleSel;
    private int _phoneTab = 2;   // ostatnio otwarta zakładka telefonu (Start), jak phone_tab na GBA
    private string _note = "";
    private int _lastGained;
    private bool _persistProfile = true;
    private bool _firstStage;

    private WorldView _view;
    private Hud _hud;
    private Phone _phone;
    private Backdrop _backdrop;
    private PushBanners _banners;
    private TitleScreen _title;
    private ClassSelectScreen _classSelect;
    private EndScreen _end;
    private EventWatcher _events;
    private HurtowniaPage _hurtownia;

    public override void _Ready()
    {
        GameInput.Register();
        RenderingServer.SetDefaultClearColor(Pal.Void);
        try
        {
            _d = GodotDataSource.LoadGameData();
        }
        catch (Exception ex)
        {
            GD.PushError($"Dane gry: {ex.Message}");
            GetTree().Quit(1);
            return;
        }
        var args = OS.GetCmdlineUserArgs();
        var smoke = args.Contains("--smoke");
        var seedArg = Array.IndexOf(args, "--seed");
        if (seedArg >= 0 && seedArg + 1 < args.Length && uint.TryParse(args[seedArg + 1], out var s)) Seed = s;

        _persistProfile = !smoke;
        _profile = smoke ? Meta.NewProfile(_d) : GodotDataSource.LoadProfile(_d);
        _diff = _d.DefaultDifficulty;
        _cls = 1;
        _g = new CoreGame(_d);
        BuildNodes(!smoke && !args.Contains("--screenshot"));

        if (smoke)
        {
            RunSmokeTest();
            return;
        }
        var shotArg = Array.IndexOf(args, "--screenshot");
        if (shotArg >= 0 && shotArg + 1 < args.Length)
        {
            TakeScreenshot(args[shotArg + 1]);
            return;
        }
        ShowTitle();
    }

    public override void _ExitTree()
    {
        Assets.ClearCache();
        Ui.ClearCache();
        PixelFont.Release();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private void BuildNodes(bool sound)
    {
        if (sound) AddChild(new Sfx()); // test dymny i zrzuty bez dźwięku (Sfx.Play jest wtedy pusty)
        _view = new WorldView();
        AddChild(_view);
        _view.Bind(_g);
        _hud = new Hud();
        AddChild(_hud);
        _view.OnFlash = (c, a) => _hud.Tint.Flash(c, a);

        var screens = new CanvasLayer { Layer = 2 };
        AddChild(screens);
        _title = new TitleScreen { Visible = false, Version = _d.Version };
        _classSelect = new ClassSelectScreen { Visible = false };
        _end = new EndScreen { Visible = false };
        screens.AddChild(_title);
        screens.AddChild(_classSelect);
        screens.AddChild(_end);

        var phoneLayer = new CanvasLayer { Layer = 3 };
        AddChild(phoneLayer);
        _backdrop = new Backdrop();
        _phone = new Phone();
        phoneLayer.AddChild(_backdrop);
        phoneLayer.AddChild(_phone);

        var top = new CanvasLayer { Layer = 4 };
        AddChild(top);
        _banners = new PushBanners();
        top.AddChild(_banners);
        _events = new EventWatcher(_banners, _view);
    }

    /// <summary>Widoczność warstw wg ekranu: mapa + HUD w trakcie budowy, telefon z rozmytym tłem, ekrany pełne.</summary>
    private void ApplyScreen(Screen s)
    {
        _screen = s;
        var inRun = s is Screen.Playing or Screen.Phone or Screen.ActionMenu or Screen.GearOffer or Screen.StageClear
            or Screen.StageCard or Screen.Hurtownia or Screen.EndMessage;
        _view.Visible = inRun;
        _hud.Visible = inRun;
        _title.Visible = s is Screen.Title or Screen.Profile;
        _classSelect.Visible = s == Screen.ClassSelect;
        _end.Visible = s == Screen.End;
        var phone = s is Screen.Profile or Screen.Phone or Screen.GearOffer or Screen.StageClear or Screen.StageCard
            or Screen.Hurtownia or Screen.EndMessage;
        _backdrop.SetOn(phone);
        _banners.Compact = phone;
        if (!phone) _phone.Close();
        if (s != Screen.ActionMenu) _hud.ShowHint(null);
        Sfx.Music(s is Screen.Title or Screen.ClassSelect or Screen.Profile ? "title" : s == Screen.End ? "" : "game");
    }

    // ------------------------------------------------------------------ przebieg budowy
    private void StartRun()
    {
        var seed = Seed != 0 ? Seed : GD.Randi() | 1u;
        _g.NewRun(_cls, seed, _diff, Meta.Mods(_d, _profile)); // Mods przed StartRun: ranga pamiątki z budów przed tą
        Meta.StartRun(_d, _profile);
        SaveProfile();
        _note = "";
        _firstStage = true;
        _banners.Clear();
        _events.Reset(_g);
        GD.Print($"Nowa budowa: {_g.CDef.Name}, {_d.Difficulties[_diff].Name}, seed {seed}");
        Refresh();
        ShowStageCard();
    }

    private void SaveProfile()
    {
        if (_persistProfile) GodotDataSource.SaveProfile(_profile);
    }

    /// <summary>Powiadomienia o nowych odznakach i zleceniach (push_achievements na GBA).</summary>
    private void PushAchievements(int badges, int contracts)
    {
        for (var i = 0; i < _d.Badges.Length; i++)
        {
            if ((badges & (1 << i)) != 0) _banners.Push("Odznaka: " + _d.Badges[i].Name, $"+{_d.Badges[i].Xp} dośw.");
        }
        for (var i = 0; i < _d.Contracts.Length; i++)
        {
            if ((contracts & (1 << i)) == 0) continue;
            var c = _d.Contracts[i];
            _banners.Push("Zlecenie: " + c.Name, c.Keepsake >= 0 ? $"+{c.Xp}, {_d.Keepsakes[c.Keepsake].Name}" : $"Wykonane! +{c.Xp} dośw.");
        }
    }

    private string BadgeNote(int got) =>
        got == 0 ? "" : "Odznaki: " + string.Join(", ", _d.Badges.Where((_, i) => (got & (1 << i)) != 0)
            .Select(b => $"{b.Name} (+{b.Xp}, uprawnienie: {RunMods.PerkLabel(b.Bonus)})"));

    private string ContractNote(int got) =>
        got == 0 ? "" : "Zlecenia wykonane: " + string.Join(", ", _d.Contracts.Where((_, i) => (got & (1 << i)) != 0)
            .Select(c => $"{c.Name} (+{c.Xp}{(c.Keepsake >= 0 ? ", pamiątka " + _d.Keepsakes[c.Keepsake].Name : "")})"));

    /// <summary>Odznaki (z bankowaniem liczników zleceń) i zlecenia - jak main.cpp na GBA: check_badges, potem check_contracts.</summary>
    private string CheckProgress()
    {
        var got = Meta.CheckBadges(_d, _profile, _g);
        var done = Meta.CheckContracts(_d, _profile);
        PushAchievements(got, done);
        return string.Join("\n", new[] { BadgeNote(got), ContractNote(done) }.Where(s => s.Length > 0));
    }

    /// <summary>Po akcji gracza: odznaki, bankowanie doświadczenia, przejścia ekranów (jak main.cpp na GBA).</summary>
    private void AfterAction(bool acted)
    {
        if (!acted)
        {
            Refresh();
            return;
        }
        Refresh();
        if (_g.St == GameStatus.StageClear)
        {
            Sfx.Play("stage");
            _banners.Clear();
            _banners.Push("Etap zaliczony", _d.Stages[_g.Stage].Name);
            _note = CheckProgress();
            if (_g.Score > _profile.Best) _profile.Best = _g.Score;
            Meta.BankXp(_profile, _g);
            SaveProfile();
            ShowStageClear();
        }
        else if (_g.St is GameStatus.Won or GameStatus.Dead)
        {
            var won = _g.St == GameStatus.Won;
            if (_g.Score > _profile.Best) _profile.Best = _g.Score;
            if (won)
            {
                ++_profile.Wins;
                Meta.AddHouse(_profile, _g);
                _view.Confetti();
            }
            _note = CheckProgress();
            _lastGained = Meta.BankXp(_profile, _g);
            SaveProfile();
            Sfx.Play(won ? "level" : "hurt");
            ShowEndMessage();
        }
        else if (_g.St == GameStatus.Playing && _g.HasOffer && _screen == Screen.Playing)
        {
            ShowOffer();
        }
    }

    /// <summary>Dalej z harmonogramu / karty etapu / Hurtowni.</summary>
    private void Advance()
    {
        if (_screen == Screen.StageClear && _g.ActCleared)
        {
            _note = "";
            ShowHurtownia();
            return;
        }
        if (_screen == Screen.StageCard)
        {
            ApplyScreen(Screen.Playing);
            if (_firstStage && _g.Stage == 0 && _g.Tier == 0) FirstStageBanners();
            _firstStage = false;
            Refresh();
            return;
        }
        _note = "";
        _g.NextStage();
        _events.Reset(_g);
        Refresh();
        ShowStageCard();
    }

    /// <summary>Podpowiedź na start budowy: moc pod R i zabrana pamiątka z rangą (jak na GBA).</summary>
    private void FirstStageBanners()
    {
        _banners.Push("R: " + _g.CDef.AbilityName, _g.CDef.AbilityDesc);
        var k = Meta.SelectedKeepsake(_d, _profile);
        if (k < 0) return;
        var runs = _profile.KeepsakeRuns[k] - 1;
        var rank = 1 + (runs >= _d.KeepsakeRankRuns[0] ? 1 : 0) + (runs >= _d.KeepsakeRankRuns[1] ? 1 : 0);
        var kd = _d.Keepsakes[k];
        _banners.Push($"{kd.Name} {UiText.Roman(rank - 1)}", RunMods.PerkLabel(new Perk(kd.Effect, kd.Values[rank - 1])));
    }

    private void Refresh()
    {
        Span<sbyte> t = stackalloc sbyte[CoreGame.MaxEnemies];
        _view.Mark(_screen is Screen.Playing or Screen.Phone && _g.St == GameStatus.Playing && _g.TargetsInRange(t) > 0 ? t[0] : -1);
        _view.Sync();
        _events.Check(_g);
        _hud.ShowGame(_g);
        if (_phone.IsOpen) _phone.QueueRedraw();
    }
}
