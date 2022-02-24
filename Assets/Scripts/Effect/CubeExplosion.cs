using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeExplosion : MonoBehaviour
{
    public float TimeToDestroy;

    void Start()
    {
        StartCoroutine(DestroyEntity());
    }

    private IEnumerator DestroyEntity()
    {
        yield return new WaitForSeconds(TimeToDestroy);
        Destroy(gameObject);
    }
}
