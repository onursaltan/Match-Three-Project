using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Rocket : Booster
{
    private const float TimeBetweenExplosions = 0.05f;

    private bool _isDirectionVertical;

    public override void Explode()
    {
        if (_shapeState != ShapeState.Explode)
        {
            Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
            BoardManager.Instance.SetGameState(GameState.RocketExplosion);

            _spriteRenderer.enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            if (_isDirectionVertical)
                ExplodeAllColumn(_col, transform.position);
            else
                ExplodeAllRow(_row, transform.position);

            instantiatedShapes[_row, _col] = null;
            StartCoroutine(WaitStartShift());
        }
    }

    public override void Merge()
    {
        if (_boosterMerge == BoosterMerge.None)
        {
            BoardManager.Instance.IncreaseDistinctColumns(_col);
            Explode();
        }
        else if (_boosterMerge == BoosterMerge.DoubleRocket)
        {
            foreach (Shape shape in _adjacentBoosters)
            {
                BoardManager.Instance.IncreaseDistinctColumns(shape._col);
                shape.MoveToMergePoint(_row, _col);
            }

            StartCoroutine(WaitForExplodeDoubleRocket());
        }
        else if (_boosterMerge == BoosterMerge.BombWithRocket)
        {
            foreach (Shape shape in _adjacentBoosters)
            {
                BoardManager.Instance.IncreaseDistinctColumns(shape._col);
                shape.MoveToMergePoint(_row, _col);
            }

            StartCoroutine(WaitForExplodeRocketWithBomb());
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (BoardManager.Instance.isMovesLeft() &&
            BoardManager.Instance.GetGameState() == GameState.Ready &&
            _shapeState == ShapeState.Waiting)
        {
            BoardManager.Instance.DecreaseRemainingMoves();
            _boosterMerge = GetBoosterMerge();
            Merge();
        }
    }

    public override void SetShapeData(ShapeData shapeData, int row, int col)
    {
        base.SetShapeData(shapeData, row, col);
        _isDirectionVertical = GetRandomBool();

        if (!_isDirectionVertical)
            transform.Rotate(new Vector3(0f, 0f, 90f));
    }

    private bool GetRandomBool()
    {
        return Random.Range(0, 2) == 1;
    }

    private void ExplodeAllColumn(int col, Vector3 position)
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
        ExplosionAnimation(true, position);

        int index = 0;

        for (int i = _row + 1; i < BoardManager.Instance.GetRowCount(); i++)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[i, col], index++));

        index = 0;

        for (int i = _row - 1; i >= 0; i--)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[i, col], index++));
    }

    private void ExplodeAllRow(int row, Vector3 position)
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
        ExplosionAnimation(false, position);

        int index = 0;

        for (int i = _col + 1; i < BoardManager.Instance.GetColumnCount(); i++)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[row, i], index++));

        index = 0;

        for (int i = _col - 1; i >= 0; i--)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[row, i], index++));
    }

    private IEnumerator WaitForExplodeDoubleRocket()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        ExplodeDoubleRocket();
    }

    private void ExplodeDoubleRocket()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
        BoardManager.Instance.SetGameState(GameState.RocketExplosion);

        _spriteRenderer.enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;

        instantiatedShapes[_row, _col] = null;

        ExplodeAllColumn(_col, transform.position);
        ExplodeAllRow(_row, transform.position);

        StartCoroutine(WaitStartShift());
    }

    private IEnumerator WaitForExplodeRocketWithBomb()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        ExplodeRocketWithBomb();
    }

    private void ExplodeRocketWithBomb()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
        BoardManager.Instance.SetGameState(GameState.RocketExplosion);

        _spriteRenderer.enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;

        instantiatedShapes[_row, _col] = null;

        Vector2 offset = _spriteRenderer.bounds.size;
        Vector3 newPosHorizontal;
        Vector3 newPosVertical;

        for (int i = -1; i < 2; i++)
        {
            if(_col + i >= 0 && _col + i < BoardManager.Instance.GetColumnCount())
            {
                newPosVertical = transform.position;
                newPosVertical.x += offset.x * i;
                ExplodeAllColumn(_col + i, newPosVertical);
            }
            if (_row + i >= 0 && _row + i < BoardManager.Instance.GetRowCount())
            {
                newPosHorizontal = transform.position;
                newPosHorizontal.y += offset.y * i;
                ExplodeAllRow(_row + i, newPosHorizontal);
            }
        }

        StartCoroutine(WaitStartShift());
    }

    private IEnumerator WaitExplodeShape(Shape shape, int index)
    {
        yield return new WaitForSeconds(TimeBetweenExplosions * (float)index);

        if (shape != null)
        {
            shape.Explode();
            BoardManager.Instance.IncreaseDistinctColumns(shape._col);
        }
    }

    private IEnumerator WaitStartShift()
    {
        int rowCount = BoardManager.Instance.GetRowCount();
        yield return new WaitForSeconds(TimeBetweenExplosions * rowCount);
        BoardManager.Instance.SetGameState(GameState.Ready);
        BoardManager.Instance.StartShiftDown();
        BoardManager.Instance.GetExplodedRows().Clear();
        Destroy(gameObject, 0.75f);
    }

    private void ExplosionAnimation(bool isDirectionVertical, Vector3 position)
    {
        Vector3 point1 = position;
        Vector3 point2 = position;

        GameObject s = Instantiate(_shapeData.ExplodeEffect, position, transform.rotation, transform.parent);
        GameObject s1 = Instantiate(_shapeData.ExplodeEffect, position, transform.rotation, transform.parent);

        if (isDirectionVertical)
        {
            point1.y += 6;
            point2.y -= 6;
        }
        else
        {
            point1.x += 6;
            point2.x -= 6;
        }

        s.transform.DOMove(point1, 1.1f);
        s1.transform.DOMove(point2, 1.1f);
    }

    public override void SetMergeSprite(int count)
    {
        throw new System.NotImplementedException();
    }
}
