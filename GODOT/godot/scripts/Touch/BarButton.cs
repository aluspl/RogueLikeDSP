namespace LifeLike.Game.Touch;

/// <summary>Przyciski paska akcji (jak menu akcji START z GBA + telefon); None = poza paskiem.</summary>
public enum BarButton
{
    None = -1,
    Attack = 0,
    Ability = 1,
    Thermos = 2,
    Wait = 3,
    Phone = 4,
    /// <summary>v0.23.0: tryb B broni (odliczanie jak Moc).</summary>
    WeaponB = 5,
    /// <summary>v0.23.0: zamiana broni z kabury (tylko z pasem monterskim).</summary>
    Swap = 6,
}
