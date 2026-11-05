namespace LifeLike.Core.Data;

/// <summary>Stan nakładany przez problem budowy przy trafieniu bohatera. Wet (v0.21.50): mokry – prąd boli bardziej. v0.23.0: Burn (podpalenie), Bleed (krwawienie), Root (unieruchomienie), DefDown (odsłonięty – mniejsza OBR), Mark (oznaczony).</summary>
public enum StatusEffect : byte { None, Poison, Shock, Slip, Paper, Wet, Burn, Bleed, Root, DefDown, Mark }
