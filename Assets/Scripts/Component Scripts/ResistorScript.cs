using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistorScript : MonoBehaviour {
	public GameObject resistor;
	public GameObject text;
	private SpriteRenderer spirit;
	private bool selected = false;
	private string ID;
	private string IDLeft;
	private string IDRight;

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

	public void setID(string newID){
		ID = newID;
	}

	public void setIDRight(string newID){
		IDRight = newID;
	}

	public void setIDLeft(string newID){
		IDLeft = newID;
	}

	public string getID(){
		return ID;
	}

	public string getIDRight(){
		return IDRight;
	}

	public string getIDLeft(){
		return IDLeft;
	}

}
