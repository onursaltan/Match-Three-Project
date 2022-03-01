using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum BoosterMerge
{
    BigLightBall, LightBallWithBomb, LightBallWithRocket, BigBomb, DoubleRocket, BombWithRocket, None
}

public abstract class Booster : Shape
{
    protected BoosterMerge _boosterMerge = BoosterMerge.None;
    protected List<Shape> _adjacentBoosters;

    public override void FindAdjacentShapes(bool isThisClickedShape, List<Shape> adjacentShapes)
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
                shapeMatrix[row, col]._shapeData.ShapeType != ShapeType.Cube)
            {
                adjacentShapes.Add(shapeMatrix[row, col]);
                shapeMatrix[row, col].FindAdjacentShapes(false, adjacentShapes);
            }
        }
    }
    public BoosterMerge GetBoosterMerge()
    {
        _adjacentBoosters = new List<Shape>();
        FindAdjacentShapes(true, _adjacentBoosters);

        if (GetSpecificBoosterCount(ShapeType.Disco) > 1)
            return BoosterMerge.BigLightBall;
        else if (GetIsBoosterExist(ShapeType.Disco) && GetIsBoosterExist(ShapeType.Bomb))
            return BoosterMerge.LightBallWithBomb;
        else if (GetIsBoosterExist(ShapeType.Disco) && GetIsBoosterExist(ShapeType.Rocket))
            return BoosterMerge.LightBallWithRocket;
        else if (GetSpecificBoosterCount(ShapeType.Bomb) > 1)
            return BoosterMerge.BigBomb;  
        else if (GetIsBoosterExist(ShapeType.Bomb) && GetIsBoosterExist(ShapeType.Rocket))
            return BoosterMerge.BombWithRocket;
        else if (GetSpecificBoosterCount(ShapeType.Rocket) > 1)
            return BoosterMerge.DoubleRocket;

        return BoosterMerge.None;
    }

    private bool GetIsBoosterExist(ShapeType shapeType)
    {
        return _adjacentBoosters.Exists(booster => booster._shapeData.ShapeType == shapeType);
    }

    private int GetSpecificBoosterCount(ShapeType shapeType)
    {
        return _adjacentBoosters.Count(booster => booster._shapeData.ShapeType == shapeType);
    }
}
