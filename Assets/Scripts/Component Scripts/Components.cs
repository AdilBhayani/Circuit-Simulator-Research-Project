using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

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

	// Use this for initialization
	void Start () {
		selectedComponentCount = 0;
		paused = false;
		componentsList = new List <GameObject> ();

		createNode ("N1", 0.05f, 0.85f, true, false, null, "R1", null, null);
		createWire ("W1", 0.05f, 0.5f, 0.85f, 0.85f, 4.5f, false);
		createResistor("R1", 0.5f, 0.85f, 10.0f);
		createWire ("W2", 0.5f, 0.95f, 0.85f, 0.85f, 4.5f, false);
		createNode ("N2", 0.95f, 0.85f, false, false, null, null, "N3", "R1");
		createWire ("W3", 0.95f, 0.95f, 0.05f, 0.85f, 8.0f, true);
		createNode ("N3", 0.95f, 0.05f, false, false, "N3", null, null, "R2");
		createWire ("W4", 0.5f, 0.95f, 0.05f, 0.05f, 4.5f, false);
		createResistor("R2", 0.5f, 0.05f, 15.0f);
		createWire ("W5", 0.05f, 0.5f, 0.05f, 0.05f, 4.5f, false);
		createNode ("N4",0.05f,0.05f,false,true, null, "R2", null, null);
		
		// Build the circuit
		Circuit ckt = new Circuit();
		Resistor r1;
		ckt.Objects.Add(
			new Voltagesource("V1", "1", "GND", 1.0),
			r1 = new Resistor("R1", "1", "2", 1e3),
			new Resistor("R2", "2", "GND", 1e3),
			new Resistor("R3", "2", "GND", 1e3)
			);

		// Simulation
		DC dc = new DC("Dc 1");
		dc.Sweeps.Add(new DC.Sweep("V1", 0, 1, 1));
		dc.OnExportSimulationData += (object sender, SimulationData data) =>
			{
			if (dc.Sweeps[0].CurrentValue == 1){
				double vr1 = data.GetVoltage("1") - data.GetVoltage("2");
				double vr2 = data.GetVoltage("2") - data.GetVoltage("GND");
				double vr3 = data.GetVoltage("2") - data.GetVoltage("GND");
				Debug.Log(r1.GetCurrent(ckt));
				Debug.Log("Vr1 is: " + vr1);
				Debug.Log("Vr2 is: " + vr2);
				Debug.Log("Vr3 is: " + vr3);
			}
			};
		ckt.Simulate(dc);
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
}
