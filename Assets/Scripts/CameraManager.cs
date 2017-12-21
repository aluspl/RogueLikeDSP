using UnityEngine;
using LifeLike;

namespace LifeLike
{
    public class CameraManager : MonoBehaviour
    {
        private Vector3 offset;         //Private variable to store the offset distance between the player and camera
        private bool isDay = false;
        private Camera _camera;
        private Color AlmostDark=new Color(0.6f,0.6f,0.6f,1f);
        private Color ColorSteel=new Color(0.4f,0.4f,0.4f,1f);

        // Use this for initialization
        void Awake ()
        {
            _camera = GetComponent<Camera>();

            if (PlayerManager.Instance!=null &&  PlayerManager.Instance.Object != null)
            {
                transform.position= new Vector3(PlayerManager.Instance.Object.transform.position.x,
                    PlayerManager.Instance.Object.transform.position.y,-10);
            }
            SetLight();
        }
	
        void LateUpdate ()
        {
            SetLight();
            // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
            if (PlayerManager.Instance!=null &&   PlayerManager.Instance.Object!=null)
             transform.position= new Vector3(PlayerManager.Instance.Object.transform.position.x,               
             PlayerManager.Instance.Object.transform.position.y,-10);     
       }

        private void SetLight()
        {
            if (GameLogicManager.Instance==null) return;
            if (!GameLogicManager.Instance.IsDay && isDay)
            {
               _camera.backgroundColor=Color.black;
                RenderSettings.ambientLight = Color.black;
            //    RenderSettings.ambientIntensity=1;
                isDay = false;
            }
            if (GameLogicManager.Instance.IsDay && !isDay)
            {
               _camera.backgroundColor=ColorSteel;
                RenderSettings.ambientLight=AlmostDark;;
              //  RenderSettings.ambientIntensity=0.5f;
                isDay = true;
            }


        }
    }
}
