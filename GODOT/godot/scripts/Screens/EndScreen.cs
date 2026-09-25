using LifeLike.Core;
using LifeLike.Game.Input;

namespace LifeLike.Game.Screens;

/// <summary>Plansza końcowa z kodem QR (run_end na GBA): po odbiorze A = kolejna budowa (NG+), Enter = tytuł.</summary>
public sealed class EndScreen : Screen
{
    public EndScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.End;
    public override string Music => "";

    public void Open() => Flow.Go(this);

    public override void Enter(bool instant)
    {
        var g = S.Game;
        var won = g.St == GameStatus.Won;
        var v = N.EndView;
        v.Won = won;
        v.Line1 = $"Wynik {g.Score}   Dni {g.Turns}   Etap {g.Stage + 1}/{S.Data.Stages.Length}   Dośw. +{S.LastGained}";
        v.Line2 = $"Rekord {S.Profile.Best}   Doświadczenie w profilu {S.Profile.Xp}" + (won ? "   Dom na Osiedlu!" : "");
        v.Note = S.Note;
        N.Banners.Clear();
    }

    public override bool HandleInput(InputCmd e)
    {
        if (e.IsTap)
        {
            var b = N.EndView.ButtonAt(e.Pointer);
            if (b == 0) return false;
            e = InputCmd.Of(b == 1 ? GameAction.A : GameAction.Start);
        }
        if (S.Game.St == GameStatus.Won && e.Is(GameAction.A))
        {
            S.NewGamePlus();
            App.Refresh();
            Flow.StageCard.Open();
            return true;
        }
        if (!e.Is(GameAction.Start | GameAction.A)) return false;
        S.Note = "";
        Flow.Title.Open();
        return true;
    }
}
