using UnityEngine;

namespace LifeLike.MapElements
{
	public class StairsController : MonoBehaviour {

		// Use this for initialization
		void Start () {
		
		}
	
		// Update is called once per frame
		void Update () {
		
		}
		void OnTriggerEnter2D(Collider2D other) {
			Debug.Log(other.tag);
			if (other.tag==Controls.Player.Tag)
			{
				GameLogicManager.Instance.FinishMap();
			}
		}
	}
}
