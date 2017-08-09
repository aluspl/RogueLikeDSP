using System.Collections.Generic;
using LifeLike.Characters;

namespace LifeLike.Interfaces
{
    public interface ICharacter
    {
        string Attack(Character enemy);

        string SpecialAction(Character enemyCharacter);

        List<string> SpecialActionsList();

    }
}