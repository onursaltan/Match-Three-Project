using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

enum CubeOperation
{
    BasicExplosion, TurnIntoRocket, TurnIntoBomb, TurnIntoDisco, Fail
}

public class Cube : Shape
{
    private const float TimeToWaitTurn = 0.03f;
    private const float TimeToExpandOut = 0.2f;
    private const float TimeToExpandIn = 0.1f;
    private const float ExpandRateScale = 1.08f;
    private const float ExpandRatePosition = 0.2f;

    public override void Explode()
    {
        Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);
        BoardManager.Instance.GetInstantiatedShapes()[_row, _col] = null;
        Destroy(gameObject);
    }

    public override void Merge()
    {
        foreach (Cube cube in BoardManager.Instance.GetAdjacentShapes())
            cube.MoveToMergePoint(_row, _col);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        CheckAdjacentShapes(true);
        HandleCubeOperation();
    }

    private CubeOperation HandleCubeOperation()
    {
        int adjacentShapesCount = BoardManager.Instance.GetAdjacentShapes().Count;

        if (adjacentShapesCount > 1 && adjacentShapesCount < 5)
        {
            BasicExplosionOperation();
            return CubeOperation.BasicExplosion;
        }
        else if (adjacentShapesCount == 5 || adjacentShapesCount <= 25)
        {
            TurnIntoRocketOperation();
            return CubeOperation.TurnIntoRocket;
        }
        else if (adjacentShapesCount == 7 || adjacentShapesCount == 8)
        {
            TurnIntoBombOperation();
            return CubeOperation.TurnIntoBomb;
        }
        else if(adjacentShapesCount >= 9)
        {
            TurnIntoDiscoOperation();
            return CubeOperation.TurnIntoDisco;
        }
        else
        {
            FailOperation();
            return CubeOperation.Fail;
        }
    }

    private void BasicExplosionOperation()
    {
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
        StartCoroutine(WaitForShiftDown());
    }

    private void FailOperation()
    {
        BoardManager.Instance.GetAdjacentShapes().Clear();
        FailAnimation();
    }

    private IEnumerator WaitForShiftDown()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        BoardManager.Instance.GetAdjacentShapes().Remove(this);
        BoardManager.Instance.StartShiftDown();
    }

    private void MoveToMergePoint(int row, int col)
    {
        Vector2 offset = _shapeSpriteRenderer.bounds.size;
        _shapeSpriteRenderer.sortingOrder = 98;

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
                    if (!(row == _row && col == _col))  // Bura deðiþcek
                        BoardManager.Instance.DestroyShape(this);

                });
            });


        transform.DOScale(new Vector3(transform.localScale.x * ExpandRateScale, transform.localScale.y * ExpandRateScale), TimeToExpandOut).SetEase(Ease.OutSine).OnComplete(() =>
        {
            transform.DOScale(new Vector3(localScaleX, localScaleY), TimeToExpandIn);
        });
    }

    private void FailAnimation()
    {

    }

    private void TurnIntoRocketOperation()
    {
        TurnIntoBooster();
        StartCoroutine(TurnIntoRocket());
    }

    private void TurnIntoBombOperation()
    {
        TurnIntoBooster();
        StartCoroutine(TurnIntoBomb());
    }

    private void TurnIntoDiscoOperation()
    {
        TurnIntoBooster();
        StartCoroutine(TurnIntoDisco());
    }

    private IEnumerator TurnIntoRocket()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut + TimeToWaitTurn);
        Rocket rocket = gameObject.AddComponent<Rocket>();
        rocket.SetShapeData(BoardManager.Instance.RocketShapeData, _row, _col);
        BoardManager.Instance.ReloadShapeToList(rocket, _row, _col);
        Destroy(gameObject.GetComponent<Cube>());
    }

    private IEnumerator TurnIntoBomb()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut + TimeToWaitTurn);
        Bomb bomb = gameObject.AddComponent<Bomb>();
        bomb.SetShapeData(BoardManager.Instance.BombShapeData, _row, _col);
        Destroy(gameObject.GetComponent<Cube>());
    }

    private IEnumerator TurnIntoDisco()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut + TimeToWaitTurn);
        Disco disco = gameObject.AddComponent<Disco>();
        disco.SetShapeData(BoardManager.Instance.DiscoShapeData, _row, _col);
        Destroy(gameObject.GetComponent<Cube>());
    }
}
