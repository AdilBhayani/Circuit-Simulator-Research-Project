using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Components : MonoBehaviour {
	public GameObject Node;
	public GameObject Wire;
	public GameObject Resistor;
	public Transform circuitPanel;
	private static int selectedComponentCount;
	private static bool paused;

	// Use this for initialization
	void Start () {
		selectedComponentCount = 0;
		paused = false;

		createNode ("First",0.05f,0.95f);
		createNode ("Last",0.05f,0.05f);
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

	private void createNode(string ID, float anchorX, float anchorY){
		GameObject newNode = (GameObject)Instantiate (Node, new Vector3 (0, 0, 0), Quaternion.identity);
		newNode.transform.SetParent (circuitPanel.transform, false);
		newNode.GetComponent<NodeScript> ().setID (ID);
		newNode.GetComponent<RectTransform> ().anchorMin = new Vector2 (anchorX, anchorY);
		newNode.GetComponent<RectTransform> ().anchorMax = new Vector2 (anchorX, anchorY);
	}
}
