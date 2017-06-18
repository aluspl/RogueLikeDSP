using LifeLike.Enums;
using UnityEngine;

namespace LifeLike
{
    public abstract class Window : MonoBehaviour 
    {
        public void CloseWindow()
		{
            WindowManager.Instance.Status = WindowState.Close;;
			Destroy(this.gameObject);
		}
        
        public void Awake(){
            SetupView();
        }
        public virtual void SetupView(){
            
        }
    }
}