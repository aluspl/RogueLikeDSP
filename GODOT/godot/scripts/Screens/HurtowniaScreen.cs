using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Hurtownia między aktami (run_hurtownia na GBA): zakupy za budżet budowy, potem kolejny etap.
/// v0.23.2: zakładki (Q/E, dotknięcie paska): Hurtownia, Skup (plecak za materiały), PSZOK (oddanie za dośw. i HP),
/// Plecak (zamiana bez tury).
/// </summary>
public sealed class HurtowniaScreen : Screen
{
    public HurtowniaScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public HurtowniaPage Page { get; private set; }

    /// <summary>v0.23.2: strona Skupu / PSZOK-u / plecaka (zakładka Tab &gt; 0).</summary>
    public BackpackPage Bag { get; private set; }

    /// <summary>v0.23.2: zakładka (HurtTabs).</summary>
    public int Tab { get; private set; }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    /// <summary>Otwórz na zakładce (zrzuty, test dymny).</summary>
    public void OpenTab(int tab, bool instant = false)
    {
        Tab = tab;
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        Show(instant);
        Sfx.Play("notify", 0.7f);
    }

    private void Show(bool instant)
    {
        if (Tab == HurtTabs.Shop)
        {
            var sel = Page?.Sel ?? 0;
            Page = new HurtowniaPage(S.Game) { Sel = sel, TabTo = SwitchTo };
            Page.Bought = () =>
            {
                if (S.Game.TraitPending) Flow.Trait.Open(this); // +2: cecha narzędzia, potem z powrotem do Hurtowni
            };
            N.Phone.OpenSingle(Page, 4, instant);
            return;
        }
        var mode = Tab == HurtTabs.Sell ? BagMode.Sell : Tab == HurtTabs.Recycle ? BagMode.Recycle : BagMode.Bag;
        Bag = new BackpackPage(S.Game) { Mode = mode, Shop = true, Tab = Tab, TabTo = SwitchTo, Done = _ => N.Hud.ShowGame(S.Game) };
        N.Phone.OpenSingle(Bag, 4, true);
    }

    /// <summary>Przejście na zakładkę k (HurtTabs).</summary>
    public void SwitchTo(int k)
    {
        if (k == Tab || k < 0 || k >= HurtTabs.Count) return;
        Tab = k;
        Sfx.Play("menu");
        Show(true);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (e.Is(GameAction.TabPrev | GameAction.TabNext) && !(Bag?.MenuOpen ?? false))
        {
            SwitchTo((Tab + (e.Is(GameAction.TabNext) ? 1 : HurtTabs.Count - 1)) % HurtTabs.Count);
            return true;
        }
        if (N.Phone.HandleInput(e))
        {
            N.Hud.ShowGame(S.Game);
            return true;
        }
        if (!e.IsBack) return false; // v0.21.51: A / Enter kupuje zaznaczone, B / Esc = dalej

        Advance();
        return true;
    }

    public void Advance()
    {
        Tab = HurtTabs.Shop; // kolejna Hurtownia od zakupów
        App.NextStage();
    }
}
