using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class playeractions : MonoBehaviour
{
    public bool isHolding = false;
    public GameObject carriedObject = null;
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("carrying", isHolding);
        animator.SetBool("moving",  Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > 0.1f);
        gameObject.transform.localScale = new Vector3(GetComponent<Rigidbody2D>().velocity.x < 0 ? 2 : -2, 2, 1);




        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(isHolding)
            {
                InteractBehavior ib = carriedObject.GetComponent<InteractBehavior>();
                ib.Interact(gameObject);
            }
            else
            {
                Vector3 MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                MousePosition.z = 0;
                int layerMask = LayerMask.GetMask("obj");
                Debug.Log("Casting ray to " + MousePosition.ToString() + " from " + transform.position.ToString());
                Debug.Log("Layer Mask " + layerMask);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, MousePosition - transform.position, 3, layerMask);
                if(hit.collider != null)
                {
                    Debug.Log("hit detected");
                    InteractBehavior ib;
                    if(hit.collider.gameObject.TryGetComponent<InteractBehavior>(out ib))
                    {
                        ib.Interact(gameObject);
                    }
                }
            }


            //RaycastHit2D[] rays = Physics2D.BoxCastAll(
            //    distance: 0.1f,
            //    origin: this.transform.position + 0.3f*Vector3.right *transform.localScale.x, 
            //    angle:0,
            //    size: new Vector2(1, 1), 
            //    direction: Vector2.right *transform.localScale.x
            //    );
                
            //InteractBehavior ib;
            //foreach (RaycastHit2D ray in rays) {
                
            //    if (ray.collider.gameObject.TryGetComponent<InteractBehavior>(out ib))
            //    {
            //        ib.Interact(this.gameObject);
            //        break;
            //    }

            //}
        }
    }
}
