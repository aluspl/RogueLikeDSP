using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Hud;
using LifeLike.Game.Phone;
using LifeLike.Game.Touch;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Screens.Play;

/// <summary>
/// Sterowanie dotykiem na mapie (jedną ręką): pasek akcji - Atak (dotknięcie: najbliższy cel, trzymanie: celownik,
/// palec przesuwa cel, puszczenie = atak), Moc, Termos, Czekaj (trzymanie: karta problemu), Telefon (trzymanie:
/// podgląd mapy etapu); przesunięcie palcem po mapie = krok, trzymanie po przesunięciu = kolejne kroki; dotknięcie
/// pola = marsz krok po kroku (AutoWalk); dotknięcie problemu = atak w zasięgu albo marsz do niego; przytrzymanie
/// problemu = jego karta; opcjonalnie gałka. Wszystko przez te same akcje co klawiatura (Aiming, EnemyLook, PlayCommands).
/// </summary>
public sealed class TouchPlay
{
    private const float PhoneHold = 0.4f;

    private readonly App _app;
    private readonly GameScreen _screen;
    private BarButton _down = BarButton.None;
    private float _held, _stickTimer;
    private bool _mapByHold, _lookByPress;

    public TouchPlay(App app, GameScreen screen)
    {
        _app = app;
        _screen = screen;
        Walk = new AutoWalk(app);
    }

    public AutoWalk Walk { get; }

    private SceneNodes N => _app.Nodes;
    private CoreGame G => _app.Session.Game;
    private TouchControls T => N.Touch;

    /// <summary>Koniec gestu bez akcji (wyjście z ekranu).</summary>
    public void Reset()
    {
        _down = BarButton.None;
        _mapByHold = _lookByPress = false;
        T.Bar.Pressed = BarButton.None;
        T.Stick.End();
        Walk.Stop();
    }

    public void Process(double delta)
    {
        Walk.Process(delta);
        var dt = (float)delta;
        if (_down == BarButton.Phone && !_mapByHold)
        {
            _held += dt;
            if (_held >= PhoneHold)
            {
                _mapByHold = true;
                if (!N.World.OverviewOn) _screen.ToggleOverview();
            }
        }
        if (!T.Stick.Active || T.Stick.Dir == Vector2I.Zero) return;
        _stickTimer -= dt;
        if (_stickTimer > 0) return;
        _stickTimer = GestureTracker.RepeatEvery;
        Step(T.Stick.Dir);
    }

    public bool Handle(in Gesture g)
    {
        if (g.Kind == GestureKind.Down) Walk.Stop();
        if (_screen.Menu.IsOpen) // menu akcji z klawiatury: dotknięcie zamyka
        {
            if (g.Kind == GestureKind.Tap)
            {
                _screen.Menu.Close();
                _app.Refresh();
            }
            return true;
        }
        if (N.World.OverviewOn && _down == BarButton.None)
        {
            if (g.Kind == GestureKind.Tap) _screen.ToggleOverview();
            return true;
        }
        switch (g.Kind)
        {
            case GestureKind.Down:
                Down(g.Pos);
                break;
            case GestureKind.Drag:
                Drag(g.Pos);
                break;
            case GestureKind.Swipe:
            case GestureKind.SwipeRepeat:
                if (_down == BarButton.Wait && _screen.Look.Active && g.Kind == GestureKind.Swipe) _screen.Look.Cycle(g.Dir.X + g.Dir.Y);
                else if (_down == BarButton.None && !T.Stick.Active && !_lookByPress) Step(g.Dir);
                break;
            case GestureKind.LongPress:
                if (_down == BarButton.None && !T.Stick.Active) LongPress(g.Pos);
                break;
            case GestureKind.Tap:
                if (_down == BarButton.None && !T.Stick.Active && !_lookByPress) Tap(g.Pos);
                break;
            case GestureKind.Up:
                Up(g.Pos);
                break;
        }
        return true;
    }

