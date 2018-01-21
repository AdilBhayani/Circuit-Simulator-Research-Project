using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateFeedback : MonoBehaviour {
	private static string message;
	private static bool messageUpdated;
	private float timer;
	public Text textObject;

	// Use this for initialization
	void Start () {
		message = "Find the equivalent resistance";
		messageUpdated = false;
		resetTimer ();
	}
	
	// Update is called once per frame
	void Update () {
		if (message != "") {
			timer -= Time.deltaTime;
		}
		if (timer <= 0) {
			message = "";
			updateUiMessage ();
		}
		if (messageUpdated) {
			updateUiMessage ();
			messageUpdated = false;
		}
	}

	private void updateUiMessage(){
		textObject.text = message;
		resetTimer ();
	}

	public static void updateMessage(string newMessage){
		message = newMessage;
		messageUpdated = true;
	}

	private void resetTimer(){
		timer = 5f;
	}
}
