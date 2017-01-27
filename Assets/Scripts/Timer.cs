using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Timer : MonoBehaviour {
    private float _timer = 0;

    void Start()
    {
    }
    void Update()
    {
        _timer += Time.deltaTime;
//        Debug.Log(_timer.ToString(CultureInfo.InvariantCulture));

    }
    public string  GetTimeInSeconds()
    {
        Debug.Log(_timer.ToString(CultureInfo.InvariantCulture));

        return string.Format("Time: {0}", _timer);
    }

    public void StartTime()
    {
        _timer = 0;
    }

}
