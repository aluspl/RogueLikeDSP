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
        public Character Statistic { get; set; }
        private Transform _target;
        public bool IsSelected { get; set; }
        private Light _selectedLight;
        private double TOLERANCE=0;

        public void Awake()
        {
            Statistic = EnemyUtils.GenerateEnemy();
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
                return Statistic != null ? Statistic.SelectedClass : "Any Class";
            }
        }

        public bool IsDead
        {
            get { return Statistic.HealthPoint <= 0; }
        }

        public bool IsHisTurn { get; set; }

        public void Update()
        {        
            _selectedLight.enabled = IsSelected;
          

        }
        protected override void OnCantMove<T>(T component)
        {
            if (Distance==1)
                FightUtils.Instance.AttackPlayer(Statistic);
        }

        public void MoveToPlayer(Player playerObject)
        {      
            if (!Statistic.isEnemy) return;
            if (Statistic.Status==Status.Sleep 
                || Statistic.Status==Status.Paralized) 
            {
                Statistic.StatusChange();
                return;
            } 
            var moveVector = (Vector2)(transform.position- playerObject.transform.position);
       
            MathUtils.RoundMoves(ref moveVector);
            //transform.eulerAngles=MathUtils.SetRotation(moveVector);
         
            if (Math.Abs(moveVector.x) > TOLERANCE || Math.Abs(moveVector.y) > TOLERANCE)
                AttemtMove<MovingObject>(moveVector);        
            transform.eulerAngles=CalculateAngle(PlayerManager.Instance.Object);

        }

     

 

        
    }
}