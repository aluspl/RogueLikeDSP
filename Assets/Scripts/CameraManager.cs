using UnityEngine;
using LifeLike;

namespace LifeLike
{
    public class CameraManager : MonoBehaviour
    {
        private Vector3 offset;         //Private variable to store the offset distance between the player and camera
        private bool isDay = false;
        private Camera _camera;

        // Use this for initialization
        void Awake ()
        {
            _camera = GetComponent<Camera>();

            if (PlayerManager.Instance!=null &&  PlayerManager.Instance.Object != null)
            {
                transform.position= new Vector3(PlayerManager.Instance.Object.transform.position.x,
                    PlayerManager.Instance.Object.transform.position.y,-1);
            }
            SetLight();
        }
	
        void LateUpdate ()
        {
            SetLight();
            // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
            if (PlayerManager.Instance!=null &&   PlayerManager.Instance.Object!=null)
             transform.position= new Vector3(PlayerManager.Instance.Object.transform.position.x,               
             PlayerManager.Instance.Object.transform.position.y,-1);     
       }

        private void SetLight()
        {
            if (GameManager.Instance==null) return;
            if (!GameManager.Instance.IsDay && isDay)
            {
               _camera.backgroundColor=Color.black;
                RenderSettings.ambientLight = Color.black;
            //    RenderSettings.ambientIntensity=1;
                isDay = false;
            }
            if (GameManager.Instance.IsDay && !isDay)
            {
               _camera.backgroundColor=Color.grey;
                RenderSettings.ambientLight=Color.white;
              //  RenderSettings.ambientIntensity=0.5f;
                isDay = true;
            }


        }
    }
}
