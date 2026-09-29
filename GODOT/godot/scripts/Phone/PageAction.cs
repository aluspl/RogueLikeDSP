using LifeLike.Game.Input;

namespace LifeLike.Game.Phone;

/// <summary>Rola przycisku strony: główny (wybierz / dalej) zawsze w prawym dolnym rogu, powrót (wróć / zostaw) w lewym,
/// pozostałe między nimi (v0.21.51 - ten sam układ na każdym oknie i przejściu).</summary>
public enum PageRole
{
    /// <summary>Z akcji: A / START = główny, B / Cancel = powrót, reszta = zwykły.</summary>
    Auto,
    Primary,
    Back,
    Other,
}

/// <summary>Przycisk strony telefonu przy sterowaniu dotykiem (zamiast podpowiedzi klawiszy): napis, akcja gry i rola.</summary>
public readonly record struct PageAction(string Label, GameAction Action, PageRole Role = PageRole.Auto)
{
    /// <summary>Rola po rozwinięciu Auto.</summary>
    public PageRole Resolved => Role != PageRole.Auto ? Role
        : (Action & (GameAction.A | GameAction.Start)) != 0 ? PageRole.Primary
        : (Action & (GameAction.B | GameAction.Cancel)) != 0 ? PageRole.Back
        : PageRole.Other;
}
