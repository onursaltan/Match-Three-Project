using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Shape : MonoBehaviour, IPointerDownHandler
{
    public ShapeData shapeData;
    public int row;
    public int col;

    private SpriteRenderer shapeSpriteRenderer;

    void Start()
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
                shapeMatrix[r, c].GetComponent<Shape>().shapeData.Color == shapeData.Color)
            {
                BoardManager.Instance.AddShapeToAdjacentShapes(shapeMatrix[r, c].GetComponent<Shape>());
                shapeMatrix[r, c].GetComponent<Shape>().CheckAdjacentShapes(false);
            }
        }
    }

    public void ShiftDown()
    {
        GameObject[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();
        int rowToShift = -1;

        for (int i = row - 1; i >= 0; i--)
        {
            if (shapeMatrix[i, col] == null)
                rowToShift = i;
        }


        if (rowToShift != -1)
        {
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

        transform.position = posToShift;

        row = rowToShift;
    }

    public abstract void Explode();
}
