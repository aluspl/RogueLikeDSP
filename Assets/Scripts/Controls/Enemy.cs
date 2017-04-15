using Characters;
using Characters.CharacterClasses;
using Utils;

namespace Controls
{
    public class Enemy : MovingObject
    {
        public BaseCharacter EnemyCharacter { get; set; }

        public Enemy()
        {
            EnemyCharacter = EnemyUtils.GenerateEnemy();
        }
        protected override void OnCantMove<T>(T component)
        {
        }
    }
}