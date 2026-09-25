namespace LifeLike.Core.Data;

/// <summary>Skutek wydarzenia na placu (core::event_effect).</summary>
public enum EventEffect : byte { FewerPickups, Cash, Inspection, Rain, Thermos }

/// <summary>
/// Wydarzenie na placu: losowy SMS na starcie etapu (nie pierwszego i nie z bossem) z modyfikatorem etapu.
/// Short = pastylka w telefonie/HUD, Info = skutek dla gracza, Good = korzystne.
/// </summary>
public sealed record SiteEventDef(string Id, string Name, string Short, string Info, StoryMsg Msg, EventEffect Effect, int Value, bool Good);
