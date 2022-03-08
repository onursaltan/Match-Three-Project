using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bomb : Booster
{

    public override void Explode()
    {
        if (_shapeState != ShapeState.Explode)
        {
            Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
            BoardManager.Instance.SetGameState(GameState.BombExplosion);

            _shapeState = ShapeState.Explode;
            _spriteRenderer.enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            Explode3x3();

            Instantiate(_shapeData.ExplodeEffect, transform.position, transform.rotation, transform.parent);
            CameraShake.Shake(0.5f, 5f);
            instantiatedShapes[_row, _col] = null;
        }
    }

    public override void Merge()
    {
        foreach (Shape shape in _adjacentBoosters)
            shape.MoveToMergePoint(_row, _col);
    }

    public override void SetShapeData(ShapeData shapeData, int row, int col)
    {
        base.SetShapeData(shapeData, row, col);
    }

 
    private void Explode3x3()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (!(i == 0 && j == 0) && !(_row + i < 0 || _col + j < 0 || _row + i > BoardManager.Instance.rows - 1 || _col + j > BoardManager.Instance.columns - 1))
                {
                    if (instantiatedShapes[_row + i, _col + j] != null)
                    {
                        instantiatedShapes[_row + i, _col + j].Explode();
                    }
                }
            }
        }
    }

    public IEnumerator WaitForExplode5x5()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        Explode5x5();
    }

    private void Explode5x5()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        for (int i = -2; i < 3; i++)
        {
            for (int j = -2; j < 3; j++)
            {
                if (!(i == 0 && j == 0) && !(_row + i < 0 || _col + j < 0 || _row + i > BoardManager.Instance.rows - 1 || _col + j > BoardManager.Instance.columns - 1))
                {
                    if (instantiatedShapes[_row + i, _col + j] != null)
                    {
                        instantiatedShapes[_row + i, _col + j].Explode();
                    }
                }
            }
        }

        _shapeState = ShapeState.Explode;
        _spriteRenderer.enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;

        Instantiate(BoardManager.Instance.BigBombEffect, transform.position, transform.rotation, transform.parent);

        CameraShake.Shake(1.25f, 6f);
        instantiatedShapes[_row, _col] = null;
    }
}
