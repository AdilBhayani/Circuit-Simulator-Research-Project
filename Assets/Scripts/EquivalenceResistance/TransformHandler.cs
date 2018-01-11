using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransformHandler : MonoBehaviour
{
    private static GameObject _wire;
    private static float historyBoxSize;
    public static List<GameObject> actions = new List<GameObject>();

    void Start()
    {
        actions = new List<GameObject>();
        historyBoxSize = 126;
    }

    public static void TransformSeries(GameObject resistor1, GameObject resistor2)
    {
        var deResistor2 = CircuitHandler.GetDoubledEndedObject(resistor2);
        DoubleEnded nextComp2 = null;
        DoubleEnded prevComp2 = null;

        if (deResistor2.GetNextComponent().Count != 0)
        {
            nextComp2 = CircuitHandler.GetDoubledEndedObject(deResistor2.GetNextComponent()[0]);
            prevComp2 = CircuitHandler.GetDoubledEndedObject(deResistor2.GetPreviousComponent()[0]);

            if (prevComp2.GetNextComponent().Contains(resistor2))
            {
                prevComp2.GetNextComponent().Remove(resistor2);
                prevComp2.GetNextComponent().AddRange(deResistor2.GetNextComponent());
            }
            if (nextComp2.GetPreviousComponent().Contains(resistor2))
            {
                nextComp2.GetPreviousComponent().Remove(resistor2);
                nextComp2.GetPreviousComponent().AddRange(deResistor2.GetPreviousComponent());
            }
        }
        else
        {
            nextComp2 = CircuitHandler.GetDoubledEndedObject(deResistor2.GetPreviousComponent()[0]);
            prevComp2 = CircuitHandler.GetDoubledEndedObject(deResistor2.GetPreviousComponent()[1]);

            if (prevComp2.GetNextComponent().Contains(resistor2))
            {
                prevComp2.GetNextComponent().Remove(resistor2);
                foreach (var item in deResistor2.GetPreviousComponent())
                {
                    if (item != prevComp2.GetCurrentComponent())
                        prevComp2.GetNextComponent().Add(item);
                }
            }
            else if(prevComp2.GetPreviousComponent().Contains(resistor2))
            {
                prevComp2.GetPreviousComponent().Remove(resistor2);
                foreach (var item in deResistor2.GetPreviousComponent())
                {
                    if (item != prevComp2.GetCurrentComponent())
                        prevComp2.GetPreviousComponent().Add(item);
                }
            }

            if (nextComp2.GetPreviousComponent().Contains(resistor2))
            {
                nextComp2.GetPreviousComponent().Remove(resistor2);
                foreach (var item in deResistor2.GetPreviousComponent())
                {
                    if (item != nextComp2.GetCurrentComponent())
                        nextComp2.GetPreviousComponent().Add(item);
                }
            }
            else if(nextComp2.GetNextComponent().Contains(resistor2))
            {
                nextComp2.GetNextComponent().Remove(resistor2);
                foreach (var item in deResistor2.GetPreviousComponent())
                {
                    if (item != nextComp2.GetCurrentComponent())
                        nextComp2.GetNextComponent().Add(item);
                }
            }

        }


        //components in series must have a single prev and next or 2 prevs or 2 nexts 
        if (prevComp2.GetNextComponent().Contains(resistor2))
        {
            prevComp2.GetNextComponent().Remove(resistor2);
            prevComp2.GetNextComponent().AddRange(deResistor2.GetNextComponent());
        }
        if (nextComp2.GetPreviousComponent().Contains(resistor2))
        {
            nextComp2.GetPreviousComponent().Remove(resistor2);
            nextComp2.GetPreviousComponent().AddRange(deResistor2.GetPreviousComponent());
        }

        var rotation = resistor2.transform.rotation;
        var position = resistor2.transform.position;

        var actionText = "Series Transformation: \n R(" + resistor1.GetComponentInChildren<TextMesh>().text +
            ") & R(" + resistor2.GetComponentInChildren<TextMesh>().text + ")";

        AddAction(actionText);

        var positionDiff = CalculatePositionDiff(prevComp2.GetCurrentComponent(), nextComp2.GetCurrentComponent());
        var centrePos = CalculateCentrePos(prevComp2.GetCurrentComponent(), nextComp2.GetCurrentComponent());

        var newWire = Instantiate(_wire);
        newWire.transform.localScale = new Vector3(positionDiff * 1.5f, 1, 1);
        newWire.transform.rotation = rotation;
        if (Math.Round(rotation.z) == 1.0f)
            newWire.transform.position = new Vector3(position.x, centrePos, 2); 
        else
            newWire.transform.position = new Vector3(centrePos, position.y, 2);

        CircuitHandler.wires.Add(new Wire(newWire, prevComp2.GetCurrentComponent(), nextComp2.GetCurrentComponent()));
        resistor1.GetComponentInChildren<TextMesh>().text = CalculateSeriesResistance(resistor1, resistor2);
        resistor2.SetActive(false);
        CircuitHandler.equation.Series(resistor1, resistor2);
        RemoveHangingComponents();
        TransformComplete(resistor1, resistor2);
    }

    static string CalculateSeriesResistance(GameObject comp1, GameObject comp2)
    {
        double resistance1 = 0;
        double resistance2 = 0;
        double.TryParse(comp1.GetComponentInChildren<TextMesh>().text, out resistance1);
        double.TryParse(comp2.GetComponentInChildren<TextMesh>().text, out resistance2);
        return (resistance1 + resistance2).ToString();
    }

    static string CalculateParallelResistance(GameObject comp1, GameObject comp2)
    {
        double resistance1 = 0.0;
        double resistance2 = 0.0;
        double.TryParse(comp1.GetComponentInChildren<TextMesh>().text, out resistance1);
        double.TryParse(comp2.GetComponentInChildren<TextMesh>().text, out resistance2);
        var result = Math.Round(((resistance1 * resistance2) / (resistance1 + resistance2)), 2);
        return result.ToString();
    }

    static float CalculatePositionDiff(GameObject node1, GameObject node2)
    {
        var xDiff = Math.Abs(node1.transform.position.x - node2.transform.position.x);
        var yDiff = Math.Abs(node1.transform.position.y - node2.transform.position.y);
        return xDiff > yDiff ? xDiff : yDiff;
    }

    static float CalculateCentrePos(GameObject node1, GameObject node2)
    {
        var xDiff = Math.Abs(node1.transform.position.x - node2.transform.position.x);
        var yDiff = Math.Abs(node1.transform.position.y - node2.transform.position.y);
        var xCentrePos = (node1.transform.position.x + node2.transform.position.x) / 2;
        var yCentrePos = (node1.transform.position.y + node2.transform.position.y) / 2;
        return xDiff > yDiff ? xCentrePos : yCentrePos;
    }

    public static void TransformParallel(GameObject resistor1, GameObject resistor2)
    {
        var deComp1 = CircuitHandler.GetDoubledEndedObject(resistor1);

        GameObject prevComp1 = null;
        GameObject nextComp1 = null;

        if(deComp1.GetPreviousComponent().Count == 2)
        {
            prevComp1 = deComp1.GetPreviousComponent()[0];
            nextComp1 = deComp1.GetPreviousComponent()[1];
        }
        else
        {
            prevComp1 = deComp1.GetPreviousComponent()[0];
            nextComp1 = deComp1.GetNextComponent()[0];
        }

        Debug.Log(prevComp1.GetInstanceID());
        Debug.Log(nextComp1.GetInstanceID());

        var actionText = "Parallel Transformation: \n R(" + resistor1.GetComponentInChildren<TextMesh>().text +
        ") & R(" + resistor2.GetComponentInChildren<TextMesh>().text + ")";

        List<Wire> wire = new List<Wire>();
        var component = CircuitHandler.GetDoubledEndedObject(prevComp1);
        if (component.GetNextComponent().Contains(resistor1))
            component.GetNextComponent().Remove(resistor1);
        else if(component.GetPreviousComponent().Contains(resistor1))
            component.GetPreviousComponent().Remove(resistor1);

        component = CircuitHandler.GetDoubledEndedObject(nextComp1);
        if (component.GetPreviousComponent().Contains(resistor1))
            component.GetPreviousComponent().Remove(resistor1);
        else if(component.GetNextComponent().Contains(resistor1))
            component.GetNextComponent().Remove(resistor1);


        foreach (var item in CircuitHandler.wires)
        {
            var comp1 = item.GetComponent1();
            var comp2 = item.GetComponent2();

            if (comp1 == resistor1 || comp2 == resistor1)
            {
                Destroy(item.GetWireObject());
                CircuitHandler.connectedComponents.Remove(item.GetWireObject());
                wire.Add(item);
            }
        }
        resistor2.GetComponentInChildren<TextMesh>().text = CalculateParallelResistance(resistor1, resistor2);
        resistor1.SetActive(false);

        foreach (var item in wire)
        {
            CircuitHandler.wires.Remove(item);
        }

        AddAction(actionText);

        CircuitHandler.equation.Parallel(resistor1, resistor2);
        RemoveHangingComponents();
        TransformComplete(resistor1, resistor2);
    }

    static void TransformComplete(GameObject resistor1, GameObject resistor2)
    {
        resistor1.GetComponent<ComponentsScript>().SetIsSelected(false);
        resistor2.GetComponent<ComponentsScript>().SetIsSelected(false);
        CircuitHandler.selected1.GetCurrentComponent().GetComponentInChildren<SpriteRenderer>().color = Color.white;
        CircuitHandler.selected2.GetCurrentComponent().GetComponentInChildren<SpriteRenderer>().color = Color.white;
        CircuitHandler.selected1 = null;
        CircuitHandler.selected2 = null;
    }

    static void AddAction(string actionText)
    {
        var tag = actions.Count == 0 ? "Action" : "Action" + actions.Count;
        var newAction = GameObject.FindGameObjectWithTag(tag);
        if (actions.Count > 7)
        {
            var history = GameObject.FindGameObjectWithTag("History");
            history.GetComponent<RectTransform>().offsetMax = new Vector2(history.GetComponent<RectTransform>().offsetMax.x + historyBoxSize, 0);
        }
        newAction.GetComponent<Text>().text = actionText;
        actions.Add(newAction);

    }

    static void RemoveHangingComponents()
    {
        var foundHanging = false;
        foreach (var item in CircuitHandler.connectedComponents.Keys)
        {
            if(item.activeSelf && item.tag == "Node")
            {
                var component = CircuitHandler.GetDoubledEndedObject(item);
                if(component.GetPreviousComponent().Count == 0 && component.GetNextComponent().Count == 1)
                {
                    var nextComp = CircuitHandler.GetDoubledEndedObject(component.GetNextComponent()[0]);
                    if (nextComp.GetPreviousComponent().Contains(item))
                        nextComp.GetPreviousComponent().Remove(item);
                    else if (nextComp.GetNextComponent().Contains(item))
                        nextComp.GetNextComponent().Remove(item);

                    foreach (var wire in CircuitHandler.wires)
                    {
                        if (wire.GetComponent1() == item || wire.GetComponent2() == item)
                        {
                            wire.GetWireObject().SetActive(false);
                            item.SetActive(false);
                            foundHanging = true;
                        }
                    }
                }
                else if(component.GetPreviousComponent().Count == 1 && component.GetNextComponent().Count == 0)
                {
                    var prevComp = CircuitHandler.GetDoubledEndedObject(component.GetPreviousComponent()[0]);
                    if (prevComp.GetPreviousComponent().Contains(item))
                        prevComp.GetPreviousComponent().Remove(item);
                    else if (prevComp.GetNextComponent().Contains(item))
                        prevComp.GetNextComponent().Remove(item);

                    foreach (var wire in CircuitHandler.wires)
                    {
                        if (wire.GetComponent1() == item || wire.GetComponent2() == item)
                        {
                            wire.GetWireObject().SetActive(false);
                            item.SetActive(false);
                            foundHanging = true;
                        }
                    }
                }
            }
            else
            {
                foreach (var wire in CircuitHandler.wires)
                {
                    var comp1 = wire.GetComponent1();
                    var comp2 = wire.GetComponent2();
                    if (!comp1.activeSelf || !comp2.activeSelf)
                        wire.GetWireObject().SetActive(false);
                }
            }
        }
        if (foundHanging)
            RemoveHangingComponents();
    }

    public static void SetWireObject(GameObject wire)
    {
        _wire = wire;
    }
}
