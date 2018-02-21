using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.IO;
using System;
using SpiceSharp.Circuits;
using UnityEngine.UI;

public class Components : MonoBehaviour {
	public GameObject Node;
	public GameObject WireHorizontal;
	public GameObject WireVertical;
	public GameObject Resistor;
	public Transform circuitPanel;
	public Text stageTitleText;
	private static int selectedComponentCount;
	private static bool paused;
	private List <GameObject> componentsList;
	private List <GameObject> resistorList;
	private List <GameObject> nodeList;
	private List <GameObject> wireList;
	private Circuit ckt;
	private Resistor[] spiceResistorArray;
	Dictionary<string,string[]> lineDictionary = new Dictionary<string,string[]>();
	private int verticalGridSize = 0;
	private int horizontalGridSize = 0;
	private int numberOfNodes = 0;
	private int numberOfResistors = 0;
	private int numberOfWires = 0;
	private bool[,] rendered;
	private bool[,] connected;
	private float wireResistance = 0.00000001f;
	private int componentCount = 0;
	private int spiceResistorArrayCount = 0;
	private SimulationData newData;
	public static string currentStage = "stage1";
	public static int numberOfStages = 0;
	private bool gameFinished = false;

	// Use this for initialization
	void Start () {
		gameFinished = false;
		currentStage = "stage1";
		paused = false;

		LoadCircuit ();
		ClearAll ();

		//RemoveUnusedWires ();

		RenderCircuit ();
		ConnectCircuit ();

		BuildCircuit ();
		SimulateCircuit ();
		PrintComponents ();

	}

	public static void increaseStage(){
		int stageNumber = Int32.Parse(currentStage.Substring (5));
		if (stageNumber < numberOfStages) {
			EfxUpdater.playLevelSwitchSoundStatic ();
		} else {
			UpdateFeedback.UpdateMessage ("Final stage reached!");
		}

	}

	private void CheckChangeStage(){
		if (numberOfResistors == 1) {
			int stageNumber = Int32.Parse (currentStage.Substring (5));
			if (stageNumber < numberOfStages) {
				stageNumber++;
				currentStage = "stage" + stageNumber.ToString ();
				lineDictionary = new Dictionary<string,string[]> ();
				verticalGridSize = 0;
				horizontalGridSize = 0;
				ClearAll ();
				LoadCircuit ();
				ClearAll ();
				RenderCircuit ();
				ConnectCircuit ();
				BuildCircuit ();
				SimulateCircuit ();
				UpdateHistory.clearHistory ();
				EfxUpdater.playLevelSwitchSoundStatic ();
				ScoreManager.IncreaseScore ((int) (TimerUIUpdater.currentTime * 5) + 100);
				UpdateFeedback.UpdateMessage ("");
				TimerUIUpdater.IncreaseTime ();
			} else if (!gameFinished) {
				ScoreManager.IncreaseScore ((int) (TimerUIUpdater.currentTime * 5) + 100);
				UpdateFeedback.UpdateMessage ("Congrats you have passed all stages!!", true);
				TimerUIUpdater.StopTimer ();
				gameFinished = true;
			}
		}
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
		ResistorScript.selectedList.Clear ();
		numberOfStages = CircuitLoader.GetNumberOfStages ();
	}

