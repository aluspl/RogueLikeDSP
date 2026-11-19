namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: test pomiarowy na żądanie – bez zmiennej środowiskowej BALANCE_OUT zgłasza się jako pominięty (nie zielony).</summary>
public sealed class BalanceFactAttribute : FactAttribute
{
    public BalanceFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BALANCE_OUT"))) Skip = "pomiar na żądanie: ustaw BALANCE_OUT=plik.md";
    }
}
