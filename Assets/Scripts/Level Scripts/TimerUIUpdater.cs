using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TimerUIUpdater keep track of the time since th game was started and ends the game once time goes to zero.
/// </summary>
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
		if (Components.getPaused ()) {
			running = false;
		}
		if (running) {
			if (currentTime < 0.0f) {
				UpdateFeedback.UpdateMessage ("Game Over! You ran out of time", true);
				running = false;
				Components.setPaused (true);
			}
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

	public static void IncreaseTime(){
		currentTime += 5.0f;
	}
}
