using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Stomach : MonoBehaviour
{
    public healthManager manager;
    public GameObject bulletPrefab;
    public GameObject bulletPrefab2;
    public GameObject bulletPrefab3;
    public List<FoodType> bulletQueue = new List<FoodType>();
    public List<GameObject> bulletsList = new List<GameObject>();
    public float timeSinceAdd = 1;
    public float timeBetweenAdds = 0.8f;
    public List<Food> foodList = new List<Food>();
    public float timeToDigest = 2;
    public bool isDigesting = false;

    [SerializeField] private AudioClip digest;


    private void popBullet()
    {
        bulletsList.RemoveAt(0);
    }

    private void Start()
    {
        manager = FindAnyObjectByType<healthManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Food food = collision.gameObject.GetComponent<Food>();
        if (food != null)
        {
            AudioSource.PlayClipAtPoint(digest, new Vector3(0, 0, 0), 1f);
            food.timeToDigest = timeToDigest;
            foodList.Add(food);
            isDigesting = true;

        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        Food food = collision.gameObject.GetComponent<Food>();
        if(food != null && foodList.Contains(food))
        {
            foodList.Remove(food);
            isDigesting = false;
        }
    }

    public void removeLatest()
    {
        if(bulletsList.Count > 0)
        {
            Destroy(bulletsList[0]);
            bulletsList.RemoveAt(0);
        }
            
    }
    private void add_to_queue()
    {
        if(bulletQueue.Count > 0 && timeSinceAdd > timeBetweenAdds)
        {
            Debug.Log("Food bullet added");
            manager.foodBullets.Add(bulletQueue[0]);
            GameObject toInstantiate = bulletPrefab;
            if (bulletQueue[0] == FoodType.Burger)
                toInstantiate = bulletPrefab2;
            else if (bulletQueue[0] == FoodType.Oat)
                toInstantiate = bulletPrefab3;
            GameObject pf = Instantiate(toInstantiate, toInstantiate.transform.position, Quaternion.identity);
            pf.SetActive(true);
            bulletQueue.RemoveAt(0);
            bulletsList.Add(pf);
            timeSinceAdd = 0;
        }
        else
        {
            timeSinceAdd += Time.deltaTime;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            Instantiate(bulletPrefab, bulletPrefab.transform.position, Quaternion.identity);
        }

        add_to_queue();
        
        for(int i = foodList.Count - 1; i >= 0; i--)
        {
            Food food = foodList[i];
            if (manager.foodBullets.Count >= 4)
            {
                food.stalled = true;
                break;
            }
            food.stalled = false;
            food.timeDigested += Time.deltaTime;
            if(food.timeDigested > timeToDigest)
            {
                bulletQueue.Add((FoodType)food.identity);
                foodList.Remove(food);
                Destroy(food.gameObject);
            }
        }
    }
}
