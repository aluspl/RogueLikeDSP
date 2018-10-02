using LifeLike.Inferfaces;
using LifeLike.Interfaces;
using UnityEngine;

namespace LifeLike.Inferfaces
{
    public interface IGameLogicManager : IManager
    {
        bool IsPlayerTurn { get; set; }
        bool IsDay { get; set; }
        IMapManager GetMapManager();

        void EndPlayerTurn();
        void GameOver();
        void FinishMap();
    }
}