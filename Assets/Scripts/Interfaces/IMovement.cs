using System.Collections;
using UnityEngine;

namespace Interfaces
{
    public interface IMovement
    {
        IEnumerator Movement(Vector3 destination);
        bool Move(Vector2 destination, out RaycastHit2D hit);

    }
}