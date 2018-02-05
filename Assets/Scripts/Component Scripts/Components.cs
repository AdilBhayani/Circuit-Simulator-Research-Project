using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.IO;
using System;

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
	private Resistor r2;
	private Resistor r3;
	Dictionary<string,string[]> lineDictionary = new Dictionary<string,string[]>();
	private int verticalGridSize = 5;
	private int horizontalGridSize = 9;
	private int numberOfNodes = 0;
	private int numberOfResistors = 0;
	private int numberOfWires = 0;
	private bool[,] rendered;
	private float wireResistance = 0.000000000001f;

	// Use this for initialization
	void Start () {
		rendered = new bool[verticalGridSize,horizontalGridSize];
		selectedComponentCount = 0;
		paused = false;
		componentsList = new List <GameObject> ();
		LoadCircuit ();
		RenderCircuit ();

		BuildCircuit ();
		SimulateCircuit ();
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
					if (componentObject.GetComponent<ResistorScript> ().getID () == "R1") {
						Destroy (componentObject);
						selectedComponentCount = 0;
					}
					if (componentObject.GetComponent<ResistorScript> ().getID () == "R0") {
						componentObject.GetComponent<ResistorScript> ().setValue (25.0f);
					}
				}
			}
			UpdateHistory.appendToHistory ("Series Transform: \nR(10) & R(15)");
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
		reader.Close ();
	}

	private void RenderCircuit(){
		Debug.Log ("Render Circuit");
		foreach (KeyValuePair<string, string[]> kvp in lineDictionary){
			switch (kvp.Key) {
			case "A":
				Debug.Log ("Its A");
				drawCircuit (kvp, 0.85f,0);
				break;
			case "B":
				Debug.Log ("Its B");
				drawCircuit (kvp, 0.85f - 0.8f / (verticalGridSize - 1),1);
				break;
			case "C":
				Debug.Log ("Its C");
				drawCircuit (kvp, 0.85f - 0.8f * 2 / (verticalGridSize - 1),2);
				break;
			case "D":
				Debug.Log ("Its D");
				drawCircuit (kvp, 0.85f - 0.8f * 3 / (verticalGridSize - 1),3);
				break;
			case "E":
				Debug.Log ("Its E");
				drawCircuit (kvp, 0.85f - 0.8f * 4 / (verticalGridSize - 1),4);
				break;
			}
		}
	}

	private void drawCircuit(KeyValuePair<string, string[]> kvp, float verticalHeight, int row){
		string[] lineComponents = kvp.Value;
		for(int x = 0; x < lineComponents.Length; x++){
			switch (lineComponents [x]) {
			case "x":
				rendered [row, x] = true;
				break;
			case "N":
				Debug.Log ("Rendering Node");
				bool firstNode = false;
				bool lastNode = false;
				if (x == 0 && kvp.Key == "A") {
					firstNode = true;
				}
				if (x == 0 && kvp.Key == "E") {
					lastNode = true;
				}
				createNode ("N" + numberOfNodes.ToString (), x * 0.9f / horizontalGridSize + 0.05f, verticalHeight, firstNode, lastNode, null, null, null, null);
				numberOfNodes++;
				rendered [row, x] = true;
				break;
			case "Wh":
				if (rendered [row, x] == false) {
					Debug.Log ("Rendering horizontal wire");
					int newX = x + 1;
					while (lineComponents [newX] == "Wh") {
						rendered [row, newX] = true;
						newX++;
					}
					float minX = x * 0.9f / horizontalGridSize - 0.05f;
					float maxX = newX * 0.9f / horizontalGridSize + 0.05f;
					createWire ("W" + numberOfWires.ToString (), minX, maxX, verticalHeight, verticalHeight, (maxX - minX) * 10f, false);
					numberOfWires++;
					rendered [row, x] = true;
				}
				break;
			case "Wv":
				if (rendered [row, x] == false) {
					Debug.Log ("Rendering vertical wire");
					int newRow = row + 1;
					char letterCharacter = (char)(newRow + 65);
					string letterString = letterCharacter.ToString ();
					string[] newLineComponents = lineDictionary [letterString];
					while (newLineComponents [x] == "Wv") {
						rendered [newRow, x] = true;
						char letterChar = (char)(newRow + 65);
						string letter = letterChar.ToString ();
						newRow++;
						newLineComponents = lineDictionary [letter];
					}
					newRow = newRow;
					float minX = x * 0.9f / horizontalGridSize + 0.05f;
					float maxY = verticalHeight + 1.0f / verticalGridSize;
					Debug.Log ("NewRow is: " + newRow);
					float minY = (verticalGridSize - newRow - 1) * 0.2f + 0.05f;
					Debug.Log ("maxY is: " + maxY);
					Debug.Log ("minY is: " + minY);
					createWire ("W" + numberOfWires.ToString (), minX, minX, minY, maxY, (maxY - minY) * 10f, true);
					numberOfWires++;
					rendered [row, x] = true;
				}
				break;
			default:
				Debug.Log ("Rendering Resistor");
				if (lineComponents[x].StartsWith("Rh") || lineComponents[x].StartsWith("Rv") ){
					string resistanceString = lineComponents [x].Substring (2);
					float resistance = float.Parse(resistanceString, System.Globalization.CultureInfo.InvariantCulture);
					Debug.Log (resistance.ToString());	
					createResistor ("R" + numberOfResistors.ToString (), x * 0.9f / horizontalGridSize + 0.05f, verticalHeight,resistance);
					numberOfResistors++;
				}
				break;
			}
		}
	}

	private void SimulateCircuit(){
		// Simulation
		DC dc = new DC("Dc 1");
		dc.Sweeps.Add(new DC.Sweep("V1", 0, 1, 1));
		dc.OnExportSimulationData += (object sender, SimulationData data) =>
		{
			if (dc.Sweeps[0].CurrentValue == 1){
				double vr1 = Math.Abs(data.GetVoltage(r1.GetNode(0)) - data.GetVoltage(r1.GetNode(1)));
				double vr2 = Math.Abs(data.GetVoltage(r2.GetNode(0)) - data.GetVoltage(r2.GetNode(1)));
				double vr3 = Math.Abs(data.GetVoltage(r3.GetNode(0)) - data.GetVoltage(r3.GetNode(1)));
				//Debug.Log(r1.GetCurrent(ckt));
				Debug.Log("Vr1 is: " + vr1);
				Debug.Log("Vr2 is: " + vr2);
				Debug.Log("Vr3 is: " + vr3);
			}
		};
		ckt.Simulate(dc);
	}

	private void BuildCircuit(){
		// Build the circuit
		ckt = new Circuit();
		ckt.Objects.Add( 
			new Voltagesource("V1", "A0", "GND", 1.0),
			r1 = new Resistor("R1", "A0", "A8", 12),
			new Resistor("Vx1", "A8", "C8", wireResistance),
			r2 = new Resistor("R2", "C2", "C8", 16),
			new Resistor("Vx2", "C8", "E8", wireResistance),
			new Resistor("Vx3", "C2", "E2", wireResistance),
			r3 = new Resistor("R3", "E2", "E8", 15),
			new Resistor ("Vx4", "E0","E2", wireResistance),
			new Resistor ("Vx5", "E0", "GND", wireResistance)
		);
	}
}

/*
 Debug.Log ("Here");
string printString = "";
foreach (KeyValuePair<string, string[]> kvp in lineDictionary){
	printString += string.Format ("Key = {0}, Value = {1}", kvp.Key, string.Join(".", kvp.Value));
	printString += "\n";
}
Debug.Log (printString);
*/