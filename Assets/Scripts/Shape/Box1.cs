using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box1 : Shape
{
    public override void Explode()
    {
        SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Box), _row, _col);
        gameObject.AddComponent<Box>();
        Destroy(GetComponent<Box1>());
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
