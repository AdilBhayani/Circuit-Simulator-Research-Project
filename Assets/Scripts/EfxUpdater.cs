using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EfxUpdater : MonoBehaviour {
	private static string currentEfxTitle = "currentEfx";
	
    // Use this for initialization
    void Start () {
		
    }
	
	// Update is called once per frame
	void Update () {
		
    }
	
	private static void playSound(string efxName){
		PlayerPrefs.SetString(currentEfxTitle, efxName);
	}
	
	public static void playButtonSoundStatic(){
		playSound("buttonPress");
	}
	
	public static void playLevelSwitchSoundStatic(){
		playSound("levelSwitch");
	}

	public void playLevelSwitchSound(){
		playLevelSwitchSoundStatic();
	}

	public void playButtonSound(){
		playButtonSoundStatic();
	}
}
