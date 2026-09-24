using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LifeLike.Core.Actions;
using LifeLike.Core.AI;
using LifeLike.Core.Data;
using LifeLike.Core.Entities;
using LifeLike.Core.Generation;
using LifeLike.Core.Grid;
using LifeLike.Core.Turns;

namespace LifeLike.Game;

/// <summary>
/// Kompozycja gry: dane JSON -> generator lochu -> aktorzy -> TurnManager -> widok.
/// Cała logika siedzi w LifeLike.Core; tu jest tylko klej z silnikiem (input, render, HUD).
/// </summary>
public partial class Main : Node2D
{
    [Export] public int MapWidth { get; set; } = 60;
    [Export] public int MapHeight { get; set; } = 40;
    [Export] public int EnemyCount { get; set; } = 10;
    [Export] public int Seed { get; set; } // 0 = losowy

    private GameDatabase _db = null!;
    private readonly SystemRandom _rng = new();
    private readonly List<string> _log = new();
    private string[] _playableClasses = [];
    private int _classIndex;

    private Dungeon _dungeon = null!;
    private TurnManager _turns = null!;
    private PlayerController _player = new();
    private Actor _hero = null!;
    private readonly List<Actor> _enemies = new();

    private WorldView _view = null!;
    private Hud _hud = null!;
    private Camera2D _camera = null!;

    public override void _Ready()
    {
        GameInput.Register();
        try
        {
            // Najpierw dane gry, potem opcjonalne mody z user://mods (nadpisują po id).
            _db = GameDatabase.Load(new GodotDataSource("res://data"), new GodotDataSource("user://mods"));
        }
        catch (DataLoadException ex)
        {
            GD.PushError(ex.Message);
            GetTree().Quit(1);
            return;
        }

        _playableClasses = _db.Classes.Keys.Where(id => id != "rat").OrderBy(id => id).ToArray();
        _view = new WorldView();
        _hud = new Hud();
        _camera = new Camera2D { Zoom = new Vector2(1.5f, 1.5f), PositionSmoothingEnabled = true };
        AddChild(_view);
        AddChild(_hud);
        AddChild(_camera);
        NewGame();

        if (OS.GetCmdlineUserArgs().Contains("--smoke")) RunSmokeTest();
    }

    /// <summary>
    /// Test dymny dla CI: godot --headless -- --smoke. Rozgrywa kilkadziesiąt tur losowo i wychodzi z kodem 0/1.
    /// </summary>
    private void RunSmokeTest()
    {
        var dirs = new[] { GridPos.Up, GridPos.Down, GridPos.Left, GridPos.Right };
        var rnd = new Random(1);
        for (var i = 0; i < 200 && !_hero.IsDead; i++)
        {
            _player.Submit(Step(dirs[rnd.Next(4)]));
            _turns.Advance();
        }
        Refresh();
        GD.Print($"SMOKE OK: klasy={_db.Classes.Count} bronie={_db.Weapons.Count} runda={_turns.Round} " +
                 $"hp={_hero.Health} wrogowie={_enemies.Count(e => !e.IsDead)}/{_enemies.Count}");
        GetTree().Quit(0);
    }

    private void NewGame()
    {
        _log.Clear();
        _enemies.Clear();
        var seed = Seed != 0 ? Seed : (int)GD.Randi();
        _dungeon = DungeonGenerator.Generate(MapWidth, MapHeight, seed);
        var cls = _db.Classes[_playableClasses[_classIndex]];

        _hero = Actor.FromClass(cls, _db);
        _dungeon.Map.Place(_hero, _dungeon.PlayerStart);
        _player = new PlayerController();
        _turns = new TurnManager(_dungeon.Map);
        _turns.Add(_hero, _player);
        _hero.Died += _ => Log("Zginąłeś. [R] / Start — nowa gra.");

        var ratClass = _db.Classes["rat"];
        foreach (var room in _dungeon.Rooms.Skip(1).Take(EnemyCount))
        {
            var rat = Actor.FromClass(ratClass, _db);
            if (!_dungeon.Map.IsWalkable(room.Center)) continue;
            _dungeon.Map.Place(rat, room.Center);
            _turns.Add(rat, new ChaseAI(_hero, sightRange: 6, _rng));
            rat.Died += a => Log($"{a.Name} ginie.");
            _enemies.Add(rat);
        }

        _turns.CommandExecuted += OnCommandExecuted;
        _view.Bind(_dungeon.Map, _hero, _enemies);
        Log($"Seed {seed}. Grasz jako: {cls.Name} ({_hero.Weapon?.Name}). [Tab] zmiana klasy.");
        Refresh();
    }

    private void OnCommandExecuted(Actor actor, IGameCommand cmd, CommandResult result)
    {
        if (cmd is AttackCommand a && result == CommandResult.Success)
            Log($"{a.Attacker.Name} atakuje {a.Target.Name} ({a.Target.Health}/{a.Target.Stats.MaxHealth} HP)");
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsEcho()) return; // gra turowa: jeden krok na wciśnięcie

        if (e.IsActionPressed(GameInput.Restart)) { NewGame(); return; }
        if (e.IsActionPressed(GameInput.NextClass))
        {
            _classIndex = (_classIndex + 1) % _playableClasses.Length;
            NewGame();
            return;
        }
        if (_hero.IsDead) return;

        IGameCommand? cmd = null;
        if (e.IsActionPressed(GameInput.Up)) cmd = Step(GridPos.Up);
        else if (e.IsActionPressed(GameInput.Down)) cmd = Step(GridPos.Down);
        else if (e.IsActionPressed(GameInput.Left)) cmd = Step(GridPos.Left);
        else if (e.IsActionPressed(GameInput.Right)) cmd = Step(GridPos.Right);
        else if (e.IsActionPressed(GameInput.Wait)) cmd = new WaitCommand(_hero);
        else if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            cmd = MouseCommand(_view.ScreenToGrid(GetGlobalMousePosition()));

        if (cmd is null) return;
        GetViewport().SetInputAsHandled();
        _player.Submit(cmd);
        _turns.Advance();
        Refresh();
    }

    private IGameCommand Step(GridPos dir) => CommandFactory.MoveOrAttack(_hero, dir, _dungeon.Map, _rng);

    /// <summary>Mysz: klik na wroga w zasięgu broni = atak, klik gdzie indziej = krok w tę stronę.</summary>
    private IGameCommand? MouseCommand(GridPos target)
    {
        if (_dungeon.Map.ActorAt(target) is { } other && other != _hero
            && _hero.Position.ChebyshevDistance(target) <= (_hero.Weapon?.Range ?? 1))
            return new AttackCommand(_hero, other, _rng);

        var dx = target.X - _hero.Position.X;
        var dy = target.Y - _hero.Position.Y;
        if (dx == 0 && dy == 0) return new WaitCommand(_hero);
        return Step(Math.Abs(dx) >= Math.Abs(dy) ? new GridPos(Math.Sign(dx), 0) : new GridPos(0, Math.Sign(dy)));
    }

    private void Log(string line)
    {
        _log.Add(line);
        if (_log.Count > 10) _log.RemoveAt(0); // jak w oryginale: max 10 linii
    }

    private void Refresh()
    {
        _camera.Position = _view.GridToScreen(_hero.Position);
        _view.QueueRedraw();
        var alive = _enemies.Count(e => !e.IsDead);
        _hud.Show(_hero, _turns.Round, alive, _log);
        if (alive == 0 && !_hero.IsDead) _hud.Banner("Loch oczyszczony! [R] nowa gra");
    }
}
