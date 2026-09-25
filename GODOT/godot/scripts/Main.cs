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
    private enum Screen { Title, Training, Playing, Overview, StageClear, Hurtownia, End, GearOffer, ActionMenu }

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

    /// <summary>
    /// --screenshot plik.png [--scene nazwa]: podgląd grafiki bez klikania, potem zrzut ekranu i wyjście.
    /// Sceny: game (domyślna – bot gra kilka tur), title (ekran tytułowy), offer (okno porównania sprzętu),
    /// menu (menu akcji z termosem; na pokaz stan zatrucia i kawy w termosie), combat (liczby: KRYT! i Unik!),
    /// overview (podgląd pod Tab).
    /// </summary>
    private async void TakeScreenshot(string path)
    {
        _persistProfile = false;
        _profile = Meta.NewProfile(_d);
        Seed = Seed != 0 ? Seed : 424242u;
        var args = OS.GetCmdlineUserArgs();
        var sceneArg = Array.IndexOf(args, "--scene");
        var scene = sceneArg >= 0 && sceneArg + 1 < args.Length ? args[sceneArg + 1] : "game";
        if (scene == "title")
        {
            _cls = 5;
            ShowTitle();
        }
        else
        {
            _cls = scene == "game" ? 0 : 5;
            StartRun();
            for (var i = 0; i < 10 && _screen == Screen.Playing; i++)
            {
                Bot.StepSmart(_g);
                AfterAction(true);
            }
            if (_screen == Screen.GearOffer) CloseOffer(_g.OfferIsBetter);
            switch (scene)
            {
                case "offer": // pokaz: założony kask z jedną cechą, pod nogami paczka z lepszym i inną cechą
                    _g.Equip(0, 1, 1);
                    _g.Pickups[0] = new Pickup(_g.Hero.X, _g.Hero.Y, PickupType.GearBox, true, 0 * 3 + 2, 3);
                    _g.Collect();
                    AfterAction(true);
                    break;
                case "menu":
                    _g.Thermos = 2;
                    _g.Hero.Hp = (short)(_g.Hero.MaxHp / 2);
                    _g.ApplyStatus(StatusEffect.Poison, 3);
                    _g.ApplyStatus(StatusEffect.Slip, 2);
                    OpenMenu();
                    _view.MenuSel = 2;
                    MenuHint();
                    break;
                case "combat": // pokaz liczb: trafienie krytyczne bohatera i unik przed ciosem problemu
                {
                    _g.EnemiesCount = 0;
                    var (ex, ey) = FreeNeighbour();
                    _g.Spawn(0, ex, ey);
                    _g.Enemies[0].Hp = _g.Enemies[0].MaxHp = 500;
                    _g.Enemies[0].Awake = true;
                    var kept = new System.Collections.Generic.List<Hit>();
                    for (var k = 0; k < 300 && kept.Count == 0; k++)
                    {
                        _g.HitsCount = 0;
                        _g.Hero.Hp = _g.Hero.MaxHp;
                        _g.PlayerAttack(0);
                        for (var h = 0; h < _g.HitsCount; h++) if (_g.Hits[h].Kind == HitKind.Crit) kept.Add(_g.Hits[h]);
                    }
                    for (var k = 0; k < 300; k++)
                    {
                        _g.HitsCount = 0;
                        _g.Hero.Hp = _g.Hero.MaxHp;
                        _g.PlayerWait();
                        if (_g.HitsCount > 0 && _g.Hits[0].Kind == HitKind.Dodge) break;
                    }
                    foreach (var h in kept) _g.AddHit(h.X, h.Y, h.Amount, h.OnHero, h.Kind);
                    AfterAction(true);
                    break;
                }
                case "overview":
                    _screen = Screen.Overview;
                    ShowOverview();
                    break;
            }
        }
        for (var i = 0; i < 30; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GetViewport().GetTexture().GetImage().SavePng(path);
        GD.Print($"Zrzut ekranu: {path}");
        GetTree().Quit();
    }

    private (int X, int Y) FreeNeighbour()
    {
        foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
        {
            int x = _g.Hero.X + dx, y = _g.Hero.Y + dy;
            if (_g.Lv.At(x, y) == Tile.Floor && !_g.Occupied(x, y)) return (x, y);
        }
        return (_g.Hero.X + 1, _g.Hero.Y);
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
            int steps = 0, offers = 0, drinks = 0;
            for (; steps < 4000 && _g.Stage < 5; steps++)
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
                if (_screen == Screen.GearOffer) // okno porównania: decyzja jak bot z GBA (lepszy – zakładam)
                {
                    offers++;
                    CloseOffer(_g.OfferIsBetter);
                    continue;
                }
                if (_screen != Screen.Playing) break;
                if (_g.Thermos > 0 && _g.Hero.Hp * 2 < _g.Hero.MaxHp) // termos przez menu akcji (Enter, strzałka w dół x2)
                {
                    OpenMenu();
                    MenuPick(2);
                    MenuPick(2);
                    drinks++;
                    continue;
                }
                Bot.StepSmart(_g);
                AfterAction(true);
            }
            if (_screen == Screen.Playing && _g.St == GameStatus.Playing)
            {
                // Ścieżki UI, na które bot mógł nie trafić: termos przez menu akcji i okno porównania sprzętu.
                _g.Thermos = Math.Max(_g.Thermos, 1);
                _g.Hero.Hp = (short)Math.Max(1, _g.Hero.MaxHp / 3);
                int before = _g.Thermos, hp = _g.Hero.Hp;
                OpenMenu();
                MenuPick(2);
                MenuPick(2);
                if (_screen == Screen.Playing && (_g.Thermos != before - 1 || _g.Hero.Hp <= hp - 20)) throw new Exception("termos z menu nie zadziałał");
                drinks++;
                if (_g.St == GameStatus.Playing && _screen == Screen.Playing)
                {
                    _g.Equip(0, 0, 0);
                    _g.Pickups[0] = new Pickup(_g.Hero.X, _g.Hero.Y, PickupType.GearBox, true, 2, 1);
                    _g.Collect();
                    AfterAction(true);
                    if (_screen != Screen.GearOffer) throw new Exception("brak okna porównania sprzętu");
                    OfferInput(new InputEventAction { Action = GameInput.Attack, Pressed = true });
                    if (_screen != Screen.Playing || _g.Equipped[0] != 2 || _g.EquippedTrait[0] != 1) throw new Exception("zakładanie z porównania nie zadziałało");
                    offers++;
                }
            }
            ShowOverview();
            var ok = _g.Stage >= 5 || _g.St is GameStatus.Dead or GameStatus.Won;
            GD.Print($"SMOKE {(ok ? "OK" : "FAIL")}: dane {_d.Version}, zawody {_d.Classes.Length}, etap {_g.Stage + 1}, " +
                     $"dzień {_g.Turns}, HP {_g.Hero.Hp}/{_g.Hero.MaxHp}, wynik {_g.Score}, budżet {_g.Cash}, kroki {steps}, " +
                     $"paczki {offers}, termos {drinks}, ekran {_screen}");
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
            sb.AppendLine($"{mark}{c.Name} – {c.Desc} HP {c.MaxHealth}, szczęście {c.Luck}, moc {c.AbilityName}{lockTxt}");
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
        sb.AppendLine($"{Hud.Luck(_g)} · Termos {_g.Thermos}/{_d.ThermosCapacity}");
        sb.AppendLine($"Sprzęt: {Hud.Gear(_g)}");
        sb.AppendLine($"Stany: {Hud.Statuses(_g)}");
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

    /// <summary>Paczka sprzętu przy zajętym slocie: porównanie obecny / nowy z cechami (jak telefon na GBA).</summary>
    private void ShowOffer()
    {
        _screen = Screen.GearOffer;
        var slot = _g.OfferSlot;
        string Row(string label, int rarity, int trait)
        {
            var gd = _d.Gear[slot * 3 + rarity];
            var t = _d.GearTraits[trait];
            return $"{label} {gd.Name} ({_d.GearRarities[rarity]}) – {Hud.StatName(gd.Stat)} +{gd.Value}, cecha: {t.Name}";
        }
        var sb = new StringBuilder();
        sb.AppendLine($"PACZKA SPRZĘTU · {_d.GearSlots[slot]}");
        sb.AppendLine();
        sb.AppendLine(Row("Teraz:", _g.Equipped[slot], _g.EquippedTrait[slot]));
        sb.AppendLine(Row("Nowy: ", _g.OfferRarity, _g.OfferTrait));
        sb.AppendLine();
        sb.AppendLine(_g.OfferIsBetter ? "Nowy jest lepszej jakości."
                      : _g.OfferRarity == _g.Equipped[slot] ? "Ta sama jakość – różni się cechą." : "Nowy jest gorszej jakości.");
        sb.AppendLine();
        sb.AppendLine($"A (Spacja/X): zakładam · B (Z): zostawiam (+{_d.GearDeclineXp + _g.OfferRarity} dośw.)");
        _hud.ShowPanel(sb.ToString());
    }

    private void CloseOffer(bool accept)
    {
        if (accept) _g.AcceptOffer();
        else _g.DeclineOffer();
        _screen = Screen.Playing;
        _hud.HidePanel();
        AfterAction(true); // decyzja nie zużywa tury, ale mogła dać awans (doświadczenie)
    }

    // ------------------------------------------------------------------ menu akcji (Enter / START)
    /// <summary>Menu akcji wokół bohatera: góra Atak, prawo Moc, dół Termos, lewo Czekaj (jak GBA v0.21.42).</summary>
    private void OpenMenu()
    {
        _screen = Screen.ActionMenu;
        _view.MenuSel = -1;
        MenuHint();
        Refresh();
    }

    private void CloseMenu()
    {
        _view.MenuSel = -2;
        _hud.ShowHint(null);
        if (_screen == Screen.ActionMenu) _screen = Screen.Playing;
        Refresh();
    }

    private void MenuHint()
    {
        var label = _view.MenuSel switch
        {
            0 => $"Atak: najbliższy cel (z{_g.Weapon.Range})",
            1 => $"Moc: {_g.CDef.AbilityName} {Hud.Roman(_g.AbilityRank() - 1)}" + (_g.AbilityCd > 0 ? $" – za {_g.AbilityCd} t." : ""),
            2 => $"Termos {_g.Thermos}/{_d.ThermosCapacity}: kawa +{_g.CoffeeHeal()} HP (zużywa turę)",
            3 => "Czekaj turę",
            _ => "Akcje: wybierz strzałką (A Atak ↑, M Moc →, T Termos ↓, C Czekaj ←)",
        };
        _hud.ShowHint(label + "\n" + (_view.MenuSel < 0 ? "Enter/Z: zamknij" : "Ta sama strzałka lub Spacja: wykonaj · Enter/Z: zamknij"));
    }

    /// <summary>Strzałka w menu: pierwszy raz wybiera, drugi raz tą samą – wykonuje.</summary>
    private void MenuPick(int k)
    {
        if (_view.MenuSel != k)
        {
            _view.MenuSel = k;
            MenuHint();
            _view.QueueRedraw();
            return;
        }
        MenuExecute();
    }

    private void MenuExecute()
    {
        var sel = _view.MenuSel;
        CloseMenu();
        var acted = sel switch
        {
            0 => AttackNearest(),
            1 => _g.PlayerAbility(),
            2 => _g.PlayerDrink(),
            3 => _g.PlayerWait(),
            _ => false,
        };
        AfterAction(acted);
    }

    private bool ActionMenuInput(InputEvent e)
    {
        for (var k = 0; k < 4; k++)
        {
            var action = k switch { 0 => GameInput.Up, 1 => GameInput.Right, 2 => GameInput.Down, _ => GameInput.Left };
            if (!Pressed(e, action)) continue;
            MenuPick(k);
            return true;
        }
        if (Pressed(e, GameInput.Attack) && _view.MenuSel >= 0) MenuExecute();
        else if (Pressed(e, GameInput.Confirm) || Pressed(e, GameInput.Wait) || Pressed(e, GameInput.Cancel)) CloseMenu();
        else return false;
        return true;
    }

    private bool OfferInput(InputEvent e)
    {
        if (Pressed(e, GameInput.Attack)) CloseOffer(true);
        else if (Pressed(e, GameInput.Wait) || Pressed(e, GameInput.Cancel)) CloseOffer(false);
        else return false;
        return true;
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
        else if (_g.St == GameStatus.Playing && _g.HasOffer && _screen == Screen.Playing)
        {
            ShowOffer();
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
        _view.TakeHits(_g);
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
            Screen.GearOffer => OfferInput(e),
            Screen.ActionMenu => ActionMenuInput(e),
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
        if (Pressed(e, GameInput.Confirm))
        {
            OpenMenu();
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
