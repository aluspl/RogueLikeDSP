using Characters;

public static class GameLogSystem
{
    public static string Attack(int damage, BaseCharacter enemy)
    {
        return string.Format("You attack enemy for {0} damage",damage);
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