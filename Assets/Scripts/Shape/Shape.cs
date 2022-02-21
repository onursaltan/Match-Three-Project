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
    private const float TimeBounce = 0.06f;
    private const float BounceAmount = 0.015f;

    public ShapeData _shapeData;
    public ShapeState _shapeState;
    public SpriteRenderer _shapeSpriteRenderer;

    public int _row;
    public int _col;

    private Sprite _CubeRocketSprite;
    private Sprite _CubeBombSprite;

    private Sequence _shiftDownSequence;

    void Awake()
    {
        _shapeSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        BoardManager.Instance.DecreaseRemainingMoves();
    }

    public virtual void SetShapeData(ShapeData shapeData, int row, int col)
    {
        this._row = row;
        this._col = col;
        _shapeData = shapeData;
        _shapeSpriteRenderer.sprite = shapeData.Sprite;
        _shapeSpriteRenderer.sortingOrder = row + 2;
    }

    public void FindAdjacentShapes(bool isThisClickedShape)
    {
        int rows = BoardManager.Instance.GetRowCount();
        int columns = BoardManager.Instance.GetColumnCount();

        if (isThisClickedShape)
            BoardManager.Instance.AddShapeToAdjacentShapes(this);

        _FindAdjacentShapes(_row, _col + 1, columns, false);
        _FindAdjacentShapes(_row, _col - 1, columns, false);
        _FindAdjacentShapes(_row + 1, _col, rows, true);
        _FindAdjacentShapes(_row - 1, _col, rows, true);
    }

    private void _FindAdjacentShapes(int row, int col, int constraint, bool isRowChanging)
    {
        Shape[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();

        int temp = isRowChanging ? row : col;

        if (temp < constraint && temp >= 0)
        {
            if (shapeMatrix[row, col] != null && !BoardManager.Instance.IsShapeCheckedBefore(shapeMatrix[row, col]) &&
                shapeMatrix[row, col]._shapeData.ShapeType == _shapeData.ShapeType)
            {
                BoardManager.Instance.AddShapeToAdjacentShapes(shapeMatrix[row, col]);
                shapeMatrix[row, col].FindAdjacentShapes(false);
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

        Vector2 offset = _shapeSpriteRenderer.bounds.size;

        float posToShift = offset.y * rowToShift;
        ShiftAnimation(posToShift, shiftDownTime, rowToShift);

        _row = rowToShift;
        _shapeSpriteRenderer.sortingOrder = _row + 1;
    }

    private void ShiftAnimation(float posToShift, float shiftDownTime, int rowToShift)
    {
        _shiftDownSequence = DOTween.Sequence();
        float shiftAmount = _row - rowToShift;

        _shiftDownSequence.Append(transform.DOLocalMoveY(posToShift, shiftDownTime * shiftAmount).SetEase(Ease.InQuad)).OnComplete(() =>
        {
            BounceShape(transform.position.y + BounceAmount * shiftAmount, posToShift);
            _shapeState = ShapeState.Waiting;
        });
    }

    private void FixPosition(float fixPos)
    {
        Vector3 fixVector = transform.localPosition;
        fixVector.y = fixPos;
        transform.localPosition = fixVector;
    }

    private int FindCurrentRow()
    {
        int currentRow;
        Vector2 offset = _shapeSpriteRenderer.bounds.size;
        currentRow = Mathf.RoundToInt(transform.localPosition.y / offset.y);
        return currentRow;
    }

    private void BounceShape(float pos, float realPos)
    {
        transform.DOMoveY(pos, TimeBounce).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo).OnComplete(() => {
            if (realPos != transform.localPosition.y)
                FixPosition(realPos);

            IsMergeExist();
        });
    }

    public abstract void Explode();

    public abstract void Merge();

    public abstract bool IsMergeExist();
}
