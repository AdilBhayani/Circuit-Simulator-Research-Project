using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EfxUpdater : MonoBehaviour {
	private string currentEfxTitle = "currentEfx";
	
    // Use this for initialization
    void Start () {
		
    }
	
	// Update is called once per frame
	void Update () {
		
    }
	
	private void playSound(string efxName){
		PlayerPrefs.SetString(currentEfxTitle, efxName);
	}
	
	public void playButtonSound(){
		playSound("buttonPress");
	}
	
	public void playLevelSwitchSound(){
		playSound("levelSwitch");
	}
}
