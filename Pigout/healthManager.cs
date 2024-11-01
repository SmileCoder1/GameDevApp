using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro.Examples;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Math;

public enum FoodType
{
    Apple = 0,
    Burger = 1,
    Oat = 2
}

public class healthManager : MonoBehaviour
{
    
    
    //organs
    public Lung lung;
    public Stomach stomach;
    public List<oxygenDispenser> oxygenDispensers= new List<oxygenDispenser>();
    public Heart heart;
    //public List<foodBullet> bulletPrefabs = new List<foodBullet>();
    public List<FoodType> foodBullets = new List<FoodType>();
    
    //status
    public float foodCnt = 0;
    public bool girth_on = true;

    public float oxygen = 1;
    public bool oxygen_on = false;

    public float bpm = 0;


    //depletion / difficulty stuff
    public float energy_depletion = 0.1f;
    public float oxygen_worth = 0.3f;
    public float oxygen_depletion = 0.01f;
    public float oxygen_damage_cooldown = 1f;
    public float timeSinceDamage = 0f;

    //health    
    [SerializeField] int maxHealth;
    private int currentHealth;

    [SerializeField] private AudioClip squeal;
    [SerializeField] private AudioClip oat;
    [SerializeField] private AudioClip appleEat;
    [SerializeField] private AudioClip noSpecial;


    void Awake()
    {
        currentHealth = maxHealth;
    }

    public int getMaxHealth()
    {
        return maxHealth;
    }

    public int getCurrentHealth()
    {
        return currentHealth;
    }


    public void takeDamage(int value)
    {
        AudioSource.PlayClipAtPoint(squeal, new Vector3(0, 0, 0), 1f);
        currentHealth = Min(maxHealth, currentHealth - value);
        if (currentHealth < 0)
        {
            SceneManager.LoadScene("GameOverScreen");
        }
    }   
 

    private void OnTriggerExit2D(Collider2D collision)
    {
        Food fod = collision.gameObject.GetComponent<Food>();
        int fodb = collision.gameObject.layer;
        if (fod != null || fodb == LayerMask.NameToLayer("foodbullet"))
        {
            foodCnt--;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Food fod = collision.gameObject.GetComponent<Food>();
        int fodb = collision.gameObject.layer;
        if (fod != null || fodb ==  LayerMask.NameToLayer("foodbullet"))
        {
            foodCnt++;
        }
    }

    public void breathe()
    {
        if (oxygenDispensers.Count == 0)
            return;
        int dispenseIdx = Random.Range(0, oxygenDispensers.Count);
        oxygenDispensers[dispenseIdx].dispense();
    }

    public void pump()
    {
        int oxygen_molecules = lung.releaseOxygen();
        oxygen = Min(1, oxygen + oxygen_molecules * oxygen_worth);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float td = Time.deltaTime;
        
        
        oxygen = Mathf.Max(oxygen - oxygen_depletion * td, 0);
        timeSinceDamage += td;
        if(oxygen <= 0 & timeSinceDamage >= oxygen_damage_cooldown && oxygen_on)
        {
            takeDamage(1);
            timeSinceDamage = 0;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (foodBullets.Count > 0)
            {
                stomach.removeLatest();
                var foodType = foodBullets[0];
                foodBullets.RemoveAt(0);
                switch (foodType)
                {
                    case (FoodType.Apple):
                        AudioSource.PlayClipAtPoint(appleEat, new Vector3(0, 0, 0), 1f);
                        takeDamage(-10);
                        break;
                    case (FoodType.Burger):
                        var outsidePlayer = FindAnyObjectByType<fetusCtrl>();
                        outsidePlayer.ShootBurger();
                        break;
                    case (FoodType.Oat):
                        AudioSource.PlayClipAtPoint(oat, new Vector3(0, 0, 0), 1f);
                        UseOat();
                        break;
                 }
            }
            else
            {
                AudioSource.PlayClipAtPoint(noSpecial, new Vector3(0, 0, 0), 1f);
            }
        }
        

    }
    public void UseOat()
    { //screen clear
        foreach (var gameObj in GameObject.FindGameObjectsWithTag("enemyBullet"))
        {
            Destroy(gameObj);
        }
    }

    private void FixedUpdate()
    {
        
    }

}
