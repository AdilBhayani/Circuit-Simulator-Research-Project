using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ComponentsScript : MonoBehaviour {

    private bool isSelected;
    private List<GameObject> components = new List<GameObject>();

    void Start()
    {
        isSelected = false;
        components = new List<GameObject>();
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) && isSelected)
        {
            ConnectionHandler.circuitComponents.Remove(gameObject);
            Wire wire = null;
            foreach (var item in ConnectionHandler.wires)
            {
                if (item.GetWireObject() == gameObject)
                    wire = item;
            }
            ConnectionHandler.wires.Remove(wire);
            Destroy(gameObject);
        }

    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && !isSelected)
        {
            if (SceneManager.GetActiveScene().name.Equals("EquivalentResistance"))
            {
                var ob = CircuitHandler.GetDoubledEndedObject(gameObject);
                Debug.Log(ob.GetCurrentComponent().GetInstanceID());
                foreach (var item in ob.GetPreviousComponent())
                {
                    Debug.Log("Prev: " + item.GetInstanceID());
                }
                foreach (var item in ob.GetNextComponent())
                {
                    Debug.Log("Next: " + item.GetInstanceID());
                }
                if (CircuitHandler.selected1 == null)
                {
                    CircuitHandler.selected1 = CircuitHandler.GetDoubledEndedObject(gameObject);
                    isSelected = true;
                    gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0.6f, 0, 1);
                }
                else if (CircuitHandler.selected2 == null)
                {
                    CircuitHandler.selected2 = CircuitHandler.GetDoubledEndedObject(gameObject);
                    isSelected = true;
                    gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0.6f, 0, 1);
                }
                else
                {
                    var feedback = GameObject.FindGameObjectWithTag("Warning");
                    feedback.GetComponent<Text>().text = "You cannot chose more than 2 components at a time.";
                }
            }
            else
            {
                isSelected = true;
                if (gameObject.tag.Equals("Wire"))
                    gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 0.6f, 0, 1);
                else
                    gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0.6f, 0, 1);
            }
        }
        else if (Input.GetMouseButtonDown(0) && isSelected)
        {
            isSelected = false;

            if (gameObject.tag.Equals("Wire"))
                gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.black;
            else
                gameObject.GetComponent<SpriteRenderer>().color = Color.white;
 
            if (CircuitHandler.selected1 != null && CircuitHandler.selected1.GetCurrentComponent() == gameObject)
                CircuitHandler.selected1 = null;
            else if (CircuitHandler.selected2 != null && CircuitHandler.selected2.GetCurrentComponent() == gameObject)
                CircuitHandler.selected2 = null;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (SceneManager.GetActiveScene().name.Equals("EquivalentResistance"))
        {
            if (collider.tag == "Connector" || collider.tag == "Wire")
            {
                var parent = collider.transform.parent;

                if (parent != null && !components.Contains(parent.gameObject))
                {
                    components.Add(parent.gameObject);
                }
                else if(!components.Contains(collider.gameObject))
                {
                    components.Add(collider.gameObject);
                }
            }
            else if (!components.Contains(collider.gameObject))
            {
                components.Add(collider.gameObject);
            }

            if (!CircuitHandler.connectedComponents.ContainsKey(gameObject))
            {
                CircuitHandler.connectedComponents.Add(gameObject, components);
            }
            else
            {
                CircuitHandler.connectedComponents.Remove(gameObject);
                CircuitHandler.connectedComponents.Add(gameObject, components);
            }
        }
    }

    public void SetIsSelected(bool boolean)
    {
        isSelected = boolean;
    }
}