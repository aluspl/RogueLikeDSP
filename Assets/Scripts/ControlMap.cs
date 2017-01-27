using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlMap : MonoBehaviour {

    private bool _resizeValue = true;

    public KeyCode KeyLeft = KeyCode.LeftArrow;

    public KeyCode KeyRight = KeyCode.RightArrow;

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
	    Debug.Log("Z axis of controller: "+nextDir.z);

	    if (nextDir.z>0)
	    {
	    //    nextDir.z += RotAmount;
	        Camera.backgroundColor=new Color(Camera.backgroundColor.r,Camera.backgroundColor.g,Camera.backgroundColor.b-BackgroundColorVector);

	    }
	    if (nextDir.z<0)
	    {
	      //  nextDir.z -= RotAmount;
	        Camera.backgroundColor=new Color(Camera.backgroundColor.r,Camera.backgroundColor.g,Camera.backgroundColor.b+BackgroundColorVector);
	    }

	    _currEuler = Vector3.Lerp(_currEuler, nextDir, Time.deltaTime * Speed);
        transform.eulerAngles += _currEuler;


        if (transform.localScale.x >= 1 || transform.localScale.y >= 1)
	        _resizeValue = false;
	    if (transform.localScale.x <= 0.4 || transform.localScale.y <= 0.4)
	        _resizeValue = true;

	    if (_resizeValue)
	    {
//	        transform.localScale = transform.localScale + new Vector3(SizeFloat, SizeFloat, 0);
	    }
	    else
	    {
//	        transform.localScale=transform.localScale-new Vector3(SizeFloat,SizeFloat,0);

	    }
	}

}

