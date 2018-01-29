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

/*	[MenuItem("Tools/Write file")]
	static void WriteString()
	{
		string path = "Circuits/test.txt";

		//Write some text to the test.txt file
		StreamWriter writer = new StreamWriter(path, true);
		writer.WriteLine("Test");
		writer.Close();

		//Re-import the file to update the reference in the editor
		AssetDatabase.ImportAsset(path); 
		TextAsset asset = Resources.Load("test.txt");

		//Print the text from the file
		Debug.Log(asset.text);
	}
	*/

	public static StreamReader ReadString()
	{
		string path = "Circuits/test.txt";
		//Read the text from directly from the test.txt file
		StreamReader reader = new StreamReader(path); 
		return reader;
	}
}
