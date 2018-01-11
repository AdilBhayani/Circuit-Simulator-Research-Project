using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentPanelScript : MonoBehaviour {

    public GameObject componentTemplate;

	// Update is called once per frame
	void OnMouseUp()
    {
        if(ConnectionHandler.templateActive == false)
            Instantiate(componentTemplate, Input.mousePosition, Quaternion.identity);
    }
}
