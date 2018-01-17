﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour {
	public GameObject wire;
	public GameObject wireSprite;
	private SpriteRenderer spirit;

	// Use this for initialization
	void Start () {
		spirit = wireSprite.GetComponent<SpriteRenderer> ();
		spirit.color = Color.black;
	}
	
	// Update is called once per frame
	void Update () {
		if (Components.getPaused ()) {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 0.05f);
		} else {
			spirit.color = new Vector4 (spirit.color.r, spirit.color.g, spirit.color.b, 1f);
		}
	}
}