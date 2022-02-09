using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Rocket : Shape
{
    private bool _isDirectionVertical;

    public override void Explode()
    {
        GameObject[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        if (_isDirectionVertical)
        {
           for(int i = 0; i < BoardManager.Instance.GetRowCount(); i++)
            {
                if(i != _row)
                    instantiatedShapes[i, _col].GetComponent<Shape>().Explode();
            }

            BoardManager.Instance.RefillBoard();
        }

        instantiatedShapes[_row, _col] = null;
        BoardManager.Instance.StartShiftDown();
        Destroy(gameObject);
    }

    public override void Merge()
    {
        throw new System.NotImplementedException();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        Explode();
    }

    public override void SetShapeData(ShapeData shapeData, int row, int col)
    {
        base.SetShapeData(shapeData, row, col);
        _isDirectionVertical = GetRandomBool();

        if(!_isDirectionVertical)
            transform.Rotate(new Vector3(0f, 0f, 90f));
    }

    private bool GetRandomBool()
    {
        return Random.Range(0, 2) == 1;
    }
}
