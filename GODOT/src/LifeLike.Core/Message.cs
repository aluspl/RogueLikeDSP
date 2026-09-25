using System.Text;

namespace LifeLike.Core;

/// <summary>
/// Linia dziennika budowy. Jak w GBA: bufor 48 bajtów UTF-8, tekst przycinany do 47 bajtów
/// (może uciąć znak w połowie – Text zamienia wtedy końcówkę na znak zastępczy).
/// </summary>
public sealed class Message
{
    public const int Len = 48;

    public readonly byte[] S = new byte[Len];
    public int N;
    public LogKind Kind = LogKind.Info;
    /// <summary>Ile razy z rzędu ten sam komunikat (x2, x3...).</summary>
    public byte Repeat = 1;

    public string Text => Encoding.UTF8.GetString(S, 0, N);

    public override string ToString() => Repeat > 1 ? $"{Text} x{Repeat}" : Text;

    public Message As(LogKind k)
    {
        Kind = k;
        return this;
    }

    public Message Add(string t)
    {
        Span<byte> buf = t.Length <= 64 ? stackalloc byte[t.Length * 3] : new byte[t.Length * 3];
        var count = Encoding.UTF8.GetBytes(t, buf);
        for (var i = 0; i < count && N < Len - 1; i++) S[N++] = buf[i];
        S[N] = 0;
        return this;
    }

    public Message Add(int v)
    {
        Span<byte> b = stackalloc byte[12];
        var k = 0;
        var neg = v < 0;
        var u = neg ? unchecked((uint)-v) : (uint)v;
        do
        {
            b[k++] = (byte)('0' + u % 10);
            u /= 10;
        }
        while (u != 0);
        if (neg) b[k++] = (byte)'-';
        while (k > 0 && N < Len - 1) S[N++] = b[--k];
        S[N] = 0;
        return this;
    }

    public bool SameText(Message o) => N == o.N && S.AsSpan(0, N).SequenceEqual(o.S.AsSpan(0, o.N));

    public Message Clone()
    {
        var m = new Message { N = N, Kind = Kind, Repeat = Repeat };
        Array.Copy(S, m.S, Len);
        return m;
    }
}
