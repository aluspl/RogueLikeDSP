using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Touch;

namespace LifeLike.Game.Debug;

/// <summary>
/// Test małpy (--monkey SEED KROKI): bot klika w grę losowo przez prawdziwe wejście – klawisze (Input.ParseInputEvent,
/// ta sama mapa co klawiatura) oraz stuknięcia, przesunięcia i przytrzymania myszą / dotykiem (Viewport.PushInput:
/// GestureTracker, przyciski ekranowe, karty, telefon). Stuka częściej w przyciski, wiersze list i zakładki (widoki
/// z ITapTargets) niż w puste miejsca. Na mapie czasem robi krok bota (Bot.StepSmart) albo skrót etapu (DebugSkip,
/// jak L+R+SELECT na GBA), żeby dojść do premii, wydarzeń, Hurtowni, podsumowania i końca budowy.
/// Błąd = wyjątek / błąd w logu Godota (MonkeyLogger), błąd rysowania (DrawErrors) albo zawieszenie: ten sam ekran
/// i stan przez StuckActions akcji i co najmniej StuckSeconds sekund. Przy błędzie wypisuje ślad ostatnich akcji, kod 1.
/// </summary>
public sealed class MonkeyTest
{
    private const int StuckActions = 400;
    private const float StuckSeconds = 6f;
    private const int TrailSize = 40;

    private static readonly Key[] Keys =
    [
        Key.Up, Key.Down, Key.Left, Key.Right, Key.Up, Key.Down, Key.Left, Key.Right, Key.Space, Key.Space, Key.Enter, Key.Enter,
        Key.Z, Key.Escape, Key.Tab, Key.R, Key.M, Key.Q, Key.E, Key.I, Key.K, Key.P, Key.Space, Key.Z, Key.Enter,
    ];

    private readonly App _app;
    private readonly uint _seed;
    private readonly int _steps;
    private readonly RandomNumberGenerator _rng = new();
    private readonly Queue<string> _trail = new();
    private readonly Dictionary<string, int> _visits = new();
    private readonly List<Rect2> _targets = new();
    private readonly List<Rect2> _local = new();
    private MonkeyLogger _log;
    private string _sig = "";
    private int _sameFor;
    private ulong _sameSince;
    private int _runs, _skips, _bot;
    private bool _countedRun;

    public MonkeyTest(App app, uint seed, int steps)
    {
        _app = app;
        _seed = seed;
        _steps = Math.Max(1, steps);
        _rng.Seed = seed;
    }

    private Node Root => _app.Root;

    private Vector2 Size => Root.GetViewport().GetVisibleRect().Size;

