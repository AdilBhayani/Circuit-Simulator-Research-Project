using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerUIUpdater : MonoBehaviour {

    public static float currentTime = 90.0f;
    public Text timerText;
    private int minutes;
    private int seconds;
	private static bool running;

	void Start () {
		currentTime = 90.0f;
		running = true;
	}

	// Update is called once per frame
	void Update () {
		if (running) {
			currentTime -= Time.deltaTime;
			minutes = (int)currentTime / 60;
			seconds = (int)currentTime - minutes * 60;
			timerText.text = string.Format ("Timer: {0}:{1}", minutes.ToString ("D2"), seconds.ToString ("D2"));
		}
    }

	public static void ResetStartTime(){
		currentTime = 90.0f;
	}

	public static void StopTimer(){
		running = false;
	}

	public static void ResumeTimer(){
		running = true;
	}
}
