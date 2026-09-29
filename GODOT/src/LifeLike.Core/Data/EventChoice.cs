namespace LifeLike.Core.Data;

/// <summary>Odpowiedź na wydarzenie: etykieta, skutek słowami i 0–3 skutki (core::event_choice).</summary>
public sealed record EventChoice(string Label, string Result, ChoiceOut[] Outs);
