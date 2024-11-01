using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPunched : InteractBehavior
{
    public Rigidbody2D rb;
    public float launchspeed;
    public GameObject pigParent = null;
    public float vertOffset = 0.4f;
    public float chuckForce = 50f;
    public float timeToReturn = 0.5f;
    public float timeSinceThrown = 1f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(pigParent != null)
        {
            transform.position = pigParent.transform.position + new Vector3(0, vertOffset, 0);
        }
        timeSinceThrown += Time.deltaTime;
        if(pigParent == null && timeToReturn < timeSinceThrown)
        {
            gameObject.layer = LayerMask.NameToLayer("obj");
        }
    }


    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    Debug.Log("Trigger exited");
    //    if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
    //    {
    //        Debug.Log("Collision should be back baby");
    //        GetComponent<CircleCollider2D>().enabled = true;
    //    }
    //}

    public void pickup(GameObject player)
    {
        Debug.Log("pick up " + gameObject.name);
        gameObject.layer = LayerMask.NameToLayer("Throw");
        pigParent = player;
        //GetComponent<CircleCollider2D>().isTrigger = true;
    }

    public void chuck()
    {
        Debug.Log("Thrown " + gameObject.name);
        Vector2 screenPos = FindAnyObjectByType<Camera>().WorldToScreenPoint(pigParent.transform.position);
        pigParent = null;
        Vector2 mousePos = Input.mousePosition;
        Vector2 throwDir = mousePos - screenPos;
        rb.velocity = rb.velocity + (chuckForce * throwDir.normalized);
        timeSinceThrown = 0;
        
    }

    public override void Interact(GameObject player)
    {
        playeractions pl = player.GetComponent<playeractions>();
        if (pigParent == null)
        {
            if (!pl.isHolding)
            {
                pickup(player);
                pl.isHolding = true;
                pl.carriedObject = gameObject;
            }
        }
        else
        {
            chuck();
            pl.isHolding = false;
            pl.carriedObject = null;
        }

        
    }

    void OnDestroy()
    {
        if (pigParent != null)
        {
            playeractions pl = pigParent.GetComponent<playeractions>();
            pl.isHolding = false;
        }
    }
}
