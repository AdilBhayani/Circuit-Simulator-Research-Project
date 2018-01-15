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
		spirit.color = new Color (1f, 1f, 1f, 1f);
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
				Debug.Log ("Display error message");
			}
		}
		Debug.Log ("Selected component Count is: " + Components.getSelectedComponentCount().ToString());
	}
}
