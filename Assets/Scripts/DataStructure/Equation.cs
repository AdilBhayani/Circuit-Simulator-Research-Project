using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Equation {

    private StringBuilder _equation;
    private int _resistorCount;

    public Equation()
    {
        _equation = new StringBuilder();
    }

    public Equation(int resistorCount)
    {
        _resistorCount = resistorCount;
    }

    public void ClearEquation()
    {
        _equation = new StringBuilder();
    }

    public void Series(GameObject resistor1, GameObject resistor2)
    {
        _equation.AppendLine(resistor1.tag + "," + resistor2.tag + ",series");
    }

    public void Parallel(GameObject resistor1, GameObject resistor2)
    {
        _equation.AppendLine(resistor1.tag + "," + resistor2.tag + ",parallel");
    }

    public string GetEquation()
    {
        return _equation.ToString();
    }

    private double SeriesCalc(double resistance1, double resistance2)
    {
        return resistance1 + resistance2;
    }

    private double ParallelCalc(double resistance1, double resistance2)
    {
        return (resistance1 * resistance2) / (resistance1 + resistance2);
    }

    private int GetResistorNumber(GameObject resistor)
    {
        var tag = resistor.tag;
        var index = tag.IndexOf('r',2);
        int num = 0;
        int.TryParse(tag.Substring(index + 1), out num);
        return num-1;
    }

    public double Calculate(FileInfo fileInfo)
    {
        StreamReader reader = fileInfo.OpenText();
        double[] resistances = new double[_resistorCount];
        string text;
        double result = 0;
        while ((text = reader.ReadLine()) != "")
        {
            var split = text.Split(',');
            var action = split[2];
            var resistor1 = GameObject.FindGameObjectWithTag(split[0]);
            var resistor2 = GameObject.FindGameObjectWithTag(split[1]);
            var num1 = GetResistorNumber(resistor1);
            var num2 = GetResistorNumber(resistor2);
            double resistance1 = 0;
            double resistance2 = 0;

            if (resistances[num1] == 0)
            {
                double.TryParse(resistor1.GetComponentInChildren<TextMesh>().text, out resistance1);
                resistances[num1] = resistance1;
            }
            else
            {
                resistance1 = resistances[num1];
            }

            if (resistances[num2] == 0)
            {
                double.TryParse(resistor2.GetComponentInChildren<TextMesh>().text, out resistance2);
                resistances[num2] = resistance2;
            }
            else
            {
                resistance2 = resistances[num2];
            }

            if (action.Equals("series"))
            {
                result = SeriesCalc(resistance1, resistance2);
                resistances[num1] = result;
                resistances[num2] = result;
            }
            else if (action.Equals("parallel"))
            {
                result = ParallelCalc(resistance1, resistance2);
                resistances[num1] = result;
                resistances[num2] = result;
            }
        }
        return Math.Round(result,2);
    }
}
