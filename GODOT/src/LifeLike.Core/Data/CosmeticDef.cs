namespace LifeLike.Core.Data;

/// <summary>Wygląd (tylko oprawa): z sekretnego zlecenia (Złota kielnia, Kask w paski) albo v0.21.52 z odznaki / zlecenia –
/// kolor kasku (Helmet = RGB 0xRRGGBB, -1 = nie kask).</summary>
public sealed record CosmeticDef(string Id, string Name, string Desc, int Helmet = -1)
{
    public bool IsHelmet => Helmet >= 0;
}
