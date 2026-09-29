using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Wyzwanie tygodnia z tytułu (#34): telefon na tle tytułu z zasadami tygodnia (data z systemu); Start zaczyna budowę
/// tygodnia, „Wyślij wynik” woła tabelę tygodnia (zaślepka ILeaderboard, bez sieci), Esc wraca.
/// </summary>
public sealed class WeeklyScreen : Screen
{
    public WeeklyScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Title;
    public override bool UsesPhone => true;
    public override string Music => "title";

    public WeeklyPage Page { get; private set; }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        Flow.Title.Populate();
        Page = new WeeklyPage(S.Data, S.Profile, S.TodayWeek);
        N.Phone.OpenSingle(Page, 0, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (e.Is(GameAction.A | GameAction.Start))
        {
            Sfx.Play("menu");
            App.StartWeekly(Page.Week);
            return true;
        }
        if (e.Is(GameAction.Select))
        {
            Submit();
            return true;
        }
        if (!e.Is(GameAction.B | GameAction.Cancel)) return false;
        N.Phone.Close();
        Flow.Title.Open();
        return true;
    }

    /// <summary>Wyślij najlepszy wynik tygodnia (bez sieci: identyfikator tabeli i komunikat).</summary>
    public void Submit()
    {
        var best = LifeLike.Core.Weekly.Best(S.Data, S.Profile, Page.Week);
        Page.Note = best < 0 ? "Najpierw rozegraj wyzwanie tygodnia" : S.Leaderboard.SubmitWeekly(Page.Week, best);
        Sfx.Play(best < 0 ? "hurt" : "notify");
        N.Phone.QueueRedraw();
    }
}
