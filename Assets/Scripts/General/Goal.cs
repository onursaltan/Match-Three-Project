using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Goal
{
    public ShapeType shapeType;
    public ShapeColor shapeColor;
    public int count;

    public Goal(ShapeType shapeType, ShapeColor shapeColor, int count)
    {
        this.shapeType = shapeType;
        this.shapeColor = shapeColor;
        this.count = count;
    }
}
