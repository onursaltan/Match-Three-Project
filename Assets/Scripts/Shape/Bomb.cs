using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bomb : Shape
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
            StartCoroutine(WaitStartShift());
        }
    }

    public override void Merge()
    {
        throw new System.NotImplementedException();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (BoardManager.Instance.isMovesLeft() && BoardManager.Instance.GetGameState() == GameState.Ready)
        {
            BoardManager.Instance.IncreaseDistinctColumns(_col);
            Explode();
            BoardManager.Instance.DecreaseRemainingMoves();
        }
    }
    public override void SetShapeData(ShapeData shapeData, int row, int col)
    {
        base.SetShapeData(shapeData, row, col);
    }

    private IEnumerator WaitStartShift()
    {
        int rowCount = BoardManager.Instance.GetRowCount();
        yield return new WaitForSeconds(0.05f * rowCount);
        BoardManager.Instance.SetGameState(GameState.Ready);
        BoardManager.Instance.StartShiftDown();
        Destroy(gameObject, 0.75f);
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
                        BoardManager.Instance.IncreaseDistinctColumns(_col + j);
                    }
                }
            }
        }
    }

    public override void SetMergeSprite(int count)
    {
        throw new System.NotImplementedException();
    }
}
