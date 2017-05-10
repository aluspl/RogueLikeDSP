using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Characters;

public class UIManager : MonoBehaviour
{

    public Text GameLog;
    public Text SelectedEnemyStatistic;
    public Text SelectedEnemyDetail;
    public Text PlayerStatistic;
    public Text PlayerDetail;

     StringBuilder _stringLog= new StringBuilder();
    public int MaxLines=10;

    public void AddLog(string log)
    {
        ClearFirstLine();

        _stringLog.AppendLine(log);
         GameLog.text = _stringLog.ToString();
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
    public BaseCharacter Player {get {return GameManager.Instance.PlayerStatistic;}}
    private void PlayerPanel()
    {
        if (Player==null) return;
        if (PlayerStatistic!=null) PlayerStatistic.text=string.Format("<b>Name: </b>{0}", Player.Name);
       
        if (PlayerDetail!=null) PlayerDetail.text=string.Format("<b>HP:</b> {0}/{1}\n<b>Enemy Killed:</b> {2}",
          Player.HealthPoint,
          Player.MaxHealthPoint,
          Player.KilledEnemies);
    }

    private void SelectedEnemyPanel()
    {
        if (EnemyUtils.SelectedEnemy==null)
        {
            if (SelectedEnemyStatistic != null)
                SelectedEnemyStatistic.text=string.Empty;
            if (SelectedEnemyDetail != null)
                SelectedEnemyDetail.text=string.Empty;
        }
        else
        {
            if (SelectedEnemyStatistic != null)
            {
                SelectedEnemyStatistic.text = string.Format("<b>Name</b>: {0} \n<b>Class Name</b>: {1}",
                    EnemyUtils.SelectedEnemy.EnemyCharacter.Name,
                    EnemyUtils.SelectedEnemy.ClassName);
            }
            if (SelectedEnemyDetail != null)
            {
                SelectedEnemyDetail.text = string.Format("<b>Distance:</b>{0}",
                    EnemyUtils.SelectedEnemy.Distance);
            }
        }
    }

    private void ClearFirstLine()
    {
            if (_stringLog.Length==0) return;
            if (_stringLog.ToString().Split('\n').Length<MaxLines) return;
            var firstLine = _stringLog.ToString().IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (firstLine >= 0)
                _stringLog.Remove(0, firstLine + Environment.NewLine.Length);

    }
    public void Awake()
    {
//        if (Instance == null) Instance = this;
//        else if (Instance!=this) Destroy(gameObject);
    }
}