    public async void Run()
    {
        _log = new MonkeyLogger();
        OS.AddLogger(_log);
        var s = _app.Session;
        s.Persist = false;
        s.Seed = _rng.Randi() | 1u;
        if (_rng.Randf() < 0.5f) // połowa przebiegów: bogaty profil (wszystkie zawody, sekrety, wygląd, inwestor)
        {
            var d = s.Data;
            s.Profile = DemoProfile.Create(d);
            s.Profile.Tutorial = 0x3F;
            s.Profile.ClassesSeen = 0xFFFF;
            foreach (var sd in d.Secrets) SecretStaging.Grant(d, s.Profile, sd.Id);
            s.Profile.SecretsNew = (ushort)_rng.RandiRange(0, (1 << d.Secrets.Length) - 1);
            s.Profile.Cosmetic = (byte)_rng.RandiRange(0, 3);
            s.Profile.Wins = 9;
            s.Profile.Rewards = (byte)d.Rewards.Length;
        }
        s.Profile.SetFlag(Profile.FlagPrologueSeen | Profile.FlagHelpSeen);
        // v0.21.53: losowy filtr ekranu (zablokowany działa jak klasyczny), filtr na telefonie i ograniczony ruch
        Settings.GameSettings.Filter = s.Data.ScreenFilters[_rng.RandiRange(0, s.Data.ScreenFilters.Length - 1)].Id;
        Settings.GameSettings.FilterPhone = _rng.Randf() < 0.7f;
        Settings.GameSettings.ReduceMotion = _rng.Randf() < 0.3f;
        Settings.GameSettings.Save();
        _app.Flow.Title.Open();
        var fail = "";
        var step = 0;
        _sameSince = Time.GetTicksMsec();
        try
        {
            for (step = 0; step < _steps; step++)
            {
                await Act();
                fail = Check();
                if (fail.Length > 0) break;
                if (step % 500 == 499) GD.Print($"MONKEY {_seed}: krok {step + 1}/{_steps}, ekran {_app.Flow.Current?.GetType().Name}, budowy {_runs}");
            }
        }
        catch (Exception ex)
        {
            fail = "wyjątek w teście: " + ex;
        }
        await DebugRunner.Frames(Root, 2);
        if (fail.Length == 0) fail = Check();
        var screens = string.Join(", ", _visits.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value}"));
        OS.RemoveLogger(_log);
        if (fail.Length > 0)
        {
            GD.Print($"MONKEY FAIL seed {_seed} krok {step + 1}/{_steps}: {fail}");
            GD.Print("Ślad (ostatnie akcje): " + string.Join(" | ", _trail));
            GD.Print("Ekrany: " + screens);
            Root.GetTree().Quit(1);
            return;
        }
        GD.Print($"MONKEY OK seed {_seed}, akcje {_steps}, {(Layout.Touch ? "dotyk" : "klawiatura+mysz")}{(Layout.Portrait ? " pion" : "")}, budowy {_runs}, " +
                 $"kroki bota {_bot}, skróty {_skips}, ekranów {_visits.Count}: {screens}");
        Root.GetTree().Quit(0);
    }

    private string Check()
    {
        if (_log.Errors.TryDequeue(out var err)) return "błąd w logu: " + err;
        if (DrawErrors.Count > 0) return $"błąd rysowania ({DrawErrors.Count}): {DrawErrors.Last}";
        var cur = _app.Flow.Current;
        if (cur is null) return "brak ekranu";
        var name = cur.GetType().Name.Replace("Screen", "");
        _visits[name] = _visits.TryGetValue(name, out var v) ? v + 1 : 1;
        var g = _app.Session.Game;
        var page = _app.Nodes.Phone.IsOpen ? _app.Nodes.Phone.Current?.GetType().Name + _app.Nodes.Phone.TabIndex : "";
        var sig = $"{name}/{page}/{_app.Coach.CurrentId}/{g.St}/{g.Stage}/{g.Turns}/{g.Hero.X},{g.Hero.Y}/{g.Hero.Hp}";
        if (sig != _sig)
        {
            _sig = sig;
            _sameFor = 0;
            _sameSince = Time.GetTicksMsec();
            return "";
        }
        if (++_sameFor >= StuckActions && Time.GetTicksMsec() - _sameSince >= StuckSeconds * 1000)
            return $"zawieszenie: {_sameFor} akcji bez zmiany ({sig})";
        return "";
    }

    private void Note(string a)
    {
        _trail.Enqueue($"{_app.Flow.Current?.GetType().Name.Replace("Screen", "")}:{a}");
        while (_trail.Count > TrailSize) _trail.Dequeue();
    }

