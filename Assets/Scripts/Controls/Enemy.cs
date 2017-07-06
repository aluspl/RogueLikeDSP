using System;
using LifeLike.Characters;
using LifeLike.Controls;
using LifeLike.Utils;
using LifeLike.Enums;

using UnityEngine;

namespace LifeLike.Controls
{
    public class Enemy : MovingObject
    {
        public Character EnemyCharacter { get; set; }
        private Transform _target;
        public bool IsSelected { get; set; }
        private Light _selectedLight;
        private double TOLERANCE=0;

        public void Awake()
        {
            EnemyCharacter = EnemyUtils.GenerateEnemy();
            _selectedLight = GetComponentInChildren<Light>();
        }
        public  int Distance {
            get {
                if (PlayerManager.Instance.Object != null)
                    return (int) Vector2.Distance(PlayerManager.Instance.Object.transform.position,
                        transform.position);
                return 0;

            }
        }
        public  string ClassName {
            get
            {
                return EnemyCharacter != null ? EnemyCharacter.SelectedClass : "Any Class";
            }
        }

        public bool IsDead
        {
            get { return EnemyCharacter.HealthPoint <= 0; }
        }

        public bool IsHisTurn { get; set; }

        public void Update()
        {        
            _selectedLight.enabled = IsSelected;
        }
        protected override void OnCantMove<T>(T component)
        {
            if (Distance==1)
                FightUtils.Instance.AttackPlayer(EnemyCharacter);
        }

        public void MoveToPlayer(Player playerObject)
        {      
            if (!EnemyCharacter.isEnemy) return;
            if (EnemyCharacter.Status==Status.Sleep 
                || EnemyCharacter.Status==Status.Paralized) 
            {
                EnemyCharacter.StatusChange();
                return;
            } 
            var moveVector = (Vector2)(transform.position- playerObject.transform.position);
       
            MathUtils.RoundMoves(ref moveVector);
            transform.eulerAngles=MathUtils.SetRotation(moveVector);
         
         
            if (Math.Abs(moveVector.x) > TOLERANCE || Math.Abs(moveVector.y) > TOLERANCE)
                AttemtMove<MovingObject>(moveVector);        
        }

     

        //This returns the angle in radians

        private Vector3 CalculateAngle(GameObject player)
        {
            var angle=MathUtils.AngleInDeg(transform.position, player.transform.position);
     //       Debug.Log(string.Format("Angle for {0} is {1}", EnemyCharacter.Name, angle));
            return  new Vector3(0,0,(float)angle);
        }

        
    }
}