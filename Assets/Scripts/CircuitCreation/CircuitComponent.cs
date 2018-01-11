using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitComponent {

    public int index;
    public ComponentType type;
    public Vector2 position;
}

public enum ComponentType
{
    Resistor = 1,
    Node,
    Wire    
}
