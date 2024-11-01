using System;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    public Rigidbody2D rb;
    public BoxCollider2D bc;
    public float speedAccel;
    public float stopAccel;
    public float maxSpeed;
    public float jumpHeightish;
    public float jumpVel;
    public float jumpGravRatio;
    public float notJumpGravRatio;
    public LayerMask levelGeometry;
    public float downGravThresh;

    private bool jumping;
    private float lastJumpY;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public bool isGrounded()
    {
        float floorDetectionMargin = 0.1f;
        Vector2 pos = transform.TransformPoint(bc.offset);
        float width = bc.bounds.size.x;
        float height = bc.bounds.size.y;

        RaycastHit2D leftCorner = Physics2D.Raycast(new Vector2(pos.x - width / 2f,
                                                        pos.y - height / 2f),
                                                        -Vector3.up, floorDetectionMargin, levelGeometry);
        RaycastHit2D rightCorner = Physics2D.Raycast(new Vector2(pos.x + width / 2f,
                                                        pos.y - height / 2f),
                                                        -Vector3.up, floorDetectionMargin, levelGeometry);
        return (leftCorner.collider != null || rightCorner.collider != null);    
    }

    private void Update()
    {
        if (isGrounded() && Input.GetKeyDown(KeyCode.UpArrow)) { 
            lastJumpY = transform.position.y;
            jumping = true;
            rb.velocity = new Vector2(rb.velocity.x, jumpVel);

        }
        if (jumping && isGrounded() && rb.velocity.y < 0.1f)
        {
            Debug.Log("jumping done, pos: " + transform.position.y);
            jumping = false;
        }
        if(jumping && rb.velocity.y > -0.2f && transform.position.y - (rb.velocity.y > 0 ? jumpHeightish : downGravThresh) > lastJumpY)
        {
            rb.gravityScale = jumpGravRatio;
        }
        else
        {
            //Debug.Log("jumping is done");
            rb.gravityScale = notJumpGravRatio;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 inputVector = new Vector2(
            (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) + (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0),
            (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) + (Input.GetKey(KeyCode.DownArrow) ? -1 : 0)
        );

       

        if((inputVector.x == 0 || (inputVector.x * rb.velocity.x < 0)) && isGrounded())
        {
            float diff = Mathf.Min(Mathf.Abs(rb.velocity.x) , stopAccel* Time.fixedDeltaTime);
            rb.velocity = new Vector2((rb.velocity.x > 0 ? rb.velocity.x - diff : rb.velocity.x + diff), rb.velocity.y);
        }
        rb.velocity = rb.velocity + inputVector * Vector2.right * speedAccel * Time.fixedDeltaTime;
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), Mathf.Max(rb.velocity.y, -20f));
        if(rb.velocity.x < 0)
        {
            this.transform.localScale = new Vector3(-Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if(rb.velocity.x > 0)
        {
            this.transform.localScale = new Vector3(Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}
