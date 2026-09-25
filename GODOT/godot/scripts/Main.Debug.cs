using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game;

/// <summary>Zrzuty ekranu (--screenshot) i test dymny (--smoke) - sceny pokazowe ustawiają stan ręcznie.</summary>
public partial class Main
{
    /// <summary>Sceny zrzutów (--scene): lista dla README i komunikatu o błędzie.</summary>
    public static readonly string[] ShotScenes =
    [
        "title", "classselect", "profile", "catalog", "estate", "team", "training", "game", "combat", "offer", "menu",
        "overview", "phone-tasks", "phone-issues", "phone-start", "phone-gear", "phone-costs", "card", "perks", "schedule",
        "hurtownia", "boss", "endmsg", "end", "banners", "map",
    ];

    private async void TakeScreenshot(string path)
    {
        _persistProfile = false;
        _profile = Meta.NewProfile(_d);
        Seed = Seed != 0 ? Seed : 424242u;
        var args = OS.GetCmdlineUserArgs();
        var sceneArg = Array.IndexOf(args, "--scene");
        var scene = sceneArg >= 0 && sceneArg + 1 < args.Length ? args[sceneArg + 1] : "game";
        if (Array.IndexOf(ShotScenes, scene) < 0)
        {
            GD.PushError($"Nieznana scena {scene}; dostępne: {string.Join(", ", ShotScenes)}");
            GetTree().Quit(1);
            return;
        }
        if (scene is "title" or "classselect" or "profile" or "catalog" or "estate" or "team" or "training" or "card" or "perks")
            _profile = DemoProfile();
        await SetupScene(scene);
        for (var i = 0; i < 50; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        var img = GetViewport().GetTexture().GetImage();
        if (img.GetWidth() < 1000) img.Resize(img.GetWidth() * 2, img.GetHeight() * 2, Image.Interpolation.Nearest);
        img.SavePng(path);
        GD.Print($"Zrzut ekranu ({scene}): {path}");
        GetTree().Quit();
    }

    private async System.Threading.Tasks.Task SetupScene(string scene)
    {
        switch (scene)
        {
            case "title":
                ShowTitle();
                return;
            case "classselect":
                _cls = 1;
                ShowClassSelect();
                return;
            case "profile":
                ShowProfile(0, true);
                return;
            case "catalog":
                ShowProfile(1, true);
                return;
            case "estate":
                ShowProfile(2, true);
                return;
            case "team":
                ShowProfile(3, true);
                return;
            case "training":
                ShowProfile(4, true);
                return;
            case "card": // karta etapu z wydarzeniem: pierwszy seed, przy którym etap 2 ma wydarzenie
                _cls = 1;
                for (var s = Seed; ; s++)
                {
                    Seed = s;
                    StartRun();
                    _g.NextStage();
                    if (_g.StageEvent >= 0) break;
                }
                _events.Reset(_g);
                Refresh();
                ShowStageCard(true);
                return;
        }

        _cls = scene == "game" ? 0 : scene == "perks" ? 1 : 5;
        StartRun();
        Advance(); // karta etapu -> gra
        if (scene == "perks") // Murarz z Warsztatami i cechą SIŁ+1, na etapie z wydarzeniem
        {
            for (var s = Seed; _g.StageEvent < 0; s++)
            {
                Seed = s;
                StartRun();
                _g.NextStage();
                Advance();
            }
            _g.Equip(1, 2, Array.FindIndex(_d.GearTraits, t => t.Effect == TraitEffect.Str));
            Refresh();
        }
        if (scene is "schedule" or "hurtownia" or "endmsg" or "end" or "boss")
        {
            await SkipStages(scene);
            return;
        }
        for (var i = 0; i < 8 && _screen == Screen.Playing; i++)
        {
            Bot.StepSmart(_g);
            AfterAction(true);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        if (_screen == Screen.Playing && !EnemyInView()) BringEnemies();
        if (scene is "game" or "perks") _banners.Clear();
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
                _banners.Clear();
                _g.Thermos = 2;
                _g.Hero.Hp = (short)(_g.Hero.MaxHp / 2);
                _g.ApplyStatus(StatusEffect.Poison, 3);
                _g.ApplyStatus(StatusEffect.Slip, 2);
                OpenMenu();
                _view.MenuSel = 2;
                MenuHint();
                break;
            case "combat": // pokaz liczb: trafienie krytyczne bohatera i unik przed ciosem problemu
                _banners.Clear();
                Combat();
                break;
            case "overview":
            case "phone-start":
                ShowPhone(2, true);
                break;
            case "phone-tasks":
                ShowPhone(0, true);
                break;
            case "phone-issues":
                ShowPhone(1, true);
                break;
            case "phone-gear":
                _g.Equip(0, 1, 1);
                _g.Equip(2, 2, 3);
                ShowPhone(3, true);
                break;
            case "phone-costs":
                ShowPhone(4, true);
                break;
            case "banners":
                _banners.Push("Awans! Poziom 2", $"+{_d.HpPerLevel} HP");
                _banners.Push("Nowe narzędzie", "Młotek 3-6");
                _banners.Push("Moc gotowa", _g.CDef.AbilityName);
                break;
            case "map":
                _banners.Clear();
                _view.ToggleOverview();
                _hud.ShowHint("Podgląd mapy etapu", "Dowolny klawisz: wróć");
                break;
        }
    }

    private void Combat()
    {
        _g.EnemiesCount = 0;
        var (ex, ey) = FreeNeighbour();
        _g.Spawn(0, ex, ey);
        _g.Enemies[0].Hp = _g.Enemies[0].MaxHp = 500;
        _g.Enemies[0].Awake = true;
        var kept = new List<Hit>();
        for (var k = 0; k < 300 && kept.Count == 0; k++)
        {
            _g.HitsCount = 0;
            _g.Hero.Hp = _g.Hero.MaxHp;
            _g.PlayerAttack(0);
            for (var h = 0; h < _g.HitsCount; h++)
            {
                if (_g.Hits[h].Kind == HitKind.Crit) kept.Add(_g.Hits[h]);
            }
        }
        for (var k = 0; k < 300; k++)
        {
            _g.HitsCount = 0;
            _g.Hero.Hp = _g.Hero.MaxHp;
            _g.PlayerWait();
            if (_g.HitsCount > 0 && _g.Hits[0].Kind == HitKind.Dodge) break;
        }
        foreach (var h in kept) _g.AddHit(h.X, h.Y, h.Amount, h.OnHero, h.Kind);
        _g.Enemies[0].Hp = 180;
        AfterAction(true);
    }

    /// <summary>Przewija etapy skrótem DebugSkip (L+R+SELECT na GBA) do sceny: harmonogram, Hurtownia, boss, koniec.</summary>
    private async System.Threading.Tasks.Task SkipStages(string scene)
    {
        for (var guard = 0; guard < 40; guard++)
        {
            if (scene == "boss" && _d.Stages[_g.Stage].Boss >= 0 && _screen == Screen.Playing)
            {
                BossScene();
                return;
            }
            _g.DebugSkip();
            AfterAction(true);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            if (_screen == Screen.StageClear && scene == "schedule") return;
            if (_screen == Screen.EndMessage)
            {
                if (scene == "end") ShowEnd();
                return;
            }
            if (_screen == Screen.StageClear && _g.ActCleared && scene == "hurtownia")
            {
                Advance();
                return;
            }
            while (_screen is Screen.StageClear or Screen.StageCard or Screen.Hurtownia) Advance();
        }
    }

    /// <summary>Boss w polu widzenia z zapowiedzianym ciosem (czerwone pola) i banerem „Przypisano Ci usterkę”.</summary>
    private void BossScene()
    {
        var b = _g.Enemies[_g.Boss];
        foreach (var (dx, dy) in new[] { (2, 0), (-2, 0), (0, 2), (0, -2), (2, 1), (-2, 1), (1, 2), (1, -2) })
        {
            int x = b.X + dx, y = b.Y + dy;
            if (_g.Lv.At(x, y) != Tile.Floor || _g.Occupied(x, y)) continue;
            _g.Hero.X = (sbyte)x;
            _g.Hero.Y = (sbyte)y;
            break;
        }
        _g.Enemies[_g.Boss].Awake = true;
        _g.Enemies[_g.Boss].Hp = (short)(_g.Enemies[_g.Boss].MaxHp * 2 / 3);
        _g.SlamTimer = 2;
        _g.SlamX = (sbyte)_g.Hero.X;
        _g.SlamY = (sbyte)_g.Hero.Y;
        _g.UpdateFov();
        _view.Sync();
        Refresh();
    }

    /// <summary>Profil pokazowy do zrzutów: część odznak (uprawnienia), zlecenia z postępem, pamiątka z rangą II, Warsztaty, domy.</summary>
    private Profile DemoProfile()
    {
        var p = Meta.NewProfile(_d);
        p.Runs = 14;
        p.Wins = 3;
        p.Best = 5613;
        p.Xp = 85;
        p.Badges = (ushort)((1 << _d.BadgeBezUsterek) | (1 << _d.BadgeSeryjny) | (1 << _d.BadgeKolekcjoner));
        p.ClassWins = 0x03;
        p.KillsTotal = 163;
        p.PowersTotal = 71;
        p.BrandTotal = 5;
        p.CleanBosses = 1;
        p.Catalog = 0x5F;
        p.HousesCount = 3;
        p.Houses[0] = 0x00;
        p.Houses[1] = 0x21;
        p.Houses[2] = 0x34;
        Meta.CheckContracts(_d, p);
        var craft = Array.FindIndex(_d.Upgrades, u => u.Effect == UpgradeEffect.Craft);
        if (craft >= 0) p.Levels[craft] = 2;
        var kielnia = Array.FindIndex(_d.Keepsakes, k => k.Effect == PerkEffect.Luck);
        if (kielnia >= 0)
        {
            p.Keepsake = (byte)(kielnia + 1);
            p.KeepsakeRuns[kielnia] = 4;
        }
        p.KeepsakeRuns[0] = 2;
        return p;
    }

    /// <summary>Pokaz: dwa problemy budowy podchodzą na 2-3 pola od bohatera (widoczne, jeden ranny i czujny).</summary>
    private void BringEnemies()
    {
        var placed = 0;
        foreach (var (dx, dy) in new[] { (2, 1), (-2, 1), (2, -1), (-2, -1), (3, 0), (-3, 0), (0, 3), (0, -3), (1, 2), (-1, -2) })
        {
            int x = _g.Hero.X + dx, y = _g.Hero.Y + dy;
            if (placed >= 2 || _g.Lv.At(x, y) != Tile.Floor || _g.Occupied(x, y)) continue;
            for (var i = 0; i < _g.EnemiesCount; i++)
            {
                ref var e = ref _g.Enemies[i];
                if (!e.Alive || _g.Visible(e.X, e.Y)) continue;
                e.X = (sbyte)x;
                e.Y = (sbyte)y;
                e.Awake = placed == 0;
                if (placed == 0) e.Hp = (short)Math.Max(1, e.MaxHp * 2 / 3);
                placed++;
                break;
            }
        }
        _g.UpdateFov();
        Refresh();
    }

    private bool EnemyInView()
    {
        for (var i = 0; i < _g.EnemiesCount; i++)
        {
            if (_g.Enemies[i].Alive && _g.Visible(_g.Enemies[i].X, _g.Enemies[i].Y)) return true;
        }
        return false;
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
    /// przez warstwę Godota (widok, HUD, telefon, harmonogram, Hurtownia, porównanie sprzętu, menu akcji), potem
    /// otwiera wszystkie zakładki telefonu i ekrany; kod wyjścia 0 = OK (także brak wyjątków przy rysowaniu).
    /// </summary>
    private async void RunSmokeTest()
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
                if (_screen is Screen.StageClear or Screen.StageCard)
                {
                    Advance();
                    continue;
                }
                if (_screen == Screen.Hurtownia)
                {
                    Bot.Shop(_g);
                    _hurtownia.Buy();
                    Advance();
                    continue;
                }
                if (_screen == Screen.GearOffer) // okno porównania: decyzja jak bot z GBA (lepszy - zakładam)
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
                if (steps % 200 == 0) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
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
                    await Frames(2);
                    OfferInput(new InputEventAction { Action = GameInput.Attack, Pressed = true });
                    if (_screen != Screen.Playing || _g.Equipped[0] != 2 || _g.EquippedTrait[0] != 1) throw new Exception("zakładanie z porównania nie zadziałało");
                    offers++;
                }
            }
            var ok = _g.Stage >= 5 || _g.St is GameStatus.Dead or GameStatus.Won;
            var stage = _g.Stage;
            // wszystkie zakładki telefonu w grze i profilu, ekrany - rysowanie bez wyjątków
            if (_g.St == GameStatus.Playing)
            {
                for (var t = 0; t < 5; t++)
                {
                    ShowPhone(t, true);
                    await Frames(2);
                }
                ClosePhone();
            }
            for (var t = 0; t < 5; t++)
            {
                ShowProfile(t, true);
                await Frames(2);
            }
            var training = _phone.Current as TrainingTab;
            if (training is null) throw new Exception("brak zakładki Koszty w profilu");
            ShowClassSelect();
            await Frames(2);
            _classSelect.Move(1);
            _profile.Keepsake = 0;
            Meta.CycleKeepsake(_d, _profile, 1);
            if (Meta.SelectedKeepsake(_d, _profile) < 0) throw new Exception("brak pamiątki startowej do wyboru");
            ShowTitle();
            await Frames(2);
            var missing = Sfx.Missing();
            if (missing.Length > 0) throw new Exception("brak dźwięków: " + missing);
            if (DrawErrors.Count > 0) throw new Exception($"błędy rysowania: {DrawErrors.Count}, ostatni: {DrawErrors.Last}");
            GD.Print($"SMOKE {(ok ? "OK" : "FAIL")}: dane {_d.Version}, zawody {_d.Classes.Length}, etap {stage + 1}, " +
                     $"dzień {_g.Turns}, HP {_g.Hero.Hp}/{_g.Hero.MaxHp}, wynik {_g.Score}, budżet {_g.Cash}, kroki {steps}, " +
                     $"paczki {offers}, termos {drinks}, zabite w profilu {_profile.KillsTotal}, moce {_profile.PowersTotal}, ekran {_screen}");
            GetTree().Quit(ok ? 0 : 1);
        }
        catch (Exception ex)
        {
            GD.PushError($"SMOKE FAIL: {ex}");
            GetTree().Quit(1);
        }
    }

    private async System.Threading.Tasks.Task Frames(int n)
    {
        for (var i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
}
