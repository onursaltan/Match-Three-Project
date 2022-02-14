using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType
{
    BlueCube, RedCube, GreenCube, Bomb, Rocket, Disco
}

[CreateAssetMenu(fileName = "New Shape", menuName = "Shape Data", order = 51)]

public class ShapeData : ScriptableObject
{
    [SerializeField]
    private ShapeType _shapeType;

    [SerializeField]
    private Sprite _sprite;

    [SerializeField] 
    private GameObject _explodeEffect;

    public ShapeType ShapeType
    {
        get
        {
            return _shapeType;
        }
    }

    public Sprite Sprite
    {
        get
        {
            return _sprite;
        }
    }

    public GameObject ExplodeEffect
    {
        get
        {
            return _explodeEffect;
        }
    }
}
