using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum ShapeState
{
    Waiting, Shifting, Merging, Explode
}

public abstract class Shape : MonoBehaviour, IPointerDownHandler
{
    private const float TimeShiftDown = 0.07f;
    private const float TimeRefillShiftDown = 0.07f;
    private const float TimeBounce = 0.1f;
    private const float BounceAmount = 0.02f;


    public ShapeData _shapeData;
    public ShapeState _shapeState;

    public int _row;
    public int _col;

    protected List<Shape> _adjacentShapes;
    protected SpriteRenderer _spriteRenderer;

    private Sequence _shiftDownSequence;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        BoardManager.Instance.ReverseShapesSprite();
        BoardManager.Instance.FindMerges();
    }

    public virtual void SetShapeData(ShapeData shapeData, int row, int col)
    {
        this._row = row;
        this._col = col;
        _shapeData = shapeData;
        _spriteRenderer.sprite = _shapeData.Sprite;
        _spriteRenderer.sortingOrder = row + 2;
    }

    public void ReverseShapeSprite()
    {
        _spriteRenderer.sprite = _shapeData.Sprite;
    }

    public abstract void SetMergeSprite(int count);

    public void FindAdjacentShapes(bool isThisClickedShape, List<Shape> adjacentShapes)
    {
        int rows = BoardManager.Instance.GetRowCount();
        int columns = BoardManager.Instance.GetColumnCount();

        if (isThisClickedShape)
            adjacentShapes.Add(this);

        _FindAdjacentShapes(_row, _col + 1, columns, false, adjacentShapes);
        _FindAdjacentShapes(_row, _col - 1, columns, false, adjacentShapes);
        _FindAdjacentShapes(_row + 1, _col, rows, true, adjacentShapes);
        _FindAdjacentShapes(_row - 1, _col, rows, true, adjacentShapes);
    }

    private void _FindAdjacentShapes(int row, int col, int constraint, bool isRowChanging, List<Shape> adjacentShapes)
    {
        Shape[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();

        int temp = isRowChanging ? row : col;

        if (temp < constraint && temp >= 0)
        {
            if (shapeMatrix[row, col] != null && !BoardManager.Instance.IsShapeCheckedBefore(adjacentShapes, shapeMatrix[row, col]) &&
                shapeMatrix[row, col]._shapeData.ShapeType == _shapeData.ShapeType && 
                shapeMatrix[row, col]._shapeData.ShapeColor == _shapeData.ShapeColor)
            {
                adjacentShapes.Add(shapeMatrix[row, col]);
                shapeMatrix[row, col].FindAdjacentShapes(false, adjacentShapes);
            }
        }
    }

    public void ShiftDown(bool isForRefill = false)
    {
            int rowToShift = isForRefill ?
                FindEmptyRow(BoardManager.Instance.GetRowCount() - 1) :
                FindEmptyRow(_row);

            HandleShiftDown(rowToShift, isForRefill);
    }

    private int FindEmptyRow(int rowIndex)
    {
        Shape[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();
        int rowToShift = -1;

        for (int i = rowIndex; i >= 0; i--)
            if (shapeMatrix[i, _col] == null)
                rowToShift = i;

        return rowToShift;
    }

    private void HandleShiftDown(int rowToShift, bool isForRefill = false)
    {
        if (rowToShift != -1)
        {
            Shape[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();

            shapeMatrix[rowToShift, _col] = this;

            if (!isForRefill)
                shapeMatrix[_row, _col] = null;

            float temp = isForRefill ? TimeRefillShiftDown : TimeShiftDown;
            Shift(rowToShift, temp);
        }
    }

    private void Shift(int rowToShift, float shiftDownTime)
    {
        if (_shapeState == ShapeState.Shifting)
        {
            _shiftDownSequence.Kill();
            _row = FindCurrentRow();
        }
        else
        {
            _shapeState = ShapeState.Shifting;
        }

        Vector2 offset = _spriteRenderer.bounds.size;

        Debug.Log(_row + " " + rowToShift);

       
        float posToShift = offset.y * rowToShift - (rowToShift * 0.08f);

        ShiftAnimation(posToShift, shiftDownTime, rowToShift);

        _row = rowToShift;
        _spriteRenderer.sortingOrder = _row + 1;
    }

    private void ShiftAnimation(float posToShift, float shiftDownTime, int rowToShift)
    {
        _shiftDownSequence = DOTween.Sequence();
        float shiftAmount = _row - rowToShift;

        _shiftDownSequence.Append(transform.DOLocalMoveY(posToShift, shiftDownTime * shiftAmount)
                           .SetEase(Ease.InQuad))
                           .Append(BounceShape(posToShift, shiftAmount))
                           .OnComplete(() =>
        {
            _shapeState = ShapeState.Waiting;
        });
    }

    private int FindCurrentRow()
    {
        int currentRow;
        Vector2 offset = _spriteRenderer.bounds.size;
        currentRow = Mathf.RoundToInt(transform.localPosition.y / offset.y);
        return currentRow;
    }

    private Tween BounceShape(float posToShift, float shiftAmount)
    {
        return transform.DOLocalMoveY(posToShift + BounceAmount * shiftAmount, TimeBounce).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
    }

    public abstract void Explode();

    public abstract void Merge();
}
