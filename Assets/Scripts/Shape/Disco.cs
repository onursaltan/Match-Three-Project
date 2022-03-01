using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Disco : Shape
{
    private const float TimeToTrailWait = 1.0f;
    private const float TimeToTrailReach = 0.6f;
    private GameObject Anticipation;
    private GameObject Explosion;
    private GameObject Trail;
    private List<Shape> toBeExploded = new List<Shape>();
    private List<GameObject> trailsInstantiated = new List<GameObject>();

    public override void Explode()
    {
        if (_shapeState != ShapeState.Explode)
        {
            BoardManager.Instance.SetGameState(GameState.DiscoExplosion);
            _shapeState = ShapeState.Explode;
            StartCoroutine(DiscoExplode());
        }
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
        Anticipation = _shapeData.ExplodeEffect.transform.Find("Anticipation").gameObject;
        Explosion = _shapeData.ExplodeEffect.transform.Find("Main Explosion").gameObject;
        Trail = _shapeData.ExplodeEffect.transform.Find("Trails").gameObject;
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
        BoardManager.Instance.SetGameState(GameState.Ready, true);
        BoardManager.Instance.StartShiftDown();
        BoardManager.Instance.GetExplodedRows().Clear();
        Destroy(gameObject, 0.75f);
    }

    private IEnumerator DiscoExplode()
    {
        _spriteRenderer.sortingOrder = 99;
        FindSameColor(this._shapeData.ShapeColor);
        Instantiate(Anticipation, transform.position, transform.rotation, transform.parent);

        //MOVING AND DESTROYING TRAILS
        foreach (Cube shape in toBeExploded)
        {
            yield return new WaitForSeconds(0.1f);
            if (shape != null)
            {
                GameObject trailInstance = Instantiate(Trail, transform.position, transform.rotation, transform.parent);
                trailsInstantiated.Add(trailInstance);
                trailInstance.transform.DOMove(shape.transform.position, TimeToTrailReach);
                BoardManager.Instance.IncreaseDistinctColumns(shape._col);
                shape.DiscoExplosion(TimeToTrailReach + TimeToTrailWait);
                DestroyGameobjectAfterSeconds(trailInstance, TimeToTrailReach + TimeToTrailWait);
            }
        }

        Anticipation.GetComponent<CubeExplosion>().TimeToDestroy = (toBeExploded.Count * 0.1f) + TimeToTrailReach + TimeToTrailWait;

        yield return new WaitForSeconds(TimeToTrailReach + TimeToTrailWait);

        foreach (Cube shape in toBeExploded)
            if (shape != null)
                shape.Explode();

        Instantiate(Explosion, transform.position, transform.rotation, transform.parent);
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
        _spriteRenderer.enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        instantiatedShapes[_row, _col] = null;
        StartCoroutine(WaitStartShift());
    }

    private void FindSameColor(ShapeColor color)
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
        foreach (Shape shape in instantiatedShapes)
        {
            if (shape != null && shape._shapeData.ShapeColor == color && shape._shapeData.ShapeType != ShapeType.Disco)
            {
                toBeExploded.Add(shape);
            }
        }
    }

    private IEnumerator _DestroyGameobjectAfterSeconds(GameObject gameObject, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }

    private void DestroyGameobjectAfterSeconds(GameObject gameObject, float seconds)
    {
        StartCoroutine(_DestroyGameobjectAfterSeconds(gameObject, seconds));
    }

}