	// Update is called once per frame
	void Update () {
		CheckChangeStage ();
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
		Debug.Log ("Node ID: " + ID + " anchorX: " + anchorX + " anchorY: " + anchorY);
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
		Debug.Log ("Wire ID is: " + ID + " xStart is " + xStart + " XEnd is: " + xEnd + " yStart is: " + yStart + " yEnd is: " + yEnd);
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
		if (selectedComponentCount == 2 && !paused) {
			string firstID = ResistorScript.selectedList [0];
			string secondID = ResistorScript.selectedList [1];
			if (string.Compare (firstID, secondID) > 0) {
				string temp = firstID;
				firstID = secondID;
				secondID = temp;
			}

			Resistor spiceResistor1 = GetSpiceResistorByID (firstID);
			Resistor spiceResistor2 = GetSpiceResistorByID (secondID);

			double current1 = Math.Abs(spiceResistor1.GetCurrent (ckt));
			double current2 = Math.Abs(spiceResistor2.GetCurrent (ckt));
			double value1 = spiceResistor1.Ask ("resistance");
			double value2 = spiceResistor2.Ask ("resistance");
			Debug.Log ("current1: " + current1 + " current2: " + current2);
			if (Math.Abs (current1 - current2) < 0.0000001f) {
				Debug.Log ("In series");
				if (numberOfResistors != 2) {
					ScoreManager.IncreaseScore (100);
				}
				UpdateHistory.appendToHistory ("Series Transform: \nR(" + String.Format("{0:0.00}",value1) + ") & R(" + String.Format("{0:0.00}",value2) +")\n");
				GameObject secondResistorObject = GetResistorByID (secondID);
				GameObject firstResistorObject = GetResistorByID (firstID);
				string location2 = secondResistorObject.GetComponent<ResistorScript> ().getLocation ();
				string location1 = firstResistorObject.GetComponent<ResistorScript> ().getLocation ();
				string secondNode0 = spiceResistorArray [Int32.Parse(secondID.Substring (1,secondID.Length-1))].GetNode (0);
				string secondNode1 = spiceResistorArray [Int32.Parse(secondID.Substring (1,secondID.Length-1))].GetNode (1);
				string firstNode0 = spiceResistorArray [Int32.Parse(firstID.Substring (1,firstID.Length-1))].GetNode (0);
				string firstNode1 = spiceResistorArray [Int32.Parse(firstID.Substring (1,firstID.Length-1))].GetNode (1);
				//If horizontally connected
				if (string.Compare (secondNode0.Substring (0, 1), secondNode1.Substring (0, 1)) == 0) {
					Debug.Log ("Second resistor horizontally connected");
					string[] secondLine = lineDictionary [location2.Substring (0, 1)];
					Debug.Log (Int32.Parse (location2.Substring (1, location2.Length - 1)));
					secondLine [Int32.Parse (location2.Substring (1, location2.Length - 1))] = "Wh";
					lineDictionary [secondNode0.Substring (0, 1)] = secondLine;

					string[] firstLine = lineDictionary [location1.Substring (0, 1)];
					if (string.Compare (firstNode0.Substring (0, 1), firstNode1.Substring (0, 1)) == 0) {
						firstLine [Int32.Parse (location1.Substring (1, location1.Length - 1))] = "Rh" + (value1 + value2).ToString ();
					} else {
						firstLine [Int32.Parse (location1.Substring (1, location1.Length - 1))] = "Rv" + (value1 + value2).ToString ();
					}
					lineDictionary [firstNode0.Substring (0, 1)] = firstLine;
					Debug.Log (string.Format ("Key = {0}, Value = {1}", "(Unknown key)", string.Join (".", secondLine)));
					string printString = "";
					foreach (KeyValuePair<string, string[]> kvp in lineDictionary) {
						printString += string.Format ("Key = {0}, Value = {1}", kvp.Key, string.Join (".", kvp.Value));
						printString += "\n";
					}
					Debug.Log (printString);
				} else {
					//Code for vertically connected resistors here.
				}
				ClearAll ();
				RenderCircuit ();
				ConnectCircuit ();
				BuildCircuit ();
				PrintComponents ();
				SimulateCircuit ();
			} else {
				Debug.Log ("Not in series");
				UpdateFeedback.UpdateMessage ("Components are not in series");
				LivesManager.DecreaseLives ();
			}

		} else {
			UpdateFeedback.UpdateMessage ("Select two components first");
		}
	}

