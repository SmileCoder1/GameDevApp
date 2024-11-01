using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneHandler : MonoBehaviour
{
    public float waitBefore;
    public float waitAfter;
    public CinemachineVirtualCamera cam;
    public Transform camTarget;
    public List<Cutscene> cutscenes= new List<Cutscene>();
    public bool finished = false;
    public GameObject cinemaUI;
    public GameObject[] tutorialThings;
    public List<Cutscene> post_cutscenesJob = new List<Cutscene>();
    public bool cutscenebars = true;
    public BlackoutControl blackout;
    public bool startOutBlack = false;
    public bool blackedout = false;
    public GameManager gameManager;
    public bool musicStart = true;

    public void instablack()
    {
        blackout = GetComponentInChildren<BlackoutControl>();
        blackout.black();
    }

    public void instaunblack()
    {
        blackout = GetComponentInChildren<BlackoutControl>();
        blackout.clear();
    }

    public void blackenout()
    {
        blackout = GetComponentInChildren<BlackoutControl>();
        blackout.blackout();
    }

    public void unblackenout()
    {
        blackout = GetComponentInChildren<BlackoutControl>();
        blackout.blackin();
    }

    // Start is called before the first frame update
    public IEnumerator play()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        gameManager.toggleMusic(musicStart);
        if (startOutBlack)
            instablack();
        tutorialThings = GameObject.FindGameObjectsWithTag("Tutorials");
        foreach(var tutorialSec in tutorialThings)
        {
            tutorialSec.SetActive(false);
        }

        Transform prevTrans = cam.Follow;
        cam.Follow = camTarget;
        GameObject obj = null;
        if(cutscenebars) 
            obj = Instantiate(cinemaUI);
        yield return new WaitForSeconds(waitBefore);
        foreach(Cutscene cutscene in cutscenes)
        {
            //print("new cutscene playing");
            
            StartCoroutine(cutscene.behav());
            if (cutscene.camTarget != null)
            {
                cam.Follow = cutscene.camTarget;
            }
            //this is not finished quite yet
            while (!cutscene.finished)
            {
                if (Input.GetKey(KeyCode.K))
                {
                    finished = true;
                    break;
                }
                yield return null;
            }
            if (finished) 
                break;
        }

        if (blackedout)
            unblackenout();

        yield return new WaitForSeconds(waitAfter);
        if(cutscenebars)
            obj.GetComponentInChildren<Animator>().SetTrigger("Hide");
        finished = true;
        cam.Follow = GameObject.FindGameObjectWithTag("Player").transform;
        

        if (post_cutscenesJob.Count > 0)
            foreach(var cutscene in post_cutscenesJob)
            {
                StartCoroutine(cutscene.behav());
                while(!cutscene.finished)
                {
                    yield return null;
                }
            }
        foreach (var tutorialSec in tutorialThings)
        {
            tutorialSec.SetActive(true);
        }
        gameManager.toggleMusic(true);

    }
}
