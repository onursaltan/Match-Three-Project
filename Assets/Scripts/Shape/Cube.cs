using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

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
    private Sequence DiscoExplosionSequence;

    private const float TimeToTurnIntoBooster = 0.33f;

    protected bool _isCubeCheckedBefore = false;
    private CubeState _cubeState;

    public override void Explode()
    {
        BoardManager.Instance.ReverseShapesSprite();
        Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);
        BoardManager.Instance.GetInstantiatedShapes()[_row, _col] = null;
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
    }

    public override void SetMergeSprite(int count)
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

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (BoardManager.Instance.isMovesLeft() &&
            BoardManager.Instance.GetGameState() == GameState.Ready &&
            _shapeState == ShapeState.Waiting)
        {
            _adjacentShapes = new List<Shape>();
            FindAdjacentShapes(true, _adjacentShapes);

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

        if (cubeOperation == CubeOperation.BasicExplosion)
            BasicExplosionOperation();
        else if (cubeOperation == CubeOperation.TurnIntoRocket)
            TurnIntoRocketOperation();
        else if (cubeOperation == CubeOperation.TurnIntoBomb)
            TurnIntoBombOperation();
        else if (cubeOperation == CubeOperation.TurnIntoDisco)
            TurnIntoDiscoOperation();
        else
            FailOperation();

    }

    private void BasicExplosionOperation()
    {
        BoardManager.Instance.DecreaseRemainingMoves();
        foreach (Shape shape in BoardManager.Instance.GetAdjacentShapes())
        {
            BoardManager.Instance.RemoveFromInstantiatedShapes(shape._row, shape._col);
            shape.Explode();
        }

        BoardManager.Instance.StartShiftDown();
    }

    private void TurnIntoBooster()
    {
        BoardManager.Instance.GetAdjacentShapes()[0].Merge();
        StartCoroutine(WaitForShiftDownAfterMerge());
    }

    private void FailOperation()
    {
        BoardManager.Instance.GetAdjacentShapes().Clear();
        FailAnimation();
    }

    private IEnumerator WaitForShiftDownAfterMerge()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        BoardManager.Instance.GetAdjacentShapes().Remove(this);
        BoardManager.Instance.SetGameState(GameState.Ready);
        BoardManager.Instance.StartShiftDown();
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

        if(rockets != null)
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

    private void TurnIntoRocketOperation()
    {
        BoardManager.Instance.DecreaseRemainingMoves();
        TurnIntoBooster();
        StartCoroutine(TurnIntoRocket());
    }

    private void TurnIntoBombOperation()
    {
        BoardManager.Instance.DecreaseRemainingMoves();
        TurnIntoBooster();
        StartCoroutine(TurnIntoBomb());
    }

    private void TurnIntoDiscoOperation()
    {
        BoardManager.Instance.DecreaseRemainingMoves();

        TurnIntoBooster();

        ShapeColor sc = ShapeColor.None;

        if (_shapeData.ShapeColor == ShapeColor.Red)
            sc = ShapeColor.Red;
        else if (_shapeData.ShapeColor == ShapeColor.Blue)
            sc = ShapeColor.Blue;
        else if (_shapeData.ShapeColor == ShapeColor.Green)
            sc = ShapeColor.Green;

        StartCoroutine(TurnIntoDisco(ShapeType.Disco, sc));
    }

    private IEnumerator TurnIntoRocket()
    {
        yield return new WaitForSeconds(TimeToTurnIntoBooster);
        Rocket rocket = gameObject.AddComponent<Rocket>();
        rocket.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Rocket, ShapeColor.None), _row, _col);
        BoardManager.Instance.ReloadShapeToList(rocket, _row, _col);
        BoardManager.Instance.SetGameState(GameState.Ready);
        Instantiate(BoardManager.Instance.RocketMergeEffect, transform.position, transform.rotation, rocket.transform);
        Destroy(gameObject.GetComponent<Cube>());
    }

    private IEnumerator TurnIntoBomb()
    {
        yield return new WaitForSeconds(TimeToTurnIntoBooster);
        Bomb bomb = gameObject.AddComponent<Bomb>();
        bomb.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Bomb, ShapeColor.None), _row, _col);
        BoardManager.Instance.ReloadShapeToList(bomb, _row, _col);
        Instantiate(BoardManager.Instance.BombMergeEffect, transform.position, transform.rotation, bomb.transform);
        BoardManager.Instance.SetGameState(GameState.Ready);
        Destroy(gameObject.GetComponent<Cube>());
    }

    private IEnumerator TurnIntoDisco(ShapeType shapeType, ShapeColor shapeColor)
    {
        yield return new WaitForSeconds(TimeToTurnIntoBooster);
        Disco disco = gameObject.AddComponent<Disco>();
        disco.SetShapeData(BoardManager.Instance.GetShapeData(shapeType, shapeColor), _row, _col);
        BoardManager.Instance.ReloadShapeToList(disco, _row, _col);
        Instantiate(BoardManager.Instance.DiscoMergeEffect, transform.position, transform.rotation, disco.transform);
        BoardManager.Instance.SetGameState(GameState.Ready);
        Destroy(gameObject.GetComponent<Cube>());
    }
}
