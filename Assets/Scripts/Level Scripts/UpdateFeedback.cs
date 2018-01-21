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
		timer = 5f;
	}
	
	// Update is called once per frame
	void Update () {
		if (message != "") {
			timer -= Time.deltaTime;
		}
		if (timer <= 0) {
			message = "";
			timer = 3f;
			updateUiMessage ();
		}
		if (messageUpdated) {
			updateUiMessage ();
			messageUpdated = false;
		}
	}

	private void updateUiMessage(){
		textObject.text = message;
	}

	public static void updateMessage(string newMessage){
		message = newMessage;
		messageUpdated = true;
	}
}
