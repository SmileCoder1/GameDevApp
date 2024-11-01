using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StageBlock : MonoBehaviour
{
    public Vector2 Attach1;
    public Vector2 Attach2;
    public Vector2 endAttach = new Vector2(0, 20);
    public int dir = 0; //0 = up, 1 = right, 2 = down, 3 = left
    public bool used = false;
    public Vector2 offset= Vector2.zero;

    
    public void attachToOld(StageBlock block)
    {
        if(block == null)
        {
            Attach1 = Vector2.zero;
            Attach2 = new Vector2(0, 18);
        }
        else
        {
            Attach1 = block.Attach2;
            Attach2 = block.Attach2 + endAttach;
        }
        dir = 0;
        transform.position = Attach1 - offset;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (used)
            return;
        Debug.Log("monke1");
        if (collision.gameObject.tag == "Player")
        {
            FindAnyObjectByType<GameManager>().addRoom();
            used = true;
            StageBuilder thing =  FindAnyObjectByType<StageBuilder>();
            Debug.Log("monke2");
            thing.loadNext();
        }
    }

}
