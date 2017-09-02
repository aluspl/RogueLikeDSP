using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using LifeLike.Utils;
using LifeLike.Characters;
using LifeLike.Inferfaces;

namespace LifeLike
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [InjectAttribute("UI")]
        public static IUIManager Instance = null;

        public Text GameLog;
        public Text ENEMYNAME;
        public Text ENEMYSTATUS;
        public Text ENEMYDISTANCE;
        public Text ENEMYHP;
        public Image ENEMYPANEL;

        public Text PLAYERHP;
        public Text PLAYERSTAMINA;
        public Text PLAYERATTACK;

        StringBuilder _stringLog = new StringBuilder();
        public int MaxLines = 10;

        public void AddLog(string log)
        {
            ClearFirstLine();

            _stringLog.AppendLine(log);
            GameLog.text = _stringLog.ToString();
        }
       void OnDisable()
        {
            GameUI.enabled=false;
        //    Debug.Log("script was disabled");
        }

        void OnEnable()
        {
            GameUI.enabled=true;
      //      Debug.Log("script was enabled");
        }
        public void ClearLog()
        {
            _stringLog.Length = 0;
            _stringLog.Capacity = 0;

        }

        public void LateUpdate()
        {
            SelectedEnemyPanel();
            PlayerPanel();
        }

        public Character Player
        {
            get { return PlayerManager.Instance?.Statistic; }
        }

        public Canvas GameUI { get; set; }
        public bool Enabled { get {return enabled;}
                              set {enabled=value;} }

        private void PlayerPanel()
        {
            if (Player == null) return;
            if (PLAYERHP != null) PLAYERHP.text = string.Format("{0}/{1}",Player.HealthPoint,Player.MaxHealthPoint);
            if (PLAYERSTAMINA != null) PLAYERSTAMINA.text = string.Format("{0}/{1}",Player.StaminaPoint,Player.MaxStaminaPoint);

            if (PLAYERATTACK != null)        PLAYERATTACK.text =Player.SelectedSpecialAttack;
                    
        }

        private void SelectedEnemyPanel()
        {
            if (EnemyUtils.SelectedEnemy == null)
            {
                if (ENEMYHP != null)
                    ENEMYHP.text = string.Empty;
                if (ENEMYNAME != null)
                    ENEMYNAME.text = string.Empty;
                if (ENEMYSTATUS != null)
                    ENEMYSTATUS.text = string.Empty;
                if (ENEMYDISTANCE != null)
                    ENEMYSTATUS.text = string.Empty;
                                    if (ENEMYPANEL!=null) ENEMYPANEL.gameObject.SetActive(false);

            }
            else
            {
                if (ENEMYHP != null)
                    ENEMYHP.text = string.Format("{0}/{1}",EnemyUtils.SelectedEnemy.Statistic.HealthPoint,EnemyUtils.SelectedEnemy.Statistic.MaxHealthPoint);
                if (ENEMYNAME != null)
                    ENEMYNAME.text = EnemyUtils.SelectedEnemy.Statistic.Name;
                if (ENEMYSTATUS != null)
                    ENEMYSTATUS.text = EnemyUtils.SelectedEnemy.Statistic.Status.ToString();
                if (ENEMYDISTANCE != null)
                    ENEMYDISTANCE.text = EnemyUtils.SelectedEnemy.Distance.ToString();

                if (ENEMYPANEL!=null)
                { 
//                    ENEMYPANEL.color=EnemyUtils.SelectedEnemy.Statistic.isEnemy? Color.red : Color.green;
                    ENEMYPANEL.color=EnemyUtils.SelectedEnemy.Statistic.isEnemy? new Color(1,0,0,0.2f) : new Color(0,1,0,0.2f) ;
                    ENEMYPANEL.gameObject.SetActive(true);               
                }
            }
        }

        private void ClearFirstLine()
        {
            if (_stringLog.Length == 0) return;
            if (_stringLog.ToString().Split('\n').Length < MaxLines) return;
            var firstLine = _stringLog.ToString().IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (firstLine >= 0)
                _stringLog.Remove(0, firstLine + Environment.NewLine.Length);

        }

        public void Awake()
        {
            if (Instance == null) Instance = this;
		//	DontDestroyOnLoad(gameObject);
//            DI.Inject(this);

            GameUI = GetComponentInChildren<Canvas>();
            

        }

        public void Destroy()
        {
            Destroy(this.gameObject);
            Instance=null;
        }
    }
}