using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Shape", menuName = "Shape Data", order = 51)]
public class ShapeData : ScriptableObject
{
    [SerializeField]
    private string shapeName;
    [SerializeField]
    private string color;
    [SerializeField]
    private Sprite sprite;

    public string ShapeName
    {
        get
        {
            return shapeName;
        }
    }

    public string Color
    {
        get
        {
            return color;
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
