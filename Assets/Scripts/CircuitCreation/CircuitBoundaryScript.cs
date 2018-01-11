using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitBoundaryScript : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D other)
    {
        other.gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }

    void OnTriggerExit2D (Collider2D other)
    {
        other.gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
}
