using System.Collections.Generic;
using UnityEngine;

public class ResetButtonHandler : MonoBehaviour {

	public void DeleteAllComponents()
    {
        var components = ConnectionHandler.circuitComponents;
        ConnectionHandler.circuitComponents = new List<GameObject>();
        ConnectionHandler.wires = new List<Wire>();

        foreach (GameObject component in components)
        {
            if (component.tag == "StartingNode" || component.tag == "EndingNode")
            {
                ConnectionHandler.circuitComponents.Add(component);
            }
            else
            {
                Destroy(component);
            }
        }
    }
}
