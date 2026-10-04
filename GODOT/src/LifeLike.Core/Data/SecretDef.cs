namespace LifeLike.Core.Data;

/// <summary>
/// Sekretne zlecenie (#39, sekcja "secrets"): Hint widać od początku („???” z podpowiedzią), Desc = warunek po wykonaniu;
/// Value = liczba (magazyny, zawody, kombinacje, HP, dni) albo problem (HelperBoss); Index = zawód, narzędzie (GameData.Tools),
/// wygląd (GameData.Cosmetics) albo ranga Respektu; News = dymek „Nowość” na tytule.
/// </summary>
public sealed record SecretDef(string Id, string Hint, string Desc, SecretKind Kind, int Value, SecretReward Reward, int Index,
    string RewardText, StoryMsg News);