    private void Down(Vector2 p)
    {
        var b = ActionBar.HitTest(p);
        if (b != BarButton.None)
        {
            _down = b;
            _held = 0;
            T.Bar.Pressed = b;
            if (b == BarButton.Attack) _screen.Aim.Begin();
            else if (b == BarButton.Wait) _screen.Look.Begin();
            return;
        }
        if (!VirtualStick.Hit(p)) return;
        T.Stick.Begin(p);
        if (T.Stick.Dir != Vector2I.Zero) Step(T.Stick.Dir);
        _stickTimer = GestureTracker.RepeatDelay;
    }

    private void Drag(Vector2 p)
    {
        if (T.Stick.Active)
        {
            if (T.Stick.Move(p) && T.Stick.Dir != Vector2I.Zero)
            {
                Step(T.Stick.Dir);
                _stickTimer = GestureTracker.RepeatDelay;
            }
            return;
        }
        if (_down == BarButton.Attack && _screen.Aim.Active && !ActionBar.Covers(p)) _screen.Aim.PointAt(N.World.UiToGrid(p));
    }

    private void Up(Vector2 p)
    {
        var b = _down;
        _down = BarButton.None;
        T.Bar.Pressed = BarButton.None;
        if (T.Stick.Active) T.Stick.End();
        if (_lookByPress)
        {
            _lookByPress = false;
            _screen.Look.Cancel();
            return;
        }
        var same = ActionBar.HitTest(p) == b;
        switch (b)
        {
            case BarButton.Attack:
                if (_screen.Aim.Active) _screen.Aim.Commit();
                break;
            case BarButton.Wait:
                if (_screen.Look.Active) _screen.Look.End();
                break;
            case BarButton.Ability when same:
                _app.AfterAction(PlayCommands.UseAbility(G, N.World));
                break;
            case BarButton.Thermos when same:
                _app.AfterAction(PlayCommands.Drink(G));
                break;
            case BarButton.Phone:
                if (_mapByHold)
                {
                    _mapByHold = false;
                    if (N.World.OverviewOn) _screen.ToggleOverview();
                }
                else if (same)
                {
                    _app.Flow.Phone.Open();
                }
                break;
        }
    }

    /// <summary>Krok w kierunku (przesunięcie palca, gałka) - zwykła tura PlayerMove.</summary>
    private void Step(Vector2I d)
    {
        if (_app.Flow.Current != _screen || d == Vector2I.Zero) return;
        _app.AfterAction(G.PlayerMove(d.X, d.Y));
    }

    private void Tap(Vector2 p)
    {
        if (N.Banners.Hit(p)) // powiadomienie: jego zakładka w telefonie
        {
            var tab = N.Banners.TabAt(p);
            _app.Flow.Phone.Open(tab >= 0 ? tab : -1);
            return;
        }
        if (ActionBar.Covers(p)) return;
        if (p.Y < Layout.SafeTop + (HudTop.Height + 1) * Layout.HudScale) // pasek HUD: pulpit postaci w telefonie
        {
            _app.Flow.Phone.Open(PhoneTabs.Start);
            return;
        }
        var g = G;
        var cell = N.World.UiToGrid(p);
        var ei = g.EnemyAt(cell.X, cell.Y);
        if (ei >= 0 && g.Visible(cell.X, cell.Y))
        {
            if (CoreGame.Cheb(g.Hero.X, g.Hero.Y, cell.X, cell.Y) <= g.Weapon.Range)
            {
                _app.AfterAction(g.PlayerAttack(ei));
                return;
            }
            if (!Walk.Start(cell, ei)) N.World.FlashRange();
            return;
        }
        var d = cell - new Vector2I(g.Hero.X, g.Hero.Y);
        if (d == Vector2I.Zero) return;
        if (Mathf.Abs(d.X) + Mathf.Abs(d.Y) == 1)
        {
            Step(d);
            return;
        }
        Walk.Start(cell);
    }

    private void LongPress(Vector2 p)
    {
        if (ActionBar.Covers(p)) return;
        var g = G;
        var cell = N.World.UiToGrid(p);
        var ei = g.EnemyAt(cell.X, cell.Y);
        if (ei < 0 || !g.Visible(cell.X, cell.Y)) return;
        _lookByPress = true;
        _screen.Look.ShowEnemy(ei);
    }
}
