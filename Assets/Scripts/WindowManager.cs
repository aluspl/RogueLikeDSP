using System;
using LifeLike;
using LifeLike.Enums;
using LifeLike.Inferfaces;
using UnityEngine;
namespace LifeLike{
public class WindowManager : MonoBehaviour, IWindowManager
{
    public CreateWindow CreateWindowPrefab;
    public DetailWindow DetailWindowPrefab;
    public EquipmentWindow EquipmentWindowPrefab;
    
    
    [InjectAttribute("UI")]
    public static IWindowManager Instance = null;

    public WindowState Status { get; set; }
    public void Awake()
    {
        Instance=this;
        Status =WindowState.Close;
    }
    public void Destroy()
    {
        Status= WindowState.Close;
    }

    private  void OpenDetailWindow()
    {
        if (PlayerManager.Instance!=null && PlayerManager.Instance.Statistic!=null)
        {
            Instantiate(DetailWindowPrefab);
            Status=WindowState.Open;

        }
        else
        {
            Status=WindowState.Close;
        }
    }
    private void OpenEquipmentWindow()
    {
        if (PlayerManager.Instance!=null && PlayerManager.Instance.Statistic!=null)
        {
            Instantiate(EquipmentWindowPrefab);
            Status=WindowState.Open;
        }
        else
        {
            Status=WindowState.Close;
        }        
    }
    private  void OpenCreateWindow()
    {
        if (PlayerManager.Instance!=null && PlayerManager.Instance.Statistic==null) 
        {
            Instantiate(CreateWindowPrefab);
            Status=WindowState.Open;

        }
        else        
        {
            Status=WindowState.Close;
        }
    }

    public void Open(WindowType type)
        {
          switch (type)
            {
                case WindowType.Create: 
                    OpenCreateWindow();
                break;
                case WindowType.Detail: 
                    OpenDetailWindow();
                break;
                case WindowType.Equipment: 
                    OpenEquipmentWindow();                
                break;
                
                default: Debug.LogError(" Window Manager: really? "); break;        
            }
        }

      

        void IManager.Destroy()
        {
            Instance=null;
        }

    }
}
