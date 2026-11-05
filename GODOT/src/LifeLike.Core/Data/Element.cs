namespace LifeLike.Core.Data;

/// <summary>v0.21.50 cz. 2 (#29): żywioł ciosu / problemu – kombinacje stanów (mokry + prąd, pył + iskra). v0.23.0: + Fire (ogień, JSON „ogien”).</summary>
public enum Element : byte { None, Water, Power, Spark, Fire }
