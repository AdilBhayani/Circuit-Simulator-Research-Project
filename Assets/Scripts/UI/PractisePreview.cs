using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class PractisePreview: MonoBehaviour
{
    public Button play;
    public Transform preview;

    public void PLAY()
    {
        SceneManager.LoadScene("EquivalentResistance");
    }

    public void back()
    {
        SceneManager.LoadScene("GameMenu");
    }

    public void EQ()
    {
        play.gameObject.SetActive(true);
        preview.gameObject.SetActive(true);
    }

    public void DY()
    {
        play.gameObject.SetActive(false);
        preview.gameObject.SetActive(false);
    }
}

