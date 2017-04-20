using UnityEngine;

namespace Controls
{
    public class FollowPlayer : MonoBehaviour
    {
        private Vector3 offset;         //Private variable to store the offset distance between the player and camera
        private bool isDay = false;
        private Camera _camera;

        // Use this for initialization
        void Start ()
        {
            _camera = GetComponent<Camera>();

            if (GameManager.Instance.PlayerObject != null)
            {
                offset = transform.position - GameManager.Instance.PlayerObject.transform.position;

            }
            SetLight();

        }
	
        void LateUpdate ()
        {
            SetLight();
            // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
            if (GameManager.Instance.PlayerObject!=null)
            transform.position = GameManager.Instance.PlayerObject.transform.position + offset;
        }

        private void SetLight()
        {
            if (!GameManager.Instance.IsDay && isDay)
            {
               _camera.backgroundColor=Color.black;
                RenderSettings.ambientLight = Color.black;
                isDay = false;
            }
            if (GameManager.Instance.IsDay && !isDay)
            {
               _camera.backgroundColor=Color.white;
                RenderSettings.ambientLight = Color.white;


                isDay = true;
            }


        }
    }
}
