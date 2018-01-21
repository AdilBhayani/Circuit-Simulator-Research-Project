using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeScript : MonoBehaviour {
	public GameObject node;
	private SpriteRenderer spirit;
	private string ID;
	private string IDUp;
	private string IDRight;
	private string IDDown;
	private string IDLeft;

	// Use this for initialization
	void Start () {
		spirit = node.GetComponent<SpriteRenderer> ();
		spirit.color = Color.white;
	}

	// Update is called once per frame
	void Update () {
		if (Components.getPaused ()) {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 0.15f);
		} else {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 1f);
		}
	}

	public void setID(string newID){
		ID = newID;
	}

	public void setIDUp(string newID){
		IDUp = newID;
	}

	public void setIDRight(string newID){
		IDRight = newID;
	}

	public void setIDDown(string newID){
		IDDown = newID;
	}

	public void setIDLeft(string newID){
		IDLeft = newID;
	}

	public string getID(){
		return ID;
	}

	public string getIDUp(){
		return IDUp;
	}

	public string getIDRight(){
		return IDRight;
	}

	public string getIDDown(){
		return IDDown;
	}

	public string getIDLeft(){
		return IDLeft;
	}
}