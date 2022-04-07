using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blank : Shape
{
    public override void Explode()
    {
    }

    public override void Merge()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sortingOrder = 2;
        GetComponent<SpriteRenderer>().enabled = false;
        SpriteMask spriteMask = gameObject.AddComponent<SpriteMask>();
        spriteMask.sprite = BoardManager.Instance.blankSprite;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
