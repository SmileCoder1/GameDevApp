using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{

    public float weight = 1f;
    public int identity = 1;
    public float timeDigested = 0f;
    public float timeToDigest = 2f;
    public bool stalled = false;
    public Color stored;

    private void Start()
    {
        stored = GetComponent<SpriteRenderer>().color;

    }

    private void Update()
    {
        float hue = (timeToDigest - timeDigested) / timeToDigest;
        if (stalled)
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0.5f, 0.5f);
        else
            gameObject.GetComponent<SpriteRenderer>().color = stored -  new Color(0, 0, 0, 1 - hue);

    }


}
