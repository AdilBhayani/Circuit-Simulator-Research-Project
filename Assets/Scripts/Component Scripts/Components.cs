using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.IO;
using System;
using SpiceSharp.Circuits;

public class Components : MonoBehaviour {
	public GameObject Node;
	public GameObject WireHorizontal;
	public GameObject WireVertical;
	public GameObject Resistor;
	public Transform circuitPanel;
	private static int selectedComponentCount;
	private static bool paused;
	private List <GameObject> componentsList;
	private List <GameObject> resistorList;
	private List <GameObject> nodeList;
	private List <GameObject> wireList;
	private Circuit ckt;
	private Resistor[] spiceResistorArray;
	private Resistor r1;
	private Resistor r2;
	Dictionary<string,string[]> lineDictionary = new Dictionary<string,string[]>();
	private int verticalGridSize = 5;
	private int horizontalGridSize = 9;
	private int numberOfNodes = 0;
	private int numberOfResistors = 0;
	private int numberOfWires = 0;
	private bool[,] rendered;
	private bool[,] connected;
	private float wireResistance = 0.000000000001f;
	private int componentCount = 0;

	// Use this for initialization
	void Start () {
		componentCount = 0;
		rendered = new bool[verticalGridSize,horizontalGridSize];
		connected = new bool[verticalGridSize, horizontalGridSize];
		selectedComponentCount = 0;
		paused = false;

		componentsList = new List <GameObject> ();
		wireList = new List <GameObject> ();
		nodeList = new List <GameObject> ();
		resistorList = new List <GameObject> ();

		spiceResistorArray = new SpiceSharp.Components.Resistor[verticalGridSize*horizontalGridSize];

		LoadCircuit ();
		RenderCircuit ();

		ConnectCircuit ();
		PrintComponents ();

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

	private void createNode(string ID, float anchorX, float anchorY, bool first, bool last, string connectionTop, string connectionRight, string connectionDown, string connectionLeft, string location){
		GameObject newNode = (GameObject)Instantiate (Node, new Vector3 (0, 0, 0), Quaternion.identity);
		newNode = setPosition (newNode, ID, anchorX, anchorX, anchorY, anchorY);
		newNode.GetComponent<NodeScript> ().setID (ID);
		newNode.GetComponent<NodeScript> ().setLocation (location);
		if (first) {
			newNode.tag = "firstNode";
		} else if (last) {
			newNode.tag = "lastNode";
		}
		componentsList.Add (newNode);
		nodeList.Add (newNode);
		componentCount++;
	}

	private void createResistor(string ID, float anchorX, float anchorY, float value, string location){
		GameObject newResistor = (GameObject)Instantiate (Resistor, new Vector3 (0, 0, 0), Quaternion.identity);
		newResistor = setPosition(newResistor, ID, anchorX, anchorX, anchorY, anchorY);
		newResistor.GetComponent<ResistorScript> ().setID (ID);
		newResistor.GetComponent<ResistorScript> ().setValue (value);
		newResistor.GetComponent<ResistorScript> ().setLocation (location);
		componentsList.Add (newResistor);
		resistorList.Add (newResistor);
		componentCount++;
		ResistorScript.increaseResistorCount ();
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
		wireList.Add (newWire);
		componentCount++;
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

	private void ConnectCircuit(){
		Debug.Log ("ConnectCircuit");
		foreach (KeyValuePair<string, string[]> kvp in lineDictionary) {
			switch (kvp.Key) {
			case "A":
				ConnectWires (kvp, 0);
				break;
			case "B":
				ConnectWires (kvp, 1);
				break;
			case "C":
				ConnectWires (kvp, 2);
				break;
			case "D":
				ConnectWires (kvp, 3);
				break;
			case "E":
				ConnectWires (kvp, 4);
				break;
			}
		}
	}

	private void ConnectWires(KeyValuePair<string, string[]> kvp, int row){
		string[] lineComponents = kvp.Value;
		for (int x = 0; x < lineComponents.Length; x++) {
			switch (lineComponents [x]) {
			case "Wh":
				if (connected [row, x] == false) {
					string location1 = "";
					string location2 = "";
					if (x >= 0) {
						location1 = convertIndexToLetter (row) + (x - 1).ToString ();
					}
					int newX = x + 1;
					while (lineComponents [newX] == "Wh") {
						connected [row, newX] = true;
						newX++;
					}
					location2 = convertIndexToLetter (row) + (newX).ToString ();
					makeHorizontalConnections (location1, location2, true);
					connected [row, x] = true;
				}
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
				string location = convertIndexToLetter (row) + x.ToString ();
				createNode ("N" + numberOfNodes.ToString (), x * 0.9f / horizontalGridSize + 0.05f, verticalHeight, firstNode, lastNode, null, null, null, null, location);
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
					string letterString = convertIndexToLetter (newRow);
					string[] newLineComponents = lineDictionary [letterString];
					while (newLineComponents [x] == "Wv") {
						rendered [newRow, x] = true;
						char letterChar = (char)(newRow + 65);
						string letter = letterChar.ToString ();
						newRow++;
						newLineComponents = lineDictionary [letter];
					}
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
					string resistorLocation = convertIndexToLetter (row) + x.ToString ();
					Debug.Log (resistance.ToString());	
					Debug.Log (resistorLocation);
					createResistor ("R" + numberOfResistors.ToString (), x * 0.9f / horizontalGridSize + 0.05f, verticalHeight,resistance, resistorLocation);
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
				double vr0 = Math.Abs(data.GetVoltage(spiceResistorArray[0].GetNode(0)) - data.GetVoltage(spiceResistorArray[0].GetNode(1)));
				double vr1 = Math.Abs(data.GetVoltage(r1.GetNode(0)) - data.GetVoltage(r1.GetNode(1)));
				double vr2 = Math.Abs(data.GetVoltage(r2.GetNode(0)) - data.GetVoltage(r2.GetNode(1)));
				//Debug.Log(r1.GetCurrent(ckt));
				Debug.Log("Vr1 is: " + vr0);
				Debug.Log("Vr2 is: " + vr1);
				Debug.Log("Vr3 is: " + vr2);
			}
		};
		ckt.Simulate(dc);
	}

	private void PrintComponents(){
		Debug.Log ("Printing resistors");
		foreach (GameObject resistorObject in resistorList){
			ResistorScript resistorObjectScript = resistorObject.GetComponent<ResistorScript> ();
			string resistorObjectID = resistorObjectScript.getID ();
			string resistorObjectLocation = resistorObjectScript.getLocation ();
			string resistorObjectLeft = resistorObjectScript.getIDLeft ();
			string resistorObjectRight = resistorObjectScript.getIDRight ();
			Debug.Log ("ID: " + resistorObjectID + ", Location: " + resistorObjectLocation + ", Left: " + resistorObjectLeft + ", Right: " + resistorObjectRight);
		}

		Debug.Log ("Printing nodes");
		foreach (GameObject nodeObject in nodeList) {
			NodeScript nodeObjectScript = nodeObject.GetComponent<NodeScript> ();
			string nodeObjectID = nodeObjectScript.getID ();
			string nodeObjectLocation = nodeObjectScript.getLocation ();
			string nodeObjectLeft = nodeObjectScript.getIDLeft ();
			string nodeObjectRight = nodeObjectScript.getIDRight ();
			string nodeObjectUp = nodeObjectScript.getIDUp ();
			string nodeObjectDown = nodeObjectScript.getIDDown ();
			Debug.Log ("ID: " + nodeObjectID + ", Location: " + nodeObjectLocation + ", Left: " + nodeObjectLeft + ", Right: " + nodeObjectRight + ", Up: " + nodeObjectUp + ", Down: " + nodeObjectDown);
		}
	}

	private void BuildCircuit(){
		// Build the circuit
		ckt = new Circuit();
		ckt.Objects.Add( 
			new Voltagesource("V1", "A0", "GND", 1.0),
			new Resistor ("Vx5", "E0", "GND", wireResistance)
		);
		foreach (GameObject resistorObject in resistorList) {
			if (resistorObject != null) {
				string ID = resistorObject.GetComponent<ResistorScript> ().getID ();
				float value = resistorObject.GetComponent<ResistorScript> ().getValue ();
				Debug.Log (value.ToString ());
			}
		}

		ckt.Objects.Add(
			spiceResistorArray[0] = new Resistor("R0", "A0", "A8", 12),
			new Resistor("Vx1", "A8", "C8", wireResistance),
			r1 = new Resistor("R1", "C2", "C8", 16),
			new Resistor("Vx2", "C8", "E8", wireResistance),
			new Resistor("Vx3", "C2", "E2", wireResistance),
			r2 = new Resistor("R2", "E2", "E8", 15),
			new Resistor ("Vx4", "E0","E2", wireResistance)
		);
	}

	private string convertIndexToLetter(int number){
		char letterCharacter = (char)(number + 65);
		return letterCharacter.ToString ();
	}

	private void makeHorizontalConnections(string location1, string location2,bool location1Lower){
		Debug.Log("Location1 is: " + location1);
		Debug.Log("Location2 is: " + location2);
		int foundObjectCount = 0;
		string object1Location = "";
		string object2Location = "";
		bool object1Resistor = false;
		bool object2Resistor = false;
		GameObject object1 = null;
		GameObject object2 = null;
		foreach (GameObject resistorObject in resistorList){
			string resistorObjectLocation = resistorObject.GetComponent<ResistorScript> ().getLocation ();
			if (resistorObjectLocation == location1 || resistorObjectLocation == location2) {
				if (object1 == null) {
					object1 = resistorObject;
					object1Resistor = true;
					object1Location = object1.GetComponent<ResistorScript> ().getLocation ();
				} else if (object2 == null) {
					object2 = resistorObject;
					object2Resistor = true;
					object2Location = object2.GetComponent<ResistorScript> ().getLocation ();
				}
				foundObjectCount++;
			}
		}
		foreach (GameObject nodeObject in nodeList) {
			string nodeObjectLocation = nodeObject.GetComponent<NodeScript> ().getLocation ();
			if (nodeObjectLocation == location1 || nodeObjectLocation == location2) {
				if (object1 == null) {
					object1 = nodeObject;
					object1Location = object1.GetComponent<NodeScript> ().getLocation ();
				} else if (object2 == null) {
					object2 = nodeObject;
					object2Location = object2.GetComponent<NodeScript> ().getLocation ();
				}
				foundObjectCount++;
			}
		}
		if (foundObjectCount < 2) {
			Debug.Log ("Missing " + (2 - foundObjectCount).ToString () + " object(s) while trying to connect locations: " + location1 + " " + location2);
		} else {
			Debug.Log ("object1Location: " + object1Location);
			Debug.Log ("object2Location: " + object2Location);
			Debug.Log ("object1Resistor: " + object1Resistor);
			Debug.Log ("object2Resistor: " + object2Resistor);

			if (object1Resistor) {
				string resistorLocation = object1.GetComponent<ResistorScript> ().getLocation ();
				if (resistorLocation == location1) {
					if (location1Lower) {
						if (object2Resistor) {
							object1.GetComponent<ResistorScript> ().setIDRight (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDLeft (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("A");
						} else {
							object1.GetComponent<ResistorScript> ().setIDRight (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDLeft (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("B");
						}
					} else {
						if (object2Resistor) {
							object1.GetComponent<ResistorScript> ().setIDLeft (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDRight (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("C");
						} else {
							object1.GetComponent<ResistorScript> ().setIDLeft (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDRight (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("D");
						}
					}
				} else {
					if (location1Lower) {
						if (object2Resistor) {
							object1.GetComponent<ResistorScript> ().setIDLeft (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDRight(object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("E");
						} else {
							object1.GetComponent<ResistorScript> ().setIDLeft (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDRight (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("F");
						}
					} else {
						if (object2Resistor) {
							object1.GetComponent<ResistorScript> ().setIDRight (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDLeft (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("G");
						} else {
							object1.GetComponent<ResistorScript> ().setIDRight (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDLeft (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("H");
						}
					}
				}
			} else {
				string nodeLocation = object1.GetComponent<NodeScript> ().getLocation ();
				if (nodeLocation == location1) {
					if (location1Lower) {
						if (object2Resistor) {
							object1.GetComponent<NodeScript> ().setIDRight (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDLeft (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("I");
						} else {
							object1.GetComponent<NodeScript> ().setIDRight (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDLeft (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("J");
						}
					} else {
						if (object2Resistor) {
							object1.GetComponent<NodeScript> ().setIDLeft (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDRight (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("K");
						} else {
							object1.GetComponent<NodeScript> ().setIDLeft (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDRight(object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("L");
						}
					}
				}else {
					if (location1Lower) {
						if (object2Resistor) {
							object1.GetComponent<NodeScript> ().setIDLeft (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDRight(object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("M");
						} else {
							object1.GetComponent<NodeScript> ().setIDLeft (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDRight (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("N");
						}
					} else {
						if (object2Resistor) {
							object1.GetComponent<NodeScript> ().setIDRight (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDLeft(object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("O");
						} else {
							object1.GetComponent<NodeScript> ().setIDRight (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDLeft (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("P");
						}
					}
				}
			}

		}
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