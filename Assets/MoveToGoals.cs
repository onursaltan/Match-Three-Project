using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveToGoals : MonoBehaviour
{

    public GameObject target;
    Vector3[] path = new Vector3[3];

    [SerializeField] public Sprite[] sprites;
    [SerializeField] GameObject child;

    float lifeTime;

    // Start is called before the first frame update
    void Start()
    {
        path[0] = transform.position;
        path[1] = new Vector3(transform.position.x + Random.Range(-1f,1f), transform.position.y - 2 + Random.Range(-0.7f,0.7f), transform.position.z); ;
        path[2] = target.transform.position;
        lifeTime = Random.Range(0.4f, 0.6f);
        transform.DOPath(path, lifeTime, PathType.CatmullRom).SetEase(Ease.InCubic);
        StartCoroutine(DestroyAfterSeconds(lifeTime));
    }

    public void Init(ShapeColor color)
    {
        switch (color)
        {
            case ShapeColor.Red:
                child.GetComponent<SpriteRenderer>().sprite = sprites[0];
                break;
            case ShapeColor.Green:
                child.GetComponent<SpriteRenderer>().sprite = sprites[1];
                break;
            case ShapeColor.Blue:
                child.GetComponent<SpriteRenderer>().sprite = sprites[2];
                break;
        }
    }

    private IEnumerator DestroyAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
