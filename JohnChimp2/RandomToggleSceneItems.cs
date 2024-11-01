using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomToggleSceneItems : MonoBehaviour
{
    public List<GameObject> items = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject item in items)
        {
            if(Random.Range(0f, 2.0f) > 1f)
                item.SetActive(true);
            else
                item.SetActive(false);
        }
    }
}
