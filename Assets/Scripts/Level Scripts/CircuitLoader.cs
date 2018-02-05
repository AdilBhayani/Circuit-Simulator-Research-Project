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

	public static StreamReader ReadString()
	{
		string path = "Circuits/test.txt";
		//Read the text from directly from the test.txt file
		StreamReader reader = new StreamReader(path); 
		return reader;
	}
}
