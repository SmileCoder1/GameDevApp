using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class Lung : MonoBehaviour
{

    public GameObject oxygenPrefab;
    public List<oxygen> oxygenList = new List<oxygen>();
    public void removeFromList(oxygen mol)
    {
        if(oxygenList.Contains(mol))
            oxygenList.Remove(mol);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }


    public int releaseOxygen()
    {
        int oxyCount = oxygenList.Count;
        for(int i = oxygenList.Count - 1; i >= 0; i--)
        {
            oxygen o = oxygenList[i];
            if(o)
            {
                Destroy(o.gameObject);
            }
            else
            {
                oxyCount--;
            }
        }
        return oxyCount;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        UnityEngine.Debug.Log("collision ocurred");
        UnityEngine.Debug.Log(collision.gameObject.name);
        oxygen mol = collision.gameObject.GetComponent<oxygen>();
        UnityEngine.Debug.Log(mol);
        if(mol != null)
        {
            oxygenList.Add(mol);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        UnityEngine.Debug.Log("exit ocurred");
        oxygen mol = collision.gameObject.GetComponent<oxygen>();
        if(mol)
        {
            oxygenList.Remove(mol);
        }
    }

    private void spawnOxygen()
    {
        GameObject obj = Instantiate(oxygenPrefab, gameObject.transform);
        obj.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.K))
        {
            spawnOxygen();
        }

    }
}
