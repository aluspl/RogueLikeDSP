namespace LifeLike.Core;

/// <summary>Arytmetyka procentów jak w core.h (pct_part, div_round) – całkowita, bez losowania.</summary>
public static class Pct
{
    /// <summary>Część procentowa z przeniesieniem reszty (dokładna średnio): (v * pct + carry) / 100.</summary>
    public static int Part(int v, int pct, ref int carry)
    {
        if (pct <= 0 || v <= 0) return 0;
        var t = v * pct + carry;
        carry = t % 100;
        return t / 100;
    }

    /// <summary>Zaokrąglenie ilorazu a / b (b &gt; 0) do najbliższej, połówki od zera.</summary>
    public static int DivRound(int a, int b) => a >= 0 ? (2 * a + b) / (2 * b) : -((-2 * a + b) / (2 * b));
}
