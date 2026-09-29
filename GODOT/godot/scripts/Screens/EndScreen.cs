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
        v.CanContinue = won && !g.Daily && g.WeeklyWeek == 0; // budowa dnia / tygodnia: bez NG+
        v.Line1 = $"Wynik {g.Score}   Dni {g.Turns}   Etap {g.StageNumber()}/{g.StagesInRun()}   Dośw. +{S.LastGained}";
        var record = g.Score > S.PrevBest ? "Nowy rekord!" : $"Rekord {S.Profile.Best}";
        var daily = g.Daily ? (S.DailyRecord ? "   Rekord dnia!" : $"   Budowa dnia nr {g.DailyDay}") : "";
        if (g.WeeklyWeek != 0) daily = S.WeeklyRecord ? "   Rekord tygodnia!" : $"   Tydzień nr {g.WeeklyWeek}";
        v.Line2 = $"{record}   Doświadczenie w profilu {S.Profile.Xp}   Respekt +{g.Respect} (masz {S.Profile.Respect})" + (won ? "   Dom na Osiedlu!" : "") + daily;
        v.Note = S.Note;
        N.Banners.Clear();
        for (var i = 0; i < S.Data.StoryArc.Length; i++) // fabuła (#35): nowy wątek w Wiadomościach (Profil > Osiedle)
        {
            if (((S.LastStory >> i) & 1) != 0) N.Banners.Push("Nowa wiadomość", S.Data.StoryArc[i].Name);
        }
    }

    public override bool HandleInput(InputCmd e)
    {
        if (e.IsTap)
        {
            var b = N.EndView.ButtonAt(e.Pointer);
            if (b == 0) return false;
            e = InputCmd.Of(b == 1 ? GameAction.A : GameAction.Start);
        }
        if (S.Game.St == GameStatus.Won && !S.Game.Daily && S.Game.WeeklyWeek == 0 && e.Is(GameAction.A))
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
