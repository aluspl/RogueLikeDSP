using LifeLike.Core;
using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Screens.Play;

/// <summary>
/// Celowanie pod A jak na GBA: wciśnięcie wybiera najbliższy cel w zasięgu, po chwili trzymania widać ramki
/// zasięgu i celownik, strzałki przełączają cel, puszczenie A atakuje. Krótkie A = atak najbliższego;
/// bez celu w zasięgu zasięg tylko mignie. v0.23.0: celowanie trybem B (F, pad L + A, przycisk Cios B) – cele i zasięg
/// wzoru B, pola wzoru na wybrany cel jaśniej; bez celu, ale z pękniętą ścianą obok (Przecinak, Łom) – otwarcie ściany.
/// </summary>
public sealed class Aiming
{
    /// <summary>Po tylu sekundach trzymania A pokazuje się zasięg i celownik (8 klatek na GBA).</summary>
    public const float RevealTime = 8 / 60f;

    private readonly App _app;
    private readonly sbyte[] _targets = new sbyte[CoreGame.MaxEnemies];
    private int _count, _sel;
    private float _time;
    private bool _revealed;

    public Aiming(App app) => _app = app;

    public bool Active { get; private set; }

    /// <summary>v0.23.0: celowanie trybem B (inaczej zwykły atak A).</summary>
    public bool ModeB { get; private set; }

    /// <summary>v0.23.0: akcja, której puszczenie atakuje (A albo WeaponB).</summary>
    private GameAction _trigger = GameAction.A;

    /// <summary>Wybrany cel (indeks wroga) albo -1.</summary>
    public int Target => Active && _count > 0 ? _targets[_sel] : -1;

    public int Count => _count;

    private SceneNodes N => _app.Nodes;

    public void Begin() => Begin(false, GameAction.A);

    /// <summary>
    /// v0.23.0: celowanie trybem A albo B; trigger = akcja, której puszczenie atakuje. Tryb B w odnowieniu – tylko podpowiedź
    /// (bez celowania i bez tury); zwraca, czy celowanie ruszyło.
    /// </summary>
    public bool Begin(bool modeB, GameAction trigger)
    {
        var g = _app.Session.Game;
        if (modeB && g.WeaponBCd > 0)
        {
            PlayCommands.Notice(g, Loc.F("cios_b_odnowienie", g.WeaponBCd));
            _app.AfterAction(false);
            return false;
        }
        Active = true;
        ModeB = modeB;
        _trigger = trigger;
        _time = 0;
        _sel = 0;
        _revealed = false;
        _count = modeB ? TargetsB(g, _targets) : g.TargetsInRange(_targets);
        N.World.Mark(Target);
        N.World.AimPattern(modeB ? g.Weapon.ModeB : g.Weapon.ModeA, Target);
        return true;
    }

    /// <summary>v0.23.0: cele trybu B – żywe, widoczne, w zasięgu wzoru B i na jego polach (od najbliższego).</summary>
    public static int TargetsB(CoreGame g, System.Span<sbyte> output)
    {
        var m = g.Weapon.ModeB;
        var reach = g.ModeRange(m);
        var n = 0;
        for (var d = 1; d <= reach; ++d)
        {
            for (var i = 0; i < g.EnemiesCount && n < output.Length; ++i)
            {
                var e = g.Enemies[i];
                if (e.Alive && g.Visible(e.X, e.Y) && CoreGame.Cheb(g.Hero.X, g.Hero.Y, e.X, e.Y) == d && g.InPattern(m, i)) output[n++] = (sbyte)i;
            }
        }
        return n;
    }

    public void Process(double delta)
    {
        if (!Active) return;
        _time += (float)delta;
        if (_revealed || _time < RevealTime) return;
        _revealed = true;
        N.World.ShowRange(true);
        Show();
    }

    /// <summary>Wejście w trakcie celowania (wszystko inne jest połykane, jak pętla aiming na GBA).</summary>
    public bool HandleInput(InputCmd e)
    {
        if (e.IsReleased(_trigger))
        {
            Commit();
            return true;
        }
        if (_count > 1 && e.Is(GameAction.Right | GameAction.Down, true)) Cycle(1);
        else if (_count > 1 && e.Is(GameAction.Left | GameAction.Up, true)) Cycle(-1);
        return true;
    }

