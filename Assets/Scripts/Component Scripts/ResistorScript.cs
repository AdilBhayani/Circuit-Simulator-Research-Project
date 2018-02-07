﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResistorScript : MonoBehaviour {
	public GameObject resistor;
	public GameObject textObject;
	private SpriteRenderer spirit;
	private bool selected = false;
	private string ID;
	private string IDUp;
	private string IDDown;
	private string IDLeft;
	private string IDRight;
	private float value;
	private static int resistorCount = 0;
	private string location;
	private bool vertical;

	// Use this for initialization
	void Start () {
		spirit = resistor.GetComponent<SpriteRenderer> ();
		spirit.color = Color.white;
		value = 0;
	}
		
	// Update is called once per frame
	void Update () {
		if (Components.getPaused ()) {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 0.3f);
		} else {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 1f);
		}
		if (Components.getSelectedComponentCount() == 0){
			spirit.color = Color.white;
			selected = false;
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

	public void setValue(float value){
		this.value = value;
		textObject.GetComponent<Text> ().text = value.ToString();
	}

	public float getValue(){
		return value;
	}

	public static int GetResistorCount(){
		return resistorCount;
	}

	public static void increaseResistorCount(){
		resistorCount++;
	}

	public static void decreaseResistorCount(){
		resistorCount--;
	}

	public string getLocation(){
		return location;
	}

	public void setLocation(string newLocation){
		location = newLocation;
	}

	public void setIDUp(string newID){
		IDUp = newID;
	}

	public void setIDDown(string newID){
		IDDown = newID;
	}

	public string getIDUp(){
		return IDUp;
	}

	public string getIDDown(){
		return IDDown;
	}

	public void SetVertical(bool newVertical){
		vertical = newVertical;
	}

	public bool getVertical(){
		return vertical;
	}
}
