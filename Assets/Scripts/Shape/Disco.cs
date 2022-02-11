using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Disco : Shape
{
    public override void Explode()
    { 
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        Explode();
    }

    public override void Merge()
    {
        throw new System.NotImplementedException();
    }
}
