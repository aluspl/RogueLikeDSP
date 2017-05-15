using Characters;

public static class GameLogSystem
{
    public static string Attack(int damage, BaseCharacter character,BaseCharacter enemy)
    {
        return string.Format("<b>{0}</b> Attack <b>{1}</b> for <b>{2}</b> damage. Enemy has now <b>{3}</b> HP",character.Name,  enemy.Name, damage, enemy.HealthPoint);
    }

    public static string MissedAttack(BaseCharacter character)
    {
        return string.Format("<b>{0}</b>: Attack Missed",character.Name);
    }

    public static string TechTalk(BaseCharacter attacker, bool success)
    {
        return success ? 
            string.Format("{0} are successfully bored enemy! He is sleeping now.  Good Job",attacker.Name) 
            : string.Format("{0} are not so boring for your enemy", attacker.Name);
    }
}
//22 489 48 77
//spoldzierlcza 72 traspress 