using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Shape
{
    public override void Explode()
    {
        GameManager.Instance.CheckGoal(_shapeData.ShapeType);
        Destroy(gameObject);
    }

    public override void Merge()
    {
        throw new System.NotImplementedException();
    }
}
