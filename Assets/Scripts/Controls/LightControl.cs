using UnityEngine;

namespace LifeLike.Controls
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
            if (InputManager.Instance==null) return;

            if (Input.GetKeyDown(InputManager.Instance.LightKey)) _isDay = !_isDay;

            SetLight();
        }
        private void SetLight()
        {
            if (_isDay)
            {
                _light.intensity = 0.6f;

            }
            if (!_isDay)
            {
                _light.intensity = 0.1f;
            }
        }
    }
}
