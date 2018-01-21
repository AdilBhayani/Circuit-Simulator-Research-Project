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
		if (Components.getPaused ()) {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 0.3f);
		} else {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 1f);
		}
	}

	void OnMouseDown(){
		if (!Components.getPaused()) {
			if (selected) {
				if (Components.decreaseSelected ()) {
					spirit.color = Color.white;
					selected = false;
				}
			} else {
				if (Components.increaseSelected ()) {
					spirit.color = Color.yellow;
					selected = true;
				} else {
					UpdateFeedback.updateMessage ("Select only two components");
				}
			}
		}
	}

	void onCollisionEnter2D(Collision2D col){
		Debug.Log ("Object is: " + col.gameObject.name);
		spirit.color = Color.blue;
	}
}
