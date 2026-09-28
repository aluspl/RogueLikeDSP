using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Premia za etap (v0.21.50 cz. 2): po zaliczonym etapie, przed harmonogramem i Hurtownią – jeden przepływ:
/// premia -> harmonogram (ścieżka) -> Hurtownia po akcie -> kolejny etap. Nowa synergia – baner „Synergia: …”.
/// </summary>
public sealed class BoonScreen : Screen
{
    public BoonScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public BoonPickPage Page { get; private set; }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        Page = new BoonPickPage(S.Game) { Picked = Pick };
        N.Phone.OpenSingle(Page, PhoneTabs.Gear, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e))
        {
            N.Hud.ShowGame(S.Game);
            return true;
        }
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Pick();
        return true;
    }

    /// <summary>Bierze zaznaczoną premię; nowa synergia - baner obok telefonu; dalej harmonogram.</summary>
    public void Pick()
    {
        var g = S.Game;
        if (!g.HasBoonOffer)
        {
            Flow.Schedule.Open();
            return;
        }
        var before = g.SynergyMask();
        var b = g.BoonOffer[Page?.Sel ?? 0];
        if (!g.PickBoon(Page?.Sel ?? 0)) return;
        Sfx.Play("buy");
        var bd = S.Data.Boons[b];
        N.Banners.Push("Premia: " + bd.Name, bd.Desc, PhoneTabs.Gear);
        var now = g.SynergyMask();
        for (var s = 0; s < S.Data.Synergies.Length; s++)
        {
            if (((now >> s) & 1) == 0 || ((before >> s) & 1) != 0) continue;
            N.Banners.Push("Synergia: " + S.Data.Synergies[s].Name, S.Data.Synergies[s].Desc, PhoneTabs.Gear);
            var line = $"Synergia {S.Data.Synergies[s].Name}: {S.Data.Synergies[s].Desc}"; // pełny opis na harmonogramie
            S.Note = S.Note.Length > 0 ? line + " " + S.Note : line;
            Sfx.Play("level", 0.8f);
        }
        App.Refresh();
        Flow.Schedule.Open();
    }
}
