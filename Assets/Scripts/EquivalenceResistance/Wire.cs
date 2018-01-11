using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire {

    private GameObject _wire;
    private GameObject _component1;
    private GameObject _component2;

	public Wire(GameObject wire, GameObject component1, GameObject component2)
    {
        _wire = wire;
        _component1 = component1;
        _component2 = component2;
    }

    public GameObject GetComponent1()
    {
        return _component1;
    }

    public GameObject GetComponent2()
    {
        return _component2;
    }

    public GameObject GetWireObject()
    {
        return _wire;
    }
}
