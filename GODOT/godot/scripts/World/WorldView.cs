using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
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
    private ActorSprite _ally; // pomocnik z brygady
    private readonly List<ActorSprite> _enemies = new();
    private readonly List<ActorSprite> _pickups = new();
    private readonly List<bool> _enemyAlive = new();
    private readonly List<bool> _pickupActive = new();
    private uint _prevAwake;
    private int _stage = -1;
    private int _turns = -1;
    private int _tier = -1;
    private int _prevBlast;
    private int _prevBlastX = -1, _prevBlastY = -1;
    private int _prevGustTurn = -1;
    private float _dustClock;

    public WorldView() => Effects = new WorldFx(this);

    /// <summary>Menu akcji pod Enter/START: -2 zamknięte, -1 otwarte bez wyboru, 0..3 = góra/prawo/dół/lewo.</summary>
    public int MenuSel { get; set; } = -2;
    public int Marked { get; private set; } = -1;
    /// <summary>Celownik nad wrogiem (celowanie pod A, podgląd pod B); -1 = brak.</summary>
    public int Reticle { get; set; } = -1;
    public CoreGame Game => _g;
    /// <summary>Profil gracza (wygląd z sekretnych zleceń: kask w paski, złota kielnia); null = bez wyglądu.</summary>
    public Func<Profile> ProfileSource { get; set; } = () => null;
    /// <summary>Złota kielnia włączona – złoty błysk broni przy krycie.</summary>
    public bool GoldGlint => _g.MasterWeapon() // v0.21.52 cz. b: broń mistrza
                             || (ProfileSource() is { } p && _g.D.CosmeticGold >= 0 && Secrets.CosmeticOn(_g.D, p, _g.D.CosmeticGold));
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
        _ally = new ActorSprite { AnimPeriod = 0.4f, Breathes = true, AnimPhase = 0.2f, Visible = false };
        _actors.AddChild(_ally);
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

    /// <summary>Punkt ekranu w pikselach UI (dotyk) -> pole mapy (przez kamerę).</summary>
    public Vector2I UiToGrid(Vector2 ui) => ScreenToGrid(GetCanvasTransform().AffineInverse() * ui);

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
        AddEnemySprites();
        _prevBlast = _g.BlastTimer;
        _prevGustTurn = -1;
        SyncPickups(true);
        _hero.MoveTo(GridToScreen(_g.Hero.X, _g.Hero.Y), true);
        _camera.SnapTo(_hero.Position);
        _stage = _g.Stage;
        _tier = _g.Tier;
    }

    /// <summary>Sprite'y dla nowych miejsc na problemy (start etapu, podział w trakcie etapu).</summary>
    private void AddEnemySprites()
    {
        for (var i = _enemies.Count; i < _g.EnemiesCount; i++)
        {
            var def = _g.D.Enemies[_g.Enemies[i].DefId];
            var s = new ActorSprite { BaseFrame = def.Frame, AltFrame = Assets.AnimB(def.Frame), AnimPeriod = 0.33f, AnimPhase = i * 0.33f };
            _actors.AddChild(s);
            _enemies.Add(s);
            _enemyAlive.Add(false);
            s.Visible = false;
            s.MoveTo(GridToScreen(_g.Enemies[i].X, _g.Enemies[i].Y), true);
            if (_g.Enemies[i].Alive && _g.Visible(_g.Enemies[i].X, _g.Enemies[i].Y)) s.Visible = true;
            _enemyAlive[i] = _g.Enemies[i].Alive;
        }
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
        if (_g.Stage != _stage || _g.Turns < _turns || _g.Tier != _tier || _g.EnemiesCount < _enemies.Count)
        {
            ResetStage();
            snap = true;
        }
        else if (_g.EnemiesCount > _enemies.Count) // podział: nowe miejsca na problemy
        {
            var first = _enemies.Count;
            AddEnemySprites();
            for (var i = first; i < _enemies.Count; i++) _enemyAlive[i] = false; // pojawienie się z kurzem w SyncEnemy
        }
        _turns = _g.Turns;
        _map.QueueRedraw();
        SyncHero(snap);
        SyncAlly(snap);
        for (var i = 0; i < _g.EnemiesCount; i++) SyncEnemy(i, snap);
        SyncPickups(snap);
        if (!snap) BehaviorFx();
        _g.ShotEvents = 0;
        _prevBlast = _g.BlastTimer;
        _prevBlastX = _g.BlastX;
        _prevBlastY = _g.BlastY;
        Effects.TakeHits();
        _fog.Sync(snap);
    }

    private void SyncHero(bool snap)
    {
        _hero.BaseFrame = Assets.HeroFrame(_g.D, ProfileSource(), _g.Cls); // kask w paski z sekretu; w miejscu oddycha (bez klatki B), w kroku - chód z actors_anim
        HelmetTint.Apply(_hero, _g.D, ProfileSource(), _g.Cls); // v0.21.52: kolor kasku (cz. b: kask mistrza tylko mistrzem)
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

    /// <summary>Pomocnik z brygady: fachowiec obok bohatera, dopóki pomaga.</summary>
    private void SyncAlly(bool snap)
    {
        if (_g.AllyTurns <= 0 || _g.HelperCalled < 0)
        {
            _ally.Visible = false;
            return;
        }
        var dst = GridToScreen(_g.AllyX, _g.AllyY);
        _ally.BaseFrame = _g.D.Brigade[_g.HelperCalled].Frame;
        if (dst.X != _ally.Position.X && _ally.Visible) _ally.Flip = dst.X < _ally.Position.X;
        _ally.MoveTo(dst, snap || !_ally.Visible);
        _ally.Visible = true;
    }

    private void SyncEnemy(int i, bool snap)
    {
        var e = _g.Enemies[i];
        var s = _enemies[i];
        var vis = e.Alive && _g.Visible(e.X, e.Y);
        if (!_enemyAlive[i] && e.Alive) // podział albo powrót: pojawia się w miejscu, z kurzem
        {
            var def = _g.D.Enemies[e.DefId];
            s.BaseFrame = def.Frame;
            s.AltFrame = Assets.AnimB(def.Frame);
            s.Revive();
            s.MoveTo(GridToScreen(e.X, e.Y), true);
            s.Visible = vis;
            if (vis && !snap) _fx.Burst(GridToScreen(e.X, e.Y), 6, Assets.PDust, 3, 1.2f, 20);
        }
        else if (_enemyAlive[i] && !e.Alive) // usunięty problem: błysk, zanikanie, pył
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
        s.Elite = e.Elite >= 0;
        s.States = vis ? BoonLook.StateBits(_g, i) : 0;
        var aw = e.Alive && e.Awake;
        if (aw && (_prevAwake & (1u << i)) == 0 && vis && !snap) // „!” nad problemem, który Cię zauważył
            _fx.Spawn(s.Position + new Vector2(0, -30), new Vector2(0, -0.3f), 0, 40, Assets.PAlert);
        if (aw) _prevAwake |= 1u << i;
        else _prevAwake &= ~(1u << i);
    }

    /// <summary>Efekty zachowań i mechanik aktu: strzały z dystansu, wybuch, poryw wiatru.</summary>
    private void BehaviorFx()
    {
        var h = _hero.Position;
        for (var i = 0; i < _g.EnemiesCount; i++)
        {
            if ((_g.ShotEvents & (1u << i)) == 0) continue;
            var p = GridToScreen(_g.Enemies[i].X, _g.Enemies[i].Y);
            var dir = (h - p) / 14f;
            for (var k = 1; k <= 4; k++)
                _fx.Spawn(p + (h - p) * (k / 5f), dir * 0.6f, 0, 8 + k * 3, Assets.PSpark, 2);
        }
        if (_prevBlast > 0 && _g.BlastTimer == 0 && _prevBlastX >= 0) // wybuch spadł
        {
            var b = GridToScreen(_prevBlastX, _prevBlastY);
            _fx.Burst(b, 12, Assets.PSpark, 2, 2.6f, 22);
            _fx.Burst(b, 8, Assets.PDust, 3, 1.8f, 26);
            Flash(new Color(1f, 0.55f, 0.15f), 0.35f);
            _camera.Shake(0.25f);
        }
        if (_g.ActIs(ActMechanic.Gust)) // poryw: pył leci w stronę porywu
        {
            var v = _g.ADef.MechValue;
            var t = _g.Turns - _g.StageStartTurn;
            if (t > 0 && t % v == 0 && _g.Turns != _prevGustTurn)
            {
                _prevGustTurn = _g.Turns;
                var d = (t / v + _g.PatternStage()) & 3;
                var gv = new Vector2(CoreGame.GustVec[d, 0], CoreGame.GustVec[d, 1]);
                for (var k = 0; k < 10; k++)
                    _fx.Spawn(h - gv * 70 + new Vector2((float)GD.RandRange(-40, 40), (float)GD.RandRange(-40, 40)), gv * 5f, 0, 24, Assets.PDust, 3);
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_hero is null) return;
        _camera.Follow(_hero.Position, delta);
        if (_g is not null && _g.ActIs(ActMechanic.Dust)) // akt III: pył wisi w powietrzu
        {
            _dustClock += (float)delta;
            if (_dustClock > 0.12f)
            {
                _dustClock = 0;
                _fx.Spawn(_hero.Position + new Vector2((float)GD.RandRange(-220, 220), (float)GD.RandRange(-150, 150)),
                    new Vector2((float)GD.RandRange(-0.3, 0.3), (float)GD.RandRange(-0.25, 0.05)), 0, 90, Assets.PDust, 3);
            }
        }
    }
}
