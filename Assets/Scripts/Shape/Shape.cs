using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum ShapeState
{
    Waiting, Shifting, Merging
}

public abstract class Shape : MonoBehaviour, IPointerDownHandler
{
    private const float TimeShiftDown = 0.07f;
    private const float TimeRefillShiftDown = 0.08f;
    private const float TimeBounce = 0.06f;
    private const float BounceAmount = 0.1f;

    public ShapeData _shapeData;
    public ShapeState _shapeState;

    public int _row;
    public int _col;

    private Sequence _shiftDownSequence;

    protected SpriteRenderer _shapeSpriteRenderer;

    void Awake()
    {
        _shapeSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public abstract void OnPointerDown(PointerEventData eventData);

    public virtual void SetShapeData(ShapeData shapeData, int row, int col)
    {
        this._row = row;
        this._col = col;
        _shapeData = shapeData;
        _shapeSpriteRenderer.sprite = shapeData.Sprite;
        _shapeSpriteRenderer.sortingOrder = row + 2;
    }

    public void CheckAdjacentShapes(bool isThisClickedShape)
    {
        int rows = BoardManager.Instance.GetRowCount();
        int columns = BoardManager.Instance.GetColumnCount();

        if (isThisClickedShape)
            BoardManager.Instance.AddShapeToAdjacentShapes(this);

        _CheckAdjacentShapes(_row, _col + 1, columns, false);
        _CheckAdjacentShapes(_row, _col - 1, columns, false);
        _CheckAdjacentShapes(_row + 1, _col, rows, true);
        _CheckAdjacentShapes(_row - 1, _col, rows, true);
    }

    private void _CheckAdjacentShapes(int row, int col, int constraint, bool isRowChanging)
    {
        Shape[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();
        int temp;

        if (isRowChanging)
            temp = row;
        else
            temp = col;

        if (temp < constraint && temp >= 0)
        {
            if (shapeMatrix[row, col] != null && !BoardManager.Instance.IsShapeCheckedBefore(shapeMatrix[row, col].GetComponent<Shape>()) &&
                shapeMatrix[row, col].GetComponent<Shape>()._shapeData.ShapeType == _shapeData.ShapeType)
            {
                BoardManager.Instance.AddShapeToAdjacentShapes(shapeMatrix[row, col].GetComponent<Shape>());
                shapeMatrix[row, col].GetComponent<Shape>().CheckAdjacentShapes(false);
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
            rowToShift = FindEmptyRow(_row);
            HandleShiftDown(rowToShift);
        }
    }

    private int FindEmptyRow(int rowIndex)
    {
        Shape[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();
        int rowToShift = -1;

        for (int i = rowIndex; i >= 0; i--)
        {
            if (shapeMatrix[i, _col] == null)
                rowToShift = i;
        }

        return rowToShift;
    }

    private void HandleShiftDown(int rowToShift)
    {
        if (rowToShift != -1)
        {
            Shape[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();

            shapeMatrix[rowToShift, _col] = this;
            shapeMatrix[_row, _col] = null;
            Shift(rowToShift, TimeShiftDown);
        }
    }

    private void HandleShiftDownForRefill(int rowToShift)
    {
        if (rowToShift != -1)
        {
            Shape[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();

            shapeMatrix[rowToShift, _col] = this;
            Shift(rowToShift, TimeRefillShiftDown);
        }
    }

    private void Shift(int rowToShift, float shiftDownTime)
    {
        if(_shapeState == ShapeState.Shifting)
        {
            _shiftDownSequence.Kill();
            _row = FindCurrentRow();
        }
        else
        {
            _shapeState = ShapeState.Shifting;
        }

        _shiftDownSequence = DOTween.Sequence();

        Vector2 offset = _shapeSpriteRenderer.bounds.size;

        float posToShift = offset.y * rowToShift;

        _shiftDownSequence.Append(transform.DOLocalMoveY(posToShift, shiftDownTime * (_row - rowToShift)).SetEase(Ease.InQuad)).OnComplete(() =>
        {
            BounceShape(transform.position.y + BounceAmount);
            _shapeState = ShapeState.Waiting;
        });
     
        
        _row = rowToShift;
        _shapeSpriteRenderer.sortingOrder = _row + 1;
    }

    private int FindCurrentRow()
    {
        int currentRow;
        Vector2 offset = _shapeSpriteRenderer.bounds.size;
        currentRow = Mathf.RoundToInt(transform.localPosition.y / offset.y);
        return currentRow;
    }

    private void BounceShape(float pos)
    {
        transform.DOMoveY(pos, TimeBounce).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
    }

    public abstract void Explode();

    public abstract void Merge();
}
