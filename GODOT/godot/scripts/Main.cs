using System;
using System.Linq;
using System.Text;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Kompozycja gry: dane (game.json wspólny z GBA) -> LifeLike.Core.Game -> widok + HUD.
/// Cała logika siedzi w LifeLike.Core (port 1:1 z GBA); tu jest tylko klej z silnikiem: ekrany tekstowe,
/// wejście (klawiatura/pad/mysz) i render. Telefon i 2.5D to kolejne kamienie milowe (docs/KONCEPCJA.md).
/// </summary>
public partial class Main : Node2D
{
    private enum Screen { Title, Training, Playing, Overview, StageClear, Hurtownia, End }

    [Export] public uint Seed { get; set; } // 0 = losowy

    private GameData _d;
    private Profile _profile;
    private CoreGame _g;
    private Screen _screen = Screen.Title;
    private int _cls, _diff, _sel;
    private string _note = "";
    private int _lastGained;
    private bool _persistProfile = true;

    private WorldView _view;
    private Hud _hud;
    private Camera2D _camera;

    public override void _Ready()
    {
        GameInput.Register();
        RenderingServer.SetDefaultClearColor(Colors.Black);
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
        _g = new CoreGame(_d);
        _view = new WorldView();
        _hud = new Hud();
        _camera = new Camera2D { Zoom = new Vector2(1.4f, 1.4f), PositionSmoothingEnabled = true };
        AddChild(_view);
        AddChild(_hud);
        AddChild(_camera);
        _view.Bind(_g);

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

    /// <summary>--screenshot plik.png: bot gra kilkadziesiąt tur, potem zrzut ekranu i wyjście (podgląd grafiki bez klikania).</summary>
    private async void TakeScreenshot(string path)
    {
        _persistProfile = false;
        _profile = Meta.NewProfile(_d);
        Seed = Seed != 0 ? Seed : 424242u;
        StartRun();
        for (var i = 0; i < 10 && _screen == Screen.Playing; i++)
        {
            Bot.StepSmart(_g);
            AfterAction(true);
        }
        for (var i = 0; i < 30; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GetViewport().GetTexture().GetImage().SavePng(path);
        GD.Print($"Zrzut ekranu: {path}");
        GetTree().Quit();
    }

    // ------------------------------------------------------------------ test dymny
    /// <summary>
    /// Test dymny: godot --headless --path GODOT/godot -- --smoke. Bot (ten sam co w testach rdzenia) gra kilka etapów
    /// przez warstwę Godota (odświeżanie widoku, HUD, ekrany harmonogramu i Hurtowni), kod wyjścia 0 = OK.
    /// </summary>
    private void RunSmokeTest()
    {
        try
        {
            _cls = 0;
            _diff = 0;
            Seed = Seed != 0 ? Seed : 424242u;
            StartRun();
            var steps = 0;
            for (; steps < 4000 && _g.Stage < 3; steps++)
            {
                if (_screen == Screen.StageClear)
                {
                    Advance();
                    continue;
                }
                if (_screen == Screen.Hurtownia)
                {
                    Bot.Shop(_g);
                    ShowHurtownia();
                    Advance();
                    continue;
                }
                if (_screen != Screen.Playing) break;
                Bot.StepSmart(_g);
                AfterAction(true);
            }
            ShowOverview();
            var ok = _g.Stage >= 3 || _g.St is GameStatus.Dead or GameStatus.Won;
            GD.Print($"SMOKE {(ok ? "OK" : "FAIL")}: dane {_d.Version}, zawody {_d.Classes.Length}, etap {_g.Stage + 1}, " +
                     $"dzień {_g.Turns}, HP {_g.Hero.Hp}/{_g.Hero.MaxHp}, wynik {_g.Score}, budżet {_g.Cash}, kroki {steps}, ekran {_screen}");
            GetTree().Quit(ok ? 0 : 1);
        }
        catch (Exception ex)
        {
            GD.PushError($"SMOKE FAIL: {ex}");
            GetTree().Quit(1);
        }
    }

    // ------------------------------------------------------------------ ekrany
    private void ShowTitle()
    {
        _screen = Screen.Title;
        var sb = new StringBuilder();
        sb.AppendLine($"PLANBUDOWLANY ROGUELIKE  {_d.Version}");
        sb.AppendLine($"Rekord: {_profile.Best} · Budowy: {_profile.Runs} · Wygrane: {_profile.Wins} · Doświadczenie: {_profile.Xp}");
        sb.AppendLine();
        sb.AppendLine("Zawód (←/→):");
        for (var i = 0; i < _d.Classes.Length; i++)
        {
            var c = _d.Classes[i];
            var mark = i == _cls ? "> " : "  ";
            var lockTxt = Meta.ClassUnlocked(_profile, i) ? "" : $"  [zablokowany – {_d.ClassCost} dośw. w Szkoleniach]";
            sb.AppendLine($"{mark}{c.Name} – {c.Desc} HP {c.MaxHealth}, moc {c.AbilityName}{lockTxt}");
        }
        sb.AppendLine();
        var diffLock = Meta.DifficultyUnlocked(_d, _profile, _diff) ? "" : " [zablokowany]";
        sb.AppendLine($"Poziom (↑/↓): {_d.Difficulties[_diff].Name}{diffLock}");
        sb.AppendLine();
        sb.AppendLine("Spacja/Enter: start · K: Szkolenia (sklep za doświadczenie)");
        if (_note.Length > 0) sb.AppendLine().AppendLine(_note);
        _hud.ShowPanel(sb.ToString());
    }

    private string[] TrainingEntries()
    {
        var list = _d.Upgrades.Select((u, i) =>
        {
            var cost = Meta.UpgradeCost(_d, _profile, i);
            return $"{u.Name} ({u.Desc}) poziom {_profile.Levels[i]}/{u.Levels} – {(cost < 0 ? "max" : cost + " dośw.")}";
        }).ToList();
        for (var i = 0; i < _d.Classes.Length; i++)
            list.Add($"Zawód: {_d.Classes[i].Name} – {(Meta.ClassUnlocked(_profile, i) ? "odblokowany" : _d.ClassCost + " dośw.")}");
        for (var i = 0; i < _d.Tools.Length; i++)
            list.Add($"Narzędzie: {_d.Weapons[_d.Tools[i].Weapon].Name} – {(Meta.ToolUnlocked(_d, _profile, i) ? "odblokowane" : _d.Tools[i].Cost + " dośw.")}");
        list.Add($"Poziom {_d.Difficulties[^1].Name} – {(_profile.Hard != 0 ? "odblokowany" : _d.HardCost + " dośw.")}");
        return list.ToArray();
    }

    private bool BuyTraining(int sel)
    {
        var u = _d.Upgrades.Length;
        var c = _d.Classes.Length;
        var t = _d.Tools.Length;
        if (sel < u) return Meta.BuyUpgrade(_d, _profile, sel);
        if (sel < u + c) return Meta.BuyClass(_d, _profile, sel - u);
        if (sel < u + c + t) return Meta.BuyTool(_d, _profile, sel - u - c);
        return Meta.BuyHard(_d, _profile);
    }

    private void ShowTraining()
    {
        _screen = Screen.Training;
        var entries = TrainingEntries();
        _sel = Math.Clamp(_sel, 0, entries.Length - 1);
        var sb = new StringBuilder();
        sb.AppendLine($"SZKOLENIA · Pozostało: {_profile.Xp} dośw. · Wydano {Meta.ShopSpent(_d, _profile)}/{Meta.ShopTotalCost(_d)}");
        sb.AppendLine();
        for (var i = 0; i < entries.Length; i++) sb.AppendLine((i == _sel ? "> " : "  ") + entries[i]);
        sb.AppendLine();
        sb.AppendLine("↑/↓ wybór · Spacja: kup · Z/Esc: wróć");
        if (_note.Length > 0) sb.AppendLine(_note);
        _hud.ShowPanel(sb.ToString());
    }

    private void ShowOverview()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"PODGLĄD · {_g.CDef.Name} · poziom {_g.HeroLevel} · dzień {_g.Turns}");
        sb.AppendLine($"HP {_g.Hero.Hp}/{_g.Hero.MaxHp} · obrona {_g.CDef.Defense}+{_g.DefBonus + _g.GearBonus(GearStat.Def)} · premia obrażeń {_g.DmgBonus + _g.GearBonus(GearStat.Dmg)}");
        sb.AppendLine($"Sprzęt: {Hud.Gear(_g)} · Stany: {Hud.Statuses(_g)}");
        sb.AppendLine($"Wynik {_g.Score} · Budżet {_g.Cash} zł · Doświadczenie {_g.Xp} · Zabite {_g.Kills}");
        sb.AppendLine();
        sb.AppendLine("Widoczne problemy budowy:");
        var any = false;
        for (var i = 0; i < _g.EnemiesCount; i++)
        {
            var e = _g.Enemies[i];
            if (!e.Alive || !_g.Visible(e.X, e.Y)) continue;
            var ed = _d.Enemies[e.DefId];
            var bonus = _g.EnemyDmgBonus();
            sb.AppendLine($"  {ed.Name}{(i == _g.Boss ? " (BOSS)" : "")} HP {e.Hp}/{e.MaxHp}, obr. {ed.MinDamage + bonus}-{ed.MaxDamage + bonus}, " +
                          $"odl. {CoreGame.Cheb(_g.Hero.X, _g.Hero.Y, e.X, e.Y)} – {ed.Desc}");
            any = true;
        }
        if (!any) sb.AppendLine("  nikogo w polu widzenia");
        sb.AppendLine();
        sb.AppendLine("Tab: wróć do gry");
        _hud.ShowPanel(sb.ToString());
    }

