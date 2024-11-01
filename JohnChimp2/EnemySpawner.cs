using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemies = new List<GameObject>();
    public List<float> probabilities = new List<float>();
    public List<Transform> spawnLocs = new List<Transform>();
    public List<int> topBot = new List<int>();
    public float probability = 0.25f;
    public int side = 0;
    public float timeSince = 0f;
    public float spawnTimer = 1f;
    public bool oneTime;
    

    float getProbability()
    {
        float rooms = FindAnyObjectByType<GameManager>().getRoomCount();
        return 1 - Mathf.Exp(-0.4f * rooms);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (oneTime)
        {

            for (int i = 0; i < enemies.Count; i++)
            {
                    GameObject obj = Instantiate(enemies[i], new Vector2(Mathf.Round(spawnLocs[i].position.x), Mathf.Round(spawnLocs[i].position.y)), Quaternion.identity);
                    Debug.Log("Spawned: " + obj.name );
                    if(obj.GetComponent<Bug>().type != Bug.bugType.FLY)
                    {
                        obj.GetComponent<Bug>().side = side;
                        obj.GetComponent<Bug>().dir = topBot[i];
                    }
                    else
                    {
                        print("supposed to spawn fly");
                    }
            }
            Destroy(gameObject);
            return;
        }
        timeSince = Random.Range(0f, 1f);
        spawnTimer = 2f - 0.5f * getProbability();
    }

    // Update is called once per frame
    void Update()
    {
        if(timeSince < spawnTimer)
        {
            timeSince += Time.deltaTime;
            return;
        }


        if (spawnLocs[0].position.y + 1 > FindAnyObjectByType<Camera>().ViewportToWorldPoint(new Vector2(0, 0)).y &&
                spawnLocs[0].position.y - 1 < FindAnyObjectByType<Camera>().ViewportToWorldPoint(new Vector2(1, 1)).y)
            if (spawnLocs[0].position.x + 1 > FindAnyObjectByType<Camera>().ViewportToWorldPoint(new Vector2(0, 0)).x &&
                spawnLocs[0].position.x - 1 < FindAnyObjectByType<Camera>().ViewportToWorldPoint(new Vector2(1, 1)).x)
                return;
        if (Random.Range(0f, 1f) < probability)
        {
            if (FindObjectsOfType<Bug>().Length > 15)
                return;
            bool spawned = false;
            for (int i = 0; i < enemies.Count - 1; i++)
            {
                if (probabilities[i] > Random.Range(0f, 1f))
                {
                    
                    spawned = true;
                    GameObject obj = Instantiate(enemies[i], spawnLocs[i].position, Quaternion.identity);
                    if(obj.GetComponent<Bug>().type != Bug.bugType.FLY)
                    {
                        obj.GetComponent<Bug>().side = side;
                        obj.GetComponent<Bug>().dir = topBot[i];
                    } 
                }
            }
            if(!spawned && enemies.Count > 0)
            {
                GameObject obj = Instantiate(enemies[enemies.Count - 1], spawnLocs[enemies.Count - 1].position, Quaternion.identity);
                if (obj.GetComponent<Bug>().type != Bug.bugType.FLY)
                {
                    obj.GetComponent<Bug>().side = side;
                    obj.GetComponent<Bug>().dir = topBot[enemies.Count - 1];
                }
            }
        }
        timeSince = 0;

    }
}
