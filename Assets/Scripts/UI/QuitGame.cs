using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour {
	
	public Transform goBack;

    public void buttonYes()
    {
        SceneManager.LoadScene("GameMenu");
    }

    public void buttonNo()
    {
        goBack.gameObject.SetActive(false);
    }
}
