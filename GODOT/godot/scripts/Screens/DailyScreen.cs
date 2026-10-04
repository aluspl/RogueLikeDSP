using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Codzienna budowa z tytułu: telefon na tle tytułu z budową dnia (data z systemu); Start zaczyna budowę dnia,
/// „Wyślij wynik” woła tabelę wyników (na razie lokalna zaślepka), Esc wraca.
/// </summary>
public sealed class DailyScreen : Screen
{
    public DailyScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Title;
    public override bool UsesPhone => true;
    public override string Music => "title";

    public DailyPage Page { get; private set; }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        Flow.Title.Populate();
        Page = new DailyPage(S.Data, S.Profile, S.Today);
        N.Phone.OpenSingle(Page, 0, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (e.Is(GameAction.A | GameAction.Start))
        {
            Start();
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

    /// <summary>Wyślij najlepszy wynik dnia (bez sieci: komunikat, że Game Center dopiero będzie).</summary>
    public void Submit()
    {
        var best = LifeLike.Core.Daily.Best(S.Data, S.Profile, Page.Day);
        Page.Note = best < 0 ? Loc.T("najpierw_rozegraj_budowe_dnia") : S.Leaderboard.Submit(Page.Day, best);
        Sfx.Play(best < 0 ? "hurt" : "notify");
        N.Phone.QueueRedraw();
    }

    public void Start()
    {
        Sfx.Play("menu");
        App.StartDaily(Page.Day);
    }
}
