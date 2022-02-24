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
    private const float TimeToWaitTurn = 0.03f;
    private const float TimeToExpandOut = 0.2f;
    private const float TimeToExpandIn = 0.1f;
    private const float TimeToTurnIntoBooster = 0.33f;
    private const float ExpandRateScale = 1.08f;
    private const float ExpandRatePosition = 0.2f;

    protected bool _isCubeCheckedBefore = false;
    private CubeState _cubeState;

    public override void Explode()
    {
        Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);
        BoardManager.Instance.GetInstantiatedShapes()[_row, _col] = null;
        Destroy(gameObject);
    }

    public override void Merge()
    {
        _shapeState = ShapeState.Merging;
        BoardManager.Instance.gameState = GameState.Merging;

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
        else if(_shapeData.ShapeColor == ShapeColor.Red)
            color = "red";
        else if (_shapeData.ShapeColor == ShapeColor.Green)
            color = "green";

        _spriteRenderer.sprite = BoardManager.Instance.GetMergeSprite(FindCubeOperation(count), color);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (BoardManager.Instance.isMovesLeft() &&
            BoardManager.Instance.gameState == GameState.Ready &&
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
        else if (cubeOperation == CubeOperation.BasicExplosion)
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
        BoardManager.Instance.StartShiftDown();
    }

    private void MoveToMergePoint(int row, int col)
    {
        Vector2 offset = _spriteRenderer.bounds.size;
        _shapeState = ShapeState.Merging;
        _spriteRenderer.sortingOrder = 98;

        int directionX = col - _col;
        int directionY = row - _row;

        float posX = transform.position.x + directionX * offset.x;
        float posY = transform.position.y + directionY * offset.y;

        float expandX = transform.position.x + ExpandRatePosition * offset.x * -1 * directionX;
        float expandY = transform.position.y + ExpandRatePosition * offset.y * -1 * directionY;

        float localScaleX = transform.localScale.x;
        float localScaleY = transform.localScale.y;

        transform.DOMove(new Vector3(expandX, expandY), TimeToExpandOut).
            SetEase(Ease.OutSine).
            OnComplete(() =>
            {
                transform.DOMove(new Vector3(posX, posY), TimeToExpandIn).OnComplete(() =>
                {
                    if (!(row == _row && col == _col))  // Bura de?i?cek
                        BoardManager.Instance.DestroyShape(this);

                    _shapeState = ShapeState.Waiting;
                });
            });

        transform.DOScale(new Vector3(transform.localScale.x * ExpandRateScale, transform.localScale.y * ExpandRateScale), TimeToExpandOut).SetEase(Ease.OutSine).OnComplete(() =>
        {
            transform.DOScale(new Vector3(localScaleX, localScaleY), TimeToExpandIn);
        });
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
        else if(_shapeData.ShapeColor == ShapeColor.Blue)
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
        BoardManager.Instance.gameState = GameState.Ready;
        Instantiate(BoardManager.Instance.RocketMergeEffect, transform.position, transform.rotation, transform.parent);
        Destroy(gameObject.GetComponent<Cube>());
    }

    private IEnumerator TurnIntoBomb()
    {
        yield return new WaitForSeconds(TimeToTurnIntoBooster);
        Bomb bomb = gameObject.AddComponent<Bomb>();
        bomb.SetShapeData(BoardManager.Instance.GetShapeData(ShapeType.Bomb, ShapeColor.None), _row, _col);
        BoardManager.Instance.ReloadShapeToList(bomb, _row, _col);
        Instantiate(BoardManager.Instance.BombMergeEffect, transform.position, transform.rotation, transform.parent);
        BoardManager.Instance.gameState = GameState.Ready;
        Destroy(gameObject.GetComponent<Cube>());
    }

    private IEnumerator TurnIntoDisco(ShapeType shapeType, ShapeColor shapeColor)
    {
        yield return new WaitForSeconds(TimeToTurnIntoBooster);
        Disco disco = gameObject.AddComponent<Disco>();
        disco.SetShapeData(BoardManager.Instance.GetShapeData(shapeType, shapeColor), _row, _col);
        BoardManager.Instance.ReloadShapeToList(disco, _row, _col);
        Instantiate(BoardManager.Instance.DiscoMergeEffect, transform.position, transform.rotation, transform.parent);
        BoardManager.Instance.gameState = GameState.Ready;
        Destroy(gameObject.GetComponent<Cube>());
    }
}