    /// <summary>Następny / poprzedni cel (odsłania celownik od razu).</summary>
    public void Cycle(int d)
    {
        if (_count == 0) return;
        _sel = (_sel + d + _count) % _count;
        if (!_revealed)
        {
            _revealed = true;
            N.World.ShowRange(true);
        }
        Show();
    }

    /// <summary>Dotyk: palec nad polem - cel najbliższy temu polu (odsłania celownik).</summary>
    public void PointAt(Vector2I cell)
    {
        if (!Active || _count == 0) return;
        var g = _app.Session.Game;
        int best = _sel, bestD = int.MaxValue;
        for (var i = 0; i < _count; i++)
        {
            var e = g.Enemies[_targets[i]];
            var d = CoreGame.Cheb(e.X, e.Y, cell.X, cell.Y);
            if (d >= bestD) continue;
            bestD = d;
            best = i;
        }
        if (best == _sel && _revealed) return;
        _sel = best;
        if (!_revealed)
        {
            _revealed = true;
            N.World.ShowRange(true);
        }
        Show();
    }

    private void Show()
    {
        N.World.Mark(Target);
        N.World.Reticle = Target;
        var g = _app.Session.Game;
        N.World.AimPattern(ModeB ? g.Weapon.ModeB : g.Weapon.ModeA, Target);
        if (_count == 0)
        {
            if (ModeB) // v0.23.0: tryb B bez celu – pęknięta ściana obok albo brak celu z opisem wzoru
            {
                var b = g.CanWeaponBOpenWall() ? Loc.T("cios_b_sciana") : Loc.F("cios_b_brak_celu", WeaponLook.Pattern(g.Weapon.ModeB));
                N.Hud.ShowHint(b, ButtonNames.Pick(Loc.T("pusc_f_wroc"), Loc.T("pusc_cios_b_wroc")));
                return;
            }
            N.Hud.ShowHint(Loc.F("celowanie_brak_celu_w_zasiegu", g.WeaponRange()), ButtonNames.Pick(Loc.T("pusc_spacje_wroc"), Loc.T("pusc_atak_wroc")));
            return;
        }
        var e = g.Enemies[Target];
        var name = g.D.Enemies[e.DefId].Name;
        var line = Loc.F("cel_hp", _sel + 1, _count, name, e.Hp, e.MaxHp);
        if (ModeB) line = ModeText.BName(g.Weapon) + ": " + line; // v0.23.0: nazwa ruchu Fachu
        N.Hud.ShowHint(line, ModeB ? ButtonNames.Pick(Loc.T("strzalki_zmiana_celu_pusc_f"), Loc.T("przesun_palec_cel_pusc_cios_b"))
            : ButtonNames.Pick(Loc.T("strzalki_zmiana_celu_pusc"), Loc.T("przesun_palec_cel_pusc_atak")));
    }

    /// <summary>Puszczenie A: atak wybranego celu (bez celu - miga zasięg jak range_flash na GBA). v0.23.0: tryb B – cios B
    /// w cel, bez celu otwarcie pękniętej ściany (Przecinak, Łom), inaczej mignięcie zasięgu wzoru B (bez tury).</summary>
    public void Commit()
    {
        var target = Target;
        var modeB = ModeB;
        Cancel();
        var g = _app.Session.Game;
        bool acted;
        if (modeB) acted = PlayCommands.WeaponB(g, N.World, target);
        else acted = target >= 0 ? g.PlayerAttack(target) : PlayCommands.AttackNearest(g, N.World);
        _app.AfterAction(acted);
    }

    /// <summary>Koniec celowania bez ataku (np. wyjście z ekranu).</summary>
    public void Cancel()
    {
        if (!Active) return;
        Active = false;
        ModeB = false;
        _trigger = GameAction.A;
        N.World.Reticle = -1;
        N.World.ShowRange(false);
        N.World.AimPattern(null, -1);
        N.Hud.ShowHint(null);
    }
}
