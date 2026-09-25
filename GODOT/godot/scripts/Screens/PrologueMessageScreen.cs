using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// SMS od inwestorki po prologu (phone_message(story_prologue) na GBA) na tle placu; potem prolog jest oznaczony
/// w profilu (prologue_seen) i budowa zaczyna się od karty etapu.
/// </summary>
public sealed class PrologueMessageScreen : Screen
{
    public PrologueMessageScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Prologue;
    public override bool UsesPhone => true;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        var page = new MessagePage("Budowa", "Enter: na plac");
        page.Add(S.Data.StoryPrologue);
        page.Info("Twój pierwszy plac budowy", Ink.Dim);
        page.Info(S.Game.CDef.Name, Ink.Brand);
        N.Phone.OpenSingle(page, 2, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        S.Profile.SetFlag(Profile.FlagPrologueSeen);
        S.Save();
        if (!S.Profile.HasFlag(Profile.FlagHelpSeen)) Flow.Help.Open(false); // pierwsza budowa: jak grać
        else Flow.StageCard.Open();
        return true;
    }
}
