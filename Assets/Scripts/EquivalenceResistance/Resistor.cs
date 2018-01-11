using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Resistor : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (SceneManager.GetActiveScene().name.Equals("EquivalentResistance"))
        {
            var value = gameObject.GetComponent<TextMesh>();
            value.text = Random.Range(5, 25).ToString();
            // change Random range value
        }

        else if(SceneManager.GetActiveScene().name.Equals("PlayScreen"))
        {
            var value = gameObject.GetComponent<TextMesh>();
            value.text = Random.Range(5, 25).ToString();
        }
	}
}
