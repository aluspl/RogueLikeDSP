using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Widok etapu: kafle (MapLayer), nakładki (OverlayLayer), postacie (ActorSprite), mgła ze światłem (FogLayer),
/// cząsteczki i liczby (FxLayer), znaczniki i menu akcji (MarksLayer), kamera (WorldCamera), efekty (WorldFx).
/// Jedyne miejsce, które zna rzutowanie siatka -> ekran (przejście na 2.5D = podmiana tej klasy).
/// </summary>
public partial class WorldView : Node2D
{
    public const int Cell = Assets.Cell;
    public static readonly Vector2I[] MenuDirs = [new(0, -1), new(1, 0), new(0, 1), new(-1, 0)];

    private CoreGame _g;
    private readonly MapLayer _map = new();
    private readonly OverlayLayer _overlay = new();
    private readonly Node2D _actors = new();
    private readonly FogLayer _fog = new();
    private readonly FxLayer _fx = new();
    private readonly MarksLayer _marks = new();
    private readonly WorldCamera _camera = new();
    private ActorSprite _hero;
    private readonly List<ActorSprite> _enemies = new();
    private readonly List<ActorSprite> _pickups = new();
    private readonly List<bool> _enemyAlive = new();
    private readonly List<bool> _pickupActive = new();
    private uint _prevAwake;
    private int _stage = -1;
    private int _turns = -1;
    private int _tier = -1;

    public WorldView() => Effects = new WorldFx(this);

    /// <summary>Menu akcji pod Enter/START: -2 zamknięte, -1 otwarte bez wyboru, 0..3 = góra/prawo/dół/lewo.</summary>
    public int MenuSel { get; set; } = -2;
    public int Marked { get; private set; } = -1;
    /// <summary>Celownik nad wrogiem (celowanie pod A, podgląd pod B); -1 = brak.</summary>
    public int Reticle { get; set; } = -1;
    public CoreGame Game => _g;
    public ActorSprite HeroSprite => _hero;
    public FxLayer Fx => _fx;
    public WorldCamera Camera => _camera;
    public WorldFx Effects { get; }
    public bool OverviewOn => _camera.Overview;

    /// <summary>Błysk ekranu (kolor, siła 0..1) - rysuje go HUD nad mapą.</summary>
    public Action<Color, float> OnFlash;

    public ActorSprite EnemySprite(int i) => i >= 0 && i < _enemies.Count ? _enemies[i] : null;

    public void Flash(Color c, float strength) => OnFlash?.Invoke(c, strength);

    public override void _Ready()
    {
        _actors.YSortEnabled = true;
        AddChild(_map);
        AddChild(_overlay);
        AddChild(_actors);
        AddChild(_fog);
        AddChild(_fx);
        AddChild(_marks);
        AddChild(_camera);
        _marks.Bind(this);
        _hero = new ActorSprite { AnimPeriod = 0.4f, Breathes = true };
        _actors.AddChild(_hero);
    }

    public void Bind(CoreGame g)
    {
        _g = g;
        _map.Bind(g);
        _overlay.Bind(g);
        _fog.Bind(g);
    }

    public Vector2 GridToScreen(int x, int y) => new(x * Cell + Cell / 2, y * Cell + Cell / 2);

    public Vector2I ScreenToGrid(Vector2 world) => new(Mathf.FloorToInt(world.X / Cell), Mathf.FloorToInt(world.Y / Cell));

    public void Mark(int enemy) => Marked = enemy;

    public void FlashRange() => _overlay.FlashRange();

    /// <summary>Ramki pól w zasięgu broni na stałe (trzymane A).</summary>
    public void ShowRange(bool on) => _overlay.RangeHeld = on;

    public void ToggleOverview() => _camera.ToggleOverview(_g);

    /// <summary>Sprite problemu w trakcie znikania blisko pozycji (trafienie, które go usunęło).</summary>
    public ActorSprite FindDying(Vector2 pos)
    {
        foreach (var s in _enemies)
        {
            if (s.Dying && s.Position.DistanceTo(pos) < Cell) return s;
        }
        return null;
    }

    /// <summary>Nowy etap / budowa: sprite'y od nowa, pozycje bez animacji.</summary>
    private void ResetStage()
    {
        foreach (var e in _enemies) e.QueueFree();
        foreach (var p in _pickups) p.QueueFree();
        _enemies.Clear();
        _pickups.Clear();
        _enemyAlive.Clear();
        _pickupActive.Clear();
        _fx.Clear();
        _prevAwake = 0;
        for (var i = 0; i < _g.EnemiesCount; i++)
        {
            var def = _g.D.Enemies[_g.Enemies[i].DefId];
            var s = new ActorSprite { BaseFrame = def.Frame, AltFrame = Assets.AnimB(def.Frame), AnimPeriod = 0.33f, AnimPhase = i * 0.33f };
            _actors.AddChild(s);
            _enemies.Add(s);
            _enemyAlive.Add(_g.Enemies[i].Alive);
            s.MoveTo(GridToScreen(_g.Enemies[i].X, _g.Enemies[i].Y), true);
        }
        SyncPickups(true);
        _hero.MoveTo(GridToScreen(_g.Hero.X, _g.Hero.Y), true);
        _camera.SnapTo(_hero.Position);
        _stage = _g.Stage;
        _tier = _g.Tier;
    }

