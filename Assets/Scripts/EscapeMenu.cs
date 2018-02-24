using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeMenu : MonoBehaviour {
    public CanvasGroup uiCanvasGroup;
    public CanvasGroup confirmQuitCanvasGroup;
    private bool quitting = false;
    private string overallMenuTitle = "OverallMenu";
    // Use this for initialization
    private void Start ()
    {
        Time.timeScale = 1;
        quitting = false;
        EnableMainUI();
        DisableConfirmUI();
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GameObject overallMenu = GameObject.FindGameObjectWithTag(overallMenuTitle);
            if (overallMenu && overallMenu.activeSelf)
            {
                if (quitting)
                {
                    EfxUpdater.playButtonSoundStatic();
                    DoConfirmQuitNo();
                }
                else
                {
                    DoQuit();
                }
            }
        }
    }

    /// <summary>
    /// Called if clicked on Cancel (confirmation)
    /// </summary>
    public void DoConfirmQuitNo()
    {
        Time.timeScale = 1;
        quitting = false;
        EnableMainUI();
        DisableConfirmUI();
    }

    /// <summary>
    /// Called if clicked on Quit (confirmation)
    /// </summary>
    public void DoConfirmQuitYes()
    {
        #if UNITY_EDITOR
	                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    /// <summary>
    /// Called if clicked on Quit
    /// </summary>
    public void DoQuit()
    {
		Components.setPaused (true);
        Time.timeScale = 0;
        quitting = true;
        //reduce the visibility of normal UI, and disable all interraction
        uiCanvasGroup.alpha = 0.35f;
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = false;

        //enable interraction with confirmation gui and make visible
        confirmQuitCanvasGroup.alpha = 1;
        confirmQuitCanvasGroup.interactable = true;
        confirmQuitCanvasGroup.blocksRaycasts = true;
        EfxUpdater.playButtonSoundStatic();
    }

    /// <summary>
    /// Enables the Main user interface
    /// </summary>
    private void EnableMainUI()
    {
        uiCanvasGroup.alpha = 1;
        uiCanvasGroup.interactable = true;
        uiCanvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Disables the confirmation dialog
    /// </summary>
    private void DisableConfirmUI()
    {
        confirmQuitCanvasGroup.alpha = 0;
        confirmQuitCanvasGroup.interactable = false;
        confirmQuitCanvasGroup.blocksRaycasts = false;
		Components.setPaused (false);
    }
}
