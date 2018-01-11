using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour {

    public void buttonYes()
    {
        SceneManager.LoadScene("PlayScreen");
    }

    public void buttonNo()
    {
        SceneManager.LoadScene("GameMenu");
    }
}