    private void SyncPickups(bool snap)
    {
        for (var i = _pickups.Count; i < _g.PickupsCount; i++)
        {
            var p = _g.Pickups[i];
            var s = new ActorSprite { BaseFrame = Assets.PickupFrame(p), Bob = true, AnimPhase = i * 0.13f, DrawOffset = new Vector2(0, 2) };
            _actors.AddChild(s);
            s.MoveTo(GridToScreen(p.X, p.Y) - new Vector2(0, 2), true);
            _pickups.Add(s);
            _pickupActive.Add(p.Active);
            if (!snap && _g.Explored(p.X, p.Y)) // drop: wyskakuje z błyskiem
            {
                s.Flash(Colors.White);
                _fx.Burst(s.Position, 6, Assets.PStar, 1, 1.4f, 22);
            }
        }
        for (var i = 0; i < _pickups.Count; i++)
        {
            var p = _g.Pickups[i];
            var s = _pickups[i];
            if (_pickupActive[i] && !p.Active && !snap) // zebrane: gwiazdki
            {
                for (var k = 0; k < 6; k++)
                    _fx.Spawn(s.Position + s.DrawOffset, new Vector2(Mathf.Cos(k * Mathf.Tau / 6), Mathf.Sin(k * Mathf.Tau / 6)) * 1.3f, 0, 20, Assets.PStar);
            }
            _pickupActive[i] = p.Active;
            s.BaseFrame = Assets.PickupFrame(p);
            s.Visible = p.Active && _g.Explored(p.X, p.Y);
        }
    }

    /// <summary>
    /// Po każdej zmianie stanu gry: pozycje, widoczność, paski HP, efekty trafień i zdarzeń tury.
    /// Zeruje listę trafień i zdarzenia tury (jak warstwa GBA po każdej turze).
    /// </summary>
    public void Sync()
    {
        if (_g is null || _hero is null) return;
        var snap = false;
        if (_g.Stage != _stage || _g.Turns < _turns || _g.Tier != _tier || _g.EnemiesCount != _enemies.Count)
        {
            ResetStage();
            snap = true;
        }
        _turns = _g.Turns;
        _map.QueueRedraw();
        SyncHero(snap);
        for (var i = 0; i < _g.EnemiesCount; i++) SyncEnemy(i, snap);
        SyncPickups(snap);
        Effects.TakeHits();
        _fog.Sync(snap);
    }

    private void SyncHero(bool snap)
    {
        var cls = _g.D.Classes[_g.Cls];
        _hero.BaseFrame = cls.Frame; // w miejscu oddycha (bez klatki B), w kroku - chód z actors_anim
        var heroDst = GridToScreen(_g.Hero.X, _g.Hero.Y);
        if (!snap && heroDst != _hero.Position && _hero.Position.DistanceTo(heroDst) <= Cell * 2) // pył spod butów
        {
            if (heroDst.X != _hero.Position.X) _hero.Flip = heroDst.X < _hero.Position.X;
            for (var k = -1; k <= 1; k += 2)
                _fx.Spawn(_hero.Position + new Vector2(k * 6, 12), new Vector2(k * 0.5f, -0.5f), 0, 18, Assets.PDust, 3);
        }
        _hero.MoveTo(heroDst, snap);
        _hero.Visible = true;
        _hero.Modulate = _g.Hero.Alive ? Colors.White : Pal.DeadHero;
    }

    private void SyncEnemy(int i, bool snap)
    {
        var e = _g.Enemies[i];
        var s = _enemies[i];
        var vis = e.Alive && _g.Visible(e.X, e.Y);
        if (_enemyAlive[i] && !e.Alive) // usunięty problem: błysk, zanikanie, pył
        {
            s.Die();
            _fx.Burst(s.Position, 8, Assets.PDust, 3, 1.6f, 22);
        }
        else if (!s.Dying)
        {
            if (vis && !s.Visible) s.Revive();
            s.Visible = vis;
        }
        _enemyAlive[i] = e.Alive;
        if (!e.Alive) return;
        s.MoveTo(GridToScreen(e.X, e.Y), snap || !s.Visible);
        s.Flip = _g.Hero.X < e.X;
        var showBar = vis && (e.Awake || e.Hp < e.MaxHp) && e.MaxHp > 0;
        s.HpFill = showBar ? Mathf.Clamp(e.Hp / (float)e.MaxHp, 0f, 1f) : -1f;
        s.HpColor = Pal.HpColor(e.Hp, e.MaxHp);
        var aw = e.Alive && e.Awake;
        if (aw && (_prevAwake & (1u << i)) == 0 && vis && !snap) // „!” nad problemem, który Cię zauważył
            _fx.Spawn(s.Position + new Vector2(0, -30), new Vector2(0, -0.3f), 0, 40, Assets.PAlert);
        if (aw) _prevAwake |= 1u << i;
        else _prevAwake &= ~(1u << i);
    }

    public override void _Process(double delta)
    {
        if (_hero is null) return;
        _camera.Follow(_hero.Position, delta);
    }
}
