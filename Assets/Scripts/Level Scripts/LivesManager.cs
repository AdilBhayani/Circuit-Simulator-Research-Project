using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivesManager : MonoBehaviour {
	public Text livesText;
	private static int livesLeft;
	private static bool livesChanged;

	// Use this for initialization
	void Start () {
		livesLeft = 3;
		livesChanged = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (livesChanged) {
			livesChanged = false;
			livesText.text = "Lives Remaining: " + livesLeft.ToString ();
			if (livesLeft == 0) {
				Components.setPaused (true);
				UpdateFeedback.UpdateMessage ("Game Over! You ran out of lives", true);
			}
		}
	}

	public static void DecreaseLives(){
		livesLeft--;
		livesChanged = true;
	}

}
