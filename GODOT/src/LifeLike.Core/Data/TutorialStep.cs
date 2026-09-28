namespace LifeLike.Core.Data;

/// <summary>
/// Samouczek menu (#25, sekcja "tutorial"): dymek Kierownika nad elementem tytułu (Screen 0) albo wyboru zawodu (1).
/// Msg: nadawca + 3 linie dymka; Gba: klawisz na GBA ("" = tylko Godot); GodotOnly: np. klucz z opcjami;
/// NeedsInvestor: tylko po odblokowaniu trybu inwestora; Link: strona do otwarcia z dymka (np. "stats"), "" = brak.
/// </summary>
public sealed record TutorialStep(string Id, string Title, StoryMsg Msg, string Gba, int Screen, bool GodotOnly, bool NeedsInvestor,
    string Link = "");
