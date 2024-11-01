using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum animState{
    Idle, 
    Running,
    Sneak,
    SneakIdle,
    WallGrab, 
    WallSlide,
    SlideStart,
    SlidePose,
    SlideStop,
    Hide,
    Jump,
    Land,
    AirUp,
    AirDown,
    AirHor,
    Vault1,
    Vault2,
    Vault3,
    Attack, 
    Vault3Run, 
    Walk
}


public class animationState
{
    public bool regState = true;
    public int stateNum;
    public List<animState> transitions= new List<animState>();
    public animationState()
    {
        regState = true;
        stateNum = 0;
        transitions = new List<animState>();
    }
    
}


public class AnimationController2 : MonoBehaviour
{
    // Start is called before the first frame update

    private Rigidbody2D rb;
    public Animator animator;
    private BoxCollider2D box;
    public GameObject particles;
    public GameObject runParticles;
    public GameObject jumpParticles;
    public GameObject landParticles;
    public AfterImageEffect afterImage;
    public GameObject rigParent;
    private StealthMovement movScript;
    public List<animationState> animationStates;
    public animState currentState = 0;
    public animState nextState = 0;
    public playerStates bigState = playerStates.Grounded;
    public float minMotionSpeed = 0.1f;
    public bool jumpStart = false;
    public bool slideStart = false;
    public bool vaultStart = false;
    public bool slideStop = false;
    public bool landing = false;
    public bool inTransition = false;
    private int animationCount = 0;
    public int priority = 0;
    public int next_priority = 0;
    public bool failed = true;
    public bool wasWallsliding = false;
    public float timeScale;
    public bool killing;
    public bool killing2;
    public int runFreq;
    public bool fxEnabled = true;
    public soundWaveEmmiter swe;

    IEnumerator startKill()
    {
        killing2 = true;
        animator.SetInteger("NextState", (int)animState.Attack);
        yield return new WaitForSeconds(1.35f);
        animator.SetInteger("NextState", (int)animState.Idle);
        killing = false;
        killing2 = false;
        
    }


    public class animationState
    {
        public bool regState = true;
        public int stateNum;
        public List<animState> transitions = new List<animState>();
        public animationState()
        {
            regState = true;
            stateNum = 0;
            transitions = new List<animState>();
        }

    }

    void Start()
    {
        swe = GetComponentInChildren<soundWaveEmmiter>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        box = GetComponent<BoxCollider2D>();
        rigParent = transform.Find("rigParent").gameObject;
        afterImage = FindObjectOfType<AfterImageEffect>();
        movScript = GetComponent<StealthMovement>();
        killing = false;
        killing2 = false;
        //for(int i = 0; i < 18; i++)
        //{
        //    animationStates.Add(new animationState());
        //}
        ////animationStates[(int)animState.Land].regState = false;
        //animationStates[(int)animState.SlideStart].regState = false;
        //animationStates[(int)animState.SlideStart].transitions.Add(animState.SlidePose);
        //animationStates[(int)animState.SlidePose].regState = false;
        //animationStates[(int)animState.SlidePose].transitions.Add(animState.SlideStop);
        //animationStates[(int)animState.SlideStop].regState = false;
        //animationStates[(int)animState.Vault1].regState = false;
        //animationStates[(int)animState.Vault2].regState = false;
        //animationStates[(int)animState.Vault3].regState = false;

    }

    private IEnumerator SlideStart()
    {
        inTransition = true;
        float yval = Mathf.Round(transform.position.y - movScript.getHeight()/4);
        if (currentState == animState.Vault1 || currentState == animState.Vault2 || currentState == animState.Vault3 || currentState == animState.Vault3Run)
            nextState = animState.SlidePose;
        else 
            nextState = animState.SlideStart;
        animator.SetInteger("NextState", (int)nextState);
        yield return new WaitForSeconds(0.1f);
        nextState = animState.SlidePose;
        GameObject obj = Instantiate(particles, new Vector3(transform.position.x - 0.3f, yval + 0.5f, 0), Quaternion.identity);
        obj.transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
        animator.SetInteger("NextState", (int)nextState);
        for(int i = 0; i < 3; i++)
        {
            if(Mathf.Abs(rb.velocity.x) <= 0.1f)
            {
                inTransition = false;
                yield break;
            }
            swe.emmitSound(5, 3, 0.5f, transform.position);
            yield return new WaitForSeconds(0.1f);
            obj = Instantiate(particles, new Vector3(transform.position.x - 0.3f, yval + 0.5f, 0), Quaternion.identity);
            obj.transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
            if (nextState != animState.SlidePose)
            {
                inTransition = false;
                yield break;
            }

        }
        nextState = animState.SlideStop;
        animator.SetInteger("NextState", (int)nextState);
        inTransition = false;
    }


