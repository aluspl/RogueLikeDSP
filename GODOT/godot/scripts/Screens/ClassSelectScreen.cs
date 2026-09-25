using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Input;

namespace LifeLike.Game.Screens;

/// <summary>Wybór zawodu (run_class_select na GBA): zawód ←/→, trudność ↑/↓, pamiątka Q/E, start budowy.</summary>
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
    }

    public override bool HandleInput(InputCmd e)
    {
        var view = N.ClassSelectView;
        var d = S.Data;
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
            var n = d.Difficulties.Length;
            var diff = S.Difficulty;
            do diff = (diff + v + n) % n;
            while (!Meta.DifficultyUnlocked(d, S.Profile, diff));
            S.Difficulty = diff;
            view.Difficulty = diff;
            Sfx.Play("menu");
            return true;
        }
        if (e.Is(GameAction.KeepPrev | GameAction.KeepNext))
        {
            Meta.CycleKeepsake(d, S.Profile, e.Is(GameAction.KeepPrev) ? -1 : 1);
            S.Save();
            Sfx.Play("menu");
            return true;
        }
        if (e.Is(GameAction.Shop))
        {
            Flow.Profile.Open(4);
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
        S.ClassId = view.Selected;
        if (!Meta.ClassUnlocked(S.Profile, S.ClassId))
        {
            view.Note = $"Ten zawód odblokujesz w Szkoleniach (K) za {d.ClassCost} dośw.";
            Sfx.Play("hurt", 0.5f);
            return true;
        }
        Sfx.Play("stage");
        App.StartRun();
        return true;
    }
}
