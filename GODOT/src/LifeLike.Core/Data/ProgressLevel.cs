namespace LifeLike.Core.Data;

/// <summary>
/// Poziom z nagrodą (core::progress_level): Xp = dośw. potrzebne na ten poziom od poprzedniego (stopnie inwestora: stawka);
/// Index = wygląd (Helmet), wątek fabuły (Story), ozdoba Osiedla (Decor), -1 = brak; Value = Respekt; Title = tytuł.
/// </summary>
public sealed record ProgressLevel(int Xp, ProgressReward Reward, int Index, int Value, string Title);
