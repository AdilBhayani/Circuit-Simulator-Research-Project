using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UpdateFeedback shows helpful messaged to the user to guide them as they play through the game.
/// </summary>
public class UpdateFeedback : MonoBehaviour {
	private static string message;
	private static bool messageUpdated;
	private float timer;
	public Text textObject;
	private static bool indefinite;

	// Use this for initialization
	void Start () {
		indefinite = false;
		message = "Find the equivalent resistance";
		messageUpdated = false;
		ResetTimer ();
	}
	
	// Update is called once per frame
	void Update () {
		if (message != "" && !indefinite) {
			timer -= Time.deltaTime;
		}
		if (timer <= 0) {
			message = "";
			UpdateUiMessage ();
		}
		if (messageUpdated) {
			UpdateUiMessage ();
			messageUpdated = false;
		}
	}

	private void UpdateUiMessage(){
		textObject.text = message;
		ResetTimer ();
	}

	public static void UpdateMessage(string newMessage){
		message = newMessage;
		messageUpdated = true;
	}

	public static void UpdateMessage(string newMessage, bool endless){
		message = newMessage;
		messageUpdated = true;
		indefinite = endless;
	}

	private void ResetTimer(){
		timer = 5f;
	}
}
