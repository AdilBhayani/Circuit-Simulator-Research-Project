using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerUIUpdater : MonoBehaviour {

    public static float startTime = 90.0f;
    public Text timerText;
    private int minutes;
    private int seconds;

	void Start () {
		startTime = 90.0f;
	}

	// Update is called once per frame
	void Update () {
        startTime -= Time.deltaTime;
        minutes = (int)startTime / 60;
        seconds = (int)startTime - minutes * 60;
        timerText.text = string.Format("Timer: {0}:{1}", minutes.ToString("D2"),seconds.ToString("D2"));
    }

	public static void ResetStartTime(){
		startTime = 90.0f;
	}
}
