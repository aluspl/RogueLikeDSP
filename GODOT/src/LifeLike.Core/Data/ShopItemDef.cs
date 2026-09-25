namespace LifeLike.Core.Data;

/// <summary>Towar w Hurtowni między aktami (płatny budżetem z budowy).</summary>
public sealed record ShopItemDef(string Id, string Name, string Desc, int Price, ShopEffect Effect);
