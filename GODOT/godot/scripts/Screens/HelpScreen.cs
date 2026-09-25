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
    private bool _fromTitle, _fromSettings;

    public HelpScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => _fromSettings ? Flow.Settings.Views : _fromTitle ? ViewSet.Title : ViewSet.Prologue;
    public override bool InRun => _fromSettings && Flow.Settings.InRun;
    public override bool UsesPhone => true;
    public override string Music => _fromSettings ? Flow.Settings.Music : _fromTitle ? "title" : "game";

    /// <summary>fromTitle = z menu tytułu (powrót na tytuł), inaczej przed pierwszym etapem.</summary>
    public void Open(bool fromTitle, bool instant = false)
    {
        _fromTitle = fromTitle;
        _fromSettings = false;
        Flow.Go(this, instant);
    }

    /// <summary>Z ustawień (powrót do ustawień, pod spodem ten sam ekran co pod nimi).</summary>
    public void OpenFromSettings()
    {
        _fromSettings = true;
        Flow.Go(this);
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
        if (_fromSettings) Flow.Settings.Reopen();
        else if (_fromTitle) Flow.Title.Open();
        else Flow.StageCard.Open();
        return true;
    }
}
