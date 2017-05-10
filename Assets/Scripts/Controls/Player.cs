using System;
using MapElements;
using UnityEngine;
using Utils;
namespace Controls
{
    public class Player : MovingObject
    {
        const bool IsButtonEnabled = true;

        protected override void Start ()
        {
            base.Start();
        }


        void Update()
        {
            MovingLogic();
        }

        private void MovingLogic()
        {
            if (!GameManager.Instance.IsPlayerTurn) return;
            if (GameManager.Instance.OpenedWindow) return;
            var moveVector = GetControllerInput();

            RoundMoves(moveVector);
            if (Math.Abs(moveVector.x) > TOLERANCE || Math.Abs(moveVector.y) > TOLERANCE)       
                AttemtMove<MovingObject>(moveVector);
             if (moveVector!=Vector2.zero)
             {
                transform.eulerAngles=MathUtils.SetRotation(moveVector);
                GameManager.Instance.EndPlayerTurn();
             }
        }

        private Vector2 GetControllerInput()
        {

            if (IsButtonEnabled)
            {
            // Keys
                 int vector = 1;

                if (Input.GetKeyDown(GameManager.Instance.KeyUp))
                {
                    //transform.eulerAngles=new Vector3(0,0,0);
                    return new Vector2(0, vector);
                }
                if (Input.GetKeyDown(GameManager.Instance.KeyLeft))
                {
                   // transform.eulerAngles=new Vector3(0,0,90);
                    return new Vector2(vector * -1, 0);
                }
                if (Input.GetKeyDown(GameManager.Instance.KeyRight))
                {
                 //   transform.eulerAngles=new Vector3(0,0,270);
                    return new Vector2(vector, 0);
                }
                if (Input.GetKeyDown(GameManager.Instance.KeyDown))
                {
                //    transform.eulerAngles=new Vector3(0,0,180);
                    return new Vector2(0, vector * -1);
                }
                else return Vector2.zero;
            }
            else
            {
                //Controller - MORE UNiversal, but sometimes too sensitive

                var x = Input.GetAxisRaw("Horizontal");
                float f;
            //    int x = (f=Input.GetAxis("Horizontal")) > 0 ? 1 : f < 0 ? -1 : 0;

                var y = Input.GetAxisRaw("Vertical");
          //    int y = (f=Input.GetAxis("Vertical")) > 0 ? 1 : f < 0 ? -1 : 0;

                if (x!=0 || y!=0) Debug.Log(string.Format("x: {0} y: {1}",x,y));
                if (x!=0) y=0;
                return new Vector2(x,y);
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
            if (component is Enemy)
            {
                GameManager.Instance.SelectEnemy(component as Enemy);
            }
        }
        private void CheckIfDisable()
        {
            GameManager.Instance.GameOver();
        }


        public static readonly string Tag = "Player";
        private static readonly double TOLERANCE=0;
    }
}