    private bool inXMotion()
    {
        return Mathf.Abs(rb.velocity.x) > minMotionSpeed;
    }

    private bool fallingDown()
    {
        return rb.velocity.y < 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        next_priority = 0;
        bigState = movScript.state;
        
        if (killing)
        {
            if(!killing2)
                StartCoroutine(startKill());
        }
        else 
        {
            if (bigState == playerStates.Airborne && inTransition)
            {
                inTransition = false;
                if (movScript.isWalled(-1 * movScript.facingDir) || movScript.isWalled(movScript.facingDir))
                    nextState = animState.WallSlide;
                else if (fallingDown())
                    nextState = animState.AirDown;
                else
                    nextState = animState.AirUp;
            }
            else if (jumpStart)
            {
                 next_priority = 3;
                nextState = animState.Jump;
                if(fxEnabled)
                    Instantiate(jumpParticles, transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            }
            else if (slideStart)
            {
                next_priority = 2;
                StartCoroutine(SlideStart());
            }
            else if (landing && (bigState == playerStates.Grounded || bigState == playerStates.Sneaking))
            {
                next_priority = 1;
                nextState = animState.Land;
                //Debug.Log("land particle");
                if(fxEnabled)
                    Instantiate(landParticles, new Vector3(transform.position.x, Mathf.Round(transform.position.y), 0), Quaternion.identity);
                landing = false;
            }
            else if (!inTransition)
            {
                next_priority = 0;
                switch (bigState)
                {
                    case playerStates.Walking:
                        nextState = animState.Walk;
                        break;

                    case playerStates.Grounded:
                        if (inXMotion())
                        {
                            nextState = animState.Running;
                            animationCount++;
                            if (animationCount > runFreq && fxEnabled)
                            {
                                GameObject obj = Instantiate(runParticles, transform.position - new Vector3(transform.localScale.x * 0.3f, 0.3f, 0), Quaternion.identity);
                                obj.transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
                                animationCount = 0;
                            }
                        }
                        else
                            nextState = animState.Idle;
                        break;
                    case playerStates.Sneaking:
                        if (inXMotion())
                            nextState = animState.Sneak;
                        else
                            nextState = animState.SneakIdle;
                        break;
                    case playerStates.Airborne:

                        next_priority = 3;

                        if (fallingDown())
                            if (movScript.isWalled(movScript.facingDir))
                                nextState = animState.WallSlide;
                            else
                                nextState = animState.AirDown;
                        else
                            nextState = animState.AirUp;
                        break;
                    case playerStates.Sliding:
                        nextState = animState.SlidePose;
                        animationCount++;
                        if (animationCount > 3)
                        {

                            animationCount = 0;
                        }
                        break;
                    case playerStates.Clinging:
                        next_priority = 3;
                        if (Input.GetKey(KeyCode.LeftShift))
                            nextState = animState.WallGrab;
                        else
                            nextState = animState.WallSlide;
                        break;
                    case playerStates.Hiding:
                        nextState = animState.Hide;
                        break;
                    case playerStates.Vaulting:
                        if (currentState == animState.Vault1)
                            //if (movScript.getInput(KeyCode.D) || movScript.getInput(KeyCode.A))
                            //    nextState = animState.Vault3Run;
                            //else 
                                nextState = animState.Vault2;
                        else
                            nextState = animState.Vault1;

                        break;
                    case playerStates.Recovering:

                        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
                            nextState = animState.Running;
                        else
                            nextState = animState.Vault3;
                        break;
                }
            }

            if (nextState != animState.WallSlide || bigState != playerStates.Clinging)
            {
                rigParent.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                rigParent.transform.localScale = new Vector3(-1, 1, 1);
            }
            failed = next_priority < priority;
            animator.SetInteger("NextState", (int)nextState);
        }
    }
}
