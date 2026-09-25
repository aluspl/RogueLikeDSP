using LifeLike.Game.Input;

namespace LifeLike.Game.Phone;

/// <summary>Przycisk strony telefonu przy sterowaniu dotykiem (zamiast podpowiedzi klawiszy): napis i akcja gry.</summary>
public readonly record struct PageAction(string Label, GameAction Action);
