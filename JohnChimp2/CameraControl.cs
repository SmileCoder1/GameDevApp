using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public bool used = false;
    public float locToSwitch = 0;
    public bool rightDir = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        
        if(used) return;
        if (collision.gameObject.tag == "Player")
        {
            if (rightDir)
                locToSwitch = locToSwitch + transform.parent.position.x;
            else
                locToSwitch = locToSwitch + transform.parent.position.y;
            CameraFollow fol = FindAnyObjectByType<CameraFollow>();
            fol.setDirection(locToSwitch);
            used = true;
        }
    }
}
