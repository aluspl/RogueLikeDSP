using Assets.Scripts.Characters;
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
                Debug.Log(GameManager.Instance.PlayerObject);
                return 0;
                //Matematyka jednak cos daje!
                //      return (int) Mathf.Sqrt(Mathf.Pow(Player.transform.position.x - SelectedEnemy.transform.position.x, 2) +
                //                  Mathf.Pow(Player.transform.position.y - SelectedEnemy.transform.position.y, 2));
            }
        }
        public  string ClassName {
            get
            {
                if (EnemyCharacter != null)
                    return EnemyCharacter.SelectedClass;
                return "Any Class";
            }
        }
        public void Update()
        {
            _selectedLight.enabled = IsSelected;
        }
        protected override void OnCantMove<T>(T component)
        {
        }
    }
}