using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquivalentResistance : MonoBehaviour {
    public Text help;
    public Transform helpScreen;
    public Boolean state = false;


    // Use this for initialization
    void Start () {
        help.text = " < Directions >: Use MOUSE to Play! \n < RESET >: Reloads current circuit with NEW values \n < UNDO >: Reverts back ONE step \n \n note: maximum TWO resistors can be selected at any time";
    }
	
	// Update is called once per frame
	void Update () {
        helpScreen.gameObject.SetActive(state);
    }

    public void HELP()
    {
        state = !state;
    }
}
