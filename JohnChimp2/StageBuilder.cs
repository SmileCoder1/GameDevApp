using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    public Transform MonkeyObj;
    public GameObject emptyWallInst;
    public GameObject platformRoomInst;
    public GameObject groundInst;
    public GameObject dom;
    public GameObject switchLUL;
    public GameObject sub;

    [System.Serializable]
    public struct sectionStruct
    {
        public GameObject section;
        public float spawnRate;
    }
    [SerializeField]
    public List<sectionStruct> sections;

    // Start is called before the first frame update
    void Awake()
    {
        switchLUL = Instantiate(emptyWallInst);
        switchLUL.GetComponent<StageBlock>().attachToOld(null);
        dom = Instantiate(platformRoomInst);
        dom.GetComponent<StageBlock>().attachToOld(switchLUL.GetComponent<StageBlock>());
        //loadNext();
        //if (.2 < Random.Range(0f, 1f))
        //{
        //    dom = Instantiate(emptyWallInst);
        //}
        //else
        //{
        //    dom = Instantiate(puzzleRoomInst);
        //}
        //Debug.Log("Section was spawned: " + dom);
        //dom.GetComponent<StageBlock>().attachToOld(switchLUL.GetComponent<StageBlock>());

    }





    //}

    //using System.Collections;
    //using System.Collections.Generic;
    //using UnityEngine;

    //public class StageBuilder : MonoBehaviour
    //{
    //    public Transform MonkeyObj;
    //    public GameObject emptyWallInst;
    //    public GameObject puzzleRoomInst;
    //    public GameObject groundInst;
    //    public GameObject dom;
    //    public GameObject switchLUL;
    //    public GameObject sub;

    //    // Start is called before the first frame update
    //    void Start()
    //    {
    //        switchLUL = FindAnyObjectByType<StageBlock>().gameObject;
    //        switchLUL.GetComponent<StageBlock>().attachToOld(null);
    //        if (.2 < Random.Range(0f, 1f))
    //        {
    //            dom = Instantiate(emptyWallInst);
    //        }
    //        else
    //        {
    //            dom = Instantiate(puzzleRoomInst);
    //        }
    //        dom.GetComponent<StageBlock>().attachToOld(switchLUL.GetComponent<StageBlock>());

    //    }

    //public void loadNext()
    //{
    //    Debug.Log("should switch here");
    //    if (sub != null)
    //        Destroy(sub);
    //    sub = switchLUL;
    //    switchLUL = dom;
    //    if (.2 < Random.Range(0f, 1f))
    //    {
    //        dom = Instantiate(emptyWallInst);
    //    }
    //    else
    //    {
    //        dom = Instantiate(puzzleRoomInst);
    //    }
    //    dom.GetComponent<StageBlock>().attachToOld(switchLUL.GetComponent<StageBlock>());
    //}

    public void loadNext()
    {
        Debug.Log("should switch here");
        if (sub != null)
            Destroy(sub);
        sub = switchLUL;
        switchLUL = dom;
        float runningTotal = 0;
        float randVal = Random.Range(0f, 1f);
        foreach (sectionStruct spawn in sections)
        {
            runningTotal += spawn.spawnRate;
            //Debug.Log("Spawn rate: " + spawn.rate);
            if (randVal <= runningTotal)
            {
                Debug.Log("Section was spawned: " + randVal);
                dom = Instantiate(spawn.section);
                break;
            }
            Debug.Log("No  Section Spawned");

        }
        Debug.Log("we got here!!!");
        dom.GetComponent<StageBlock>().attachToOld(switchLUL.GetComponent<StageBlock>());
    }

}