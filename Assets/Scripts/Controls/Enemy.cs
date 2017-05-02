using System;
using Characters;
using UnityEngine;
using Utils;

namespace Controls
{
    public class Enemy : MovingObject
    {
        public BaseCharacter EnemyCharacter { get; set; }
        private Transform _target;
        public bool IsSelected { get; set; }
        private Light _selectedLight;
        private double TOLERANCE=0;

        public void Awake()
        {
            EnemyCharacter = EnemyUtils.GenerateEnemy();
            _selectedLight = GetComponentInChildren<Light>();
            //    _target = GameObject.FindGameObjectWithTag(PlayerStatistic.Tag).transform;
        }
        public  int Distance {
            get {
                if (GameManager.Instance.PlayerObject != null)
                    return (int) Vector2.Distance(GameManager.Instance.PlayerObject.transform.position,
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
                GameManager.Instance.FightSystem.AttackPlayer(EnemyCharacter);
        }

        public void MoveToPlayer(GameObject playerObject)
        {       
            var moveVector = (Vector2)(transform.position- playerObject.transform.position);
         //   transform.eulerAngles=CalculateAngle(playerObject);
            Debug.Log(string.Format("{0} is at x:{1} y:{2} moving to x: {3} y: {4} You are at x: {5} y: {6}",     
                EnemyCharacter.Name, 
                transform.position.x,
                transform.position.y,
                moveVector.x, 
                moveVector.y, 
                playerObject.transform.position.x,
                playerObject.transform.position.y ));
            
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