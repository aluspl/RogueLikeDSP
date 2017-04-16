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
        public Light SelectedLight;
        public void Awake()
        {
            EnemyCharacter = EnemyUtils.GenerateEnemy();
            SelectedLight = GetComponentInChildren<Light>();
            //    _target = GameObject.FindGameObjectWithTag(Player.Tag).transform;
        }

        public void Update()
        {
            SelectedLight.enabled = IsSelected;
        }
        protected override void OnCantMove<T>(T component)
        {
        }
    }
}