    private async Task Act()
    {
        var flow = _app.Flow;
        // świeże okno ignoruje wejście 0,4 s (ScreenFlow.InputLocked): człowiek i tak nie stuka szybciej – czekamy,
        // ale czasem (10%) małpa wali w ekran także w czasie blokady
        for (var i = 0; i < 400 && flow.InputLocked && _rng.Randf() >= 0.1f; i++) await DebugRunner.Frames(Root, 1);
        // Ustawienia (Z / Esc na tytule i mapie) zjadały ~45% akcji na klawiaturze: częściej z nich wychodzimy
        if (flow.Current == flow.Settings && _rng.Randf() < 0.35f)
        {
            await Press(_rng.Randf() < 0.5f ? Key.Escape : Key.Z, 1, false);
            return;
        }
        var inGame = flow.Current == flow.Game && _app.Session.Game.St == GameStatus.Playing;
        // wagi: klawisz, stuknięcie w cel, stuknięcie gdziekolwiek, przesunięcie, przytrzymanie, trzymany A/B, krok bota, skrót etapu, prawy klik
        float[] w = Layout.Touch
            ? [16, 40, 10, 12, 5, 2, inGame ? 25 : 0, inGame ? 2.5f : 0, 0]
            : [50, 22, 5, 3, 2, 5, inGame ? 25 : 0, inGame ? 2.5f : 0, 3];
        switch (Pick(w))
        {
            case 0:
            {
                var k = Keys[_rng.RandiRange(0, Keys.Length - 1)];
                // Z / Esc na tytule i mapie otwierają Ustawienia – zwykle zamiast nich inny klawisz (mniej czasu w Ustawieniach)
                if (k is Key.Z or Key.Escape && (flow.Current == flow.Title || flow.Current == flow.Game) && _rng.Randf() < 0.75f)
                    k = _rng.Randf() < 0.5f ? Key.Space : Key.Enter;
                await Press(k, _rng.RandiRange(1, 3), false);
                break;
            }
            case 1:
                await Tap(TargetPoint());
                break;
            case 2:
                await Tap(RandomPoint());
                break;
            case 3:
                await Swipe();
                break;
            case 4:
                await LongPress(_rng.Randf() < 0.6f ? TargetPoint() : RandomPoint());
                break;
            case 5:
                await Press(_rng.Randf() < 0.5f ? Key.Space : Key.Z, _rng.RandiRange(20, 45), true);
                break;
            case 6:
                Note("bot");
                Bot.StepSmart(_app.Session.Game);
                _app.AfterAction(true);
                _bot++;
                await DebugRunner.Frames(Root, _rng.RandiRange(1, 3));
                break;
            case 7:
                Note("skrót etapu");
                _app.Session.Game.DebugSkip();
                _app.AfterAction(true);
                _skips++;
                await DebugRunner.Frames(Root, 2);
                break;
            default:
                await Click(RandomPoint(), MouseButton.Right);
                break;
        }
        var g = _app.Session.Game;
        if (g.Turns == 0 && _app.Flow.Current == _app.Flow.Game && !_countedRun)
        {
            _runs++;
            _countedRun = true;
        }
        if (g.Turns > 0) _countedRun = false;
        if (_rng.Randf() < 0.15f) await DebugRunner.Frames(Root, _rng.RandiRange(3, 12)); // czasem chwila na animacje
    }

    private int Pick(float[] w)
    {
        var r = _rng.Randf() * w.Sum();
        for (var i = 0; i < w.Length; i++)
        {
            if ((r -= w[i]) < 0) return i;
        }
        return w.Length - 1;
    }

    private Vector2 RandomPoint() => new(_rng.RandfRange(0, Size.X - 1), _rng.RandfRange(0, Size.Y - 1));

    /// <summary>Losowy punkt w losowym przycisku / wierszu / zakładce widocznych widoków (współrzędne okna gry).</summary>
    private Vector2 TargetPoint()
    {
        _targets.Clear();
        Collect(Root);
        if (_app.Nodes.Settings.IsVisibleInTree() && _rng.Randf() < 0.15f) _targets.Add(Hud.SettingsButton.Rect); // klucz rzadziej (Ustawienia)
        var screen = new Rect2(Vector2.Zero, Size);
        _targets.RemoveAll(r => r.Size.X < 2 || r.Size.Y < 2 || !screen.Intersects(r));
        if (_targets.Count == 0) return RandomPoint();
        var t = _targets[_rng.RandiRange(0, _targets.Count - 1)];
        return new Vector2(_rng.RandfRange(t.Position.X + 1, t.End.X - 1), _rng.RandfRange(t.Position.Y + 1, t.End.Y - 1));
    }

    private void Collect(Node n)
    {
        if (n is CanvasItem ci && !ci.IsVisibleInTree()) return;
        if (n is ITapTargets tt && n is CanvasItem item)
        {
            _local.Clear();
            tt.TapTargets(_local);
            var xf = item.GetGlobalTransformWithCanvas();
            foreach (var r in _local) _targets.Add(xf * r);
        }
        foreach (var c in n.GetChildren()) Collect(c);
    }

