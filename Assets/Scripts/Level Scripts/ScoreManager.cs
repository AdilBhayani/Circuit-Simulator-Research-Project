using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {
	public Text scoreText;
	private static int score;
	private static bool scoreChanged;

	// Use this for initialization
	void Start () {
		score = 0;
		scoreChanged = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (scoreChanged) {
			scoreChanged = false;
			scoreText.text = string.Format ("Score: {0}", score.ToString ().PadLeft (4, '0'));
		}
	}

	public static void IncreaseScore(int amount){
		scoreChanged = true;
		score += amount;
	}
}
