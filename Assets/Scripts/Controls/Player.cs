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
            Rotate();
        }

        private void Rotate()
        {
            if (WindowManager.Instance.Status==WindowState.Open) return;

            if (InputManager.Instance==null) return;
            Quaternion quaternion;
            switch(InputManager.Instance.ControlType)
            {
                case KeyConfig.Keyboard:           
                {
                    var mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, Input.mousePosition.z));                 
                    quaternion = Quaternion.Euler(0, 0, Mathf.Atan2(mousePosition.y, mousePosition.x) * Mathf.Rad2Deg - 90);                             
                    break;
                }   
                case KeyConfig.GamePad:           
                {                    
                    quaternion = Quaternion.Euler(0, 0, Mathf.Atan2(Input.GetAxisRaw("RHorizontal"),Input.GetAxisRaw("RVertical")) * Mathf.Rad2Deg - 90);                             
                    break;
                }
                case KeyConfig.GamePadOSX:           
                {                    
                    quaternion = Quaternion.Euler(0, 0, Mathf.Atan2(Input.GetAxisRaw("RHorizontal"),Input.GetAxisRaw("RVertical")) * Mathf.Rad2Deg - 90);                             
                    break;
                }
                default: 
                    quaternion= Quaternion.Euler(0,0,0);
                    break;
            }   
            transform.rotation = quaternion;
        }

        private void MovingLogic()
        {
            if (GameLogicManager.Instance==null) return;
            if (!GameLogicManager.Instance.IsPlayerTurn) return;
            if (WindowManager.Instance.Status==WindowState.Open) return;
            
            var moveVector = GetControllerInput();

            if (Math.Abs(moveVector.x) > TOLERANCE || Math.Abs(moveVector.y) > TOLERANCE)       
             {
                RoundMoves(ref moveVector);
                AttemtMove<MovingObject>(moveVector);
                if (EnemyUtils.SelectedEnemy==null)
                    transform.eulerAngles=MathUtils.SetRotation(moveVector);
                else
                    RotateToEnemy();
                GameLogicManager.Instance.EndPlayerTurn();
             }
        }

        public void RotateToEnemy()
        {
            transform.eulerAngles = CalculateAngle(EnemyUtils.SelectedEnemy);
        }

        private Vector2 GetControllerInput()
        {
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
