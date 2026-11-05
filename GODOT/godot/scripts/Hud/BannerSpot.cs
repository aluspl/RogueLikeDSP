namespace LifeLike.Game.Hud;

/// <summary>v0.22.0 (UX A): gdzie ekran pokazuje powiadomienia, żeby nie zasłaniały treści.</summary>
public enum BannerSpot
{
    /// <summary>Prawy górny róg (na mapie pod paskiem HUD); przy telefonie na środku – kolumna po lewej.</summary>
    Default,

    /// <summary>Tytuł: pod rekordem, zadaniami i kluczem ustawień.</summary>
    BelowTitleStats,

    /// <summary>Telefon na cały ekran: od dołu, nad przyciskami strony i paskiem zakładek.</summary>
    Bottom,
}
