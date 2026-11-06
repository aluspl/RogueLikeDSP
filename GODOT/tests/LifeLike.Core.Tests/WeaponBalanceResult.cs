namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: komplet pomiarów WeaponBalanceTests (poziomy, meta, kontrakty, bot tylko z bronią domyślną).</summary>
public sealed class WeaponBalanceResult
{
    public int Runs { get; set; }

    public WeaponBalanceTable Easy { get; set; }

    public WeaponBalanceTable Normal { get; set; }

    public WeaponBalanceTable Hard { get; set; }

    public WeaponBalanceTable DefaultOnly { get; set; }

    public double Training { get; set; }

    public double Respect { get; set; }

    public double Act0 { get; set; }

    public double Investor { get; set; }

    public double[] ContractNone { get; set; }

    public double[] ContractFull { get; set; }
}
