using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public abstract class Shape : MonoBehaviour, IPointerDownHandler
{
    public ShapeData shapeData;
    public int row;
    public int col;

    private SpriteRenderer shapeSpriteRenderer;

    void Awake()
    {
        shapeSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CheckAdjacentShapes(true);
        BoardManager.Instance.HandleShiftDown();
    }

    public void SetShapeData(ShapeData _shapeData, int row, int col)
    {
        this.row = row;
        this.col = col;
        shapeData = _shapeData;
    }

    public void CheckAdjacentShapes(bool isThisClickedShape)
    {
        int rows = BoardManager.Instance.GetRowCount();
        int columns = BoardManager.Instance.GetColumnCount();

        if (isThisClickedShape)
            BoardManager.Instance.AddShapeToAdjacentShapes(this);

        _CheckAdjacentShapes(row, col + 1, columns, false);
        _CheckAdjacentShapes(row, col - 1, columns, false);
        _CheckAdjacentShapes(row + 1, col, rows, true);
        _CheckAdjacentShapes(row - 1, col, rows, true);
    }

    private void _CheckAdjacentShapes(int r, int c, int constraint, bool isRowChanging)
    {
        GameObject[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();
        int temp;

        if (isRowChanging)
            temp = r;
        else
            temp = c;

        if (temp < constraint && temp >= 0)
        {
            if (shapeMatrix[r, c] != null && !BoardManager.Instance.IsShapeCheckedBefore(shapeMatrix[r, c].GetComponent<Shape>()) &&
                shapeMatrix[r, c].GetComponent<Shape>().shapeData.ShapeType == shapeData.ShapeType)
            {
                BoardManager.Instance.AddShapeToAdjacentShapes(shapeMatrix[r, c].GetComponent<Shape>());
                shapeMatrix[r, c].GetComponent<Shape>().CheckAdjacentShapes(false);
            }
        }
    }

    public void ShiftDown()
    {
        int rowToShift = FindEmptyRow();
        HandleShiftDown(rowToShift);
    }

    private int FindEmptyRow()
    {
        GameObject[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();
        int rowToShift = -1;

        for (int i = row - 1; i >= 0; i--)
        {
            if (shapeMatrix[i, col] == null)
                rowToShift = i;
        }

        return rowToShift;
    }

    private void HandleShiftDown(int rowToShift)
    {
        if (rowToShift != -1)
        {
            GameObject[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();

            shapeMatrix[rowToShift, col] = this.gameObject;
            shapeMatrix[row, col] = null;
            _ShiftDown(rowToShift);
        }
    }

    private void _ShiftDown(int rowToShift)
    {
        Vector2 offset = shapeSpriteRenderer.bounds.size;

        Vector3 posToShift = transform.position;
        posToShift.y -= offset.y * (row - rowToShift);

        transform.DOMove(posToShift, 0.15f * (row - rowToShift)).SetEase(Ease.InSine).OnComplete(() =>
        {
            BounceShape(transform.position.y + 0.05f);
        });

        row = rowToShift;
    }

    private void BounceShape(float pos)
    {
        transform.DOMoveY(pos, 0.1f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
    }

    public abstract void Explode();
}
