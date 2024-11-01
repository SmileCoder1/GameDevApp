using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class oxygenDispenser : MonoBehaviour
{

    public GameObject oxygenPrefab;
    public List<GameObject> oxygenMolecules = new List<GameObject>();
    public Vector2 spawnLoc;
    //public float aoe = 120f;
    public float topSpeed = 5;
    public float timeSince = 0f;
    public float debugFrequency = 2.2f;
    public float breathCount;
    public float breathFrequency = 0.05f;


    // Start is called before the first frame update
    void Start()
    {
        spawnLoc = transform.position;
    }


    /*
    public void drawAoe()
    {
        Vector3 axis = new Vector3(0, 0, 1);
        Vector2 middle = new Vector2(0, -5);
        Vector2 leftEdge = Transform.RotateAround(middle, axis, aoe / 2);
        
        Gizmos.DrawRay(spawnLoc, );
    }
    */

    public void dispense()
    {
        GameObject oxygenMolecule =  Instantiate(oxygenPrefab, spawnLoc, Quaternion.identity);
        oxygenMolecule.SetActive(true);
        float yspeed = UnityEngine.Random.Range(0.1f * topSpeed, -1 * topSpeed);
        float xspeed = UnityEngine.Random.Range(-2 * topSpeed, 2 * topSpeed);
        float rotationalSpeed = Random.Range(10, 270);

        oxygenMolecule.GetComponent<Rigidbody2D>().angularVelocity = rotationalSpeed;
        oxygenMolecule.GetComponent<Rigidbody2D>().velocity = new Vector2(xspeed, yspeed);
        //oxygenMolecules.Add(oxygenMolecule);
    }

    public void takeBreath()
    {
        StartCoroutine(Breath());
    }



    // Update is called once per frame
    void Update()
    {


    }

    private void FixedUpdate()
    {
        
    }

    IEnumerator Breath()
    {
        for (int i = 0; i < breathCount; ++i)
        {
            dispense();
            yield return new WaitForSeconds(breathFrequency);
        }
    }


}
