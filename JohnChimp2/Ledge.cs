using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ledge : MonoBehaviour
{
    public List<GameObject> untouchableEdges = new List<GameObject>();
    public List<GameObject> spikeEdges = new List<GameObject>();
    public bool randomAssignment = false;
    public bool randomSpike = false;

    // Start is called before the first frame update
    void Start()
    {
        if (randomSpike)
        {
            for(int i = 0; i < 4; i++)
            {
                if(Random.Range(0f, 2f) > 1f)
                {
                    spikeEdges[i].SetActive(true);
                }
            }
        }
        if(randomAssignment)
        {
            for(int i = 0; i < 4; i++)
            {
                if(Random.Range(0f, 2f) > 1f)
                {
                    untouchableEdges[i].SetActive(true);
                }
            }
        }
        
    }

    public void setUntouchables(int ledges)
    {
        for (int i = 0; i < spikeEdges.Count; i++)
        {
            bool toSet = (ledges & (1 << i)) > 0;
            spikeEdges[i].SetActive(toSet);
        }
        for (int i = 0; i < untouchableEdges.Count; i++)
        {
            bool toSet = (ledges & (1 << i)) > 0;
            untouchableEdges[i].SetActive(toSet);
        }

    }
}
