using LifeLike.Inferfaces;

namespace LifeLike.Inferfaces
{
    public interface IGameManager : IManager
    {
        bool IsPlayerTurn { get; set; }
        bool OpenedWindow { get; set; }
        bool IsDay { get; set; }

        void EndPlayerTurn();
        void GameOver();
        void FinishMap();
    }
}