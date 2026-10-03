using LifeLike.Core;
namespace LifeLike.Game.Phone;

/// <summary>Zakładki telefonu w trakcie budowy (kolejność paska jak phone_tab na GBA).</summary>
public static class PhoneTabs
{
    public const int Tasks = 0, Issues = 1, Start = 2, Gear = 3, Costs = 4;

    public static string[] Labels => [Loc.T("zadania_2"), Loc.T("usterki"), Loc.T("start"), Loc.T("sprzet_5"), Loc.T("koszty")];
}
