using Assets.Scripts.Characters;
using Characters;

public static class GameLogSystem
{
    public static string Attack(int damage, BaseCharacter enemy)
    {
        return string.Format("You attack {1} for {0} damage. Enemy has now {2} HP",damage, enemy.Name, enemy.HealthPoint);
    }

    public static string MissedAttack()
    {
        return "You Missed";
    }

    public static string TechTalk(bool success)
    {
        return success ? "You are successfully bored enemy! He is sleeping now.  Good Job" : "You are not so boring for your enemy";
    }
}