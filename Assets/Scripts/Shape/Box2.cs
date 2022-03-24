using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box2 : Shape
{
    public override void Explode()
    {
        SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Box2), _row, _col);
        gameObject.AddComponent<Box1>();
        Destroy(GetComponent<Box2>());
    }

    public override void Merge()
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
