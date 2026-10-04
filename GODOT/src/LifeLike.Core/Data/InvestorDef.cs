namespace LifeLike.Core.Data;

/// <summary>
/// Modyfikator trybu inwestora (sekcja "investor", jak Heat w Hadesie): dostępny po pierwszej wygranej.
/// Value: budżet – % zł; problemy – +% HP; kontrola – o ile tur częściej cios bossa; termin – +obrażenia.
/// XpPct – premia doświadczenia, Stake – punkty stawki.
/// </summary>
public sealed record InvestorDef(string Id, string Name, string Desc, InvestorEffect Effect, int Value, int XpPct, int Stake);
