using System.Collections.Generic;
using LifeLike.Characters;

namespace LifeLike.Interfaces
{
    public interface ICharacter
    {
        int GetActionsPoints();
        string Attack(Character enemy);

        string SpecialAction(Character enemyCharacter);

        List<string> SpecialActionsList();

    }
}