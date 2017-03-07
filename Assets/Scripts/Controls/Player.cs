using System;
using System.Security.Cryptography.X509Certificates;
using MapUtils;
using UnityEngine;

namespace Controls
{
    public class Player : MovingObject
    {
        public bool IsButtonEnabled = true;

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
            if (IsButtonEnabled)
            {
            // Keys
                const int vector = 1;

                if (Input.GetKeyDown(GameManager.Instance.KeyUp))
                {
                    transform.eulerAngles=new Vector3(0,0,0);
                    return new Vector2(0, vector);
                }
                if (Input.GetKeyDown(GameManager.Instance.KeyLeft))
                {
                    transform.eulerAngles=new Vector3(0,0,90);
                    return new Vector2(vector * -1, 0);
                }
                if (Input.GetKeyDown(GameManager.Instance.KeyRight))
                {
                    transform.eulerAngles=new Vector3(0,0,270);
                    return new Vector2(vector, 0);
                }
                if (Input.GetKeyDown(GameManager.Instance.KeyDown))
                {
                    transform.eulerAngles=new Vector3(0,0,180);
                    return new Vector2(0, vector * -1);
                }
                else return Vector2.zero;
            }
            else
            {
                //Controller - MORE UNiversal, but sometimes too sensitive

                var x = (int)Input.GetAxisRaw("Horizontal");

                var y = (int)Input.GetAxisRaw("Vertical");
                if (x != 0) y = 0;

                return new Vector2(x,y);
            }
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
        private void CheckIfDisable()
        {
            GameManager.Instance.GameOver();
        }


        public static string Tag = "Player";
        private static double TOLERANCE=0;
    }
}
