using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : Shape
{
    public override void Explode()
    {
        Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);
        Destroy(gameObject);
    }
}
