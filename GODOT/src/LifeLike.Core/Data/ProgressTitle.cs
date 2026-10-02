namespace LifeLike.Core.Data;

/// <summary>Tytuł spoza odznak i zleceń (core::progress_title): Source 0 = poziom inspektora, 1 = stopień inwestora (stawka).</summary>
public sealed record ProgressTitle(string Name, int Source, int Level);
