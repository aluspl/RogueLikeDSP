namespace Characters
{
    public class GameLogSystem
    {
        public static string Attack(int damage, BaseCharacter enemy)
        {
            return string.Format("You attack {0} for {1} damage. He has now {2} points of life",enemy.Name, damage, enemy.HealthPoint);
        }
    }
}