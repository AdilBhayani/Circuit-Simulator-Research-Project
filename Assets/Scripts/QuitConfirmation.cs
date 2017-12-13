﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitConfirmation : MonoBehaviour
{

    public CanvasGroup uiCanvasGroup;
    public CanvasGroup confirmQuitCanvasGroup;
    private bool quitting = false;

    // Use this for initialization
    private void Awake()
    {
        quitting = false;
        EnableMainUI();
        DisableConfirmUI();
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GameObject overallMenu = GameObject.FindGameObjectWithTag("OverallMenu");
            if (overallMenu && overallMenu.activeSelf)
            {
                if (quitting)
                {
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
        quitting = true;
        //reduce the visibility of normal UI, and disable all interraction
        uiCanvasGroup.alpha = 0.35f;
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
    }
}
