using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum ShapeState
{
    Waiting, Shifting
}

public abstract class Shape : MonoBehaviour, IPointerDownHandler
{
    public ShapeData _shapeData;
    public ShapeState _shapeState;

    private const float TimeShiftDown = 0.15f;
    private const float TimeRefillShiftDown = 0.15f;
    private const float TimeBounce = 0.1f;
    private const float BounceAmount = 0.05f;

    public int row;
    public int col;

    private SpriteRenderer _shapeSpriteRenderer;

    void Awake()
    {
        _shapeSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CheckAdjacentShapes(true);
        BoardManager.Instance.HandleShiftDown();
    }

    public void SetShapeData(ShapeData shapeData, int row, int col)
    {
        this.row = row;
        this.col = col;
        _shapeData = shapeData;
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
                shapeMatrix[r, c].GetComponent<Shape>()._shapeData.ShapeType == _shapeData.ShapeType)
            {
                BoardManager.Instance.AddShapeToAdjacentShapes(shapeMatrix[r, c].GetComponent<Shape>());
                shapeMatrix[r, c].GetComponent<Shape>().CheckAdjacentShapes(false);
            }
        }
    }

    public void ShiftDown(bool forRefill = false)
    {
        int rowToShift;

        if (forRefill)
        {
            rowToShift = FindEmptyRow(BoardManager.Instance.GetRowCount() - 1);
            HandleShiftDownForRefill(rowToShift);
        }
        else
        {
            rowToShift = FindEmptyRow(row);
            HandleShiftDown(rowToShift);
        }
    }

    private int FindEmptyRow(int rowIndex)
    {
        GameObject[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();
        int rowToShift = -1;

        for (int i = rowIndex; i >= 0; i--)
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
            Shift(rowToShift, TimeShiftDown);
        }
    }

    private void HandleShiftDownForRefill(int rowToShift)
    {
        if (rowToShift != -1)
        {
            GameObject[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();

            shapeMatrix[rowToShift, col] = this.gameObject;
            Shift(rowToShift, TimeRefillShiftDown);
        }
    }

    private void Shift(int rowToShift, float shiftDownTime)
    {
        Vector2 offset = _shapeSpriteRenderer.bounds.size;

        Vector3 posToShift = transform.position;
        posToShift.y -= offset.y * (row - rowToShift);

        transform.DOMove(posToShift, shiftDownTime * (row - rowToShift)).SetEase(Ease.InQuad).OnComplete(() =>
        {
            BounceShape(transform.position.y + BounceAmount);
        });

        row = rowToShift;
    }

    private void BounceShape(float pos)
    {
        transform.DOMoveY(pos, TimeBounce).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
    }

    public abstract void Explode();
}
