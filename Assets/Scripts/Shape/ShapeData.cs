using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType
{
    BlueCube, RedCube, GreenCube, Bomb
}

[CreateAssetMenu(fileName = "New Shape", menuName = "Shape Data", order = 51)]

public class ShapeData : ScriptableObject
{
    [SerializeField]
    private ShapeType shapeType;
    [SerializeField]
    private Sprite sprite;

    public ShapeType ShapeType
    {
        get
        {
            return shapeType;
        }
    }

    public Sprite Sprite
    {
        get
        {
            return sprite;
        }
    }
}
