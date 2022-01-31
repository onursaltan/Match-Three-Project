using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Shape : MonoBehaviour, IPointerDownHandler
{
    public ShapeData shapeData;
    public int x;
    public int y;
    private bool isChecked = false;

    void Start()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CheckAdjacentShapes(-1, -1);
    }

    public void SetShapeData(ShapeData _shapeData, int x, int y)
    {
        this.x = x;
        this.y = y;
        shapeData = _shapeData;
    }

    public void CheckAdjacentShapes(int previousX, int previousY)
    {

        GameObject[,] shapeMatrix = BoardManager.Instance.GetShapeMatrix();
        int rows = BoardManager.Instance.GetRowCount();
        int columns = BoardManager.Instance.GetColumnCount();

        if (previousX == -1 && previousY == -1)
            BoardManager.Instance.AddShapeToAdjacentShapes(this);


        if (y + 1 < columns && !BoardManager.Instance.IsShapeCheckedBefore(shapeMatrix[x, y + 1].GetComponent<Shape>()) && previousY != y + 1 && shapeMatrix[x, y + 1].GetComponent<Shape>().shapeData.Color == shapeData.Color )
        {
            BoardManager.Instance.AddShapeToAdjacentShapes(shapeMatrix[x, y + 1].GetComponent<Shape>());
            shapeMatrix[x, y + 1].GetComponent<Shape>().CheckAdjacentShapes(x, y);
        }

        if (y - 1 >= 0 && !BoardManager.Instance.IsShapeCheckedBefore(shapeMatrix[x, y - 1].GetComponent<Shape>()) && previousY != y - 1 && shapeMatrix[x, y - 1].GetComponent<Shape>().shapeData.Color == shapeData.Color)
        {
            BoardManager.Instance.AddShapeToAdjacentShapes(shapeMatrix[x, y - 1].GetComponent<Shape>());
            shapeMatrix[x, y - 1].GetComponent<Shape>().CheckAdjacentShapes(x, y);
        }

        if (x + 1 < rows && !BoardManager.Instance.IsShapeCheckedBefore(shapeMatrix[x + 1, y].GetComponent<Shape>()) && previousX != x + 1 && shapeMatrix[x + 1, y].GetComponent<Shape>().shapeData.Color == shapeData.Color)
        {
            BoardManager.Instance.AddShapeToAdjacentShapes(shapeMatrix[x + 1, y].GetComponent<Shape>());
            shapeMatrix[x + 1, y].GetComponent<Shape>().CheckAdjacentShapes(x, y);
        }

        if (x - 1 >= 0 && !BoardManager.Instance.IsShapeCheckedBefore(shapeMatrix[x - 1, y].GetComponent<Shape>()) && previousX != x - 1 && shapeMatrix[x - 1, y].GetComponent<Shape>().shapeData.Color == shapeData.Color)
        {
            BoardManager.Instance.AddShapeToAdjacentShapes(shapeMatrix[x - 1, y].GetComponent<Shape>());
            shapeMatrix[x - 1, y].GetComponent<Shape>().CheckAdjacentShapes(x, y);
        }
    }

    public void SetIsChecked(bool _isChecked)
    {
        this.isChecked = _isChecked;
    }

    public abstract void ShiftDown();

    public abstract void Explode();


}
