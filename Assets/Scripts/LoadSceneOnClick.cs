using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// LoadSceneOnClick changes the scene
/// </summary>
public class LoadSceneOnClick : MonoBehaviour {

	public void LoadByIndex( int sceneIndex){
		SceneManager.LoadScene (sceneIndex);
	}

	public void increaseStage(){
		Components.increaseStage ();
	}
}
