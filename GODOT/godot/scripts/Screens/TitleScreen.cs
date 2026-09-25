using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Screens.Views;

namespace LifeLike.Game.Screens;

/// <summary>Tytuł (run_title na GBA): menu Nowa budowa / Profil / Szkolenia / Jak grać, rekord i doświadczenie z profilu.</summary>
public sealed class TitleScreen : Screen
{
    private int _sel;

    public TitleScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Title;
    public override string Music => "title";

    public void Open() => Flow.Go(this);

    public override void Enter(bool instant) => Populate();

    /// <summary>Rekord, liczba budów, doświadczenie, wybór i notatka na planszy tytułu (także pod telefonem profilu).</summary>
    public void Populate()
    {
        var v = N.TitleView;
        v.Best = S.Profile.Best;
        v.Runs = S.Profile.Runs;
        v.Xp = S.Profile.Xp;
        v.Sel = _sel;
        v.Note = S.Note;
    }

    public override bool HandleInput(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _sel = (_sel + v + TitleView.Items.Length) % TitleView.Items.Length;
            S.Note = "";
            Sfx.Play("menu");
            Populate();
            return true;
        }
        if (e.Is(GameAction.Profile))
        {
            Flow.Profile.Open(0);
            return true;
        }
        if (e.Is(GameAction.Shop))
        {
            Flow.Profile.Open(4);
            return true;
        }
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Sfx.Play("menu");
        switch (_sel)
        {
            case 0:
                Flow.ClassSelect.Open();
                break;
            case 1:
                Flow.Profile.Open(0);
                break;
            case 2:
                Flow.Profile.Open(4);
                break;
            default:
                Flow.Help.Open(true);
                break;
        }
        return true;
    }
}
