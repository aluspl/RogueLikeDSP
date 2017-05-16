using System.Collections.Generic;
using LifeLike.Characters;

namespace LifeLike.Interfaces
{
    public interface ICharacter
    {
        int GetActionsPoints();
        string Attack(BaseCharacter enemy);

        string SpecialAction(BaseCharacter enemyCharacter);

        List<string> SpecialActionsList();

    }
}