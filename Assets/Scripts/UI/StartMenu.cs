using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu: MonoBehaviour 
{
    public void GameMenu(string name)
	{
		SceneManager.LoadScene ("GameMenu");
	}

	public void HelpMenu (string name)
	{
		SceneManager.LoadScene ("HelpScreen");
	}

    public void Exit ()
    {
        Application.Quit();
    }

}


