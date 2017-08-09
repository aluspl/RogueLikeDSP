using System;
using System.Collections;
using LifeLike.Interfaces;
using UnityEngine;
using LifeLike.Utils;
namespace LifeLike.Controls
{
    public abstract class MovingObject : MonoBehaviour
    {
        public float MoveTime = 0.05f;

        public LayerMask BlockingLayer;
        public LayerMask CharacterLayer;

        private BoxCollider2D _boxCollider2D;
        private Rigidbody2D _rb2D;
        private float _inverseMoveTime;

        // Use this for initialization
       protected virtual void Start ()
       {
           _boxCollider2D = GetComponent<BoxCollider2D>();
           _rb2D = GetComponent<Rigidbody2D>();
           _inverseMoveTime=  1f / MoveTime;
       }

        public bool Move(Vector2 destination, out RaycastHit2D hit)
        {
            var start = (Vector2)transform.position;
            
            var endPoint = start + destination;
            
            RoundMoves(ref endPoint);

            _boxCollider2D.enabled = false;
                hit = Physics2D.Linecast(start, endPoint, BlockingLayer);
            if (hit.transform == null) 
                hit =  Physics2D.Linecast(start, endPoint, CharacterLayer);
            _boxCollider2D.enabled = true;

            if (hit.transform != null) return false;
                StartCoroutine(Movement(endPoint));
            return true;
        }

        protected static void RoundMoves(ref Vector2 direction)
        {
            direction.x =  (float)Math.Floor(direction.x);
            direction.y =  (float)Math.Floor(direction.y);
        }


        //Move Object to selected Destination
        public IEnumerator Movement(Vector3 destination)
        {
            var remainingDistance = (transform.position - destination).sqrMagnitude;
            
           while (remainingDistance>float.Epsilon)
           {
               var newPosition=Vector3.MoveTowards(_rb2D.position, destination, _inverseMoveTime * Time.deltaTime);
               _rb2D.MovePosition(newPosition);
               remainingDistance = (transform.position - destination).sqrMagnitude;
               yield return null;
           }
            transform.position=destination;
            yield return null;
        }
       //This returns the angle in radians

        public Vector3 CalculateAngle(MovingObject enemy)
        {
            var angle=MathUtils.AngleInDeg(transform.position, enemy.transform.position);        
            return  new Vector3(0,0,(float)angle);
        }
        protected virtual void AttemtMove<T>(Vector2 direction)
            where T : Component
        {
            RaycastHit2D hit;
            var canMove = Move(direction, out hit);
            if (hit.transform==null) return;
            var hitComponent = hit.transform.GetComponent<T>() ;
            if (!canMove && hitComponent!= null)
                OnCantMove(hitComponent);
        }

        protected abstract void OnCantMove<T>(T component)
            where T : Component;

    }
}
