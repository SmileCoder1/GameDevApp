using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class forcedActions : Cutscene
{

    public StealthMovement anala;
    public List<StealthMovement> movScripts = new List<StealthMovement>();
    public List<forceInput> forceInputs = new List<forceInput>();
    public List<GameObject> gameObjects = new List<GameObject>();
    public List<CutSceneDialogue> dialogue = new List<CutSceneDialogue>();
    public bool returnControl = true;
    public bool rightDir = true;
    public bool justRegain = false;
    public bool first = true;
    public bool isHide = false;
    public CutSceneHandler daddyCut;
    public float timeToBlackout = 0;
    public bool blackOutWhenDone = false;
    public bool blackOutWhenFinished = false;
    public bool speaking = false;
    public bool waitForSpeaking = true;

    public bool checkDone()
    {
        foreach(forceInput fInput in forceInputs)
        {
            if (isHide && anala.GetComponent<StealthMovement>().state == playerStates.Hiding) continue;
            else if (fInput.inside || first) return false;
        }
        return true;
    }

    public IEnumerator blackout()
    {
        yield return new WaitForSeconds(timeToBlackout);
        if(!finished)
            daddyCut.blackenout();
    }


    public IEnumerator handleDialogue()
    {
        //assumption right now that all dialogue is sorted
        speaking = true;
        float time = 0;
        AudioSource last = null;
        for(int i = 0; i < dialogue.Count; i++)
        {
            if(time <= dialogue[i].timeToStart)
            {
                yield return new WaitForSeconds(dialogue[i].timeToStart - time);
                time = dialogue[i].timeToStart;

            }
            last = dialogue[i].speaker;
            last.PlayOneShot(dialogue[i].monologue, 1);
            //print("Audio playing");
        }

        while(last != null && last.isPlaying)
        {
            yield return null;
        }
        speaking = false;
    }


    public override IEnumerator behav()
    {
        daddyCut = transform.parent.GetComponent<CutSceneHandler>();
        yield return new WaitForSeconds(waitBefore);

        StartCoroutine(handleDialogue());

        if(daddyCut.blackedout)
        {
            daddyCut.unblackenout();
        }    

        if(blackOutWhenDone)
        {
            StartCoroutine(blackout());
        }

        foreach (GameObject go in gameObjects)
        {
            go.SetActive(true);
            forceInput fi = go.GetComponent<forceInput>();
            fi.shadowAnala.SetActive(true);
            StealthMovement smScript = fi.shadowAnala.GetComponent<StealthMovement>();
            smScript.controlsEnabled = false;
            movScripts.Add(smScript);

        }
        anala = GameObject.FindGameObjectWithTag("Player").GetComponent<StealthMovement>();



        if (!justRegain)
        {
            
            
            while (!checkDone())
            {
                yield return new WaitForFixedUpdate();
                first = false;
            }
            if (returnControl)
                anala.controlsEnabled = true;
        }
        else
            anala.controlsEnabled = true;

        
        foreach(GameObject go in gameObjects)
        {
            go.SetActive(false);
        }

        if (waitForSpeaking)
            while (speaking)
                yield return null;
        
        yield return new WaitForSeconds(waitAfter);
        finished = true;

    }
}
