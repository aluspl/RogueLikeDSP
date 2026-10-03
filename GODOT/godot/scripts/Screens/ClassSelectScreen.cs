using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;
using LifeLike.Game.Screens.Views;

namespace LifeLike.Game.Screens;

/// <summary>Wybór zawodu (run_class_select na GBA): zawód ←/→, trudność ↑/↓, pamiątka Q/E, tryb inwestora Tab, I = opis statystyk
/// (dotknięcie wiersza statystyki = dymek), start budowy.</summary>
public sealed class ClassSelectScreen : Screen
{
    public ClassSelectScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.ClassSelect;
    public override string Music => "title";

    public void Open() => Flow.Go(this);

    public override void Enter(bool instant)
    {
        S.Note = "";
        var v = N.ClassSelectView;
        v.Setup(S.Data, S.Profile, S.ClassId);
        v.Difficulty = S.Difficulty;
        v.Note = "";
        App.Coach.Begin(this, 1, v.CoachRect, () => Flow.Stats.OpenClass(N.ClassSelectView.Selected));   // samouczek menu, dymki nowości
    }

    public override void Process(double delta) => App.Coach.Update();

    public override bool HandleInput(InputCmd e)
    {
        if (App.Coach.HandleInput(e)) return true;
        var view = N.ClassSelectView;
        var d = S.Data;
        if (e.IsTap) return Tap(e);
        if (e.HDir != 0)
        {
            view.Move(e.HDir);
            S.ClassId = view.Selected;
            view.Note = "";
            return true;
        }
        var v = e.VDir;
        if (v != 0)
        {
            CycleDifficulty(v);
            return true;
        }
        if (e.Is(GameAction.KeepPrev | GameAction.KeepNext))
        {
            CycleKeepsake(e.Is(GameAction.KeepPrev) ? -1 : 1);
            return true;
        }
        if (e.Is(GameAction.Shop))
        {
            Flow.Profile.Open(4);
            return true;
        }
        if (e.Is(GameAction.Select) && InvestorPage.Available(S.Data, S.Profile, N.ClassSelectView.Selected))
        {
            S.ClassId = view.Selected;
            Flow.Investor.Open();
            return true;
        }
        if (e.Is(GameAction.Info)) // opis statystyk zawodu (START na GBA)
        {
            S.ClassId = view.Selected;
            Flow.Stats.OpenClass(view.Selected);
            return true;
        }
        if (e.Is(GameAction.Profile))
        {
            Flow.Profile.Open(0);
            return true;
        }
        if (e.Is(GameAction.B | GameAction.Cancel))
        {
            Flow.Title.Open();
            return true;
        }
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Start();
        return true;
    }

    private bool Tap(InputCmd e)
    {
        var view = N.ClassSelectView;
        var (hit, arg) = view.HitAt(e.Pointer);
        switch (hit)
        {
            case ClassSelectHit.Portrait:
                view.MoveTo(arg);
                S.ClassId = view.Selected;
                view.Note = "";
                return true;
            case ClassSelectHit.Difficulty:
                CycleDifficulty(1);
                return true;
            case ClassSelectHit.Keepsake:
                CycleKeepsake(1);
                return true;
            case ClassSelectHit.Back:
                Flow.Title.Open();
                return true;
            case ClassSelectHit.Investor:
                S.ClassId = view.Selected;
                Flow.Investor.Open();
                return true;
            case ClassSelectHit.Start:
                Start();
                return true;
            case ClassSelectHit.Stat: // dymek z opisem statystyki (drugie dotknięcie chowa)
                view.TipStat = view.TipStat == arg ? -1 : arg;
                return true;
            case ClassSelectHit.Stats:
                S.ClassId = view.Selected;
                Flow.Stats.OpenClass(view.Selected);
                return true;
            default:
                return false;
        }
    }

    private void CycleDifficulty(int v)
    {
        var d = S.Data;
        var n = d.Difficulties.Length;
        var diff = S.Difficulty;
        do diff = (diff + v + n) % n;
        while (!Meta.DifficultyUnlocked(d, S.Profile, diff));
        S.Difficulty = diff;
        N.ClassSelectView.Difficulty = diff;
        Sfx.Play("menu");
    }

    private void CycleKeepsake(int d)
    {
        Meta.CycleKeepsake(S.Data, S.Profile, d);
        S.Save();
        Sfx.Play("menu");
    }

    private void Start()
    {
        var view = N.ClassSelectView;
        var d = S.Data;
        S.ClassId = view.Selected;
        if (!Meta.ClassUnlocked(S.Data, S.Profile, S.ClassId))
        {
            view.Note = $"Ten zawód odblokujesz w Szkoleniach{ButtonNames.Pick(" (K)", "")} za {Meta.ClassCost(d, S.Profile)} dośw.";
            Sfx.Play("hurt", 0.5f);
            return;
        }
        Sfx.Play("stage");
        App.StartRun();
    }
}
