using System.Collections;
using UnityEngine;

namespace LifeLike.Interfaces
{
    public interface IMovement
    {
        void Movement(Vector3 destination);
        bool Move(Vector2 destination, out RaycastHit2D hit);

    }
}