    private void ShowStageClear()
    {
        _screen = Screen.StageClear;
        var sb = new StringBuilder();
        sb.AppendLine(_g.ActCleared ? $"AKT {Hud.Roman(_d.Stages[_g.Stage].Act)} ZALICZONY! Premia +{_g.ActBonus} zł" : "ETAP ZALICZONY");
        sb.AppendLine();
        sb.AppendLine("Harmonogram:");
        for (var i = 0; i < _d.Stages.Length; i++)
            sb.AppendLine($"{(i <= _g.Stage ? "[x]" : i == _g.Stage + 1 ? "[>]" : "[ ]")} {i + 1}. {_d.Stages[i].Name}");
        sb.AppendLine();
        sb.AppendLine($"Wynik: {_g.Score}  Dni: {_g.Turns}  Doświadczenie budowy: {_g.Xp}");
        if (_note.Length > 0) sb.AppendLine(_note);
        var story = _d.StoryStages[Math.Min(_g.Stage + 1, _d.StoryStages.Length - 1)];
        sb.AppendLine();
        sb.AppendLine($"SMS od {story.From}: {string.Join(" ", story.Lines)}");
        sb.AppendLine();
        sb.AppendLine(_g.ActCleared ? "Spacja/Enter: do Hurtowni" : "Kawa: +5 HP · Spacja/Enter: dalej");
        _hud.ShowPanel(sb.ToString());
    }

