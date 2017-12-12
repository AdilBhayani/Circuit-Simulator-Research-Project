using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitConfirmation : MonoBehaviour
{

    public CanvasGroup uiCanvasGroup; public CanvasGroup confirmQuitCanvasGroup;

    // Use this for initialization
    private void Awake()
    {
        enableMainUI();
        disableConfirmUI();
    }

    /// <summary>
    /// Called if clicked on No (confirmation)
    /// </summary>
    public void DoConfirmQuitNo()
    {
        enableMainUI();
        disableConfirmUI();
    }

    /// <summary>
    /// Called if clicked on Yes (confirmation)
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
        //reduce the visibility of normal UI, and disable all interraction
        uiCanvasGroup.alpha = 0.2f;
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = false;

        //enable interraction with confirmation gui and make visible
        confirmQuitCanvasGroup.alpha = 1;
        confirmQuitCanvasGroup.interactable = true;
        confirmQuitCanvasGroup.blocksRaycasts = true;
    }


    /// <summary>
    /// Enables the Main user interface
    /// </summary>
    private void enableMainUI()
    {
        uiCanvasGroup.alpha = 1;
        uiCanvasGroup.interactable = true;
        uiCanvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Disables the confirmation dialog
    /// </summary>
    private void disableConfirmUI()
    {
        confirmQuitCanvasGroup.alpha = 0;
        confirmQuitCanvasGroup.interactable = false;
        confirmQuitCanvasGroup.blocksRaycasts = false;
    }
}
