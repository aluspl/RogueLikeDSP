using System;
using System.Security.Cryptography.X509Certificates;
using MapUtils;
using UnityEngine;
using UnityEngine.PlaymodeTests;

namespace Controls
{
    public class Player : MovingObject
    {
        protected override void Start ()
        {
//            _animator = GetComponent<Animator>();
            base.Start();
        }


        void Update()
        {
            var moveVector=GetControllerInput();
            if (Math.Abs(moveVector.x) > TOLERANCE || Math.Abs(moveVector.y) > TOLERANCE)
            AttemtMove<MovingObject>(moveVector);

        }

        private Vector2 GetControllerInput()
        {
// Keys
             const int horizontal = 1;
             const int vertical = 1;

            if (Input.GetKeyDown(GameManager.Instance.KeyUp))
                return new Vector2(0,vertical);
            if (Input.GetKeyDown(GameManager.Instance.KeyLeft))
                return new Vector2(horizontal*-1,0);
            if (Input.GetKeyDown(GameManager.Instance.KeyRight))
                return new Vector2(horizontal,0);
            if (Input.GetKeyDown(GameManager.Instance.KeyDown))
                return new Vector2(0,vertical*-1);

            //Controller - MORE UNiversal, but sometimes too sensitive

             var x = (int)Input.GetAxisRaw("Horizontal");

             var y = (int)Input.GetAxisRaw("Vertical");
            if (x != 0) y = 0;

            return new Vector2(x,y);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Exit.Tag))
            {
                GameManager.Instance.FinishMap();
                enabled = false;
            }
        }

        protected override void AttemtMove<T>(Vector2 direction)
        {
            base.AttemtMove<T>(direction);
        //    RaycastHit2D hit;
        //    Debug.Log("Put any action only for this Object Here");
        }



        //Action when player meet something
        protected override void OnCantMove<T>(T component)
        {
            if (component is Door)
            {
              //  var door = component as Door;
            }
        }
        [Test]
        private void CheckIfDisable()
        {
            GameManager.Instance.GameOver();
        }


        public static string Tag = "Player";
        private static double TOLERANCE=0;
    }
}
