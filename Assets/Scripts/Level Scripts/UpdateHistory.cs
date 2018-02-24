using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UpdateHistory updates the history on the left side of the screen to show the past transformations.
/// </summary>
public class UpdateHistory : MonoBehaviour {
	private static string history;
	public GameObject historyPanel;
	private static bool historyUpdated;

	// Use this for initialization
	void Start () {
		clearHistory ();
	}
	
	// Update is called once per frame
	void Update () {
		if (historyUpdated) {
			historyUpdated = false;
			historyPanel.GetComponent<Text> ().text = history;
		}
	}

	public static void appendToHistory (string additionalHistory){
		history += additionalHistory;
		historyUpdated = true;
	}

	public static void clearHistory(){
		history = "";
		historyUpdated = true;
	}
}
