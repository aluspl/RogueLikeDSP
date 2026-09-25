namespace LifeLike.Core.Data;

/// <summary>Wiadomość w telefonie (fabuła): nadawca + 3 linie dymka.</summary>
public sealed record StoryMsg(string From, string[] Lines);
