using System;
using LifeLike.Enums;
using LifeLike.MapElements;
using LifeLike.Utils;
using UnityEngine;

namespace LifeLike.Controls
{
    public class Player : MovingObject
    {

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
            if (!GameLogicManager.Instance.IsPlayerTurn) return;
            if (WindowManager.Instance.Status==WindowState.Open) return;
            var moveVector = GetControllerInput();

            if (Math.Abs(moveVector.x) > TOLERANCE || Math.Abs(moveVector.y) > TOLERANCE)       
             {
                RoundMoves(ref moveVector);

                 Debug.Log(string.Format("x: {0} y: {1}",moveVector.x,moveVector.y));

                AttemtMove<MovingObject>(moveVector);
                transform.eulerAngles=MathUtils.SetRotation(moveVector);
                GameLogicManager.Instance.EndPlayerTurn();
             }
        }

        private Vector2 GetControllerInput()
        {

            
                //Controller - MORE UNiversal, but sometimes too sensitive

                var x = Input.GetAxisRaw("Horizontal");
                var y = Input.GetAxisRaw("Vertical");
                if (x!=0) y=0;
                return new Vector2(x,y);
            


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
                EnemyManager.Instance.SelectEnemy(component as Enemy);
            }
        }
        private void CheckIfDisable()
        {
            GameLogicManager.Instance.GameOver();
        }


        public static readonly string Tag = "Player";
        private static readonly double TOLERANCE=0;
    }
}
