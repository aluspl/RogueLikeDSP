using UnityEngine;

namespace Controls
{
    public class FollowPlayer : MonoBehaviour {
        public GameObject player;       //Public variable to store a reference to the player game object
        private Vector3 offset;         //Private variable to store the offset distance between the player and camera

        // Use this for initialization
        void Start ()
        {
            player = GameObject.FindGameObjectWithTag(Player.Tag);
            offset = transform.position - player.transform.position;

        }
	
        void LateUpdate ()
        {
            // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
            transform.position = player.transform.position + offset;
        }
    }
}
