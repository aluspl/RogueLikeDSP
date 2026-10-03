namespace LifeLike.Core;

// v0.21.52 cz. c: zadania dnia i tygodnia – liczniki budowy (DailyTasks.Metric) – port 1:1 z core.h.
public sealed partial class Game
{
    /// <summary>Wezwania brygady w budowie.</summary>
    public byte HelpersCalled;
    /// <summary>Zakupy w Hurtowni w budowie (zadanie „Wygraj bez Hurtowni”).</summary>
    public byte ShopBuys;
}
