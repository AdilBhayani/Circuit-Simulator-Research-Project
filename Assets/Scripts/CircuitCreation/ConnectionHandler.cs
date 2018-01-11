using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionHandler : MonoBehaviour {

    public static List<GameObject> circuitComponents;
    public static List<Wire> wires;
    public GameObject wire;
    public GameObject node;
    public float multiplier;

    private GameObject newWire;
    private float scale;

    private Vector2 componentPos1;
    private Vector2 componentPos2;
    public static GameObject connector1;
    public static GameObject connector2;

    public static bool templateActive;
    private GameObject connectionFeedback;

    // Use this for initialization
    void Start () {
        wire.GetComponent<BoxCollider2D>().isTrigger = false;
        templateActive = false;
        circuitComponents = new List<GameObject>();
        wires = new List<Wire>();
        connectionFeedback = GameObject.FindGameObjectWithTag("ConnectionFeedback");
        connectionFeedback.SetActive(false);

        Vector3 startNodePos = new Vector3(-7, 3, 0);
        Vector3 endNodePos = new Vector3(-7, -2, 0);

        var startNode = Instantiate(node, startNodePos, Quaternion.identity);
        startNode.tag = "StartingNode";
        startNode.GetComponent<BoxCollider2D>().enabled = false;
        circuitComponents.Add(startNode);

        var endNode = Instantiate(node, endNodePos, Quaternion.identity);
        endNode.tag = "EndingNode";
        endNode.GetComponent<BoxCollider2D>().enabled = false;
		circuitComponents.Add(endNode);
    }
	
	// Update is called once per frame
	void Update () {
		if(connector1 != null && connector2 != null)
        {
            ConnectComponents(connector1, connector2);

            connector1 = null;
            connector2 = null;
        }
	}

    void ConnectComponents(GameObject connector1, GameObject connector2)
    {
        var pos1 = connector1.transform.position;
        var pos2 = connector2.transform.position;

        var x = (connector1.transform.position.x + connector2.transform.position.x) / 2;
        var y = (connector1.transform.position.y + connector2.transform.position.y) / 2;
        var z = 2.0f;
        var wirePos = new Vector3(x, y, z);
        var parent1 = connector1.transform.parent.gameObject;
        var parent2 = connector2.transform.parent.gameObject;
        var xDiff = pos1.x - pos2.x;
        var yDiff = pos1.y - pos2.y;

        if (parent1.tag == "StartingNode" && parent2.tag == "EndingNode" ||
            parent2.tag == "StartingNode" && parent1.tag == "EndingNode")
        {
            ResetConnectors(connector1, connector2);
            StartCoroutine(ShowFeedback("Starting node and Ending node cannot be connected."));
            return;
        }

        if (parent1.gameObject == parent2.gameObject)
        {
            ResetConnectors(connector1, connector2);
            StartCoroutine(ShowFeedback("You cannot connect connectors from the same component."));
            return;
        }

        if (parent1.tag == "Resistor" && parent2.tag == "Resistor")
        {
            ResetConnectors(connector1, connector2);
            StartCoroutine(ShowFeedback("Resistors must be connected using nodes."));
            return;
        }

        if (!IsCorrectConnector(xDiff, yDiff))
        {
            ResetConnectors(connector1, connector2);
            StartCoroutine(ShowFeedback("Two components must be on the same axis position."));
            return;
        }

        if (connector1.tag == connector2.tag)
        {
            ResetConnectors(connector1, connector2);
            StartCoroutine(ShowFeedback("Invalid connection. Double check the selected connectors."));
            return;
        }

        foreach (var item in wires)
        {
            var comp1 = item.GetComponent1();
            var comp2 = item.GetComponent2();
            if ((parent1 == comp1 || parent1 == comp2) && (parent2 == comp1 || parent2 == comp2))
            {
                ResetConnectors(connector1, connector2);
                StartCoroutine(ShowFeedback("Two components are already connected."));
                return;
            }
        }

        xDiff = Mathf.Round(xDiff);
        yDiff = Mathf.Round(yDiff);

        if (xDiff < 0)
        {
            newWire = Instantiate(wire, wirePos, Quaternion.identity);
            scale = isNode(parent1.tag, parent2.tag) ? Mathf.Abs(pos1.x - pos2.x) * multiplier + 0.2f : Mathf.Abs(pos1.x - pos2.x) * multiplier;
            newWire.transform.localScale = new Vector3(scale, 1, 1);

        }
        else if(xDiff > 0)
        {
            newWire = Instantiate(wire, wirePos, Quaternion.Euler(0, 0, 180f));
            scale = isNode(parent1.tag, parent2.tag) ? Mathf.Abs(pos1.x - pos2.x) * multiplier + 0.2f : Mathf.Abs(pos1.x - pos2.x) * multiplier;
            newWire.transform.localScale = new Vector3(scale, 1, 1);
        }
        else if(yDiff < 0)
        {
            newWire = Instantiate(wire, wirePos, Quaternion.Euler(0, 0, 90f));
            scale = isNode(parent1.tag, parent2.tag) ? Mathf.Abs(pos1.y - pos2.y) * multiplier + 0.2f : Mathf.Abs(pos1.y - pos2.y) * multiplier;
            newWire.transform.localScale = new Vector3(scale, 1, 1);
        }
        else if (yDiff > 0)
        {
            newWire = Instantiate(wire, wirePos, Quaternion.Euler(0, 0, 270f));
            scale = isNode(parent1.tag, parent2.tag) ? Mathf.Abs(pos1.y - pos2.y) * multiplier + 0.2f : Mathf.Abs(pos1.y - pos2.y) * multiplier;
            newWire.transform.localScale = new Vector3(scale, 1, 1);
        }

        circuitComponents.Add(newWire);
        wires.Add(new Wire(newWire, connector1.transform.parent.gameObject, connector2.transform.parent.gameObject));
        ResetConnectors(connector1, connector2);

    }

    bool isNode(string parentTag1, string parentTag2)
    {
        if (parentTag1 == "Node" || parentTag1 == "StartingNode" || parentTag1 == "EndingNode" ||
            parentTag2 == "Node" || parentTag2 == "StartingNode" || parentTag2 == "EndingNode")
            return true;
        return false;
    }

    bool isNode(string parentTag)
    {
        if (parentTag == "Node" || parentTag == "StartingNode" || parentTag == "EndingNode")
            return true;
        return false;
    }

    void ResetConnectors(GameObject connector1, GameObject connector2)
    {
        try
        {
            connector1.GetComponent<SpriteRenderer>().color = Color.white;
            connector2.GetComponent<SpriteRenderer>().color = Color.white;

            connector1.GetComponent<SpriteRenderer>().enabled = false;
            connector2.GetComponent<SpriteRenderer>().enabled = false;
        }
        catch (System.Exception)
        {
            
        }
    }

    bool IsCorrectConnector(float xDiff, float yDiff)
    {
        if (Mathf.Abs(xDiff) > 0.1f && Mathf.Abs(yDiff) > 0.1f)
            return false;
        return true;
    }

    IEnumerator ShowFeedback(string feedback)
    {
        connectionFeedback.GetComponent<Text>().text = feedback;
        connectionFeedback.SetActive(true);
        yield return new WaitForSeconds(2);
        connectionFeedback.SetActive(false);
    }
}
