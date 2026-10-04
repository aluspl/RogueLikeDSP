using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Screens;

namespace LifeLike.Game.Guide;

/// <summary>
/// Samouczek menu (#25, run_tutorial / run_unlocks na GBA): przy pierwszym wejściu na tytuł i wybór zawodu dymki
/// Kierownika Marka po kolei nad elementami ekranu (GameData.TutorialSteps, te same teksty co na GBA), potem po jednym
/// dymku przy pierwszym odblokowaniu nowości (Meta.PendingUnlock: Respekt, codzienna budowa, Akt 0, tryb inwestora,
/// nowy zawód). A / Spacja / dotknięcie = dalej, B / Esc / „Pomiń” = pomiń resztę, I / „Statystyki” przy kroku
/// o statystykach otwiera ich opis (po powrocie samouczek trwa dalej). Flagi w profilu (Meta.TutorialDone, MarkUnlock).
/// </summary>
public sealed class Coach
{
    private readonly App _app;
    private readonly List<int> _steps = new();
    private int _k, _mode, _screen = -1, _unlock = -1, _cls = -1;   // _mode: 0 brak, 1 główny samouczek, 2 dymek nowości
    private Screen _owner;
    private Func<string, Rect2> _hole = _ => new Rect2();
    private Action _onLink = () => { };

    public Coach(App app) => _app = app;

    public bool Active => _mode != 0;

    /// <summary>Id bieżącego kroku (samouczek) albo dymka nowości; "" gdy nic nie jest pokazane.</summary>
    public string CurrentId => _mode == 1 ? D.TutorialSteps[_steps[_k]].Id : _mode == 2 ? D.TutorialUnlocks[_unlock].Id : "";

    /// <summary>Ile kroków ma bieżący samouczek ekranu (test dymny).</summary>
    public int StepsCount => _steps.Count;

    private GameData D => _app.Session.Data;
    private Profile P => _app.Session.Profile;
    private CoachView V => _app.Nodes.Coach;

    /// <summary>
    /// Wejście na ekran (0 tytuł, 1 wybór zawodu): główny samouczek, jeśli nieobejrzany, potem dymki nowości.
    /// Po powrocie ze strony otwartej z dymka (statystyki) samouczek toczy się dalej od tego samego kroku.
    /// </summary>
    public void Begin(Screen owner, int screen, Func<string, Rect2> hole, Action onLink)
    {
        _owner = owner;
        _hole = hole;
        _onLink = onLink;
        if (Active && _screen == screen)
        {
            Show();
            return;
        }
        _screen = screen;
        _mode = 0;
        _steps.Clear();
        if (Meta.TutorialPending(P, screen))
        {
            for (var i = 0; i < D.TutorialSteps.Length; i++)
            {
                if (D.TutorialSteps[i].Screen == screen && Meta.TutorialStepShown(D, P, i, true)) _steps.Add(i);
            }
            if (_steps.Count > 0)
            {
                _mode = 1;
                _k = 0;
                Show();
                return;
            }
            FinishMain();
            return;
        }
        NextUnlock();
    }

    /// <summary>Co klatkę z ekranu-właściciela: podświetlenie idzie za elementem (animacje, zmiana okna).</summary>
    public void Update()
    {
        var on = Active && _app.Flow.Current == _owner;
        V.Visible = on;
        if (on) V.Hole = _hole(CurrentId);
    }

    /// <summary>Wejście, gdy dymek jest na ekranie: wszystko trafia do samouczka (true = obsłużone).</summary>
    public bool HandleInput(InputCmd e)
    {
        if (!Active || _app.Flow.Current != _owner) return false;
        if (e.IsTap)
        {
            switch (V.HitAt(e.Pointer))
            {
                case CoachHit.Skip:
                    Skip();
                    break;
                case CoachHit.Link:
                    Link();
                    break;
                default:
                    Next();
                    break;
            }
            return true;
        }
        if (e.Is(GameAction.A | GameAction.Start)) Next();
        else if (e.Is(GameAction.B | GameAction.Cancel)) Skip();
        else if (e.Is(GameAction.Info) && V.Link) Link();
        return true;
    }

    /// <summary>Dalej: kolejny krok albo koniec (dymek nowości: obejrzany).</summary>
    public void Next()
    {
        Sfx.Play("menu");
        if (_mode == 1)
        {
            if (++_k < _steps.Count) Show();
            else FinishMain();
            return;
        }
        if (_mode == 2)
        {
            Meta.MarkUnlock(P, _unlock, _cls);
            _app.Session.Save();
            NextUnlock();
        }
    }

    /// <summary>Pomiń resztę samouczka (dymek nowości: jak dalej).</summary>
    public void Skip()
    {
        if (_mode == 1)
        {
            Sfx.Play("menu");
            FinishMain();
            return;
        }
        Next();
    }

    private void Link()
    {
        Sfx.Play("menu");
        V.Visible = false;
        _onLink();
    }

    private void FinishMain()
    {
        Meta.TutorialDone(P, _screen);
        _app.Session.Save();
        NextUnlock();
    }

    private void NextUnlock()
    {
        _unlock = Meta.PendingUnlock(D, P, _screen, out _cls);
        _mode = _unlock >= 0 ? 2 : 0;
        if (_mode == 0)
        {
            V.Visible = false;
            return;
        }
        Show();
    }

    private void Show()
    {
        var s = _mode == 1 ? D.TutorialSteps[_steps[_k]] : D.TutorialUnlocks[_unlock];
        var secret = _mode == 2 && _unlock == TutorialUnlock.Secret && _cls >= 0 && _cls < D.Secrets.Length;
        var msg = secret ? D.Secrets[_cls].News : s.Msg; // v0.21.51 cz. 2: co wykonano i jaka nagroda
        V.Title = s.Title;
        V.From = msg.From.Length > 0 ? msg.From : s.Msg.From;
        V.Lines = Array.ConvertAll(msg.Lines, ButtonNames.Localize);
        V.Avatar = D.Classes[0].Frame;   // Kierownik budowy
        V.Single = _mode == 2;
        V.Link = _mode == 1 && s.Link.Length > 0;
        V.Last = _mode == 1 && _k == _steps.Count - 1;
        V.Pill = _mode == 1 ? $"{_k + 1}/{_steps.Count}" : _cls >= 0 && !secret ? D.Classes[_cls].Name : Loc.T("nowosc");
        V.PillBrand = _mode == 2;
        Sfx.Play("notify", 0.6f);
        Update();
    }
}
