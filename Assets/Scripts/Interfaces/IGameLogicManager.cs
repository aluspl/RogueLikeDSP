using LifeLike.Inferfaces;

namespace LifeLike.Inferfaces
{
    public interface IGameLogicManager : IManager
    {
        bool IsPlayerTurn { get; set; }
        bool IsDay { get; set; }

        void EndPlayerTurn();
        void GameOver();
        void FinishMap();
    }
}