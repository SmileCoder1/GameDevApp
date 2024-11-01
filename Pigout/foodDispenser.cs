using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foodDispenser : MonoBehaviour
{
    public GameObject applePrefab;
    public GameObject burgerPrefab;
    public GameObject oatPrefab;
    public List<GameObject> foodList = new List<GameObject>();
    public Vector2 spawnLoc;
    public float topSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
        spawnLoc = transform.position;
    }

    public void dispenseApple()
    {
        GameObject appleFood = Instantiate(applePrefab, spawnLoc, Quaternion.identity);
        appleFood.SetActive(true);
        float yspeed = UnityEngine.Random.Range(0.1f * topSpeed, -1 * topSpeed);
        float xspeed = UnityEngine.Random.Range(-2 * topSpeed, 2 * topSpeed);
        float rotationalSpeed = Random.Range(10, 270);

        appleFood.GetComponent<Rigidbody2D>().angularVelocity = rotationalSpeed;
        appleFood.GetComponent<Rigidbody2D>().velocity = new Vector2(xspeed, yspeed);
        foodList.Add(appleFood);
    }

    public void dispenseBurger()
    {
        GameObject burgerFood = Instantiate(burgerPrefab, spawnLoc, Quaternion.identity);
        burgerFood.SetActive(true);
        float yspeed = UnityEngine.Random.Range(0.1f * topSpeed, -1 * topSpeed);
        float xspeed = UnityEngine.Random.Range(-2 * topSpeed, 2 * topSpeed);
        float rotationalSpeed = Random.Range(10, 270);

        burgerFood.GetComponent<Rigidbody2D>().angularVelocity = rotationalSpeed;
        burgerFood.GetComponent<Rigidbody2D>().velocity = new Vector2(xspeed, yspeed);
        foodList.Add(burgerFood);

    }

    public void dispenseOats()
    {
        GameObject oatFood = Instantiate(oatPrefab, spawnLoc, Quaternion.identity);
        oatFood.SetActive(true);
        float yspeed = UnityEngine.Random.Range(0.1f * topSpeed, -1 * topSpeed);
        float xspeed = UnityEngine.Random.Range(-2 * topSpeed, 2 * topSpeed);
        float rotationalSpeed = Random.Range(10, 270);

        oatFood.GetComponent<Rigidbody2D>().angularVelocity = rotationalSpeed;
        oatFood.GetComponent<Rigidbody2D>().velocity = new Vector2(xspeed, yspeed);
        foodList.Add(oatFood);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
