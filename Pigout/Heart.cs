using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : InteractBehavior
{
    
    public healthManager manager;
    [SerializeField] private AudioClip pump;
    public bool isbeating;
    private float timeaftersuccess = 0f;



    public void beat()
    {
        AudioSource.PlayClipAtPoint(pump, new Vector3(0, 0, 0), 1f);
        FindAnyObjectByType<healthManager>().pump();
        Debug.Log("heart is pumping right now");
        isbeating = true;
    }

    public override void Interact(GameObject go)
    {
        beat();
    }


    private void Update()
    {
        timeaftersuccess += Time.deltaTime;
        if (timeaftersuccess > 1f)
        {
            isbeating = false;
            timeaftersuccess = 0f;
        }
    }

}