    private static InputEventKey KeyEvent(Key k, bool pressed) => new() { PhysicalKeycode = k, Keycode = k, Pressed = pressed };

    private async Task Press(Key k, int frames, bool hold)
    {
        Note((hold ? "trzymaj " : "") + k);
        Godot.Input.ParseInputEvent(KeyEvent(k, true));
        if (hold) // celowanie / karta problemu: strzałki w trakcie trzymania
        {
            var until = Time.GetTicksMsec() + (ulong)(frames * 16);
            for (var i = 0; i < 600 && Time.GetTicksMsec() < until; i++)
            {
                await DebugRunner.Frames(Root, 1);
                if (_rng.Randf() >= 0.1f) continue;
                Godot.Input.ParseInputEvent(KeyEvent(Key.Right, true));
                Godot.Input.ParseInputEvent(KeyEvent(Key.Right, false));
            }
        }
        else await DebugRunner.Frames(Root, frames);
        Godot.Input.ParseInputEvent(KeyEvent(k, false));
        await DebugRunner.Frames(Root, 1);
    }

    private void Mouse(Vector2 p, bool pressed, MouseButton b = MouseButton.Left) =>
        Root.GetViewport().PushInput(new InputEventMouseButton
        {
            Position = p, GlobalPosition = p, ButtonIndex = b, Pressed = pressed,
            ButtonMask = pressed ? (b == MouseButton.Left ? MouseButtonMask.Left : MouseButtonMask.Right) : 0,
        }, true);

    private void Motion(Vector2 p, Vector2 rel) =>
        Root.GetViewport().PushInput(new InputEventMouseMotion { Position = p, GlobalPosition = p, Relative = rel, ButtonMask = MouseButtonMask.Left }, true);

    private async Task Tap(Vector2 p)
    {
        Note($"stuk {p.X:0},{p.Y:0}");
        Mouse(p, true);
        await DebugRunner.Frames(Root, _rng.RandiRange(1, 3));
        Mouse(p, false);
        await DebugRunner.Frames(Root, 1);
    }

    private async Task Click(Vector2 p, MouseButton b)
    {
        Note($"klik {b} {p.X:0},{p.Y:0}");
        Mouse(p, true, b);
        await DebugRunner.Frames(Root, 1);
        Mouse(p, false, b);
        await DebugRunner.Frames(Root, 1);
    }

    private async Task Swipe()
    {
        var from = _rng.Randf() < 0.5f ? TargetPoint() : RandomPoint();
        Vector2[] dirs = [Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right];
        var dir = dirs[_rng.RandiRange(0, 3)];
        var len = _rng.RandfRange(30, 140);
        var keep = _rng.Randf() < 0.2f; // palec trzymany dalej = kolejne kroki
        Note($"przesuń {dir} {len:0} od {from.X:0},{from.Y:0}{(keep ? " i trzymaj" : "")}");
        Mouse(from, true);
        var n = _rng.RandiRange(3, 8);
        var p = from;
        for (var i = 1; i <= n; i++)
        {
            var q = from + dir * len * i / n;
            Motion(q, q - p);
            p = q;
            await DebugRunner.Frames(Root, 1);
        }
        if (keep)
        {
            var until = Time.GetTicksMsec() + 700;
            while (Time.GetTicksMsec() < until) await DebugRunner.Frames(Root, 1);
        }
        Mouse(p, false);
        await DebugRunner.Frames(Root, 1);
    }

    private async Task LongPress(Vector2 p)
    {
        Note($"przytrzymaj {p.X:0},{p.Y:0}");
        Mouse(p, true);
        var until = Time.GetTicksMsec() + (ulong)_rng.RandiRange(500, 900);
        while (Time.GetTicksMsec() < until) await DebugRunner.Frames(Root, 1);
        if (_rng.Randf() < 0.4f) // przy trzymaniu palec przesuwa cel (Atak, Czekaj)
        {
            var q = p + new Vector2(_rng.RandfRange(-60, 60), _rng.RandfRange(-60, 60));
            Motion(q, q - p);
            await DebugRunner.Frames(Root, 2);
            p = q;
        }
        Mouse(p, false);
        await DebugRunner.Frames(Root, 1);
    }
}
