using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Equvilant : MonoBehaviour
{
    public Text help;
    public Transform helpScreen;
    public Boolean state = false;

    public void Start()
    {
        help.text = " < Directions >: Use MOUSE to Play! \n < RESET >: Reloads current circuit with NEW values \n < UNDO >: Reverts back ONE step \n \n note: maximum TWO resistors can be selected at any time";
    }

    public void Update()
    {
        helpScreen.gameObject.SetActive(state);
    }

    public void HELP()
    {
        state = !state;
    }
}