	public void checkTransformParallel(){
		if (selectedComponentCount == 2 && !paused) {
			string firstID = ResistorScript.selectedList [0];
			string secondID = ResistorScript.selectedList [1];
			if (string.Compare (firstID, secondID) > 0) {
				string temp = firstID;
				firstID = secondID;
				secondID = temp;
			}

			Resistor spiceResistor1 = GetSpiceResistorByID (firstID);
			Resistor spiceResistor2 = GetSpiceResistorByID (secondID);

			double voltage1 = Math.Abs (newData.GetVoltage (spiceResistor1.GetNode (0)) - newData.GetVoltage (spiceResistor1.GetNode (1)));
			double voltage2 = Math.Abs (newData.GetVoltage (spiceResistor2.GetNode (0)) - newData.GetVoltage (spiceResistor2.GetNode (1)));
			double value1 = spiceResistor1.Ask ("resistance");
			double value2 = spiceResistor2.Ask ("resistance");
			Debug.Log ("Voltage1: " + voltage1 + " Voltage 2: " + voltage2);
			if (Math.Abs (voltage1 - voltage2) < 0.0000001f) {
				Debug.Log ("In parallel");
				if (numberOfResistors != 2) {
					ScoreManager.IncreaseScore (100);
				}
				UpdateHistory.appendToHistory ("Parallel Transform: \nR(" + String.Format("{0:0.00}",value1) + ") & R(" + String.Format("{0:0.00}",value2) +")\n");
				GameObject secondResistorObject = GetResistorByID (secondID);
				GameObject firstResistorObject = GetResistorByID (firstID);
				string location2 = secondResistorObject.GetComponent<ResistorScript> ().getLocation ();
				string location1 = firstResistorObject.GetComponent<ResistorScript> ().getLocation ();
				Debug.Log ("Location 1 is: " + location1);
				Debug.Log ("Location 2 is: " + location2);
				string secondNode0 = spiceResistorArray [Int32.Parse(secondID.Substring (1,secondID.Length-1))].GetNode (0);
				string secondNode1 = spiceResistorArray [Int32.Parse(secondID.Substring (1,secondID.Length-1))].GetNode (1);
				string firstNode0 = spiceResistorArray [Int32.Parse(firstID.Substring (1,firstID.Length-1))].GetNode (0);
				string firstNode1 = spiceResistorArray [Int32.Parse(firstID.Substring (1,firstID.Length-1))].GetNode (1);
				//If horizontally connected
				if (string.Compare (firstNode0.Substring (0, 1), firstNode1.Substring (0, 1)) == 0) {
					Debug.Log ("First resistor horizontally connected");
					string[] firstLine = lineDictionary [location1.Substring (0, 1)];
					firstLine [Int32.Parse (location1.Substring (1, location1.Length - 1))] = "x";
					//Delete all horizontal wires to the right till a node or resistor
					string horizontalWire = "Wh";
					int currentIndex = Int32.Parse(location1.Substring (1, location1.Length - 1));
					while (horizontalWire == "Wh" && currentIndex < (horizontalGridSize - 1)) {
						horizontalWire = firstLine [currentIndex + 1];
						if (horizontalWire == "Wh") {
							firstLine [currentIndex + 1] = "x";
						}
						currentIndex++;
					}
					horizontalWire = "Wh";
					currentIndex = Int32.Parse(location1.Substring (1, location1.Length - 1));
					while (horizontalWire == "Wh" && currentIndex > 0) {
						horizontalWire = firstLine [currentIndex - 1];
						if (horizontalWire == "Wh") {
							firstLine [currentIndex - 1] = "x";
						}
						currentIndex--;
					}
					lineDictionary [firstNode0.Substring (0, 1)] = firstLine;
					string[] secondLine = lineDictionary [location2.Substring (0, 1)];
					if (string.Compare (secondNode0.Substring (0, 1), secondNode1.Substring (0, 1)) == 0) {
						secondLine [Int32.Parse (location2.Substring (1, location2.Length - 1))] = "Rh" + ((value1*value2)/(value1 + value2)).ToString ();
					} else {
						secondLine [Int32.Parse (location2.Substring (1, location2.Length - 1))] = "Rv" + ((value1*value2)/(value1 + value2)).ToString ();
					}
					lineDictionary [secondNode0.Substring (0, 1)] = secondLine;
					string printString = "";
					foreach (KeyValuePair<string, string[]> kvp in lineDictionary) {
						printString += string.Format ("Key = {0}, Value = {1}", kvp.Key, string.Join (".", kvp.Value));
						printString += "\n";
					}
					Debug.Log (printString);

				} else {
					Debug.Log ("Vertically connected");
				}
				RemoveUnusedWires ();
				ClearAll ();
				RenderCircuit ();
				ConnectCircuit ();
				BuildCircuit ();
				PrintComponents ();
				SimulateCircuit ();
			} else {
				Debug.Log ("Not in parallel");
				UpdateFeedback.UpdateMessage ("Components are not in parallel");
				LivesManager.DecreaseLives ();
			}


		} else {
			UpdateFeedback.UpdateMessage ("Select two components first");
		}
	}

	private void LoadCircuit(){
		stageTitleText.text = "Stage " + currentStage.Substring (5);
		StreamReader reader = CircuitLoader.ReadString (currentStage);
		string circuitLine = reader.ReadLine ();
		int count = 65;
		while (circuitLine != null) {
			if (circuitLine.StartsWith("//")){
				circuitLine = reader.ReadLine ();
			}else{
				verticalGridSize++;
				Debug.Log (circuitLine);
				char letterChar = (char)count;
				string letter = letterChar.ToString ();
				count++;
				string[] circuitLineArray = circuitLine.Split (',');
				horizontalGridSize = circuitLineArray.Length;
				lineDictionary.Add(letter, circuitLineArray);
				circuitLine = reader.ReadLine ();
			}
		}
		reader.Close ();
		Debug.Log ("verticalGridSize: " + verticalGridSize + " horizontalGridSize: " + horizontalGridSize);
	}

