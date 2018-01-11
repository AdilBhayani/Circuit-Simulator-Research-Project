using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CircuitCreator: MonoBehaviour
{
    public Text help;
    public Transform helpScreen;
    public Boolean state = false;

	public void GameMenu (string name)
	{
		SceneManager.LoadScene ("GameMenu");
	}

    public void Start()
    {
        help.text = " <R>: Rotates the Component \n < Right Click >: Unselects Selected Component \n < Delete Component >: 'CLICK' component + | backspace | or | delete | \n \n note: resistors MUST be connected by a 'NODE' Component";
    }

    public void Update()
    {
        helpScreen.gameObject.SetActive(state);
    }

    public void HELP ()
    {
        state = !state;
    }
}


