using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Components : MonoBehaviour {
	private static int selectedComponentCount;
	private static bool paused;

	// Use this for initialization
	void Start () {
		selectedComponentCount = 0;
		paused = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static bool increaseSelected(){
		if (selectedComponentCount < 2) {
			selectedComponentCount++;
			return true;
		}
		return false;
	}

	public static bool decreaseSelected(){
		if (selectedComponentCount > 0) {
			selectedComponentCount--;
			return true;
		}
		return false;
	}

	public static int getSelectedComponentCount(){
		return selectedComponentCount;
	}

	public static void setPaused(bool newPaused){
		paused = newPaused;
	}

	public static bool getPaused(){
		return paused;
	}
}
