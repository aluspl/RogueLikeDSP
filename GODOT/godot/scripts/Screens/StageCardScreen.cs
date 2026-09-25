using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Karta etapu (stage_card na GBA): SMS od inwestorki / kierownika i drugi SMS z wydarzeniem na placu.</summary>
public sealed class StageCardScreen : Screen
{
    public StageCardScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        var g = S.Game;
        var d = S.Data;
        var sd = d.Stages[g.Stage];
        var page = new MessagePage($"Akt {UiText.Roman(sd.Act)}, {g.Stage + 1}/{d.Stages.Length}", "Enter: do roboty");
        page.Add(g.StageStory);
        var ev = g.CurrentEvent;
        if (ev is not null && g.Turns == g.StageStartTurn) page.Add(ev.Msg);
        if (sd.Boss >= 0) page.Info($"Uwaga: {d.Enemies[sd.Boss].Name}!", Ink.Late);
        else page.Info($"{sd.Name}: problemy {g.EnemyHpPct()}%", Ink.Dim);
        if (ev is not null) page.Info($"{ev.Name}: {ev.Info}", ev.Good ? Ink.Done : Ink.Late);
        page.Info(g.DDef.Name + (g.Tier > 0 ? $" NG+{g.Tier}" : ""), Ink.Dim);
        N.Phone.OpenSingle(page, 2, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Advance();
        return true;
    }

    /// <summary>Do roboty: mapa etapu; na start budowy podpowiedzi (moc, pamiątka).</summary>
    public void Advance()
    {
        var g = S.Game;
        Flow.Game.Open();
        if (S.FirstStage && g.Stage == 0 && g.Tier == 0) App.Banners.FirstStageHints();
        S.FirstStage = false;
        App.Refresh();
    }
}
