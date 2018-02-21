using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CircuitLoader : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static StreamReader ReadString(string currentStage)
	{
		string path = "Circuits/" + currentStage + ".txt";
		//Read the text from directly from the test.txt file
		StreamReader reader = new StreamReader(path); 
		return reader;
	}

	public static int GetNumberOfStages(){
		string path = "Circuits/";
		string pattern = "stage*.txt";
		string[] stages = Directory.GetFiles(path,pattern);
		return stages.Length;
	}
}
