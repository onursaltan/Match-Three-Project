using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Shape
{
    public override void Explode()
    {
        BoardManager.Instance.GetShapeMatrix()[_row, _col] = null;
        Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);
        GameManager.Instance.CheckGoal(_shapeData.ShapeType);
        BoardManager.Instance.StartShiftDown(new List<int> { _col });
        Destroy(gameObject);
    }

    public override void Merge()
    {
        throw new System.NotImplementedException();
    }
}
