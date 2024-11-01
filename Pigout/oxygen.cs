using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oxygen : MonoBehaviour
{


    /*Some documentation
        potency of oxygen goes down over time --> move to heart quicker 
        should change the drop off to be not linear though and maybe increase the amount of time to kill oxygen
    */

    public float expireTime = 6f;
    public float aliveTime = 0;
    public GameObject carrier = null; //will replace this with the character's script
    public Lung lungs;
    // Start is called before the first frame update
    void Start()
    {
        lungs = FindAnyObjectByType<Lung>();
    }

    // Update is called once per frame
    void Update()
    {
        aliveTime += Time.deltaTime;
        float hue = (expireTime - aliveTime) / expireTime;
        GetComponent<SpriteRenderer>().color = new Color(hue, hue, hue);
        if(aliveTime >= expireTime)
        {
            Destroy(gameObject);
        }
    }


    private void OnDestroy()
    {
        //todo remove from carrying character's posession
        lungs.removeFromList(this);
        
    }

}
