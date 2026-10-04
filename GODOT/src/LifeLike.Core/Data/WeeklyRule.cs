namespace LifeLike.Core.Data;

/// <summary>Zasada wyzwania tygodnia (#34, core::weekly_rule): zawód, bez kawy, elity, pogoda, bez Hurtowni, materiały, HP, ciosy, budżet.</summary>
public enum WeeklyRule : byte { Cls, NoCoffee, ElitePct, Weather, NoShop, MatsPct, HpPct, DmgPct, Cash }
