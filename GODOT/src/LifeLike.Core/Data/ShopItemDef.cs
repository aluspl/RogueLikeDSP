namespace LifeLike.Core.Data;

/// <summary>
/// Towar w Hurtowni między aktami: płatny budżetem z budowy (Price zł) albo materiałem (Material &gt;= 0, MatCost sztuk).
/// </summary>
public sealed record ShopItemDef(string Id, string Name, string Desc, int Price, ShopEffect Effect, int Material = -1, int MatCost = 0);
