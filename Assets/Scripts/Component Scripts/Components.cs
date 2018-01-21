using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Components : MonoBehaviour {
	public GameObject Node;
	public GameObject WireHorizontal;
	public GameObject WireVertical;
	public GameObject Resistor;
	public Transform circuitPanel;
	private static int selectedComponentCount;
	private static bool paused;

	// Use this for initialization
	void Start () {
		selectedComponentCount = 0;
		paused = false;

		createNode ("N1", 0.05f, 0.85f, true, false);
		createWire ("W1", 0.05f, 0.5f, 0.85f, 0.85f, 4.5f, false);
		createResistor("R1", 0.5f, 0.85f);
		createWire ("W2", 0.5f, 0.95f, 0.85f, 0.85f, 4.5f, false);
		createNode ("N2", 0.95f, 0.85f, false, false);
		createWire ("W3", 0.95f, 0.95f, 0.05f, 0.85f, 8.0f, true);
		createNode ("N3", 0.95f, 0.05f, false, false);
		createWire ("W4", 0.5f, 0.95f, 0.05f, 0.05f, 4.5f, false);
		createResistor("R2", 0.5f, 0.05f);
		createWire ("W5", 0.05f, 0.5f, 0.05f, 0.05f, 4.5f, false);
		createNode ("N4",0.05f,0.05f,false,true);

	}

	// Update is called once per frame
	void Update () {
		
	}

	public static bool increaseSelected(){
		if (selectedComponentCount < 2) {
			selectedComponentCount++;
			return true;
		}
		return false;
	}

	public static bool decreaseSelected(){
		if (selectedComponentCount > 0) {
			selectedComponentCount--;
			return true;
		}
		return false;
	}

	public static int getSelectedComponentCount(){
		return selectedComponentCount;
	}

	public static void setPaused(bool newPaused){
		paused = newPaused;
	}

	public static bool getPaused(){
		return paused;
	}

	private void createNode(string ID, float anchorX, float anchorY, bool first, bool last){
		GameObject newNode = (GameObject)Instantiate (Node, new Vector3 (0, 0, 0), Quaternion.identity);
		newNode = setPosition (newNode, ID, anchorX, anchorX, anchorY, anchorY);
		newNode.GetComponent<NodeScript> ().setID (ID);

		if (first) {
			newNode.tag = "firstNode";
		} else if (last) {
			newNode.tag = "lastNode";
		}
	}

	private void createResistor(string ID, float anchorX, float anchorY){
		GameObject newResistor = (GameObject)Instantiate (Resistor, new Vector3 (0, 0, 0), Quaternion.identity);
		newResistor = setPosition(newResistor, ID, anchorX, anchorX, anchorY, anchorY);
		newResistor.GetComponent<ResistorScript> ().setID (ID);
	}

	private void createWire(string ID, float xStart, float xEnd, float yStart, float yEnd, float scale, bool vertical){
		GameObject newWire;
		if (vertical) {
			newWire = (GameObject)Instantiate (WireVertical, new Vector3 (0, 0, 0), Quaternion.identity);
		} else {
			newWire = (GameObject)Instantiate (WireHorizontal, new Vector3 (0, 0, 0), Quaternion.identity);
		}
		newWire = setPosition (newWire, ID, xStart, xEnd, yStart, yEnd);
		Vector3 temp = newWire.GetComponent<RectTransform> ().localScale;
		if (vertical) {
			temp.y = temp.y * scale;
		} else {
			temp.x = temp.x * scale;
		}
		newWire.GetComponent<RectTransform> ().localScale = temp;
		newWire.GetComponent<WireScript> ().setID (ID);
	}

	private GameObject setPosition(GameObject newObject, string ID, float anchorXMin, float anchorXMax, float anchorYMin, float anchorYMax){
		newObject.transform.SetParent (circuitPanel.transform, false);
		newObject.GetComponent<RectTransform> ().anchorMin = new Vector2 (anchorXMin, anchorYMin);
		newObject.GetComponent<RectTransform> ().anchorMax = new Vector2 (anchorXMax, anchorYMax);
		return newObject;
	}
}
