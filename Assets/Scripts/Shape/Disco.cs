using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Disco : Shape
{
    private GameObject Anticipation;
    private GameObject Explosion;
    private GameObject Trail;

    public override void Explode()
    {
        if (_shapeState != ShapeState.Explode)
        {
            Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
            BoardManager.Instance.gameState = GameState.BoosterExplosion;

            _shapeState = ShapeState.Explode;
            _spriteRenderer.enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            ExplodeSameColor();

            instantiatedShapes[_row, _col] = null;
            StartCoroutine(WaitStartShift());
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

            BoardManager.Instance.IncreaseDistinctColumns(_col);
            Explode();
    }

    public override void SetShapeData(ShapeData shapeData, int row, int col)
    {
        base.SetShapeData(shapeData, row, col);
        Anticipation = _shapeData.ExplodeEffect.transform.Find("Anticipation").gameObject;
    }

    public override void Merge()
    {
        throw new System.NotImplementedException();
    }

    public override void SetMergeSprite(int count)
    {
        throw new System.NotImplementedException();
    }

    private IEnumerator WaitStartShift()
    {
        int rowCount = BoardManager.Instance.GetRowCount();
        yield return new WaitForSeconds(0.05f * rowCount);
        BoardManager.Instance.StartShiftDown();
        BoardManager.Instance.GetExplodedRows().Clear();
        BoardManager.Instance.gameState = GameState.Ready;
        Destroy(gameObject, 0.75f);
    }

    private void ExplodeSameColor()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        foreach (Shape shape in instantiatedShapes)
        {
            if (shape._shapeData.ShapeType == ShapeType.BlueCube)
            {
                shape.Explode();
                BoardManager.Instance.IncreaseDistinctColumns(shape._col);
            }
        }
    }
}
