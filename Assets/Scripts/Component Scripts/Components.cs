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
	private float wireResistance = 0.00000001f;
	private int componentCount = 0;
	private int spiceResistorArrayCount = 0;
	private SimulationData newData;

	// Use this for initialization
	void Start () {
		paused = false;

		ClearAll ();
		LoadCircuit ();

		RenderCircuit ();
		ConnectCircuit ();
		//PrintComponents ();

		BuildCircuit ();
		SimulateCircuit ();
	}

	private void ClearAll(){
		if (componentsList != null) {
			foreach (GameObject component in componentsList) {
				Destroy (component);
			}
		}
		numberOfNodes = 0;
		numberOfResistors = 0;
		numberOfWires = 0;
		componentCount = 0;
		selectedComponentCount = 0;
		spiceResistorArrayCount = 0;
		rendered = new bool[verticalGridSize,horizontalGridSize];
		connected = new bool[verticalGridSize, horizontalGridSize];
		componentsList = new List <GameObject> ();
		wireList = new List <GameObject> ();
		nodeList = new List <GameObject> ();
		resistorList = new List <GameObject> ();
		spiceResistorArray = new SpiceSharp.Components.Resistor[verticalGridSize*horizontalGridSize];
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
			string firstID = ResistorScript.selectedList [0];
			string secondID = ResistorScript.selectedList [1];
			if (string.Compare (firstID, secondID) > 0) {
				string temp = firstID;
				firstID = secondID;
				secondID = temp;
			}

			Resistor spiceResistor1 = GetSpiceResistorByID (firstID);
			Resistor spiceResistor2 = GetSpiceResistorByID (secondID);

			double current1 = spiceResistor1.GetCurrent (ckt);
			double current2 = spiceResistor2.GetCurrent (ckt);
			double value1 = spiceResistor1.Ask ("resistance");
			double value2 = spiceResistor2.Ask ("resistance");

			if (Math.Abs (current1 - current2) < 0.000001f) {
				Debug.Log ("In series");
				UpdateHistory.appendToHistory ("Series Transform: \nR(" + value1.ToString() + ") & R(" + value2.ToString() +")\n");
				GameObject secondResistorObject = GetResistorByID (secondID);
				string location2 = secondResistorObject.GetComponent<ResistorScript> ().getLocation ();
				string node0 = spiceResistorArray [Int32.Parse(secondID.Substring (1,secondID.Length-1))].GetNode (0);
				string node1 = spiceResistorArray [Int32.Parse(secondID.Substring (1,secondID.Length-1))].GetNode (1);
				if (string.Compare (node0.Substring(0,1), node1.Substring(0,1)) == 0) {
					Debug.Log ("Horizontally connected");
					string[] line = lineDictionary[node0.Substring (0, 1)];
					Debug.Log (Int32.Parse (location2.Substring (1, location2.Length - 1)));
					line [Int32.Parse (location2.Substring (1, location2.Length - 1))] = "Wh";
					Debug.Log(string.Format ("Key = {0}, Value = {1}", "A", string.Join(".", line)));
					lineDictionary [node0.Substring (0, 1)] = line;
					string printString = "";
					foreach (KeyValuePair<string, string[]> kvp in lineDictionary){
						printString += string.Format ("Key = {0}, Value = {1}", kvp.Key, string.Join(".", kvp.Value));
						printString += "\n";
					}
					Debug.Log (printString);
					ClearAll ();
					RenderCircuit ();
					ConnectCircuit ();
					BuildCircuit ();
					PrintComponents ();
					SimulateCircuit ();
				}
			} else {
				Debug.Log ("Not in series");
				UpdateFeedback.updateMessage ("Components are not in series");
			}

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
						location1 = ConvertIndexToLetter (row) + (x - 1).ToString ();
					}
					int newX = x + 1;
					while (lineComponents [newX] == "Wh") {
						connected [row, newX] = true;
						newX++;
					}
					location2 = ConvertIndexToLetter (row) + (newX).ToString ();
					MakeHorizontalConnections (location1, location2, true);
					connected [row, x] = true;
				}
				break;
			case "Wv":
				if (connected [row, x] == false) {
					string location1 = "";
					string location2 = "";
					if (row > 0) {
						location1 = ConvertIndexToLetter (row - 1) + x.ToString ();
					}
					int newRow = row + 1;
					string letterString = ConvertIndexToLetter (newRow);
					string[] newLineComponents = lineDictionary [letterString];
					while (newLineComponents [x] == "Wv") {
						connected [newRow, x] = true;
						string letter = ConvertIndexToLetter (newRow);
						newRow++;
						newLineComponents = lineDictionary [letter];
					}
					location2 = ConvertIndexToLetter (newRow) + x.ToString ();
					MakeVerticalConnections (location1, location2, true);
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
				string location = ConvertIndexToLetter (row) + x.ToString ();
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
					string letterString = ConvertIndexToLetter (newRow);
					string[] newLineComponents = lineDictionary [letterString];
					while (newLineComponents [x] == "Wv") {
						rendered [newRow, x] = true;
						string letter = ConvertIndexToLetter (newRow);
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
					string resistorLocation = ConvertIndexToLetter (row) + x.ToString ();
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
				double vr1 = Math.Abs(data.GetVoltage(spiceResistorArray[1].GetNode(0)) - data.GetVoltage(spiceResistorArray[1].GetNode(1)));
				double vr2 = Math.Abs(data.GetVoltage(spiceResistorArray[2].GetNode(0)) - data.GetVoltage(spiceResistorArray[2].GetNode(1)));
				//Debug.Log(r1.GetCurrent(ckt));
				Debug.Log("Vr1 is: " + vr0);
				Debug.Log("Vr2 is: " + vr1);
				Debug.Log("Vr3 is: " + vr2);
				newData = data;
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
		Debug.Log ("Build circuit");
		// Build the circuit
		ckt = new Circuit();
		ckt.Objects.Add( 
			new Voltagesource("V1", "A0", "GND", 1.0),
			new Resistor ("Vx0", "E0", "GND", wireResistance)
		);

		foreach (GameObject resistorObject in resistorList) {
			if (resistorObject != null) {
				ResistorScript resistorObjectScript = resistorObject.GetComponent<ResistorScript> ();
				string resistorObjectID = resistorObjectScript.getID ();
				string resistorObjectLocation = resistorObjectScript.getLocation ();
				string resistorObjectLeft = resistorObjectScript.getIDLeft ();
				string resistorObjectRight = resistorObjectScript.getIDRight ();
				float resistorvalue = resistorObjectScript.getValue ();

				string objectLeftLocation = null;
				string objectRightLocation = null;

				if (resistorObjectLeft.StartsWith("R")){
					GameObject leftResistorObject =  GetResistorByID (resistorObjectLeft);
					objectLeftLocation = leftResistorObject.GetComponent<ResistorScript> ().getLocation ();
				}else{
					GameObject leftNodeObject =  GetNodeByID (resistorObjectLeft);
					objectLeftLocation = leftNodeObject.GetComponent<NodeScript> ().getLocation ();
				}

				if (resistorObjectRight.StartsWith("R")){
					GameObject rightResistorObject =  GetResistorByID (resistorObjectRight);
					objectRightLocation = rightResistorObject.GetComponent<ResistorScript> ().getLocation ();
				}else{
					GameObject rightNodeObject =  GetNodeByID (resistorObjectRight);
					objectRightLocation = rightNodeObject.GetComponent<NodeScript> ().getLocation ();
				}
				 
				Debug.Log ("objectLeftLocation is: " + objectLeftLocation);
				Debug.Log ("objectRightLocation is: " + objectRightLocation);
				Debug.Log ("ID: " + resistorObjectID + ", Left: " + resistorObjectLeft + ", Right: " + resistorObjectRight + ", Value: " + resistorvalue);
				ckt.Objects.Add (spiceResistorArray [spiceResistorArrayCount] = new Resistor (resistorObjectID, objectLeftLocation, objectRightLocation, resistorvalue));
				spiceResistorArrayCount++;
			}
		}

		int wireCounter = 1;
		foreach (GameObject nodeObject in nodeList) {
			if (nodeObject != null) {
				NodeScript nodeObjectScript = nodeObject.GetComponent<NodeScript> ();
				string nodeObjectID = nodeObjectScript.getID ();
				string nodeObjectLocation = nodeObjectScript.getLocation ();
				string nodeObjectLeft = nodeObjectScript.getIDLeft ();
				string nodeObjectRight = nodeObjectScript.getIDRight ();
				string nodeObjectUp = nodeObjectScript.getIDUp ();
				string nodeObjectDown = nodeObjectScript.getIDUp ();

				if (!string.IsNullOrEmpty(nodeObjectLeft) && nodeObjectLeft.StartsWith("N")){
					GameObject leftNodeObject =  GetNodeByID (nodeObjectLeft);
					string objectLeftLocation = leftNodeObject.GetComponent<NodeScript> ().getLocation ();
					ckt.Objects.Add(new Resistor ("Vx"+wireCounter.ToString(),nodeObjectLocation,objectLeftLocation, wireResistance));
					wireCounter++;
				}
				if (!string.IsNullOrEmpty(nodeObjectRight) && nodeObjectRight.StartsWith("N")){
					GameObject rightNodeObject =  GetNodeByID (nodeObjectRight);
					string objectRightLocation = rightNodeObject.GetComponent<NodeScript> ().getLocation ();
					ckt.Objects.Add(new Resistor ("Vx"+wireCounter.ToString(),nodeObjectLocation,objectRightLocation, wireResistance));
					wireCounter++;
				}

				if (!string.IsNullOrEmpty(nodeObjectUp) && nodeObjectUp.StartsWith("N")){
					GameObject upNodeObject =  GetNodeByID (nodeObjectUp);
					string objectUpLocation = upNodeObject.GetComponent<NodeScript> ().getLocation ();
					ckt.Objects.Add(new Resistor ("Vx"+wireCounter.ToString(),nodeObjectLocation,objectUpLocation, wireResistance));
					wireCounter++;
				}
				if (!string.IsNullOrEmpty(nodeObjectDown) && nodeObjectDown.StartsWith("N")){
					GameObject downNodeObject =  GetNodeByID (nodeObjectDown);
					string objectDownLocation = downNodeObject.GetComponent<NodeScript> ().getLocation ();
					ckt.Objects.Add(new Resistor ("Vx"+wireCounter.ToString(),nodeObjectLocation,objectDownLocation, wireResistance));
					wireCounter++;
				}
			}
		}
	}

	private string ConvertIndexToLetter(int number){
		char letterCharacter = (char)(number + 65);
		return letterCharacter.ToString ();
	}

	private void MakeVerticalConnections(string location1, string location2, bool location1Lower){
		Debug.Log ("Location 1 vertical is: " + location1);
		Debug.Log ("Location 2 vertical is: " + location2);
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
			if (object1Resistor) {
				string resistorLocation = object1.GetComponent<ResistorScript> ().getLocation ();
				if (resistorLocation == location1) {
					if (location1Lower) {
						if (object2Resistor) {
							object1.GetComponent<ResistorScript> ().setIDDown (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDUp (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("A");
						} else {
							object1.GetComponent<ResistorScript> ().setIDDown (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDUp (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("B");
						}
					} else {
						if (object2Resistor) {
							object1.GetComponent<ResistorScript> ().setIDUp (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDDown (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("C");
						} else {
							object1.GetComponent<ResistorScript> ().setIDUp (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDDown (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("D");
						}
					}
				} else {
					if (location1Lower) {
						if (object2Resistor) {
							object1.GetComponent<ResistorScript> ().setIDUp (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDDown(object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("E");
						} else {
							object1.GetComponent<ResistorScript> ().setIDUp (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDDown (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("F");
						}
					} else {
						if (object2Resistor) {
							object1.GetComponent<ResistorScript> ().setIDDown (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDUp (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("G");
						} else {
							object1.GetComponent<ResistorScript> ().setIDDown (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDUp (object1.GetComponent<ResistorScript> ().getID ());
							Debug.Log ("H");
						}
					}
				}
			} else {
				string nodeLocation = object1.GetComponent<NodeScript> ().getLocation ();
				if (nodeLocation == location1) {
					if (location1Lower) {
						if (object2Resistor) {
							object1.GetComponent<NodeScript> ().setIDDown (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDUp (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("I");
						} else {
							object1.GetComponent<NodeScript> ().setIDDown (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDUp (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("J");
						}
					} else {
						if (object2Resistor) {
							object1.GetComponent<NodeScript> ().setIDUp (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDDown (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("K");
						} else {
							object1.GetComponent<NodeScript> ().setIDUp (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDDown (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("L");
						}
					}
				}else {
					if (location1Lower) {
						if (object2Resistor) {
							object1.GetComponent<NodeScript> ().setIDUp (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDDown(object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("M");
						} else {
							object1.GetComponent<NodeScript> ().setIDUp (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDDown (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("N");
						}
					} else {
						if (object2Resistor) {
							object1.GetComponent<NodeScript> ().setIDDown (object2.GetComponent<ResistorScript> ().getID ());
							object2.GetComponent<ResistorScript> ().setIDUp(object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("O");
						} else {
							object1.GetComponent<NodeScript> ().setIDDown (object2.GetComponent<NodeScript> ().getID ());
							object2.GetComponent<NodeScript> ().setIDUp (object1.GetComponent<NodeScript> ().getID ());
							Debug.Log ("P");
						}
					}
				}
			}
		}
	}

	private void MakeHorizontalConnections(string location1, string location2,bool location1Lower){
		//Debug.Log("Location1 is: " + location1);
		//Debug.Log("Location2 is: " + location2);
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
			//Debug.Log ("object1Location: " + object1Location);
			//Debug.Log ("object2Location: " + object2Location);
			//Debug.Log ("object1Resistor: " + object1Resistor);
			//Debug.Log ("object2Resistor: " + object2Resistor);

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

	private GameObject GetResistorByID(string ID){
		foreach (GameObject resistorObject in resistorList) {
			ResistorScript resistorObjectScript = resistorObject.GetComponent<ResistorScript> ();
			if (resistorObjectScript.getID () == ID) {
				return resistorObject;
			}
		}
		Debug.Log ("Resistor with that ID could not be found");
		return null;
	}

	private GameObject GetNodeByID(string ID){
		foreach (GameObject nodeObject in nodeList) {
			NodeScript nodeObjectScript = nodeObject.GetComponent<NodeScript> ();
			if (nodeObjectScript.getID () == ID) {
				return nodeObject;
			}
		}
		Debug.Log ("Node with that ID could not be found");
		return null;
	}

	private Resistor GetSpiceResistorByID(string ID){
		return spiceResistorArray[Int32.Parse(ID.Substring(1))];
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
*/