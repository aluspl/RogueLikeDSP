namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: wynik pomiaru jednego ustawienia bota – wygrane i udział porażek na etapach (% ×10) na zawód.</summary>
public sealed class WeaponBalanceTable
{
    public WeaponBalanceTable(int classes, int stages)
    {
        Win10 = new int[classes];
        Death10 = new int[classes][];
        for (var c = 0; c < classes; ++c) Death10[c] = new int[stages];
    }

    public int[] Win10 { get; }

    public int[][] Death10 { get; }

    /// <summary>Wygrane % zwykłych zawodów (bez sekretów).</summary>
    public double OpenPct { get; set; }

    /// <summary>Wygrane % wszystkich zawodów.</summary>
    public double AllPct { get; set; }
}
