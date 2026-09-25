namespace LifeLike.Game.Phone;

/// <summary>Zakładki telefonu w trakcie budowy (kolejność paska jak phone_tab na GBA).</summary>
public static class PhoneTabs
{
    public const int Tasks = 0, Issues = 1, Start = 2, Gear = 3, Costs = 4;

    public static readonly string[] Labels = ["Zadania", "Usterki", "Start", "Sprzęt", "Koszty"];
}
