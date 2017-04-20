﻿using System;
using System.Collections;
using Interfaces;
using UnityEngine;

namespace Controls
{
    public abstract class MovingObject : MonoBehaviour, IMovement
    {
        public float MoveTime = 0.1f;

        public LayerMask BlockingLayer;
        public LayerMask EnemyLayer;

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
            var end = start + destination;
            _boxCollider2D.enabled = false;
            hit = Physics2D.Linecast(start, end, BlockingLayer);
            _boxCollider2D.enabled = true;

            if (hit.transform != null) return false;
                StartCoroutine(Movement(end));
            return true;
        }

        protected static void RoundMoves(Vector3 direction)
        {
            direction.x = (float) Math.Round(direction.x,1);
            direction.y = (float) Math.Round(direction.y,1);
        }


        //Move Object to selected Destination
        public IEnumerator Movement(Vector3 destination)
        {
            Debug.Log(string.Format("Move location X {0} Y{1}",destination.x,destination.y));

            var remainingDistance = (transform.position - destination).sqrMagnitude;
            while (remainingDistance>float.Epsilon)
            {
                var newPosition=Vector3.MoveTowards(_rb2D.position, destination, _inverseMoveTime * Time.deltaTime);
                _rb2D.MovePosition(newPosition);
                remainingDistance = (transform.position - destination).sqrMagnitude;
                yield return null;
            }
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
