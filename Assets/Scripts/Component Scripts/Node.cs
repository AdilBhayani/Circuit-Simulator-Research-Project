using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {
	public GameObject node;
	private SpriteRenderer spirit;

	// Use this for initialization
	void Start () {
		spirit = node.GetComponent<SpriteRenderer> ();
		spirit.color = Color.white;
	}

	// Update is called once per frame
	void Update () {
		if (Components.getPaused ()) {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 0.15f);
		} else {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 1f);
		}
	}
}