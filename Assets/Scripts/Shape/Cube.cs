using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

enum CubeState
{
    Basic, Rocket, Bomb, Disco,
}

public enum CubeOperation
{
    BasicExplosion, TurnIntoRocket, TurnIntoBomb, TurnIntoDisco, Fail
}

public class Cube : Shape
{
    private const float TimeToTurnIntoBooster = 0.33f;

    private Sequence DiscoExplosionSequence;
    protected bool _isCubeCheckedBefore = false;

    private List<Shape> _adjacentGoals;

    private delegate void CubeOperationDelegate();
    private CubeOperationDelegate _cubeOperation;

    public override void Explode()
    {
        BoardManager.Instance.GetInstantiatedShapes()[_row, _col] = null;
        Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);
        GameManager.Instance.CheckGoal(_shapeData.ShapeType, _row, _col, true, _shapeData.ShapeColor, transform);
        Destroy(gameObject);
    }

    public override void Merge()
    {
        _shapeState = ShapeState.Merging;
        BoardManager.Instance.SetGameState(GameState.Merging);

        foreach (Cube cube in BoardManager.Instance.GetAdjacentShapes())
        {
            cube.MoveToMergePoint(_row, _col);

            if (!(cube._row == _row && cube._col == _col))
                BoardManager.Instance.RemoveFromInstantiatedShapes(cube._row, cube._col);
        }

        GameManager.Instance.CheckGoal(_shapeData.ShapeType, _row, _col, false, _shapeData.ShapeColor);
    }

    public override void SetMergeSprite(int count)
    {
        if (count > 4)
        {
            string color = "";

            if (_shapeData.ShapeColor == ShapeColor.Blue)
                color = "blue";
            else if (_shapeData.ShapeColor == ShapeColor.Red)
                color = "red";
            else if (_shapeData.ShapeColor == ShapeColor.Green)
                color = "green";

            _spriteRenderer.sprite = BoardManager.Instance.GetMergeSprite(FindCubeOperation(count), color);
        }
        else
        {
            _spriteRenderer.sprite = _shapeData.Sprite;
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (BoardManager.Instance.isMovesLeft() &&
            BoardManager.Instance.GetGameState() == GameState.Ready &&
            _shapeState == ShapeState.Waiting)
        {
            _adjacentShapes = new List<Shape>();
            _adjacentGoals = new List<Shape>();
            FindAdjacentShapes(true, _adjacentShapes, _adjacentGoals);

            if (IsAllShapesStateWaiting(_adjacentShapes))
            {
                BoardManager.Instance.SetAdjacentShapes(_adjacentShapes);
                HandleCubeOperation();
            }
        }
    }

    public IEnumerator DiscoExplosion(float WaitForTrail)
    {
        yield return new WaitForSeconds(WaitForTrail);
        //CUBE HIGHLIGHT
        //GameObject InstantiatedHighlight = Instantiate(BoardManager.Instance.DiscoHighlight, transform.position, transform.rotation, transform);
        //GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 10;
        if (this != null)
        {
            DiscoExplosionSequence = DOTween.Sequence();

            DiscoExplosionSequence.Append(transform.DORotate(new Vector3(0, 0, 6), 0.07f)
                               .SetEase(Ease.InQuad))
                               .Append(transform.DORotate(new Vector3(0, 0, -6), 0.14f)
                               .SetEase(Ease.InQuad))
                               .Append(transform.DORotate(new Vector3(0, 0, 0), 0.07f)
                               .SetEase(Ease.InQuad));
            DiscoExplosionSequence.SetLoops(-1, LoopType.Restart);


            Instantiate(BoardManager.Instance.LightBallCube, transform);
        }
    }

    private bool IsAllShapesStateWaiting(List<Shape> adjacentShapes)
    {
        foreach (Shape shape in adjacentShapes)
            if (shape._shapeState != ShapeState.Waiting)
                return false;

        return true;
    }

    public void SetCheckBefore(List<Shape> adjacentShapes)
    {
        foreach (Cube cube in adjacentShapes)
            cube._isCubeCheckedBefore = true;
    }

    private CubeOperation FindCubeOperation(int adjacentShapesCount)
    {
        if (adjacentShapesCount > 1 && adjacentShapesCount < 5)
            return CubeOperation.BasicExplosion;
        else if (adjacentShapesCount == 5 || adjacentShapesCount == 6)
            return CubeOperation.TurnIntoRocket;
        else if (adjacentShapesCount == 7 || adjacentShapesCount == 8)
            return CubeOperation.TurnIntoBomb;
        else if (adjacentShapesCount >= 9)
            return CubeOperation.TurnIntoDisco;
        else
            return CubeOperation.Fail;
    }

    private void HandleCubeOperation()
    {
        CubeOperation cubeOperation = FindCubeOperation(BoardManager.Instance.GetAdjacentShapes().Count);
        _cubeOperation += BoardManager.Instance.DecreaseRemainingMoves;
        _cubeOperation += ExplodeGoalShapes;

        if (cubeOperation == CubeOperation.BasicExplosion)
        {
            _cubeOperation += BasicExplosionOperation;
        }
        else if (cubeOperation == CubeOperation.TurnIntoRocket)
        {
            _cubeOperation += TurnIntoBooster<Rocket>;
        }
        else if (cubeOperation == CubeOperation.TurnIntoBomb)
        {
            _cubeOperation += TurnIntoBooster<Bomb>;
        }
        else if (cubeOperation == CubeOperation.TurnIntoDisco)
        {
            _cubeOperation += TurnIntoBooster<Disco>;
        }
        else
        {
            _cubeOperation -= ExplodeGoalShapes;
            _cubeOperation += FailOperation;
            _cubeOperation -= BoardManager.Instance.DecreaseRemainingMoves;
        }

        _cubeOperation?.Invoke();
        ResetCubeOperation();
    }

    private void ResetCubeOperation()
    {
        _cubeOperation -= BoardManager.Instance.DecreaseRemainingMoves;
        _cubeOperation -= TurnIntoBooster<Shape>;
        _cubeOperation -= FailOperation;
        _cubeOperation -= ExplodeGoalShapes;
    }

    private void BasicExplosionOperation()
    {
        List<int> columns = new List<int>();

        foreach (Shape shape in BoardManager.Instance.GetAdjacentShapes())
        {
            BoardManager.Instance.RemoveFromInstantiatedShapes(shape._row, shape._col);
            shape.Explode();

            if (!columns.Contains(shape._col))
                columns.Add(shape._col);
        }

        BoardManager.Instance.StartShiftDown(columns);
    }

    private void ExplodeGoalShapes()
    {
        foreach (Shape shape in _adjacentGoals)
        {
            BoardManager.Instance.RemoveFromInstantiatedShapes(shape._row, shape._col);
            shape.Explode();
        }
    }

    private void TurnIntoBooster<T>() where T : Shape
    {
        BoardManager.Instance.GetAdjacentShapes()[0].Merge();
        StartCoroutine(WaitForShiftDownAfterMerge());
        StartCoroutine(TurnIntoBoosterRoutine<T>());
    }

    private IEnumerator TurnIntoBoosterRoutine<T>(ShapeColor shapeColor = ShapeColor.None) where T : Shape
    {
        yield return new WaitForSeconds(TimeToTurnIntoBooster);
        T shape = gameObject.AddComponent<T>();

        if (shape is Rocket)
        {
            shape.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Rocket, ShapeColor.None), _row, _col);
            Instantiate(BoardManager.Instance.RocketMergeEffect, transform.position, transform.rotation, transform);
        }
        else if (shape is Bomb)
        {
            shape.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Bomb, ShapeColor.None), _row, _col);
            Instantiate(BoardManager.Instance.BombMergeEffect, transform.position, transform.rotation, transform);
        }
        else if (shape is Disco)
        {
            shapeColor = FindDiscoColor();
            shape.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Disco, shapeColor), _row, _col);
            Instantiate(BoardManager.Instance.DiscoMergeEffect, transform.position, transform.rotation, transform);
        }

        BoardManager.Instance.ReloadShapeToList(shape, _row, _col);
        BoardManager.Instance.DelayedShiftDown(FindMergeShapesColumns(), 0.01f);
        Destroy(gameObject.GetComponent<Cube>());
    }

    private List<int> FindMergeShapesColumns()
    {
        List<int> columns = new List<int>();
        List<Shape> adjacentShapes = BoardManager.Instance.GetAdjacentShapes();

        foreach(Shape shape in adjacentShapes)
            if (!columns.Contains(shape._col))
                columns.Add(shape._col);

        return columns;
    }

    private ShapeColor FindDiscoColor()
    {
        if (_shapeData.ShapeColor == ShapeColor.Red)
            return ShapeColor.Red;
        else if (_shapeData.ShapeColor == ShapeColor.Blue)
            return ShapeColor.Blue;
        else if (_shapeData.ShapeColor == ShapeColor.Green)
            return ShapeColor.Green;

        return ShapeColor.None;
    }

    private IEnumerator WaitForShiftDownAfterMerge()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        BoardManager.Instance.GetAdjacentShapes().Remove(this);
        BoardManager.Instance.SetGameState(GameState.Ready);
    }

    #region Disco Converts
    public void ConvertCubeToRocket(float timeToConvert, List<Rocket> rockets)
    {
        StartCoroutine(WaitForConvertToRocket(timeToConvert, rockets));
    }

    private IEnumerator WaitForConvertToRocket(float timeToConvert, List<Rocket> rockets = null)
    {
        yield return new WaitForSeconds(timeToConvert);
        Instantiate(BoardManager.Instance.DiscoMergeEffect, transform.position, transform.rotation, transform.parent);
        Rocket rocket = gameObject.AddComponent<Rocket>();
        rocket.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Rocket), _row, _col);
        BoardManager.Instance.ReloadShapeToList(rocket, _row, _col);

        if (rockets != null)
            rockets.Add(rocket);

        Destroy(this);
    }

    public void ConvertCubeToBomb(float timeToConvert, List<Bomb> bombs)
    {
        StartCoroutine(WaitForConvertToBomb(timeToConvert, bombs));
    }

    private IEnumerator WaitForConvertToBomb(float timeToConvert, List<Bomb> bombs = null)
    {
        yield return new WaitForSeconds(timeToConvert);
        Instantiate(BoardManager.Instance.DiscoMergeEffect, transform.position, transform.rotation, transform.parent);
        Bomb bomb = gameObject.AddComponent<Bomb>();
        bomb.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Bomb), _row, _col);
        BoardManager.Instance.ReloadShapeToList(bomb, _row, _col);

        if (bombs != null)
            bombs.Add(bomb);

        Destroy(this);
    }

    #endregion

    private void FailOperation()
    {
        BoardManager.Instance.GetAdjacentShapes().Clear();
        FailAnimation();
    }

    private void FailAnimation()
    {
        transform.DORotate(new Vector3(0, 0, 10), 0.05f).OnComplete(() =>
        {
            transform.DORotate(new Vector3(0, 0, -10), 0.1f).OnComplete(() =>
            {
                transform.DORotate(new Vector3(0, 0, 0), 0.05f).OnComplete(() =>
                {
                    transform.DORotate(new Vector3(0, 0, 5), 0.03f).OnComplete(() =>
                    {
                        transform.DORotate(new Vector3(0, 0, -5), 0.06f).OnComplete(() =>
                        {
                            transform.DORotate(new Vector3(0, 0, 0), 0.03f);

                        });
                    });
                });
            });
        });
    }
}
