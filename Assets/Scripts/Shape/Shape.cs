using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Shape : MonoBehaviour, IPointerDownHandler
{
    public ShapeData shapeData;

    void Start()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(shapeData.name);
    }

    public void SetShapeData(ShapeData _shapeData)
    {
        shapeData = _shapeData;
    }

    public abstract void ShiftDown();

    public abstract void Explode();


}
