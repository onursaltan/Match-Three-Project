using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Rocket : Shape
{
    private bool _isDirectionVertical;

    public override void Explode()
    {
        GameObject[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        if (_isDirectionVertical)
        {
            for (int i = 0; i < BoardManager.Instance.GetRowCount(); i++)
            {
                if (i != _row)
                    instantiatedShapes[i, _col].GetComponent<Shape>().Explode();
            }

            BoardManager.Instance.GetDistinctColumns().Add(_col, BoardManager.Instance.GetRowCount());
        }

        HandleExplosion();
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
        //  _isDirectionVertical = GetRandomBool();

        _isDirectionVertical = true;
        if (_isDirectionVertical)
        {
        }
        else
        {
            transform.Rotate(new Vector3(0f, 0f, 90f));
        }
    }

    private bool GetRandomBool()
    {
        return Random.Range(0, 2) == 1;
    }

    private void HandleExplosion()
    {
        Vector3 pointTop = transform.position;
        Vector3 pointBottom = transform.position;

        pointTop.y += 1;
        pointBottom.y -= -1;

        GameObject s = Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);
        GameObject s1 = Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);

        s.transform.DOMove(pointTop, 0.1f).OnComplete( () => {
            pointTop.y += 6;
            s.transform.DOMove(pointTop, 1.1f);
        });

        s1.transform.DOMove(pointBottom, 0.1f).OnComplete(() => {
            pointBottom.y -= 6;
            s1.transform.DOMove(pointBottom, 1.1f);
        });
    }
}
