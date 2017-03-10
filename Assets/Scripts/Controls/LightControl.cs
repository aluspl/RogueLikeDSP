using UnityEngine;

namespace Controls
{
    public class LightControl : MonoBehaviour {
        private bool _isDay=false;
        private Light _light;

        // Use this for initialization
        void Start ()
        {
            _light=GetComponent<Light>();
            SetLight();

        }
	
        // Update is called once per frame
        void Update ()
        {
            SetLight();
        }
        private void SetLight()
        {
            if (!GameManager.Instance.IsDay && _isDay)
            {
                _light.intensity = 0.6f;

                _isDay = false;
            }
            if (GameManager.Instance.IsDay && !_isDay)
            {
                _light.intensity = 0;
                _isDay = true;
            }
        }
    }
}
