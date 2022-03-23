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
            BoardManager.Instance.GetInstantiatedShapes()[_row, _col] = null;
            _shapeState = ShapeState.Explode;
            BoardManager.Instance.SetGameState(GameState.RocketExplosion);

            _spriteRenderer.enabled = false;
            _boxCollider2D.enabled = false;

            if (_isDirectionVertical)
                ExplodeAllColumn(_col, transform.position);
            else
                ExplodeAllRow(_row, transform.position);
        }
    }

    public override void Merge()
    {
        foreach (Shape shape in _adjacentBoosters)
            shape.MoveToMergePoint(_row, _col);
    }

    public override void SetShapeData(ShapeData shapeData, int row, int col, bool isForBoosterMerge = false)
    {
        base.SetShapeData(shapeData, row, col, isForBoosterMerge);
        _isDirectionVertical = GetRandomBool();

        if (!_isDirectionVertical)
            transform.Rotate(new Vector3(0f, 0f, 90f));
    }

    public void ExplodeAfterWait(float timeToWait)
    {
        StartCoroutine(WaitForExplode(timeToWait));
    }

    private IEnumerator WaitForExplode(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        Explode();
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

        List<int> columns = new List<int>
        {
            col
        };

        StartCoroutine(WaitToShiftRoutine(0.55f, columns));
    }

    private IEnumerator WaitToShiftRoutine(float time, List<int> columns)
    {
        yield return new WaitForSeconds(time);

        BoardManager.Instance.StartShiftDown(columns);
    }

    private void ExplodeAllRow(int row, Vector3 position)
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
        ExplosionAnimation(false, position);

        List<int> columns = new List<int>();

        int index = 0;

        for (int i = _col + 1; i < BoardManager.Instance.GetColumnCount(); i++)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[row, i], index++));

        index = 0;

        for (int i = _col - 1; i >= 0; i--)
            StartCoroutine(WaitExplodeShape(instantiatedShapes[row, i], index++));

        for (int i = 0; i < BoardManager.Instance.GetColumnCount(); i++)
            columns.Add(i);

        StartCoroutine(WaitToShiftRoutine(0.55f, columns));
    }

    public IEnumerator WaitForExplodeDoubleRocket()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        ExplodeDoubleRocket();
    }

    private void ExplodeDoubleRocket()
    {
        BoardManager.Instance.GetInstantiatedShapes()[_row, _col] = null;

        _spriteRenderer.enabled = false;
        _boxCollider2D.enabled = false;

        ExplodeAllColumn(_col, transform.position);
        ExplodeAllRow(_row, transform.position); 
    }

    public IEnumerator WaitForExplodeRocketWithBomb()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        StartCoroutine(ExplodeRocketWithBomb());
    }

    public IEnumerator ExplodeRocketWithBomb()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
        
        _spriteRenderer.enabled = false;
        _boxCollider2D.enabled = false;

        GameObject RocketBombEffect = Instantiate(BoardManager.Instance.RocketBombMerge, transform.position, Quaternion.identity, transform);
        yield return new WaitForSeconds(1f);
        Destroy(RocketBombEffect);

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
    }

    private IEnumerator WaitExplodeShape(Shape shape, int index)
    {
        yield return new WaitForSeconds(TimeBetweenExplosions * (float)index);

        if (shape != null)
            shape.Explode();
    }

    private void ExplosionAnimation(bool isDirectionVertical, Vector3 position)
    {
        Vector3 point1 = position;
        Vector3 point2 = position;

        GameObject s = Instantiate(_shapeData.ExplodeEffect, position, transform.rotation, transform.parent);
        GameObject s1 = Instantiate(_shapeData.ExplodeEffect, position, transform.rotation, transform.parent);

        GameObject halfRocket1 = s.transform.Find("Half Rocket").gameObject;
        GameObject halfRocket2 = s1.transform.Find("Half Rocket").gameObject;

        if (isDirectionVertical)
        {
            halfRocket1.transform.Rotate(0f, 0f, 180f);
            halfRocket2.transform.Rotate(0f, 0f, 0f);
            point1.y += 12;
            point2.y -= 12;
        }
        else
        {
            halfRocket1.transform.Rotate(0f, 0f, 0f);
            halfRocket2.transform.Rotate(0f, 0f, 180f);
            point1.x += 12;
            point2.x -= 12;
        }

        s.transform.DOMove(point1, 2.2f);
        s1.transform.DOMove(point2, 2.2f);
    }
}
