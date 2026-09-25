using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Widok etapu: kafle (MapLayer), nakładki (OverlayLayer), postacie (ActorSprite), mgła ze światłem (FogLayer),
/// cząsteczki i liczby (FxLayer), znaczniki i menu akcji (MarksLayer), kamera ze wstrząsem i podglądem mapy.
/// Jedyne miejsce, które zna rzutowanie siatka -> ekran (przejście na 2.5D = podmiana tej klasy).
/// Efekty zdarzeń tury (trafienia, „!”, śmierć problemu, dropy) jak refresh() w GBA/src/main.cpp.
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
    private readonly Camera2D _camera = new();
    private ActorSprite _hero;
    private readonly List<ActorSprite> _enemies = new();
    private readonly List<ActorSprite> _pickups = new();
    private readonly List<bool> _enemyAlive = new();
    private readonly List<bool> _pickupActive = new();
    private uint _prevAwake;
    private int _stage = -1;
    private int _turns = -1;
    private int _tier = -1;
    private float _shake;
    private bool _overview;

    /// <summary>Menu akcji pod Enter/START: -2 zamknięte, -1 otwarte bez wyboru, 0..3 = góra/prawo/dół/lewo.</summary>
    public int MenuSel { get; set; } = -2;
    public int Marked { get; private set; } = -1;
    public CoreGame Game => _g;
    public ActorSprite HeroSprite => _hero;
    public FxLayer Fx => _fx;
    public bool OverviewOn => _overview;

    /// <summary>Błysk ekranu (kolor, siła 0..1) - rysuje go HUD nad mapą.</summary>
    public Action<Color, float> OnFlash;

    public ActorSprite EnemySprite(int i) => i >= 0 && i < _enemies.Count ? _enemies[i] : null;

    public override void _Ready()
    {
        _actors.YSortEnabled = true;
        AddChild(_map);
        AddChild(_overlay);
        AddChild(_actors);
        AddChild(_fog);
        AddChild(_fx);
        AddChild(_marks);
        _camera.PositionSmoothingEnabled = true;
        _camera.PositionSmoothingSpeed = 9f;
        _camera.LimitLeft = -Cell;
        _camera.LimitTop = -Cell * 2;
        _camera.LimitRight = Level.W * Cell + Cell;
        _camera.LimitBottom = Level.H * Cell + Cell * 2;
        AddChild(_camera);
        _marks.Bind(this);
        _hero = new ActorSprite { AnimPeriod = 0.4f };
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

    /// <summary>Podgląd całego etapu (L na GBA): kamera oddala się 4x.</summary>
    public void ToggleOverview()
    {
        _overview = !_overview;
        _camera.Zoom = _overview ? new Vector2(0.25f, 0.25f) : Vector2.One;
        _camera.PositionSmoothingEnabled = !_overview;
        if (_overview) _camera.Position = new Vector2(Level.W * Cell / 2, Level.H * Cell / 2);
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
        _camera.Position = _hero.Position;
        _camera.ResetSmoothing();
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

        var cls = _g.D.Classes[_g.Cls];
        _hero.BaseFrame = cls.Frame;
        _hero.AltFrame = Assets.AnimB(cls.Frame);
        var heroDst = GridToScreen(_g.Hero.X, _g.Hero.Y);
        if (!snap && heroDst != _hero.Position && _hero.Position.DistanceTo(heroDst) <= Cell * 2) // pył spod butów
        {
            if (heroDst.X != _hero.Position.X) _hero.Flip = heroDst.X < _hero.Position.X;
            for (var k = -1; k <= 1; k += 2)
                _fx.Spawn(_hero.Position + new Vector2(k * 6, 12), new Vector2(k * 0.5f, -0.5f), 0, 18, Assets.PDust, 3);
        }
        _hero.MoveTo(heroDst, snap);
        _hero.Visible = true;
        _hero.Modulate = _g.Hero.Alive ? Colors.White : new Color(0.5f, 0.5f, 0.55f);

        for (var i = 0; i < _g.EnemiesCount; i++)
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
            if (!e.Alive) continue;
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
        SyncPickups(snap);
        TakeHits();
        _fog.Sync(snap);
    }

    /// <summary>Trafienia z rdzenia: liczby, iskry, błyski, szturchnięcia, wstrząs i dźwięk (hit / hurt).</summary>
    private void TakeHits()
    {
        var heroHitEnemy = false;
        for (var i = 0; i < _g.HitsCount; i++)
        {
            var h = _g.Hits[i];
            var pos = GridToScreen(h.X, h.Y);
            var f = new Floater { Pos = pos + new Vector2(0, -26 - 15 * (i % 3)) };
            switch (h.Kind)
            {
                case HitKind.Dodge:
                    f.Text = "Unik!";
                    f.Ink = Ink.MapGood;
                    break;
                case HitKind.Crit:
                    f.Text = $"KRYT! -{h.Amount}";
                    f.Ink = Ink.MapLoot;
                    f.Life = 1f;
                    OnFlash?.Invoke(new Color(1f, 0.87f, 0.25f), 0.3f);
                    break;
                default:
                    f.Text = $"-{h.Amount}";
                    f.Ink = h.OnHero ? Ink.MapBad : Ink.Map;
                    break;
            }
            _fx.AddFloater(f);
            if (h.Kind != HitKind.Dodge) _fx.Burst(pos, h.OnHero ? 3 : 5, Assets.PSpark, 2, 1.5f, 16);
            if (!h.OnHero && h.Kind != HitKind.Dodge)
            {
                var ei = _g.EnemyAt(h.X, h.Y);
                var target = ei >= 0 ? _enemies[ei] : FindDying(pos);
                target?.Flash(Colors.White);
                if (!heroHitEnemy)
                {
                    heroHitEnemy = true;
                    if (CoreGame.Cheb(_g.Hero.X, _g.Hero.Y, h.X, h.Y) <= 1) _hero.Bump(pos - _hero.Position);
                    else _hero.Bump(new Vector2(Mathf.Sign(pos.X - _hero.Position.X), Mathf.Sign(pos.Y - _hero.Position.Y)) * 0.6f);
                }
            }
            if (i == 0 || h.OnHero != _g.Hits[i - 1].OnHero) Sfx.Play(h.OnHero && h.Kind != HitKind.Dodge ? "hurt" : "hit");
        }
        if (_g.HeroHit)
        {
            _hero.Flash(new Color(1f, 0.25f, 0.25f));
            _hero.Blink(0.27f);
            _shake = 0.14f;
            OnFlash?.Invoke(new Color(1f, 0.13f, 0.13f), 0.3f);
            for (var i = 0; i < _g.EnemiesCount; i++) // problemy obok bohatera „uderzają”
            {
                var e = _g.Enemies[i];
                if (e.Alive && e.Awake && CoreGame.Cheb(e.X, e.Y, _g.Hero.X, _g.Hero.Y) <= 1)
                    _enemies[i].Bump(_hero.Position - _enemies[i].Position);
            }
        }
        _g.HitsCount = 0;
        _g.TurnEvents = 0;
        _g.HeroHit = false;
    }

    private ActorSprite FindDying(Vector2 pos)
    {
        foreach (var s in _enemies)
        {
            if (s.Dying && s.Position.DistanceTo(pos) < Cell) return s;
        }
        return null;
    }

    /// <summary>Gwiazdki awansu dookoła bohatera.</summary>
    public void LevelUpFx()
    {
        for (var k = 0; k < 8; k++)
        {
            var a = k * Mathf.Tau / 8;
            _fx.Spawn(_hero.Position, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 1.1f, 0, 30, Assets.PStar);
        }
        _hero.Flash(new Color(1f, 0.95f, 0.5f));
    }

    /// <summary>Konfetti na odbiór budowy (wygrana).</summary>
    public void Confetti()
    {
        var c = _camera.GetScreenCenterPosition();
        for (var k = 0; k < 40; k++)
            _fx.Spawn(c + new Vector2(_fx.Rand(-300, 300), -190 - _fx.Rand(0, 60)), new Vector2(_fx.Rand(-0.6f, 0.6f), _fx.Rand(1f, 2.4f)),
                      0.02f, 150, Assets.PConfetti + _fx.RandInt(4));
    }

    /// <summary>Efekty mocy zawodów + błysk ekranu w kolorze mocy (ability_fx w main.cpp).</summary>
    public void AbilityFx()
    {
        var hits = _g.Hits;
        var hitsCount = _g.HitsCount;
        const int hitsFrom = 0;
        var h = _hero.Position;
        Vector2[] dir8 = [new(2, 0), new(1, 1), new(0, 2), new(-1, 1), new(-2, 0), new(-1, -1), new(0, -2), new(1, -1)];
        Color flash;
        switch (_g.CDef.Ability)
        {
            case AbilityEffect.Stun:
                flash = new Color(0.65f, 0.52f, 1f);
                foreach (var d in dir8) _fx.Spawn(h, d * 1.8f, 0, 22, Assets.PRing, 2);
                for (var i = 0; i < _g.EnemiesCount; i++)
                {
                    var e = _g.Enemies[i];
                    if (e.Alive && e.Stun > 0 && _g.Visible(e.X, e.Y))
                        _fx.Spawn(GridToScreen(e.X, e.Y) + new Vector2(8, -16), new Vector2(0.4f, -0.8f), 0, 40, Assets.PZzz);
                }
                break;
            case AbilityEffect.Wall:
                flash = new Color(1f, 0.52f, 0.2f);
                for (var i = 0; i < _g.WallsCount; i++)
                {
                    var w = GridToScreen(_g.Walls[i].X, _g.Walls[i].Y);
                    _fx.Spawn(w + new Vector2(0, 8), new Vector2(_fx.Rand(-1, 1), -3.2f), 0.3f, 20, Assets.PBrick);
                    _fx.Spawn(w + new Vector2(-8, 12), new Vector2(-0.6f, -0.4f), 0, 16, Assets.PDust, 3);
                    _fx.Spawn(w + new Vector2(8, 12), new Vector2(0.6f, -0.4f), 0, 16, Assets.PDust, 3);
                }
                break;
            case AbilityEffect.Volley:
                flash = new Color(1f, 0.9f, 0.26f);
                for (var i = hitsFrom; i < hitsCount; i++)
                {
                    var t = GridToScreen(hits[i].X, hits[i].Y);
                    _fx.Spawn(h, (t - h) / 10f, 0, 10, Assets.PNail);
                }
                break;
            case AbilityEffect.Chain:
            {
                flash = new Color(0.4f, 0.9f, 1f);
                var from = h;
                for (var i = hitsFrom; i < hitsCount; i++)
                {
                    if (hits[i].OnHero) continue;
                    var to = GridToScreen(hits[i].X, hits[i].Y);
                    for (var k = 1; k <= 3; k++) _fx.Spawn(from + (to - from) * k / 4f, Vector2.Zero, 0, 12, Assets.PBolt, 2);
                    from = to;
                }
                break;
            }
            case AbilityEffect.Flush:
                flash = new Color(0.26f, 0.97f, 0.52f);
                for (var k = 0; k < 10; k++)
                    _fx.Spawn(h + new Vector2(_fx.Rand(-150, 150), 8), new Vector2(0, _fx.Rand(-3f, -1f)), 0, 26, (k & 1) == 1 ? Assets.PPlus : Assets.PDrop);
                break;
            default:
                flash = Colors.White;
                foreach (var d in dir8) _fx.Spawn(h + d * 12, new Vector2(-d.Y, d.X) * 1.6f, 0, 16, Assets.PSpark, 2);
                break;
        }
        OnFlash?.Invoke(flash, 0.38f);
    }

    public override void _Process(double delta)
    {
        if (_hero is null || _overview) return;
        _camera.Position = _hero.Position;
        if (_shake > 0)
        {
            _shake = Mathf.Max(0, _shake - (float)delta);
            _camera.Offset = new Vector2(((int)(_shake * 60) & 1) == 1 ? 4 : -4, 0);
        }
        else
        {
            _camera.Offset = Vector2.Zero;
        }
    }
}
