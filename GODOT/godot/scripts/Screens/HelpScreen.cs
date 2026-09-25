using LifeLike.Core;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Jak grać (run_help na GBA): przy pierwszej budowie po prologu (potem karta etapu) albo z menu tytułu.
/// Zapisuje flagę help_seen w profilu.
/// </summary>
public sealed class HelpScreen : Screen
{
    private bool _fromTitle;

    public HelpScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => _fromTitle ? ViewSet.Title : ViewSet.Prologue;
    public override bool UsesPhone => true;
    public override string Music => _fromTitle ? "title" : "game";

    /// <summary>fromTitle = z menu tytułu (powrót na tytuł), inaczej przed pierwszym etapem.</summary>
    public void Open(bool fromTitle, bool instant = false)
    {
        _fromTitle = fromTitle;
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant) => N.Phone.OpenSingle(new HelpPage(), 2, instant);

    public override bool HandleInput(InputCmd e)
    {
        if (!e.Is(GameAction.A | GameAction.Start | GameAction.B | GameAction.Cancel)) return false;
        if (!S.Profile.HasFlag(Profile.FlagHelpSeen))
        {
            S.Profile.SetFlag(Profile.FlagHelpSeen);
            S.Save();
        }
        if (_fromTitle) Flow.Title.Open();
        else Flow.StageCard.Open();
        return true;
    }
}
