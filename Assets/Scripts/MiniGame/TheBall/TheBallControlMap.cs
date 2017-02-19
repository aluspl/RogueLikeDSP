using UnityEngine;

namespace MiniGame.TheBall
{
    public class TheBallControlMap : MonoBehaviour {

        public float RotAmount = 180.0f;
        public float Speed = 5.0f;
        public float BackgroundColorVector=0.01f;
        public float SizeFloat= 0.0001f;
        public Camera Camera;

        private Vector3 _currEuler;

        public void Start () {
            RotAmount = 1.0f;
            Speed = 1.0f;
            Camera = Camera.main;
            _currEuler = Vector3.zero;;
            transform.eulerAngles = Vector3.zero;;
            Camera.hdr = true;

        }

        public CharacterController CharCtrl { get; set; }

        // Update is called once per frame
        public void Update ()
        {
            var nextDir = new Vector3(0, 0, Input.GetAxisRaw("Horizontal"));
          //  Debug.Log("Z axis of controller: "+nextDir.z);

            if (nextDir.z>0)
            {
                Camera.backgroundColor=new Color(Camera.backgroundColor.r,Camera.backgroundColor.g,Camera.backgroundColor.b-BackgroundColorVector);

            }
            if (nextDir.z<0)
            {
                Camera.backgroundColor=new Color(Camera.backgroundColor.r,Camera.backgroundColor.g,Camera.backgroundColor.b+BackgroundColorVector);
            }

            _currEuler = Vector3.Lerp(_currEuler, nextDir, Time.deltaTime * Speed);
            transform.eulerAngles += _currEuler;


        }

    }
}

