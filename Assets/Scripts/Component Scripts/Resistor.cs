using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resistor : MonoBehaviour {
	public GameObject resistor;
	private SpriteRenderer spirit;
	private bool selected = false;

	// Use this for initialization
	void Start () {
		spirit = resistor.GetComponent<SpriteRenderer> ();
		spirit.color = Color.white;
	}
		
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseDown(){
		if (selected) {
			if (Components.decreaseSelected ()) {
				spirit.color = Color.white;
				selected = false;
			}
		} else {
			if (Components.increaseSelected()) {
				spirit.color = Color.yellow;
				selected = true;
			} else {
				UpdateFeedback.updateMessage ("Select only two components");
			}
		}
	}
}
