using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Disco : Shape
{
    private GameObject Anticipation;
    private GameObject Explosion;
    private GameObject Trail;

    public override void Explode()
    {
        //Instantiate(Anticipation, transform.position, transform.rotation, transform.parent);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        Explode();
    }

    public override void SetShapeData(ShapeData shapeData, int row, int col)
    {
        base.SetShapeData(shapeData, row, col);
        Anticipation = _shapeData.ExplodeEffect.transform.Find("Anticipation").gameObject;
    }

    public override void Merge()
    {
        throw new System.NotImplementedException();
    }

    public override void SetMergeSprite(int count)
    {
        throw new System.NotImplementedException();
    }
}
