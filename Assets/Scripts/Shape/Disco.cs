using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

public class Disco : Booster
{
    private const float TimeToTrailWait = 1.0f;
    private const float TimeToTrailReach = 0.6f;

    private GameObject Anticipation;
    private GameObject Explosion;
    private GameObject Trail;
    private GameObject BigExplosion;
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

    public override void SetShapeData(ShapeData shapeData, int row, int col)
    {
        base.SetShapeData(shapeData, row, col);
        Anticipation = _shapeData.ExplodeEffect.transform.Find("Anticipation").gameObject;
        Explosion = _shapeData.ExplodeEffect.transform.Find("Main Explosion").gameObject;
        Trail = _shapeData.ExplodeEffect.transform.Find("Trails").gameObject;
    }

    public override void Merge()
    {
        foreach (Shape shape in _adjacentBoosters)
        {
            shape.MoveToMergePoint(_row, _col);
        }
    }

    public IEnumerator WaitForBigLightBall()
    {
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        BigLightBall();
    }

    private void BigLightBall()
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        foreach (Shape shape in instantiatedShapes)
        {
            if (shape != null)
            {
                if (shape.GetType() == typeof(Cube))
                    shape.Explode();
                else
                    if (shape != this)
                    BoardManager.Instance.DestroyShape(shape);
            }
        }

        GameObject BigBombInstance = Instantiate(BoardManager.Instance.GreatBombEffect, new Vector3(-0.0146000003f, -0.950800002f, 0f), transform.rotation, transform.parent);

        CameraShake.Shake();
        _spriteRenderer.enabled = false;
        instantiatedShapes[_row, _col] = null;
    }

    public IEnumerator WaitForLightBallWithBomb()
    {
        FindSameColor(_shapeData.ShapeColor);
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        StartCoroutine(LightBallWithBomb());
    }

    private IEnumerator LightBallWithBomb()
    {
        List<Shape> instantiatedShapes = BoardManager.Instance.Array2DToList(BoardManager.Instance.GetInstantiatedShapes());
        List<Shape> cubes = instantiatedShapes.FindAll(shape => shape != null && shape._shapeData.ShapeType == ShapeType.Cube && shape._shapeData.ShapeColor == _shapeData.ShapeColor);
        List<Bomb> bombs = new List<Bomb>();

        foreach (Cube cube in cubes)
        {
            yield return new WaitForSeconds(0.1f);
            MoveTrailToTarget(cube);
            cube.ConvertCubeToBomb(TimeToTrailReach, bombs);
        }

        yield return new WaitForSeconds(cubes.Count * 0.1f + 0.2f);
        GameObject explosionInstance = Instantiate(Explosion, transform.position, transform.rotation, transform.parent);

        _spriteRenderer.enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;

        Shape[,] instantiatedShapess = BoardManager.Instance.GetInstantiatedShapes();
        instantiatedShapess[_row, _col] = null;

        foreach (Bomb bomb in bombs)
        {
            yield return new WaitForSeconds(0.1f);
            bomb.Explode();
        }
    }

    public IEnumerator WaitForLightBallWithRocket()
    {
        FindSameColor(_shapeData.ShapeColor);
        yield return new WaitForSeconds(TimeToExpandIn + TimeToExpandOut);
        StartCoroutine(LightBallWithRocket());
    }

    private IEnumerator LightBallWithRocket()
    {
        List<Shape> instantiatedShapes = BoardManager.Instance.Array2DToList(BoardManager.Instance.GetInstantiatedShapes());
        List<Shape> cubes = instantiatedShapes.FindAll(shape => shape != null && shape._shapeData.ShapeType == ShapeType.Cube && shape._shapeData.ShapeColor == _shapeData.ShapeColor);
        List<Rocket> rockets = new List<Rocket>();

        foreach (Cube cube in cubes)
        {
            yield return new WaitForSeconds(0.1f);
            MoveTrailToTarget(cube);
            cube.ConvertCubeToRocket(TimeToTrailReach, rockets);
        }

        yield return new WaitForSeconds(cubes.Count * 0.1f + 0.2f);

        GameObject explosionInstance = Instantiate(Explosion, transform.position, transform.rotation, transform.parent);

        _spriteRenderer.enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;

        Shape[,] instantiatedShapess = BoardManager.Instance.GetInstantiatedShapes();
        instantiatedShapess[_row, _col] = null;

        foreach (Rocket rocket in rockets)
        {
            yield return new WaitForSeconds(0.1f);
            rocket.Explode();
        }
    }

    private IEnumerator DiscoExplode()
    {
        _spriteRenderer.sortingOrder = 99;
        FindSameColor(this._shapeData.ShapeColor);
        GameObject anticipationInstance = Instantiate(Anticipation, transform.position, transform.rotation, transform.parent);

        //MOVING AND DESTROYING TRAILS
        foreach (Cube shape in toBeExploded)
        {
            yield return new WaitForSeconds(0.1f);

            if (shape != null)
            {
                MoveTrailToTarget(shape);
                StartCoroutine(shape.DiscoExplosion(TimeToTrailReach));
            }
        }

        //Anticipation.GetComponent<CubeExplosion>().TimeToDestroy = (toBeExploded.Count * 0.1f) + TimeToTrailReach + TimeToTrailWait;

        yield return new WaitForSeconds(TimeToTrailReach + TimeToTrailWait);

        foreach (Cube shape in toBeExploded)
            if (shape != null)
                shape.Explode();

        StartCoroutine(DiscoPopEffect(anticipationInstance));
        
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();
        _spriteRenderer.enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        instantiatedShapes[_row, _col] = null;
    }

    public List<Shape> GetToBeExploded()
    {
        return toBeExploded;
    }


    private IEnumerator DiscoPopEffect(GameObject antInstance)
    {
        Destroy(antInstance);
        GameObject explosionInstance = Instantiate(Explosion, transform.position, transform.rotation, transform.parent);
        yield return new WaitForSeconds(1f);
        Destroy(explosionInstance);
    }

    private IEnumerator MakeMoveTrails(List<Shape> trailTargets)
    {
        foreach (Cube shape in trailTargets)
        {
            yield return new WaitForSeconds(0.1f);

            if (shape != null)
            {
                GameObject trailInstance = Instantiate(Trail, transform.position, transform.rotation, transform.parent);
                trailsInstantiated.Add(trailInstance);
                trailInstance.transform.DOMove(shape.transform.position, TimeToTrailReach);
                shape.ConvertCubeToRocket(TimeToTrailReach, null);
                DestroyGameobjectAfterSeconds(trailInstance, TimeToTrailReach + TimeToTrailWait);
            }
        }
    }

    private void MoveTrailToTarget(Cube shape)
    {
        GameObject trailInstance = Instantiate(Trail, transform.position, transform.rotation, transform.parent);
        trailsInstantiated.Add(trailInstance);
        trailInstance.transform.DOMove(shape.transform.position, TimeToTrailReach);
        DestroyGameobjectAfterSeconds(trailInstance, TimeToTrailReach + TimeToTrailWait);
    }

    private void FindSameColor(ShapeColor color)
    {
        Shape[,] instantiatedShapes = BoardManager.Instance.GetInstantiatedShapes();

        foreach (Shape shape in instantiatedShapes)
        {
            if (shape != null && shape._shapeData.ShapeColor == color && shape._shapeData.ShapeType == ShapeType.Cube)
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
