using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Rocket : Shape
{
    private const float TimeBetweenExplosions = 0.05f;
    private bool _isDirectionVertical;

    public override void Explode()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        _shapeSpriteRenderer.enabled = false;

        if (_isDirectionVertical)
            ExplodeAllColumn();
        else
            ExplodeAllRow();

        ExplosionAnimation();
        instantiatedShapes[_row, _col] = null;
        StartCoroutine(WaitStartShift());
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

    private void ExplodeAllColumn()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        int index = 0;

        for (int i = _row + 1; i < BoardManager.Instance.GetRowCount(); i++)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[i, _col].GetComponent<Shape>(), index++));

        index = 0;

        for (int i = _row - 1; i >= 0; i--)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[i, _col].GetComponent<Shape>(), index++));

        BoardManager.Instance.GetDistinctColumns().Add(_col, BoardManager.Instance.GetRowCount());
    }

    private void ExplodeAllRow()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        int index = 0;

        for (int i = _col + 1; i < BoardManager.Instance.GetColumnCount(); i++)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[_row, i].GetComponent<Shape>(), index++));

        index = 0;

        for (int i = _col - 1; i >= 0; i--)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[_row, i].GetComponent<Shape>(), index++));

        Dictionary<int, int> distinctColumns = BoardManager.Instance.GetDistinctColumns();

        for (int i = 0; i < BoardManager.Instance.GetColumnCount(); i++)
            distinctColumns.Add(i, 1);
    }

    private IEnumerator WaitExplodeShape(Shape shape, int index)
    {
        yield return new WaitForSeconds(TimeBetweenExplosions * (float)index);
        shape.Explode();
    }

    private IEnumerator WaitStartShift()
    {
        int rowCount = BoardManager.Instance.GetRowCount();
        int biggerConstraint = Mathf.Max(rowCount - _row, (rowCount + _row) % rowCount);
        yield return new WaitForSeconds(TimeBetweenExplosions * biggerConstraint);
        BoardManager.Instance.StartShiftDown();
        Destroy(gameObject);
    }

    private void ExplosionAnimation()
    {
        Vector3 point1 = transform.position;
        Vector3 point2 = transform.position;

        GameObject s = Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);
        GameObject s1 = Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);

        if (_isDirectionVertical)
        {
            point1.y += 6;
            point2.y -= 6;
        }
        else
        {
            point1.x += 6;
            point2.x -= 6;
        }

        s.transform.DOMove(point1, 1.1f);
        s1.transform.DOMove(point2, 1.1f);
    }
}