    private void ShowHurtownia()
    {
        _screen = Screen.Hurtownia;
        _sel = Math.Clamp(_sel, 0, _d.Hurtownia.Length - 1);
        var sb = new StringBuilder();
        sb.AppendLine($"HURTOWNIA · Budżet: {_g.Cash} zł");
        sb.AppendLine();
        for (var i = 0; i < _d.Hurtownia.Length; i++)
        {
            var it = _d.Hurtownia[i];
            sb.AppendLine($"{(i == _sel ? "> " : "  ")}{it.Name} – {it.Price} zł{(_g.Cash >= it.Price ? "" : " (za mało)")}: {it.Desc}");
        }
        sb.AppendLine();
        sb.AppendLine($"HP {_g.Hero.Hp}/{_g.Hero.MaxHp} · broń {_g.Weapon.Name} · sprzęt: {Hud.Gear(_g)}");
        sb.AppendLine("↑/↓ wybór · Spacja: kup · Enter/Z: dalej");
        if (_note.Length > 0) sb.AppendLine(_note);
        _hud.ShowPanel(sb.ToString());
    }

    private void ShowEnd()
    {
        _screen = Screen.End;
        var won = _g.St == GameStatus.Won;
        var story = won ? _d.StoryWin : _d.StoryLose;
        var sb = new StringBuilder();
        sb.AppendLine(won ? "ODBIÓR ZALICZONY!" : "BUDOWA WSTRZYMANA");
        sb.AppendLine();
        sb.AppendLine($"Wynik {_g.Score} · Dni {_g.Turns} · Etap {_g.Stage + 1}/{_d.Stages.Length} · Dośw. +{_lastGained}");
        sb.AppendLine($"Rekord {_profile.Best} · Doświadczenie w profilu {_profile.Xp}{(won ? " · Dom na Osiedlu!" : "")}");
        if (_note.Length > 0) sb.AppendLine(_note);
        sb.AppendLine();
        sb.AppendLine($"SMS od {story.From}: {string.Join(" ", story.Lines)}");
        sb.AppendLine();
        sb.AppendLine(won ? "Spacja: kolejna budowa (NG+) · Enter: menu" : "Spacja/Enter: menu");
        _hud.ShowPanel(sb.ToString());
    }

