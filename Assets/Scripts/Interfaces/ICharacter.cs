using System.Collections.Generic;
using Characters;

namespace Interfaces
{
    public interface ICharacter
    {
        int GetActionsPoints();
        string Attack(BaseCharacter enemy);

        string SpecialAction(BaseCharacter enemyCharacter, string actionName);

        List<string> SpecialActionsList();

    }
}