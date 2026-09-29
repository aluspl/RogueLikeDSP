namespace LifeLike.Core;

/// <summary>
/// Rodzaj znajdźki; kolejność = kolejność wag dropów. Document: Akt 0 (Arg = GameData.Documents); v0.21.50 cz. 3: EventTile –
/// wydarzenie z wyborem (Arg = GameData.ChoiceEvents), StoreKey – klucz do magazynu, Chest – skrzynia w magazynie.
/// </summary>
public enum PickupType : byte { Coffee, Helmet, Plan, Tool, GearBox, Document, EventTile, StoreKey, Chest }
