namespace LifeLike.Core.Data;

/// <summary>
/// Tytuł spoza odznak i zleceń (core::progress_title): Source 0 = poziom inspektora, 1 = stopień inwestora (stawka);
/// v0.21.52 cz. c: 2 = kolekcja (Level = komplet + 1), 3 = seria dni (dni), 4 = zadania (wykonane łącznie).
/// </summary>
public sealed record ProgressTitle(string Name, int Source, int Level);
