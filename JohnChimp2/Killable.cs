using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.IK;

public class Killable : MonoBehaviour
{
    public bool dead = false;
    private AudioSource deadSound;
    public AudioClip dieSound;

    private void Start()
    {
        deadSound = gameObject.AddComponent<AudioSource>();
        deadSound.clip = dieSound;
    }
    public IEnumerator Kill()
    {
        dead = true;
        deadSound.Play();
        if (GameObject.FindWithTag("MainCamera").GetComponent<CameraFollow>().justFollowMode)
        {
            Debug.Log("Killing");
            dead = true;
            HingeJoint2D[] monkeyLimbs = GetComponentsInChildren<HingeJoint2D>();
            foreach(HingeJoint2D limb in monkeyLimbs)
        {
            limb.enabled = false;
            limb.transform.parent = null;
            Rigidbody2D obj = limb.gameObject.GetComponent<Rigidbody2D>();
            if(obj != null)
            {
                obj.AddForce(new Vector2(Random.Range(-100, 100), Random.Range(-100, 100)));
            }
        }
        //Destroy(gameObject);
        yield return new WaitForSeconds(2f);
        }
        //Debug.LogError("I DIED");
        //Application.Quit();
        yield return new WaitForSeconds(0f);

        GameOverManager gm = FindObjectOfType<GameOverManager>();
        gm.gameOverTriggered();

        //Debug.LogError("I DIED");
        //Application.Quit();
        //EditorApplication.ExitPlaymode();
    }
}
