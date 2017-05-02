using Characters;

public static class GameLogSystem
{
    public static string Attack(int damage, BaseCharacter enemy)
    {
        return string.Format("Attack <b>{1}</b> for <b>{0}</b> damage. Enemy has now <b>{2}</b> HP",damage, enemy.Name, enemy.HealthPoint);
    }

    public static string MissedAttack()
    {
        return "Attack Missed";
    }

    public static string TechTalk(BaseCharacter attacker, bool success)
    {
        return success ? 
            string.Format("{0} are successfully bored enemy! He is sleeping now.  Good Job",attacker.Name) 
            : string.Format("{0} are not so boring for your enemy", attacker.Name);
    }
}