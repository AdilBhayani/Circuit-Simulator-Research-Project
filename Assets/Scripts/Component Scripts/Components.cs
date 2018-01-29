using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.IO;

public class Components : MonoBehaviour {
	public GameObject Node;
	public GameObject WireHorizontal;
	public GameObject WireVertical;
	public GameObject Resistor;
	public Transform circuitPanel;
	private static int selectedComponentCount;
	private static bool paused;
	private List <GameObject> componentsList;
	private Circuit ckt;
	private Resistor r1;

	// Use this for initialization
	void Start () {
		selectedComponentCount = 0;
		paused = false;
		componentsList = new List <GameObject> ();
		LoadCircuit ();


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

	private void createNode(string ID, float anchorX, float anchorY, bool first, bool last, string connectionTop, string connectionRight, string connectionDown, string connectionLeft){
		GameObject newNode = (GameObject)Instantiate (Node, new Vector3 (0, 0, 0), Quaternion.identity);
		newNode = setPosition (newNode, ID, anchorX, anchorX, anchorY, anchorY);
		newNode.GetComponent<NodeScript> ().setID (ID);

		if (first) {
			newNode.tag = "firstNode";
		} else if (last) {
			newNode.tag = "lastNode";
		}
		componentsList.Add (newNode);
	}

	private void createResistor(string ID, float anchorX, float anchorY, float value){
		GameObject newResistor = (GameObject)Instantiate (Resistor, new Vector3 (0, 0, 0), Quaternion.identity);
		newResistor = setPosition(newResistor, ID, anchorX, anchorX, anchorY, anchorY);
		newResistor.GetComponent<ResistorScript> ().setID (ID);
		newResistor.GetComponent<ResistorScript> ().setValue (value);
		componentsList.Add (newResistor);
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
		componentsList.Add (newWire);
	}

	private GameObject setPosition(GameObject newObject, string ID, float anchorXMin, float anchorXMax, float anchorYMin, float anchorYMax){
		newObject.transform.SetParent (circuitPanel.transform, false);
		newObject.GetComponent<RectTransform> ().anchorMin = new Vector2 (anchorXMin, anchorYMin);
		newObject.GetComponent<RectTransform> ().anchorMax = new Vector2 (anchorXMax, anchorYMax);
		return newObject;
	}

	public void checkTransformSeries(){
		if (selectedComponentCount == 2) {
			foreach (GameObject componentObject in componentsList) {
				if (componentObject.GetComponents<ResistorScript> ().Length != 0) {
					if (componentObject.GetComponent<ResistorScript> ().getID () == "R2") {
						Destroy (componentObject);
					}
					if (componentObject.GetComponent<ResistorScript> ().getID () == "R1") {
						componentObject.GetComponent<ResistorScript> ().setValue (25.0f);
					}
				}
			}
			UpdateHistory.appendToHistory ("-Series Transform: \nR(10) & R(15)");
		} else {
			UpdateFeedback.updateMessage ("Select two components first");
		}
	}

	public void checkTransformParallel(){
		if (selectedComponentCount == 2) {
			UpdateFeedback.updateMessage ("Components are not in parallel");
		} else {
			UpdateFeedback.updateMessage ("Select two components first");
		}
	}

	private void LoadCircuit(){
		StreamReader reader = CircuitLoader.ReadString ();
		string circuitLine = reader.ReadLine ();
		int count = 65;
		Dictionary<string,string[]> lineDictionary = new Dictionary<string,string[]>();

		while (circuitLine != null) {
			if (circuitLine.StartsWith("//")){
				circuitLine = reader.ReadLine ();
			}else{
				Debug.Log (circuitLine);
				char letterChar = (char)count;
				string letter = letterChar.ToString ();
				count++;
				string[] circuitLineArray = circuitLine.Split (',');
				lineDictionary.Add(letter, circuitLineArray);
				circuitLine = reader.ReadLine ();
			}
		}
		Debug.Log ("Here");
		string printString = "";
		foreach (KeyValuePair<string, string[]> kvp in lineDictionary){
			//string arrayValue = string.Join (".", kvp.Value);
			printString += string.Format ("Key = {0}, Value = {1}", kvp.Key, string.Join(".", kvp.Value));
			printString += "\n";
		}
		Debug.Log (printString);
	}
}
