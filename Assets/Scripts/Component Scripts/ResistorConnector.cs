using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistorConnector : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void onTriggerStay2D(Collision2D col){
		Debug.Log("hello");
		Debug.Log ("Object is: " + col.gameObject.name);
	}
}
