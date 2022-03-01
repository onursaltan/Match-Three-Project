using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public enum BoosterMerge
{
    BigLightBall, LightBallWithBomb, LightBallWithRocket, BigBomb, DoubleRocket, BombWithRocket, None
}

public abstract class Booster : Shape
{
    protected BoosterMerge _boosterMerge = BoosterMerge.None;
    protected List<Shape> _adjacentBoosters;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (BoardManager.Instance.isMovesLeft() &&
            BoardManager.Instance.GetGameState() == GameState.Ready &&
            _shapeState == ShapeState.Waiting)
        {
            BoardManager.Instance.DecreaseRemainingMoves();
            BoardManager.Instance.IncreaseDistinctColumns(_col);
            _boosterMerge = GetBoosterMerge();
            HandleBoosterExplosion();
        }

    }

    private void HandleBoosterExplosion()
    {
        switch (_boosterMerge)
        {
            case BoosterMerge.BigLightBall:
                HandleBigLightBall();
                break;
            case BoosterMerge.LightBallWithBomb:
                HandleLightBallWithBomb();
                break;
            case BoosterMerge.LightBallWithRocket:
                HandleLightBallWithRocket();
                break;
            case BoosterMerge.BigBomb:
                HandleBigBomb();
                break;
            case BoosterMerge.BombWithRocket:
                HandleBombWithRocket();
                break;
            case BoosterMerge.DoubleRocket:
                HandleDoubleRocket();
                break;
            case BoosterMerge.None:
                Explode();
                break;
        }

    }

    private void SetAdjacentBoosters(List<Shape> adjacentBoosters)
    {
        _adjacentBoosters = adjacentBoosters;
    }

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

    private void HandleBigLightBall()
    {
        Disco disco;
        if (GetType() != typeof(Disco))
        {
            disco = gameObject.AddComponent<Disco>();
            disco.SetAdjacentBoosters(_adjacentBoosters);
            disco.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Disco, ShapeColor.Blue), _row, _col);
        }
        else
            disco = (Disco)this;

        disco.Merge();
        StartCoroutine(disco.WaitForBigLightBall());
    }
    private void HandleLightBallWithBomb()
    {
        /*Disco disco;

        if (GetType() != typeof(Bomb))
        {
            disco = gameObject.AddComponent<Disco>();
            disco.SetAdjacentBoosters(_adjacentBoosters);
            disco.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Disco, ShapeColor.Blue), _row, _col);
        }
        else
            disco = (Disco)this;

        disco.Merge();
        StartCoroutine(disco.WaitForLightBallWithBomb());
        */
        throw new System.NotImplementedException();

    }
    private void HandleLightBallWithRocket()
    {
        /* Disco disco;

         if (GetType() != typeof(Bomb))
         {
             disco = gameObject.AddComponent<Disco>();
             disco.SetAdjacentBoosters(_adjacentBoosters);
             disco.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Disco, ShapeColor.Blue), _row, _col);
         }
         else
             disco = (Disco)this;

         disco.Merge();
         StartCoroutine(disco.WaitForBigLightBall());*/

        throw new System.NotImplementedException();
    }

    private void HandleBigBomb()
    {
        Bomb bomb;
        if (GetType() != typeof(Bomb))
        {
            bomb = gameObject.AddComponent<Bomb>();
            bomb.SetAdjacentBoosters(_adjacentBoosters);
            bomb.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Bomb, ShapeColor.None), _row, _col);
        }
        else
            bomb = (Bomb)this;

        bomb.Merge();
        StartCoroutine(bomb.WaitForExplode5x5());
    }

    private void HandleBombWithRocket()
    {
        Rocket rocket;
        if (GetType() != typeof(Rocket))
        {
            rocket = gameObject.AddComponent<Rocket>();
            rocket.SetAdjacentBoosters(_adjacentBoosters);
            rocket.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Rocket, ShapeColor.None), _row, _col);
        }
        else
            rocket = (Rocket)this;

        rocket.Merge();
        StartCoroutine(rocket.WaitForExplodeRocketWithBomb());
    }

    private void HandleDoubleRocket()
    {
        Rocket rocket = (Rocket)this;
        rocket.Merge();
        StartCoroutine(rocket.WaitForExplodeDoubleRocket());
    }
}