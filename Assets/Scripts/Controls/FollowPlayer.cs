using UnityEngine;

namespace Controls
{
    public class FollowPlayer : MonoBehaviour {
        public GameObject player;       //Public variable to store a reference to the player game object
        private Vector3 offset;         //Private variable to store the offset distance between the player and camera
        private bool isDay = false;
        private Camera _camera;

        // Use this for initialization
        void Start ()
        {
            _camera = GetComponent<Camera>();
            player = GameObject.FindGameObjectWithTag(Player.Tag);
            if (player!=null)
            offset = transform.position - player.transform.position;
            SetLight();
        }
	
        void LateUpdate ()
        {
            SetLight();
            // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
            if (player!=null)
            transform.position = player.transform.position + offset;
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
