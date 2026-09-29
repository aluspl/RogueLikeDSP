namespace LifeLike.Core.Data;

/// <summary>Wydarzenie z wyborem (#30): SMS na polu etapu i 2–3 odpowiedzi (core::choice_event_def).</summary>
public sealed record ChoiceEventDef(string Id, string Name, StoryMsg Msg, EventChoice[] Choices);
