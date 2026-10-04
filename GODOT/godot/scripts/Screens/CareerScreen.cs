using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// v0.21.52 cz. d (#47): Mapa kariery – po pierwszej budowie „Nowa budowa” na tytule otwiera telefon z kontraktami;
/// wybór odblokowanego kontraktu zapisuje go w profilu i przechodzi do wyboru zawodu, Esc wraca na tytuł.
/// </summary>
public sealed class CareerScreen : Screen
{
    public CareerScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Title;
    public override bool UsesPhone => true;
    public override string Music => "title";

    public CareerPage Page { get; private set; }

    /// <summary>Mapa kariery jest po pierwszej budowie (wcześniej od razu wybór zawodu).</summary>
    public bool Available => S.Profile.Runs > 0;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        Flow.Title.Populate();
        Page = new CareerPage(S.Data, S.Profile, Pick);
        N.Phone.OpenSingle(Page, 0, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.B | GameAction.Cancel)) return false;
        N.Phone.Close();
        Flow.Title.Open();
        return true;
    }

    public void Pick(int k)
    {
        if (!Career.Unlocked(S.Data, S.Profile, k))
        {
            Sfx.Play("hurt");
            return;
        }
        Sfx.Play("menu");
        S.Profile.Contract = (byte)k;
        S.Save();
        N.Phone.Close();
        Flow.ClassSelect.Open();
    }
}
