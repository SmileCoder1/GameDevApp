using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class forceInput : MonoBehaviour
{

    public GameObject shadowAnala;
    public Transform shadPos;
    public StealthMovement script;
    private Bounds boundBox;
    private List<virtualButton> buttons;
    private Dictionary<InputActionReference, virtualButton> buttonDict;
    public GameObject toDisplayWhileFrozen;
    public bool W;
    public bool space;
    public bool Shift;
    public bool S;
    public bool D;
    public bool A;
    public bool K;
    public bool L;
    public bool M;
    public bool walk;
    public bool walkdir;
    public bool spamSpace;
    public bool longJumpForce;
    public bool bigJumpForce;
    public bool pause;
    public float pauseLen;
    public float pauseTime;
    public bool inside = false;
    public bool pauseBool = false;
    public bool freeze;
    public bool freezeBool = false;
    public float freezeTime;
    public BoxCollider2D bc;
    public float timeout = 1000f;
    public float curTime = 0f;
    [SerializeField]
    private InputActionReference movement, cameraMovement, kill, interactL, interactK, playerJump, down, shift;


    // Start is called before the first frame update
    void Start()
    {
        if(shadowAnala == null)
            shadowAnala = transform.parent.Find("ShadowAnala").gameObject;

        shadPos = shadowAnala.transform;
        script = shadowAnala.GetComponent<StealthMovement>();
        buttons = script.virtualButtons;
        buttonDict = script.actionToVirt;
        bc = GetComponent<BoxCollider2D>();
        boundBox = GetComponent<BoxCollider2D>().bounds;

        movement = script.movement;
        cameraMovement = script.cameraMovement;
        kill = script.kill;
        interactL = script.interactL;
        interactK = script.interactK;
        playerJump = script.playerJump;
        down = script.down;
        shift = script.shift;
        
    }

    IEnumerator stallRoutine()
    {

        pauseBool = true;
        //print("pause started");
        yield return new WaitForSeconds(pauseLen);
        //print("pause ended");
        //print("applied in stall");
        applyPresses();
    }

    IEnumerator freezeRoutine()
    {
        script.freeze = true;
        freezeBool = true;
        //print("freeze started");
        GameObject obj = null;
        if(toDisplayWhileFrozen!= null)
        {
            obj = Instantiate(toDisplayWhileFrozen, shadowAnala.transform);
            obj.transform.localScale = new Vector2(3f, 3f);
        }
        yield return new WaitForSeconds(freezeTime);
        if (obj != null)
            Destroy(obj);
        //print("freeze ended");
        freezeBool = false;
        script.freeze = false;
        if (pause)
        {
            StartCoroutine(stallRoutine());
        }
        else
        {
            //print("applied in freeze");
            applyPresses();

        }
    }

    void deapplyPresses()
    {
        //print("deapplied");
        if (D)
        {
            //print("D pressed");
            script.DtoAdd--;
        }
        if(K)
        {
            buttonDict[interactK].pressed -= 1;
            buttonDict[interactK].newPressed = false;
        }
        if(L)
        {
            buttonDict[interactL].pressed -= 1;
            buttonDict[interactL].newPressed = false;
        }
        if (M)
        {
            buttonDict[kill].pressed -= 1;
            buttonDict[kill].newPressed = false;
        }
        if (W)
        {
            script.WtoAdd--;
        }
        if (space)
        {
            buttonDict[playerJump].pressed -= 1;
            buttonDict[playerJump].newPressed = false;
        }
        if (A)
        {
            script.AtoAdd--;
        }
        if (S)
        {
            buttonDict[down].pressed -= 1;
            buttonDict[down].newPressed = false;
            script.StoAdd--;
        }
        if (Shift)
        {
            buttonDict[shift].pressed -= 1;
            buttonDict[shift].newPressed = false;
        }
        if(walk)
        {
            script.walk = false;
        }
        inside = false;
    }

    void applyPresses()
    {
        //print("applied");
        //float DtoAdd = D ? 1 : 0;
        //float AtoAdd = A ? -1 : 0;
        //float WtoAdd = W ? 1 : 0;
        //float StoAdd = S ? -1 : 0;
        //script.forceX = DtoAdd + AtoAdd;
        //script.forceY = WtoAdd + StoAdd;
        if (D)
        {
            //print("D pressed");
            script.DtoAdd++;
        }
        if (W)
        {
            script.WtoAdd++;
        }
        if (space)
        {
            buttonDict[playerJump].pressed += 1;
            buttonDict[playerJump].newPressed = true;
            
        }
        if(Shift)
        {
            buttonDict[shift].pressed += 1;
            buttonDict[shift].newPressed = true;
        }
        if (A)
        {
            script.AtoAdd++;
        }
        if (S)
        {
            buttonDict[down].pressed += 1;
            buttonDict[down].newPressed = true;
            script.StoAdd++;
        }
        if (K)
        {
            buttonDict[interactK].pressed += 1;
            buttonDict[interactK].newPressed = true;
        }
        if (L)
        {
            buttonDict[interactL].pressed += 1;
            buttonDict[interactL].newPressed = true;
        }
        if (M)
        {
            buttonDict[kill].pressed += 1;
            buttonDict[kill].newPressed = true;
        }
        if (longJumpForce)
        {
            script.state = playerStates.Vaulting;
            script.bufferedAction = StealthMovement.movementTech.longJump;
            script.vaultPossible = true;
        }
        if(bigJumpForce)
        {
            script.state = playerStates.Vaulting;
            //script.bufferedAction = StealthMovement.movementTech.bigJump;
            //script.vaultPossible = true;
        }
        if(walk)
        {
            script.walk = true;
            script.walkdir = walkdir;
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        bool exited = false;
        if(inside)
        {
            curTime += Time.fixedDeltaTime;
            if (curTime > timeout)
            {
                deapplyPresses();
                inside = false;
                exited = true;

            }
                
        }

        if(inside && (!freeze || freezeBool) && (!pause || pauseBool))
        {
            //if (spamSpace)
            //{
            //    print("presspress");
            //    buttonDict[KeyCode.Space].newPressed = !buttonDict[KeyCode.Space].newPressed;
            //}
        }

        bool toExit = false;
       
        if(!toExit && bc.isTrigger && shadPos.position.x <= boundBox.max.x && shadPos.position.x >= boundBox.min.x && shadPos.position.y <= boundBox.max.y && shadPos.position.y >= boundBox.min.y)
        {
            if (!inside && curTime < timeout)
            {
                
                if(freeze)
                {
                    StartCoroutine(freezeRoutine());
                }
                if(!freezeBool)
                {
                    if (pause)
                    {
                        StartCoroutine(stallRoutine());
                    }
                    //print("reached");
                    if (!pauseBool)
                    {
                        //print("applied in reg");
                        applyPresses();

                    }
                }
                inside = true;


            }
        }
        else
        {
            if(inside && bc.isTrigger)
                deapplyPresses();
            //if (inside && bc.isTrigger)
            //{
            //    //if (D)
            //    //{
            //    //    buttonDict[KeyCode.D].pressed -= 1;
            //    //}
            //    //if (W)
            //    //{
            //    //    buttonDict[KeyCode.W].pressed -= 1;
            //    //}
            //    if (space)
            //    {
            //        buttonDict[playerJump].pressed -= 1;
            //    }
            //    //if (A)
            //    //{
            //    //    buttonDict[KeyCode.A].pressed -= 1;
            //    //}
            //    if (S)
            //    {
            //        buttonDict[down].pressed -= 1;
            //    }
            //    if (Shift)
            //    {
            //        buttonDict[shift].pressed -= 1;
            //    }
            //    inside = false;
            //}
        }
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject == shadowAnala)
        {
            inside = true;
            applyPresses();

        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject == shadowAnala)
        {
            inside = false;
            if(curTime < timeout) deapplyPresses();
            curTime = 0;

        }
    }
}


