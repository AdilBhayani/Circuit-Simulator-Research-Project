using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerUIUpdater : MonoBehaviour {

    public float startTime;
    public Text timerText;
    private int minutes;
    private int seconds;
	
	// Update is called once per frame
	void Update () {
        startTime -= Time.deltaTime;
        minutes = (int)startTime / 60;
        Debug.Log("minutes is: " + minutes);
        seconds = (int)startTime - minutes * 60;
        timerText.text = string.Format("Timer: {0}:{1}", minutes.ToString("D2"),seconds.ToString("D2"));
    }
}