	private void RenderCircuit(){
		Debug.Log ("Render Circuit");
		foreach (KeyValuePair<string, string[]> kvp in lineDictionary){
			drawCircuit(kvp,0.85f - 0.8f * ConvertLetterToIndex(kvp.Key) / (verticalGridSize - 1), ConvertLetterToIndex(kvp.Key));
		}
	}

	private void ConnectCircuit(){
		Debug.Log ("ConnectCircuit");
		foreach (KeyValuePair<string, string[]> kvp in lineDictionary) {
			ConnectWires (kvp, ConvertLetterToIndex (kvp.Key));
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
					bool wentInLoop = false;
					while (newLineComponents [x] == "Wv") {
						wentInLoop = true;
						connected [newRow, x] = true;
						string letter = ConvertIndexToLetter (newRow);
						newRow++;
						newLineComponents = lineDictionary [letter];
					}
					if (wentInLoop) {
						newRow--;
					}
					Debug.Log("newRow is: "+ newRow);
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
				createNode ("N" + numberOfNodes.ToString (), x * 0.8f / (horizontalGridSize - 1) + 0.05f, verticalHeight, firstNode, lastNode, null, null, null, null, location);
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
					float minX = (x - 1) * 0.8f / (horizontalGridSize - 1) + 0.05f;
					float maxX = newX * 0.8f / (horizontalGridSize - 1) + 0.05f;
					Debug.Log ("horizontalGridSize is: " + horizontalGridSize + " x is: " + x + " newX is: " + newX);
					createWire ("W" + numberOfWires.ToString (), minX, maxX, verticalHeight, verticalHeight, (maxX - minX) * 10.0f, false);
					numberOfWires++;
					rendered [row, x] = true;
				}
				break;
			case "Wv":
				if (rendered [row, x] == false) {
					Debug.Log ("Rendering vertical wire");
					Debug.Log ("verticalHeight is: " + verticalHeight);
					int newRow = row + 1;
					int rowCounter = 1;
					string letterString = ConvertIndexToLetter (newRow);
					string[] newLineComponents = lineDictionary [letterString];
					bool looped = false;
					while (newLineComponents [x] == "Wv") {
						looped = true;
						rendered [newRow, x] = true;
						string letter = ConvertIndexToLetter (newRow);
						newRow++;
						newLineComponents = lineDictionary [letter];
						rowCounter++;
					}
					if (looped)
						rowCounter--;
					Debug.Log ("rowCounter is: " + rowCounter);
					float minX = x * 0.8f / (horizontalGridSize - 1) + 0.05f;
					float maxY = verticalHeight + 0.8f / (verticalGridSize - 1);
					Debug.Log ("NewRow is: " + newRow);
					float minY = verticalHeight - rowCounter * 0.8f / (verticalGridSize - 1);
					Debug.Log ("maxY is: " + maxY);
					Debug.Log ("minY is: " + minY);
					createWire ("W" + numberOfWires.ToString (), minX, minX, minY, maxY, (maxY - minY) * 10.0f, true);
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
					createResistor ("R" + numberOfResistors.ToString (), x * 0.8f / (horizontalGridSize - 1) + 0.05f, verticalHeight,resistance, resistorLocation);
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
			new Resistor ("Vx0", ConvertIndexToLetter(verticalGridSize-1)+ "0", "GND", wireResistance)
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

	private int ConvertLetterToIndex(string letter){
		char letterCharacter = letter.ToCharArray () [0];
		Debug.Log (((int)letterCharacter) - 65);
		return ((int)letterCharacter) - 65;
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

	private void RemoveUnusedWires(){
		string[] unusedNodes = new string[horizontalGridSize * verticalGridSize];
		string[] unusedNodesDirection = new string[horizontalGridSize * verticalGridSize];
		int numberOfUnusedNodes = 0;
		int rowIndex = 0;
		int columnIndex = 0;
		Debug.Log ("RemoveUnusedWires");
		foreach (string[] line in lineDictionary.Values) {
			foreach (string locationValue in line) {
				if (locationValue == "N") {
					int nodeConnections = 0;
					string direction = "";
					if (rowIndex > 0) {
						if (CheckLocation (rowIndex-1, columnIndex)) {
							nodeConnections++;
							direction = "up";
						}
					}
					if (columnIndex < (horizontalGridSize - 1)) {
						if (CheckLocation (rowIndex, columnIndex + 1)) {
							nodeConnections++;
							direction = "right";
						}
					}
					if (rowIndex < (verticalGridSize - 1)) {
						if (CheckLocation (rowIndex + 1, columnIndex)) {
							nodeConnections++;
							direction = "down";
						}
					}
					if (columnIndex > 0) {
						if (CheckLocation(rowIndex,columnIndex - 1)){
							nodeConnections++;
							direction = "left";
						}
					}
					string nodeLocation = ConvertIndexToLetter (rowIndex) + columnIndex;
					string bottomLeftLocation = ConvertIndexToLetter (verticalGridSize - 1) + "0";
					//Debug.Log ("Bottom left location is: " + bottomLeftLocation);
					if (nodeConnections < 2 && nodeLocation != "A0" && nodeLocation != bottomLeftLocation) {
						Debug.Log ("Node at location: " +  nodeLocation + " has only one connection");
						unusedNodes [numberOfUnusedNodes] = nodeLocation;
						unusedNodesDirection [numberOfUnusedNodes] = direction;
						Debug.Log ("Direction is: " + direction);
						numberOfUnusedNodes++;
					}
				}
				columnIndex++;
			}
			rowIndex++;
			columnIndex = 0;
		}

		for (int i = 0; i < numberOfUnusedNodes; i++) {
			DestroyConnections (unusedNodes [i], unusedNodesDirection [i]);
		}
		if (numberOfUnusedNodes > 0) {
			RemoveUnusedWires ();
		}
	}

	private bool CheckLocation(int rowIndex, int columnIndex){
		string letter = ConvertIndexToLetter (rowIndex);
		string[] line = lineDictionary [letter];
		string locationValue = line [columnIndex];
		if (locationValue != "x") {
			return true;
		}
		return false;
	}

	private void DestroyConnections(string nodeLocation, string direction){
		string letter = nodeLocation.Substring (0, 1);
		string number = nodeLocation.Substring (1, nodeLocation.Length - 1);
		Debug.Log ("Number is: " + number + " Letter is: " + letter);
		string[] line = lineDictionary [letter];
		line [Int32.Parse(number)] = "x";
		lineDictionary [letter] = line;

		if (direction == "up" && string.Compare(letter, "A") != 0) {
			int numberedLetter = ConvertLetterToIndex(letter) - 1;
			string newLetter = ConvertIndexToLetter (numberedLetter);
			string [] newLine = lineDictionary [newLetter];

			while (numberedLetter > 0 && string.Compare(newLine[Int32.Parse(number)],"Wv") == 0) {
				newLine [Int32.Parse(number)] = "x";
				lineDictionary [newLetter] = newLine;

				numberedLetter--;
				newLetter = ConvertIndexToLetter (numberedLetter);
				newLine = lineDictionary [newLetter];
			}

		} else if (direction == "right" && Int32.Parse(number) < (horizontalGridSize - 1)){
			int columnNumber = Int32.Parse (number) + 1;

			while (columnNumber < (horizontalGridSize - 1) && line [columnNumber] == "Wh") {
				line [columnNumber] = "x";
				columnNumber++;
			}
			lineDictionary [letter] = line;
		
		} else if (direction == "down" && string.Compare(letter, ConvertIndexToLetter(verticalGridSize-1)) != 0) {
			int numberedLetter = ConvertLetterToIndex(letter) + 1;
			string newLetter = ConvertIndexToLetter (numberedLetter);
			string [] newLine = lineDictionary [newLetter];
			Debug.Log ("newLetter is: " + newLetter);
			Debug.Log("numberedLetter is: " + numberedLetter.ToString());
			Debug.Log ("Inside else if of direction down");
			while (numberedLetter < (verticalGridSize - 1) && string.Compare(newLine[Int32.Parse(number)],"Wv") == 0) {
				Debug.Log ("Inside while loop");
				newLine [Int32.Parse(number)] = "x";
				lineDictionary [newLetter] = newLine;

				numberedLetter++;
				newLetter = ConvertIndexToLetter (numberedLetter);
				newLine = lineDictionary [newLetter];
			}
		} else if (direction == "left" && Int32.Parse(number) > 0) {
			int columnNumber = Int32.Parse (number) - 1;

			while (columnNumber >= 0 && line [columnNumber] == "Wh") {
				line [columnNumber] = "x";
				columnNumber--;
			}
			lineDictionary [letter] = line;
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