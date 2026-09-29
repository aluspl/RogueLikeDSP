namespace LifeLike.Core.Data;

/// <summary>Wątek SMS-ów fabuły (#35, core::story_thread): nazwa, podpowiedź, warunek (wartość: liczba albo boss), 1-2 wiadomości.</summary>
public sealed record StoryThread(string Id, string Name, string Hint, StoryTrigger Trigger, int Value, StoryMsg[] Messages);