    // ------------------------------------------------------------------ przebieg budowy
    private void StartRun()
    {
        var seed = Seed != 0 ? Seed : GD.Randi() | 1u;
        _g.NewRun(_cls, seed, _diff, Meta.Mods(_d, _profile));
        ++_profile.Runs;
        SaveProfile();
        _note = "";
        _screen = Screen.Playing;
        _hud.HidePanel();
        GD.Print($"Nowa budowa: {_g.CDef.Name}, {_d.Difficulties[_diff].Name}, seed {seed}");
        Refresh();
    }

    private void SaveProfile()
    {
        if (_persistProfile) GodotDataSource.SaveProfile(_profile);
    }

    private string BadgeNote(int got) =>
        got == 0 ? "" : "Odznaki: " + string.Join(", ", _d.Badges.Where((_, i) => (got & (1 << i)) != 0).Select(b => $"{b.Name} (+{b.Xp})"));

    /// <summary>Po akcji gracza: odznaki, bankowanie doświadczenia, przejścia ekranów (jak main.cpp na GBA).</summary>
    private void AfterAction(bool acted)
    {
        if (!acted)
        {
            Refresh();
            return;
        }
        if (_g.St == GameStatus.StageClear)
        {
            _note = BadgeNote(Meta.CheckBadges(_d, _profile, _g));
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
            }
            _note = BadgeNote(Meta.CheckBadges(_d, _profile, _g));
            _lastGained = Meta.BankXp(_profile, _g);
            SaveProfile();
            ShowEnd();
        }
        Refresh();
    }

    /// <summary>Dalej z harmonogramu / Hurtowni.</summary>
    private void Advance()
    {
        if (_screen == Screen.StageClear && _g.ActCleared)
        {
            _note = "";
            _sel = 0;
            ShowHurtownia();
            return;
        }
        _note = "";
        _g.NextStage();
        _screen = Screen.Playing;
        _hud.HidePanel();
        Refresh();
    }

    private void Refresh()
    {
        _camera.Position = _view.GridToScreen(_g.Hero.X, _g.Hero.Y);
        Span<sbyte> t = stackalloc sbyte[CoreGame.MaxEnemies];
        _view.Mark(_screen == Screen.Playing && _g.TargetsInRange(t) > 0 ? t[0] : -1);
        _view.QueueRedraw();
        _hud.ShowGame(_g);
    }

    // ------------------------------------------------------------------ wejście
    private static bool Pressed(InputEvent e, string action, bool echo = false) => e.IsActionPressed(action, echo);

    private static int MenuDir(InputEvent e) => Pressed(e, GameInput.Up, true) ? -1 : Pressed(e, GameInput.Down, true) ? 1 : 0;

    public override void _UnhandledInput(InputEvent e)
    {
        if (_g is null) return;
        var handled = _screen switch
        {
            Screen.Title => TitleInput(e),
            Screen.Training => TrainingInput(e),
            Screen.Playing => PlayInput(e),
            Screen.Overview => OverviewInput(e),
            Screen.StageClear => StageClearInput(e),
            Screen.Hurtownia => HurtowniaInput(e),
            Screen.End => EndInput(e),
            _ => false,
        };
        if (handled) GetViewport().SetInputAsHandled();
    }

    private bool TitleInput(InputEvent e)
    {
        if (Pressed(e, GameInput.Left) || Pressed(e, GameInput.Right))
        {
            _cls = (_cls + (Pressed(e, GameInput.Left) ? _d.Classes.Length - 1 : 1)) % _d.Classes.Length;
        }
        else if (MenuDir(e) != 0)
        {
            _diff = (_diff + MenuDir(e) + _d.Difficulties.Length) % _d.Difficulties.Length;
        }
        else if (Pressed(e, GameInput.Shop))
        {
            _note = "";
            _sel = 0;
            ShowTraining();
            return true;
        }
        else if (Pressed(e, GameInput.Attack) || Pressed(e, GameInput.Confirm))
        {
            if (!Meta.ClassUnlocked(_profile, _cls)) _note = "Ten zawód odblokujesz w Szkoleniach (K).";
            else if (!Meta.DifficultyUnlocked(_d, _profile, _diff)) _note = "Ten poziom odblokujesz w Szkoleniach (K).";
            else
            {
                StartRun();
                return true;
            }
        }
        else
        {
            return false;
        }
        ShowTitle();
        return true;
    }

    private bool TrainingInput(InputEvent e)
    {
        var n = TrainingEntries().Length;
        if (MenuDir(e) != 0) _sel = (_sel + MenuDir(e) + n) % n;
        else if (Pressed(e, GameInput.Attack) || Pressed(e, GameInput.Confirm))
        {
            _note = BuyTraining(_sel) ? "Kupione!" : "Za mało doświadczenia albo już masz.";
            SaveProfile();
        }
        else if (Pressed(e, GameInput.Wait) || Pressed(e, GameInput.Cancel) || Pressed(e, GameInput.Shop))
        {
            _note = "";
            ShowTitle();
            return true;
        }
        else return false;
        ShowTraining();
        return true;
    }

    private bool PlayInput(InputEvent e)
    {
        if (Pressed(e, GameInput.Overview))
        {
            _screen = Screen.Overview;
            ShowOverview();
            return true;
        }
        int dx = 0, dy = 0;
        if (Pressed(e, GameInput.Up, true)) dy = -1;
        else if (Pressed(e, GameInput.Down, true)) dy = 1;
        else if (Pressed(e, GameInput.Left, true)) dx = -1;
        else if (Pressed(e, GameInput.Right, true)) dx = 1;
        bool acted;
        if (dx != 0 || dy != 0) acted = _g.PlayerMove(dx, dy);
        else if (Pressed(e, GameInput.Attack)) acted = AttackNearest();
        else if (Pressed(e, GameInput.Wait)) acted = _g.PlayerWait();
        else if (Pressed(e, GameInput.Ability)) acted = _g.PlayerAbility();
        else if (e is InputEventMouseButton { Pressed: true } mb) acted = MouseAction(mb);
        else return false;
        AfterAction(acted);
        return true;
    }

    /// <summary>A: atak najbliższego widocznego celu w zasięgu (jak celowanie na GBA bez przełączania celu).</summary>
    private bool AttackNearest()
    {
        Span<sbyte> t = stackalloc sbyte[CoreGame.MaxEnemies];
        return _g.TargetsInRange(t) > 0 ? _g.PlayerAttack(t[0]) : _g.PlayerAttackNearest();
    }

    /// <summary>Mysz: lewy klik na wroga w zasięgu = atak, gdzie indziej = krok w tę stronę; prawy klik = czekaj.</summary>
    private bool MouseAction(InputEventMouseButton mb)
    {
        if (mb.ButtonIndex == MouseButton.Right) return _g.PlayerWait();
        if (mb.ButtonIndex != MouseButton.Left) return false;
        var cell = _view.ScreenToGrid(GetGlobalMousePosition());
        var ei = _g.EnemyAt(cell.X, cell.Y);
        if (ei >= 0 && _g.Visible(cell.X, cell.Y) && CoreGame.Cheb(_g.Hero.X, _g.Hero.Y, cell.X, cell.Y) <= _g.Weapon.Range)
            return _g.PlayerAttack(ei);
        int dx = cell.X - _g.Hero.X, dy = cell.Y - _g.Hero.Y;
        if (dx == 0 && dy == 0) return _g.PlayerWait();
        return Math.Abs(dx) >= Math.Abs(dy) ? _g.PlayerMove(Math.Sign(dx), 0) : _g.PlayerMove(0, Math.Sign(dy));
    }

    private bool OverviewInput(InputEvent e)
    {
        if (!Pressed(e, GameInput.Overview) && !Pressed(e, GameInput.Cancel) && !Pressed(e, GameInput.Wait)) return false;
        _screen = Screen.Playing;
        _hud.HidePanel();
        Refresh();
        return true;
    }

    private bool StageClearInput(InputEvent e)
    {
        if (!Pressed(e, GameInput.Attack) && !Pressed(e, GameInput.Confirm)) return false;
        Advance();
        return true;
    }

    private bool HurtowniaInput(InputEvent e)
    {
        if (MenuDir(e) != 0)
        {
            _sel = (_sel + MenuDir(e) + _d.Hurtownia.Length) % _d.Hurtownia.Length;
            _note = "";
        }
        else if (Pressed(e, GameInput.Attack))
        {
            _note = _g.HurtowniaBuy(_sel) ? "Kupione! Enter: dalej" : "Za mały budżet";
            Refresh();
        }
        else if (Pressed(e, GameInput.Confirm) || Pressed(e, GameInput.Wait) || Pressed(e, GameInput.Cancel))
        {
            Advance();
            return true;
        }
        else return false;
        ShowHurtownia();
        return true;
    }

    private bool EndInput(InputEvent e)
    {
        if (_g.St == GameStatus.Won && Pressed(e, GameInput.Attack))
        {
            _g.NewGamePlus();
            _note = "";
            _screen = Screen.Playing;
            _hud.HidePanel();
            Refresh();
            return true;
        }
        if (!Pressed(e, GameInput.Confirm) && !Pressed(e, GameInput.Attack)) return false;
        _note = "";
        ShowTitle();
        return true;
    }
}
