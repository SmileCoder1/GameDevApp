using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Rendering;
using UnityEngine.Timeline;
//using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.InputSystem;
//using UnityEngine.Windows;

public enum playerStates
{
    Grounded,
    Airborne,
    Vaulting,
    Recovering,
    Sliding,
    Clinging,
    Sneaking,
    Hiding,
    Wiring,
    Whacking,
    MonkeyBars, 
    Walking
}

public class virtualButton
{
    public int pressed;
    public bool newPressed;
}
public class StealthMovement : MonoBehaviour
{

    [Header("Playable Or Not")]
    public bool controlsEnabled;


    [Header("Maneuverability")]
    public float acceleration;
    public float airAcceleration;
    public float maxHorizontalSpeed;
    public float maxVerticalSpeed;
    public float globalGravity;
    public float fastFallGravity;
    public float fastFallSpeedCap;

    [Header("Jump")]
    public float jumpHeight;

    [Header("Buffer and timing")]
    public float bufferWindow;

    [Header("Vault")]
    public float vaultAnimationTime;
    public float vaultRecoveryAnimationTime;

    public float forwardVaultSpeedIncrease; //added on to maxspeed
    public float forwardVaultVerticalHeight;
    public float boostLength; //any boost to player speed (such as forward vault) will go away after this amount of time

    public float bigVaultHorizontalSpeed; //not increased over normal top speed
    public float bigVaultVerticalHeight;

    public float upVaultVerticalHeight;

    public float backVaultVerticalHeight;
    public float backVaultHorizontalSpeed; //not increased over normal top speed

    public GameObject[] vaultParticles;
    private Coroutine vaultCoroutine;

    [Header("Slide")]
    public float slideSpeedIncrease;
    public float slideAnimationTime;
    public float slideOvertimeSlowdown;

    [Header("Wall Jump")]
    public float wallJumpVerticalHeight;
    public float wallJumpHorizontalSpeed;
    public float wallSlideGravity;
    public float wallSlideSpeedCap;

    [Header("Sneak")]
    public float sneakSpeed;
    public float sneakNoiseRadius;

    [Header("Other Script Requirements")]
    public LayerMask levelGeometry; //don't put more than one layer in here
    public float floorDetectionMargin; //Needs to be above zero
    public float wallDetectionMargin; //Needs to be above zero
    public bool startBig; //basicaly toggle this at lv 6 anala
                          // Start is called before the first frame update

    [Header("Arnold's janky public variables")]
    public AnimationController controller;
    public AnimationController2 controller2;
    public Animator animator;
    public GameObject vaultEffect;
    Transform space;


    [SerializeField]
    public playerStates state = playerStates.Grounded;
    public enum movementTech
    {
        longJump,
        bigJump,
        highJump,
        backFlip,
        slide,
        sneak,
        prepJump,
        nothing
    }

    private class buttonInput
    {
        public float downLast; //the last time the button was pressed
        public bool down; //is the button currently being pressed?
        public bool justPressed; //if the button was pressed last frame
        public bool read; //past tense

    }

    [HideInInspector] public bool KDown = false, ShiftDown = false, WDown = false, SDown = false, LDown = false, MDown = false, SpaceDown = false;
    [HideInInspector] public bool K = false, Shift = false, S = false, L = false, M = false, Space = false;
    [HideInInspector] public float HorizInput = 0f, VertInput = 0f;



    [Header("Force State Controls")]
    //public bool KDown2 = false, ShiftDown2 = false, WDown2 = false, SDown2 = false, ADown2 = false, LDown2 = false, DDown2 = false, MDown2 = false, SpaceDown2 = false;
    //public bool K2 = false, Shift2 = false, W2 = false, S2 = false, A2 = false, D2 = false, L2 = false, M2 = false, Space2 = false;

    virtualButton K2, LShift2, RShift2, W2, S2, A2, L2, D2, M2, Space2;
    KeyCode[] keyHack = {KeyCode.M, KeyCode.L, KeyCode.K, KeyCode.Space, KeyCode.S, KeyCode.LeftShift};



    private List<InputActionReference> buttons = new List<InputActionReference>();

    private KeyCode[] buttons2 =
        {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.LeftShift,
        KeyCode.RightShift, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.Space};

    public List<virtualButton> virtualButtons = new List<virtualButton>();

    public Dictionary<InputActionReference, virtualButton> actionToVirt = new Dictionary<InputActionReference, virtualButton>();
    public Dictionary<KeyCode, virtualButton> keyToVirt = new Dictionary<KeyCode, virtualButton>();

    
    private Dictionary<InputActionReference, buttonInput> buffer = new Dictionary<InputActionReference, buttonInput>();
    private Dictionary<KeyCode, buttonInput> buffer2 = new Dictionary<KeyCode, buttonInput>();
    public Rigidbody2D rb;
    public BoxCollider2D bc;
    private List<Corner> corners;
    private List<GameObject> hideableObjects;
    private float boostAmt;
    private Vector3 vaultTarget; //points to where we are climbing up to
    private float height;
    private float width;
    private float moveStartTime;
    public movementTech bufferedAction;
    private float noiseRadius; //maximum generally 1
    public SpriteRenderer sr;
    private GameObject hideObject;
    public bool jumpStart = false;
    public bool slideStart = false;
    public bool slideExit = false;
    public bool wallParticles = false;
    public bool sneak = false;
    private bool wallgrab = false;
    [SerializeField] public float facingDir; //-1 means left, 1 means right (or negative positive works)
    public float timescale;
    private bool landing = false;
    private bool prevGround = false;
    public bool killing = false;
    private GameObject hideStart = null;
    public bool vaultPossible = false;
    public bool vaultFudge = false;
    private CameraTarget camTarget;
    private bool stick = false;
    public bool paused = false;
    public bool freeze = false;
    private Vector2 storedVelocity;
    public bool vaultDisallowed;
    [HideInInspector]
    public bool holdingItem = false;
    [HideInInspector] public GameObject itemHeld;
    private GameManager gameManager;
    [SerializeField]
    public InputActionReference movement, cameraMovement, kill, interactL, interactK, playerJump, down, shift;
    private EntityHitbox hitbox;
    public bool logVault = false;
    public bool walk = false;
    public bool walkdir = false;
    public float walkSpeed = 2;

    [HideInInspector] public bool Wopen;
    [HideInInspector] public bool Aopen;
    [HideInInspector] public bool Dopen;
    [HideInInspector] public bool Sopen;
    [HideInInspector] public bool Spaceopen;
    [HideInInspector] public int DtoAdd = 0;
    [HideInInspector] public int AtoAdd = 0;
    [HideInInspector] public int WtoAdd = 0;
    [HideInInspector] public int StoAdd = 0;
    [HideInInspector] public int SHIFTS = 0;
    [HideInInspector] public float forceX;
    [HideInInspector] public float forceY;



    void Start()
    {
        //        void Awake() {
        //    #if UNITY_EDITOR
        //	QualitySettings.vSyncCount = 0;  // VSync must be disabled
        //	Application.targetFrameRate = 45;
        //    #endif
        //}
        //rb.gravityScale = 1;
        Time.timeScale = timescale;
        gameManager = GameManager.Instance;
        camTarget = GetComponentInChildren<CameraTarget>();
        state = playerStates.Grounded;
        Physics2D.IgnoreLayerCollision(gameObject.layer, 8, true);
        controller = GetComponent<AnimationController>();       
        controller2 = GetComponent<AnimationController2>();
        Physics2D.IgnoreLayerCollision(6, 12, true); //move this to game manager later
        corners = new List<Corner>();
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        Physics2D.IgnoreLayerCollision(gameObject.layer, 3, false);
        height = bc.bounds.size.y;
        width = bc.bounds.size.x;
        maxHorizontalSpeed = Mathf.Abs(maxHorizontalSpeed);
        facingDir = 1;
        if (boostLength < 0.05f) boostLength = 0.05f;
        bufferedAction = movementTech.nothing;
        sr = GetComponent<SpriteRenderer>();
        noiseRadius = 0;
        Physics2D.gravity = new Vector2(0, -globalGravity);
        hideObject = null; hideableObjects = new List<GameObject>();
        animator = GetComponentInChildren<Animator>();
        space = GetComponent<Transform>();
        hitbox = GetComponent<EntityHitbox>();


        //virtualButtons = new List<virtualButton>(new[] {W2, A2, S2, D2, LShift2, RShift2, K2, L2, M2, Space2});
        virtualButtons = new List<virtualButton>(new[] { M2, L2, K2, Space2, S2, LShift2 });
        buttons = new List<InputActionReference>(new[] { kill, interactL, interactK, playerJump, down, shift});

        for (int i = 0; i < buttons.Count; i++)
        {
            InputActionReference key = buttons[i];
            buttonInput b = new buttonInput();
            b.downLast = float.NegativeInfinity;
            b.justPressed = false;
            b.down = false;
            b.read = true;
            buffer.Add(key, b);
            virtualButtons[i] = new virtualButton();
            actionToVirt.Add(key, virtualButtons[i]);
            keyToVirt.Add(keyHack[i], virtualButtons[i]);
        }

        if (startBig)
        {
            GetComponent<BigMode>().enabled = true;
        }


        //foreach(KeyCode key in buttons) what this here for lol
        //{
        //    buttonInput b = new buttonInput();
        //    b.downLast = float.NegativeInfinity;
        //    b.justPressed = false;
        //    b.down = false;
        //    b.read = true;
        //    b.updatable = true;
        //    buffer.Add(key, b);
        //}
    }

    public void BIG()
    {
        GetComponent<BigMode>().enabled = true;
    }
    public IEnumerator startStealthKill(GameObject hidingObj = null)
    {
        killing = true;
        controller2.killing = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1.35f);
        killing = false;
        controller2.killing = false;
        
        if (hidingObj)
        {
            //print("rehiding after kill");
            enterHide(hidingObj);
        }

        // give health to player
        hitbox.Heal(1);
    }

    public float getHeight()
    {
        return height;
    }

    private bool rawAction(InputActionReference action)
    {
        //Debug.Log("Cringe input thing: " + action.action.ReadValue<float>());
        return action.action.ReadValue<float>() > 0;
    }

    private void readInputsDown() //called in fixed update - for potato laptops that run less than 50 frames I think
    {
        
            foreach (KeyValuePair<InputActionReference, buttonInput> input in buffer)
            {

                bool toCheck = (controlsEnabled) ? rawAction(input.Key) : actionToVirt[input.Key].newPressed;
                if(toCheck)
                {
                    //if (input.Value.justPressed)
                    //{
                    //    input.Value.justPressed = false;
                    //}
                    if (!input.Value.down)
                    {
                        //Debug.Log("cringe button was up");
                        if (input.Key == space)
                        {
                            //Debug.Log("cringe button was up");
                        }
                        input.Value.justPressed = true;
                        input.Value.read = false;
                        input.Value.downLast = Time.time;
                    }
                    input.Value.down = true;
                }
                else
                {
                    input.Value.down = false;
                }
            }
    }

    private void resetInputs() //maybe uneeded
    {
        foreach (KeyValuePair<InputActionReference, buttonInput> input in buffer)
        {

            input.Value.justPressed = false;
            //if(input.Key != KeyCode.Space || !vaultFudge) 
            actionToVirt[input.Key].newPressed = false;
        }
    }

    public void useInput(InputActionReference key)
    {
        try
        {
            buffer[key].read = true;
        }
        catch
        {
        }
    }

    public bool getInput(InputActionReference key)
    {
        try
        {
            if(controlsEnabled) return buffer[key].down;
            else return actionToVirt[key].pressed > 0;
        }
        catch
        {
            return false;
        }
    }
    public bool getInputDown(InputActionReference key)
    {
        try
        {
            if (Time.time - buffer[key].downLast <= bufferWindow && !buffer[key].read || buffer[key].justPressed)
            {
                //print("getInputdown worksed");
                print(buffer[key].justPressed);
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
        
    }

    public Vector3 position()
    {
        return transform.TransformPoint(bc.offset);
    }

    public float getNoiseRadius()
    {
        return noiseRadius;
    }

    public bool isHiding()
    {
        return state == playerStates.Hiding;
    }
    public void setHideableObject(GameObject hide)
    {
        hideableObjects.Add(hide);
    }
    public void unsetHideableObject(GameObject hide)
    {
        if (hideableObjects.Contains(hide))
        {
            hideableObjects.Remove(hide);
        }
    }

    private void updateAnimationValue()
    {
        //controller.xVelocity = Mathf.Abs(rb.velocity.x);
        //bool prevGround = controller.isGrounded;
        //controller.isGrounded = isGrounded();
        //if (!prevGround && controller.isGrounded)
        //    controller.landing = true;
        //else 
        //    controller.landing = false;
        //if (jumpStart)
        //    controller.jumpStart = true;
        //else
        //    controller.jumpStart = false;
        //if(isWalled(facingDir))
        //    controller.walled = true;
        //else 
        //    controller.walled = false;
        //if (slideStart)
        //    controller.slideStart = true;
        //else
        //    controller.slideStart = false;
        //if (slideExit)
        //    controller.slideExit = true;
        //else
        //    controller.slideExit = false;
        //if (Input.GetKeyDown(KeyCode.Space))
        //    controller.space = true;
        //else
        //    controller.space = false;

        //controller.sneaking = (state == playerStates.Sneaking);
        //controller.wallParticles = wallParticles;
        //if(wallgrab)
        //    controller.wallGrab = true;
        //else 
        //    controller.wallGrab = false;
        //print(state == playerStates.Sneaking);

        //slideStart = false;
        //slideExit = false;
        //jumpStart = false;
        //wallParticles = false;
        //landing = false;
        //if (isGrounded())
        //{
        //    if (!prevGround)
        //    {
        //        landing = true;
        //    }
        //    prevGround = true;
        //}
        //else
        //    prevGround = false;

        //controller2.landing = landing;
        controller2.jumpStart = jumpStart;
        controller2.slideStart = slideStart;
        controller2.slideStop = slideExit;
        //controller2.landing = landing;
        //landing = false;
        jumpStart = false;
        slideStart = false;
        slideExit = false;
    }
    public bool isGrounded()
    {
        Vector2 pos = transform.TransformPoint(bc.offset);
        float width = bc.bounds.size.x;
        float height = bc.bounds.size.y;

        RaycastHit2D leftCorner = Physics2D.Raycast(new Vector2(pos.x - width / 2f,
                                                        pos.y - height / 2f),
                                                        -Vector3.up, floorDetectionMargin, levelGeometry);
        RaycastHit2D rightCorner = Physics2D.Raycast(new Vector2(pos.x + width / 2f,
                                                        pos.y - height / 2f),
                                                        -Vector3.up, floorDetectionMargin, levelGeometry);
        return (leftCorner.collider != null || rightCorner.collider != null);
    }

    public bool isGrounded(bool both)
    {
        Vector2 pos = transform.TransformPoint(bc.offset);
        float width = bc.bounds.size.x;
        float height = bc.bounds.size.y;

        RaycastHit2D leftCorner = Physics2D.Raycast(new Vector2(pos.x - width / 2f,
                                                        pos.y - height / 2f),
                                                        -Vector3.up, floorDetectionMargin, levelGeometry);
        RaycastHit2D rightCorner = Physics2D.Raycast(new Vector2(pos.x + width / 2f,
                                                        pos.y - height / 2f),
                                                        -Vector3.up, floorDetectionMargin, levelGeometry);
        if (both)
        {
            return (leftCorner.collider != null && rightCorner.collider != null);
        }
        else
        {
            return (leftCorner.collider != null || rightCorner.collider != null);
        }
        
    }
    public Collider2D isWalled(float dir, out RaycastHit2D hit)
    {
        Vector2 pos = transform.TransformPoint(bc.offset);
        float width = bc.bounds.size.x;
        float height = bc.bounds.size.y;
        RaycastHit2D topCorner = Physics2D.Raycast(new Vector2(pos.x + (width / 2f - wallDetectionMargin) * dir,
                                                       pos.y + (0.5f) * height / 2f),
                                                       Vector3.right * dir, wallDetectionMargin * 2, levelGeometry);
        RaycastHit2D bottomCorner = Physics2D.Raycast(new Vector2(pos.x + (width / 2f - wallDetectionMargin) * dir,
                                                        pos.y - (0) * height / 2f),
                                                        Vector3.right * dir, wallDetectionMargin*2, levelGeometry);
        if (topCorner.collider != null || bottomCorner.collider != null)
        {
            hit = topCorner;
            return topCorner.collider;
        }
        hit = topCorner;
        return null;
    }
    public Collider2D isWalled(float dir)
    {
        Vector2 pos = transform.TransformPoint(bc.offset);
        float width = bc.bounds.size.x;
        float height = bc.bounds.size.y;
        RaycastHit2D topCorner = Physics2D.Raycast(new Vector2(pos.x + (width / 2f - wallDetectionMargin) * dir,
                                                        pos.y + (0.5f) * height / 2f),
                                                        Vector3.right * dir, wallDetectionMargin * 2, levelGeometry);
        RaycastHit2D bottomCorner = Physics2D.Raycast(new Vector2(pos.x + (width / 2f - wallDetectionMargin) * dir,
                                                         pos.y - (0) * height / 2f),
                                                         Vector3.right * dir, wallDetectionMargin * 2, levelGeometry);
        if (topCorner.collider != null || bottomCorner.collider != null)
        {
            return topCorner.collider;
        }
        return null;
    }
    public bool isCeilinged() //not done yet - this will be for detecting if we can come out of slide/crounch
    {
        Vector2 pos = transform.TransformPoint(bc.offset);
        float width = bc.bounds.size.x;
        float height = bc.bounds.size.y;
        RaycastHit2D leftCorner = Physics2D.Raycast(new Vector2(pos.x - width / 2f,
                                                        pos.y -height/2f+ 0.7f),
                                                        Vector3.up, 1f, levelGeometry);
        RaycastHit2D rightCorner = Physics2D.Raycast(new Vector2(pos.x + width / 2f,
                                                        pos.y -height/2f+ 0.7f),
                                                        Vector3.up, 1f, levelGeometry);
        return (leftCorner.collider != null || rightCorner.collider != null);
    }

    private bool boostSpeedRegulation(float HorizInput) //pass in the current player horizontal input
    {
        float speed = rb.velocity.x;
        if (speed > maxHorizontalSpeed)
        {
            speed -= Time.fixedDeltaTime * (boostAmt) / boostLength;
            
            if (HorizInput < 0)
            {
                speed += HorizInput * acceleration * Time.fixedDeltaTime;
            }
            rb.velocity = new Vector2(speed, rb.velocity.y);
            return true;
        }
        else if (speed < -maxHorizontalSpeed)
        {
            speed += Time.fixedDeltaTime * (boostAmt) / boostLength;
            
            if (HorizInput > 0)
            {
                speed += HorizInput * acceleration * Time.fixedDeltaTime;
            }
            rb.velocity = new Vector2(speed, rb.velocity.y);
            return true;
        }
        return false;
    }
    private void boost(float amount)
    {
        boostAmt = amount;
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            rb.velocity = new Vector2(maxHorizontalSpeed + boostAmt, rb.velocity.y);
        }
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            rb.velocity = new Vector2(-maxHorizontalSpeed - boostAmt, rb.velocity.y);
        }
    }

    public void setFacingDir(float dir)
    {
        if (dir == 0)
        {
            return;
        }
        else
        {
            facingDir = Mathf.Sign(dir);
            if (dir < 0)
            {
                //sr.flipX = true;
                space.localScale = new UnityEngine.Vector3(-1, transform.localScale.y, 1);
            }
            else
            {
                space.localScale = new UnityEngine.Vector3(1, transform.localScale.y, 1);

            }
            //TODO: activate animations for facing the other way
        }
    }

    public void allowVault(bool v)
    {
        vaultDisallowed = !v;
    }
    public void setCorner(Corner corn)
    {
        corners.Add(corn);
    }
    public void unsetCorner(Corner corn)
    {
        if (corners.Contains(corn))
        {
            corners.Remove(corn);
        }
    }
    public void enterHideOnStart(GameObject obj)
    {
        hideStart = obj;
    }
    private void enterHide(GameObject obj)
    {
        hideObject = obj;
        transform.position = hideObject.transform.position
             - Vector3.up * (hideObject.GetComponent<SpriteRenderer>().bounds.size.y / 2
             - bc.bounds.size.y / 2);
        GetComponent<SortingGroup>().sortingLayerName = "HidingLayer";
        gameObject.layer = 11;
        hideObject.GetComponent<Hideable>().fadeOut();

    }
    private void exitHide()
    {
        hideObject.GetComponent<Hideable>().fadeIn();
        hideObject = null;
        GetComponent<SortingGroup>().sortingLayerName = "Entities";
        gameObject.layer = 6;
    }

    private void exitVault()
    {
        //Debug.Log("exit vault");
        transform.position = vaultTarget;
        rb.gravityScale = 1;
        //Physics2D.IgnoreLayerCollision(gameObject.layer, 3, false);
        bc.isTrigger = false;
    }
    private void enterVault(Corner corn)
    {
        corn.activateCorner();
        controller.vault = true;
        vaultTarget = corn.getWorldCoord() + new Vector3(width / 2f * facingDir, height / 2f, 0);
        Vector3 vaultVector = vaultTarget - transform.position;
        rb.gravityScale = 0;
        //Debug.Log("enter vault");
        bc.isTrigger = true;
        //Physics2D.IgnoreLayerCollision(gameObject.layer, 3, true);
        rb.velocity = vaultVector / vaultAnimationTime;
        moveStartTime = Time.time;

        bufferedAction = movementTech.nothing;
        vaultPossible = false;
    }
    private void enterSlide()
    {
        slideStart = true;
        boostAmt = slideSpeedIncrease;
        moveStartTime = Time.time;
        bc.size = new Vector2(width, height/2);
        bc.offset = new Vector2(0, -height/4);
        rb.velocity = Vector2.right * (maxHorizontalSpeed + slideSpeedIncrease) * facingDir;
        //print("SLIDE DEBUG: slide + " + Input.GetAxis("Horizontal"));
    }
    private void exitSlide()
    {
        slideExit = true;
        bc.size = new Vector2(width, height);
        bc.offset = Vector2.zero;
    }
    public void enterSneak()
    {
        bc.size = new Vector2(width, height/2);
        bc.offset = new Vector2(0, -height/4);
    }
    public void exitSneak()
    {
        bc.size = new Vector2(width, height);
        bc.offset = Vector2.zero;
    }
    public void bigJump()
    {
        boostAmt = 2;
        rb.velocity = new Vector2(facingDir * bigVaultHorizontalSpeed, jumpSpeedGivenHeight(bigVaultVerticalHeight));
        StartVaultParticlesCoroutine();
    }
    private void longJump()
    {
        boostAmt = forwardVaultSpeedIncrease;
        rb.velocity = new Vector2(facingDir * (forwardVaultSpeedIncrease + maxHorizontalSpeed), jumpSpeedGivenHeight(forwardVaultVerticalHeight));
        StartVaultParticlesCoroutine();
    }
    private void backFlip()
    {
        setFacingDir(-facingDir);
        rb.velocity = new Vector2(backVaultHorizontalSpeed * facingDir, jumpSpeedGivenHeight(backVaultVerticalHeight));
        StartVaultParticlesCoroutine();
    }
    private void highJump()
    {
        
        rb.velocity = Vector2.up * jumpSpeedGivenHeight(upVaultVerticalHeight);
        StartVaultParticlesCoroutine();
    }
    private void jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeedGivenHeight(jumpHeight));
    } 
    private void enterWallCling(RaycastHit2D hit, float dir)
    {
        //TODO - Wall cling animation
        //transform.position = new Vector3(wall.transform.position.x - (wall.bounds.size.x / 2f + width / 2f) * dir, transform.position.y, 0); //temp - need to fix for tilemaps
        transform.position = new Vector3(Mathf.Round(hit.point.x) - (width / 2f) * dir, transform.position.y, 0);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shift)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            wallgrab = true;

        }
        else
        {
            rb.velocity = new Vector2(0, Mathf.Max(rb.velocity.y, -wallSlideSpeedCap));
            rb.gravityScale = -wallSlideGravity / Physics2D.gravity.y;
        }
        wallParticles = true;
        controller2.rigParent.transform.localScale = new Vector3(-1, 1, 1);

        setFacingDir(-dir);
    }
    private void exitWallCling()
    {
        rb.gravityScale = 1f;
        wallgrab = false;
        if(gameObject.CompareTag("Player"))
            camTarget.setOffset(Vector2.zero);
        setFacingDir(-1*facingDir);
        stick = false;
    }
    private void wallJump()
    {
        exitWallCling();
        setFacingDir(-1*facingDir);
        rb.velocity = new Vector2(wallJumpHorizontalSpeed * facingDir, jumpSpeedGivenHeight(wallJumpVerticalHeight));
    }

    private float jumpSpeedGivenHeight(float height)
    {
        return Mathf.Pow(-2f * Physics2D.gravity.y * height, 0.5f);
    }
    private void callMech()
    {
        //TO-DO: call the transform ability from the player controller script
    }
    // Update is called once per frame
    private void purgeCorners()
    {
        for(int i = 0; i < corners.Count; i++)
        {
            Corner corn = corners[i];
            if (corn.removedFromList)
            {
                corners.Remove(corn);
            }
        }
    }

    private bool enemyVaultOK(Vector2 pos)
    {
        RaycastHit2D check = Physics2D.Raycast(pos + Vector2.right * width / 2f * facingDir - Vector2.up * floorDetectionMargin, Vector3.up, 1f, levelGeometry);
        return(check.collider == null);
    }

    void StateMachine()
    {
        
        Vector2 cameraVector;
       
        K = getInput(interactK);
        Shift = getInput(shift);
        
        S = getInput(down);
        L = getInput(interactL);
        M = getInput(kill);
        Space = getInput(playerJump);

        KDown = getInputDown(interactK);
        ShiftDown = getInputDown(shift);
        SDown = getInputDown(down);
        LDown = getInputDown(interactL);
        MDown = getInputDown(kill);
        SpaceDown = getInputDown(playerJump);

        bool wantToCrouch = GameManager.isToggleCrouchEnabled && SDown || !GameManager.isToggleCrouchEnabled && S;


        //Wopen = W;
        //Aopen = A;
        //Dopen = D;
        //Sopen = S;
        //Spaceopen = Space;

        if (controlsEnabled)
        {
            HorizInput = movement.action.ReadValue<Vector2>().x;
            VertInput = movement.action.ReadValue<Vector2>().y;
            //HorizInput = Input.GetAxisRaw("Horizontal");
            //VertInput = Input.GetAxisRaw("Vertical");
            cameraVector = cameraMovement.action.ReadValue<Vector2>();
        }
        else
        {
            
            HorizInput = Convert.ToInt16(DtoAdd > 0) - Convert.ToInt16(AtoAdd > 0);
            VertInput = Convert.ToInt16(WtoAdd>0) - Convert.ToInt16(StoAdd > 0);
            //HorizInput = D ? 1 : A ? -1 : 0;
            //VertInput = W ? 1 : S ? -1 : 0;
        }







        //GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        //GoombaController viableEnemy = null;
        //foreach (GameObject enemy in enemies)
        //{
        //    print(enemy.name);    
        //    if(Mathf.Abs(Vector2.Distance(enemy.transform.position, transform.position)) < 2f)
        //    {
        //        //Debug.Log("Enemy in range " + enemy.name);
        //        GoombaController ctrl = enemy.GetComponent<GoombaController>();
        //        if (ctrl != null)
        //        {
        //            string currGoombaState = ctrl.currentState.getStateName();
        //            if ((currGoombaState == "GoombaPatroling" || currGoombaState == "GoombaSus") && !(ctrl.isGoingRight ^ (facingDir > 0)))
        //            {
        //                //Debug.Log("viable");
        //                viableEnemy = ctrl;

        //                break;
        //            }
        //        }

        //        //else
        //        //{
        //        //    if(ctrl)Debug.Log(ctrl.isGoingRight + " " + facingDir);
        //        //}
        //    }
        //}
        //if (MDown && !killing && viableEnemy)
        //{
        //    viableEnemy.isAttacked = true;
        //    float newPos = viableEnemy.transform.position.x;
        //    if (viableEnemy.isGoingRight)
        //        newPos -= 0.9f;
        //    else
        //        newPos += 0.9f;
        //    transform.position = new Vector3(newPos, transform.position.y, transform.position.z);
        //    // rotate to face enemy direction
        //    setFacingDir((viableEnemy.isGoingRight) ? 1 : -1);
        //    Destroy(viableEnemy.gameObject.GetComponentInChildren<TutorialSection>().gameObject);
        //    StartCoroutine(startKill());
        //}



        if (hideStart != null)
        {
            enterHide(hideStart);
            hideStart = null;
            state = playerStates.Hiding;
        }

        if (walk)
            state = playerStates.Walking;

        if (!killing)

            switch (state)
            {
                case playerStates.Grounded:
                {
                
                    if (!isGrounded())
                    {
                        //Debug.Log("bounce");
                        //TODO - activate airborne animation (taking into account facingDir)
                        state = playerStates.Airborne;
                        break;
                    }
                    if (LDown
                       && hideableObjects.Count > 0)
                    {
                        useInput(interactL);
                        enterHide(hideableObjects[0]);
                        state = playerStates.Hiding; break;
                    }
                    if (HorizInput * rb.velocity.x < 0f) //if we are trying to turn around
                    {
                        rb.velocity = new Vector2(0, rb.velocity.y);
                        //TODO - activate running animation in new direction (maybe do that in setFacingDir())
                    }
                    if (SpaceDown)
                    {
                        useInput(playerJump);
                        jump();
                        state = playerStates.Airborne;
                        jumpStart = true;
                        break;
                    }
                    if (ShiftDown)
                    {
                        useInput(shift);
                        if (HorizInput * facingDir < 0)
                            setFacingDir(Mathf.Sign(HorizInput));
                        enterSlide();
                        rb.velocity = Vector2.right * (maxHorizontalSpeed + slideSpeedIncrease) * facingDir;
                        state = playerStates.Sliding; break;
                    }
                    if (SDown)
                    {
                        useInput(down);
                        enterSneak();
                        state = playerStates.Sneaking; break;
                    }
                    if (HorizInput * facingDir < 0)
                    {
                        setFacingDir(Mathf.Sign(HorizInput));
                    }
                    
                    else if (Mathf.Abs(HorizInput) < 0.1) //else if we are trying to stop
                    {
                    
                        //TODO - activate standing animation
                        rb.velocity = new Vector2(0, rb.velocity.y);
                    }

                    float speed = rb.velocity.x;
                    if (!boostSpeedRegulation(HorizInput))
                    {
                        speed += HorizInput * acceleration * Time.fixedDeltaTime;
                        if (speed > maxHorizontalSpeed)
                        {
                            rb.velocity = new Vector2(maxHorizontalSpeed, rb.velocity.y);
                        }
                        else if (speed < -maxHorizontalSpeed)
                        {
                            rb.velocity = new Vector2(-maxHorizontalSpeed, rb.velocity.y);
                        }
                        else
                        {
                            rb.velocity = new Vector2(speed, rb.velocity.y);
                        }
                    }
               

                   break;
                }
                case playerStates.Airborne:
                {
                    if (isGrounded() && rb.velocity.y < 0.01f)
                    {
                        rb.gravityScale = 1.0f;
                        if (S)
                        {
                            state = playerStates.Sneaking;
                            enterSneak();
                        }
                        else
                        {
                            state = playerStates.Grounded;
                        }
                            
                            controller2.landing = true;
                            //if (SpaceDown)
                            //{
                            //    Debug.Log("We are jump plese");
                            //    useInput(KeyCode.Space);
                            //    StartCoroutine("jump");
                            //    state = playerStates.Airborne;
                            //    jumpStart = true;
                            //    break;
                            //}
                            break;
                    }
                    if (SDown)
                    {
                        useInput(down);
                        rb.gravityScale = -fastFallGravity / Physics2D.gravity.y;
                        if (rb.velocity.y < -fastFallSpeedCap)
                        {
                            rb.velocity = new Vector2(rb.velocity.x, -fastFallSpeedCap);
                        }
                    }
                    else if (rb.velocity.y < -maxVerticalSpeed)
                    {
                        rb.gravityScale = 1;
                        rb.velocity = new Vector2(rb.velocity.x, -maxVerticalSpeed);
                    }
                    float bigToe = transform.position.x + width / 2 * facingDir;
                    if (corners.Count > 0)
                    {
                        Corner corner = corners[corners.Count - 1];
                        if (
                            ((corner.getDirection() * facingDir < 0 ) //player input and corner should be facing each other if corner has direction
                        && (corner.getWorldCoord().x - bigToe) * facingDir >= 0
                        && corner.getDirection() != 0 
                        && !S
                        /*&& (corner.getWorldCoord().x - bigToe) * HorizInput >= 0
                        && Mathf.Abs(HorizInput) >= 0.2*/)
                        || (corner.getDirection() == 0 && Space && enemyVaultOK(corner.getWorldCoord())))
                        {
                            enterVault(corner);
                            try
                            {
                                GameObject eff = Instantiate(vaultEffect, transform.position - Vector3.up * 0.5f, Quaternion.identity);
                                eff.GetComponent<SpriteRenderer>().sortingLayerName = "Foreground";
                                eff.transform.SetParent(this.transform, true);
                            }
                            catch
                            {
                                Debug.Log("effects not exist pls make it so you don't have to attach them arnold");
                            }
                        
                            state = playerStates.Vaulting;
                            break;
                        }
                    }
                    if (SpaceDown)
                    {
                        useInput(playerJump);
                        if (isWalled(facingDir))
                        {
                            setFacingDir(-facingDir);
                            wallJump();
                        }
                        else if (isWalled(-facingDir))
                        {
                            wallJump();
                        }
                    }
                    Collider2D wall = isWalled(Mathf.Sign(HorizInput), out RaycastHit2D hit);
                    float wallDir = Mathf.Sign(HorizInput);
                    if(wall == null)
                    {
                        wall = isWalled(Mathf.Sign(-1), out hit);
                        wallDir = -1;

                    }

                        //Debug.Log((wall != null) + " Player grab? pls " + (Mathf.Sign(HorizInput) * rb.velocity.x >= 0));
                        if (wall != null
                        && (HorizInput * wallDir > 0 || Shift))
                        //&& Mathf.Sign(HorizInput) * rb.velocity.x >= 0
                        //&& !isGrounded())
                    {
                        if (Shift || rb.velocity.y < 0 && !S)
                        {
                            enterWallCling(hit, Mathf.Sign(wallDir));
                            state = playerStates.Clinging;
                            break;
                        }
                    }

                    if (rb.velocity.x * facingDir < 0f && HorizInput * facingDir <0f) //if we are trying to turn around
                    {
                        setFacingDir(-facingDir);
                    }
                    //airspeed regulation:
                    float speed = rb.velocity.x;
                
                    if (speed > maxHorizontalSpeed)
                    {
                        if (HorizInput < 0)
                        {
                            speed += HorizInput * acceleration * Time.fixedDeltaTime;
                        }
                    }
                    else if (speed < -maxHorizontalSpeed)
                    {
                        if (HorizInput > 0)
                        {
                            speed += HorizInput * acceleration * Time.fixedDeltaTime;
                        }
                    }
                    else
                    {
                        speed += HorizInput * airAcceleration * Time.fixedDeltaTime;
                        if(speed > Mathf.Abs(maxHorizontalSpeed))
                        {
                            speed = Mathf.Sign(speed) * Mathf.Abs(maxHorizontalSpeed);
                        }
                    }
                    rb.velocity = new Vector2(speed, rb.velocity.y);
                
                    break;
                }
                case playerStates.Vaulting:
                {
                    //Debug.Log("vault going");

                        
                    if (SpaceDown)
                    {
                        useInput(playerJump);
                        vaultPossible = true;
                    }
                    if (ShiftDown)
                    {
                        useInput(shift);
                        vaultPossible = true;
                        bufferedAction = movementTech.slide;
                    }
                    else if (bufferedAction != movementTech.slide)
                    {

                        //float horiz = Input.GetAxisRaw("Horizontal") * facingDir;
                        float horiz = HorizInput * facingDir;
                        float vert = VertInput;
                        if (horiz > 0.5f && vert > 0.5f) { bufferedAction = movementTech.bigJump; }
                        else if (horiz >= 0.5f)
                        {
                            //if(bufferedAction == movementTech.highJump || bufferedAction == movementTech.bigJump)
                            //{
                            //    bufferedAction = movementTech.bigJump;
                            //}
                            //else
                            {
                                bufferedAction = movementTech.longJump;
                            }
                        }
                        else if (horiz <= -0.5f) { bufferedAction = movementTech.backFlip; }
                        else if (vert >= 0.5f)
                        {
                            bufferedAction = movementTech.highJump;
                        }
                        else if (vert <= -0.5f) { bufferedAction = movementTech.nothing; } //player DIs down
                        else { bufferedAction = movementTech.prepJump; } //if the player provides no DI
                    }
                    if (Time.time - moveStartTime > vaultAnimationTime && !vaultFudge)
                    {
                        if (vaultDisallowed)
                        {
                            rb.velocity = new Vector2(0, 0);
                            moveStartTime = Time.time;
                            state = playerStates.Grounded;
                            exitVault();
                            break;
                        }
                        
                        
                        Vector2 velocityVector = Vector2.zero;
                        state = playerStates.Airborne;
                        
                        if (S  && !vaultPossible)
                        {
                            bufferedAction = movementTech.sneak;
                            vaultPossible = true;
                        }
                        
                        if(vaultPossible)
                        {
                            switch (bufferedAction)
                            {
                                case movementTech.slide:
                                {
                                    enterSlide();
                                    state = playerStates.Sliding;
                                    break;
                                }
                                case movementTech.bigJump:
                                {
                                    bigJump();
                                    break;
                                }
                                case movementTech.longJump:
                                {
                                    longJump();
                                    break;
                                }
                                case movementTech.backFlip:
                                {
                                    backFlip();
                                    break;
                                }
                                case movementTech.highJump:
                                {
                                    highJump();
                                    break;
                                }
                                case movementTech.nothing:
                                {
                                    //if (D || A)
                                    //    state = playerStates.Grounded;
                                    //else
                                    //{
                                        rb.velocity = new Vector2(0, 0);
                                        moveStartTime = Time.time;
                                        state = playerStates.Recovering;
                                    //}
                                    break;
                                }
                                case movementTech.prepJump:
                                {
                                    rb.velocity = new Vector2(0, 0);
                                    moveStartTime = Time.time;
                                    state = playerStates.Recovering;
                                    break;
                                }
                                case movementTech.sneak:
                                {
                                    rb.velocity = new Vector2(0, 0);
                                    moveStartTime = Time.time;
                                    state = playerStates.Sneaking;
                                    enterSneak();
                                    break;
                                }
                            }
                        }
                        exitVault();
                        if (isCeilinged())
                        {
                            if (!(state == playerStates.Sliding))
                            {
                                state = playerStates.Sneaking;
                                rb.velocity = Vector2.zero;
                                enterSneak();
                            }
                        }
                        else if (!vaultPossible)
                        {
                            rb.velocity = new Vector2(0, 0);
                            moveStartTime = Time.time;
                            state = playerStates.Recovering;
                            break;
                        }

                        break;
                    }
                    
                    break;
                }
                case playerStates.Recovering:
                {
                    
                    //Debug.Log("prep jump = " + (bufferedAction == movementTech.prepJump));
                    bool bufferBreak = false;

                    float speed = rb.velocity.x;
                    //Debug.Log("I speed: "+ speed);
                    if (Mathf.Abs(HorizInput) > 0.1f && HorizInput * facingDir > 0 && !isCeilinged())
                    {
                        //switch to running start animation if not already
                        
                        rb.velocity = new Vector2(rb.velocity.x + maxHorizontalSpeed / vaultRecoveryAnimationTime * Time.fixedDeltaTime * facingDir, 0);
                        //Debug.Log("should be speedign up" + rb.velocity.x);
                    }
                    else if (HorizInput * facingDir < 0)
                    {
                        setFacingDir(-facingDir);
                        state = playerStates.Grounded; break;
                    }
                    //if (!boostSpeedRegulation(HorizInput))
                    //{
                        //speed += HorizInput * acceleration * Time.fixedDeltaTime;
                        //if (speed > maxHorizontalSpeed)
                        //{
                        //    rb.velocity = new Vector2(maxHorizontalSpeed, rb.velocity.y);
                        //}
                        //else if (speed < -maxHorizontalSpeed)
                        //{
                        //    rb.velocity = new Vector2(-maxHorizontalSpeed, rb.velocity.y);
                        //}
                        //else
                        //{
                        //    rb.velocity = new Vector2(speed, rb.velocity.y);
                        //}
                    //}
                    
                    if (SpaceDown && !(bufferedAction == movementTech.prepJump))
                    {
                        useInput(playerJump);
                        bufferBreak = true;
                    }
                    if (S)
                    {
                        bufferedAction = movementTech.sneak;
                        bufferBreak = true;
                    }
                    if (ShiftDown)
                    {
                        useInput(shift);
                        bufferedAction = movementTech.slide;
                        bufferBreak = true;
                    }
                    else
                    {
                        bool DI = true;
                        bool prep = bufferedAction == movementTech.prepJump;
                        float horiz = HorizInput * facingDir;
                        float vert = VertInput;
                        if (horiz > 0.5f && vert > 0.5f) { bufferedAction = movementTech.bigJump; }
                        else if (horiz >= 0.5f)
                        {
                            if (bufferedAction == movementTech.highJump || bufferedAction == movementTech.bigJump)
                            {
                                bufferedAction = movementTech.bigJump;
                            }
                            else
                            {
                                bufferedAction = movementTech.longJump;
                            }
                        }
                        else if (horiz <= -0.5f) { bufferedAction = movementTech.backFlip; }
                        else if (vert >= 0.5f)
                        {
                            bufferedAction = movementTech.highJump;
                        }
                        else if (vert <= -0.5f && bufferedAction != movementTech.prepJump) { bufferedAction = movementTech.nothing;} //player DIs down
                        else { bufferedAction = movementTech.prepJump; DI = false;} //if the player provides no DI
                        if(DI && prep)
                        {
                            bufferBreak = true;
                        }
                    }
                    if(Time.time - moveStartTime > vaultRecoveryAnimationTime)
                    {
                        rb.velocity = new Vector2(rb.velocity.x, 0);
                        state = playerStates.Airborne;
                        state = playerStates.Grounded;
                        break;
                    }
                    if (bufferBreak)
                    {
                        state = playerStates.Airborne;
                        switch (bufferedAction)
                        {
                            case movementTech.slide:
                            {
                                enterSlide();
                                state = playerStates.Sliding;
                                //StartVaultParticlesCoroutine();
                                break;
                            }
                            case movementTech.bigJump:
                            {
                                bigJump();
                             
                                break;
                            }
                            case movementTech.longJump:
                            {
                                longJump();
                                break;
                            }
                            case movementTech.backFlip:
                            {
                                backFlip();
                                break;
                            }
                            case movementTech.highJump:
                            {
                                highJump();
                                break;
                            }
                            case movementTech.nothing:
                            {
                                rb.velocity = new Vector2(rb.velocity.x, 0);
                                bufferedAction = movementTech.nothing;
                                state = playerStates.Grounded;
                                break;
                            }
                            case movementTech.prepJump:
                            {
                                rb.velocity = new Vector2(rb.velocity.x, 0);
                                bufferedAction = movementTech.nothing;
                                state = playerStates.Grounded;
                                break;
                            }
                            case movementTech.sneak:
                            {
                                rb.velocity = new Vector2(0, 0);
                                moveStartTime = Time.time;
                                state = playerStates.Sneaking;
                                enterSneak();
                                break;
                            }
                        }
                        if (isCeilinged())
                        {
                            if (!(state == playerStates.Sliding))
                            {
                                state = playerStates.Sneaking;
                                rb.velocity = Vector2.zero;
                                enterSneak();
                            }
                        }
                        break;
                    }
                    
                    break;
                }
                case playerStates.Sliding:
                {
                    if (Time.time - moveStartTime > slideAnimationTime)
                    {
                        
                        if (isCeilinged() && isGrounded() || S)
                        {
                            if (S)
                            {
                                exitSlide();
                                enterSneak();
                                state = playerStates.Sneaking; break;
                            }
                        }
                        else if (Shift)
                        {
                            float slideTopSpeed = maxHorizontalSpeed + slideSpeedIncrease;
                            float decel = Time.fixedDeltaTime * slideTopSpeed / slideOvertimeSlowdown;
                            rb.velocity = new Vector2(facingDir * Mathf.Max(Mathf.Abs(rb.velocity.x) - decel, 0), 0);
                            if(rb.velocity.x == 0)
                            {
                                exitSlide();
                                if (isGrounded())
                                {
                                    if (S)
                                    {
                                        enterSneak();
                                        state = playerStates.Sneaking; break;
                                    }
                                    else
                                    {
                                        state = playerStates.Grounded; break;
                                    }
                                }
                                else
                                {
                                    state = playerStates.Airborne; break;
                                }
                            }
                            else if (SpaceDown && !isCeilinged()) //if player jumps while slow
                            {
                                useInput(playerJump);
                                exitSlide();
                                jump();
                                jumpStart = true;
                                state = playerStates.Airborne;
                                break;
                            }
                        }
                        else if (!isCeilinged())
                        {
                            exitSlide();
                            if (isGrounded())
                            {
                                if (S)
                                {
                                    enterSneak();
                                    state = playerStates.Sneaking; break;
                                }
                                else
                                {
                                    state = playerStates.Grounded; break;
                                }
                            }
                            else
                            {
                                state = playerStates.Airborne; break;
                            }
                        }
                    }
                    else if (Mathf.Abs(rb.velocity.x) < 0.8f * (maxHorizontalSpeed + slideSpeedIncrease)) //or we hit something
                    {
                        if (!isGrounded())
                        {
                            exitSlide();
                            state = playerStates.Airborne;
                            break;
                        }
                        else
                        {
                            if (isCeilinged())
                            {
                                Debug.Log("we are ing the right place");
                                exitSlide();
                                enterSneak();
                                state = playerStates.Sneaking; break;
                            }
                            else
                            {
                                exitSlide();
                                state = playerStates.Grounded; break;
                            }
                        }
                    }
                    if (!isGrounded()) //if player slides off an edge
                    {
                        exitSlide();
                        state = playerStates.Airborne;
                        break;
                    }
                    else if (SpaceDown && !isCeilinged()) //if player jumps
                    {
                        exitSlide();
                        useInput(playerJump);
                        longJump();
                        jumpStart = true;
                        state = playerStates.Airborne;
                        break;
                    }
                    if (LDown
                        && hideableObjects.Count > 0)
                    {
                        useInput(interactL);
                        exitSlide();
                        enterHide(hideableObjects[0]);
                        state = playerStates.Hiding; break;
                    }
                    break;
                }
                case playerStates.Clinging:
                {
                    if (isGrounded(true))
                    {
                        exitWallCling();
                        state = playerStates.Grounded;
                    }
                    if (Shift)
                    {
                        stick = true;
                    }
                    else
                    {
                        stick = false;
                    }
                    if (stick)
                    {
                        rb.velocity = Vector2.zero;
                        rb.gravityScale = 0;
                    }
                    else
                    {
                        rb.velocity = new Vector2(0, Mathf.Max(rb.velocity.y, -wallSlideSpeedCap));
                        rb.gravityScale = -wallSlideGravity / Physics2D.gravity.y;
                    }
                    if ((HorizInput * facingDir > 0 || S) && !stick && HorizInput * facingDir >= 0) //maybe add condition |input| > 0.2 for joystick?
                    {
                        setFacingDir(-1*facingDir);
                        exitWallCling();
                        state = playerStates.Airborne;
                    }   
                    if (!isWalled(-facingDir))
                    {
                        exitWallCling();
                        state = playerStates.Airborne;
                        break;
                    }
                    
                    if (SpaceDown)
                    {
                        wallJump();
                        useInput(playerJump);
                        state = playerStates.Airborne;
                    }
                    
                    break;
                }
                case playerStates.Sneaking:
                {
                    bool unsneak = GameManager.isToggleCrouchEnabled && (SDown || WDown) 
                        || !GameManager.isToggleCrouchEnabled && !S;
                    if (!isGrounded() && !isCeilinged())
                    {
                        exitSneak();
                        state = playerStates.Airborne;
                        break;
                    }
                    if (SpaceDown && !isCeilinged())
                    {
                        exitSneak();
                        useInput(playerJump);
                        jumpStart = true;
                        StartCoroutine("jump");
                        state = playerStates.Airborne;
                        break;
                    }
                    if (ShiftDown)
                    {
                        useInput(shift);
                        enterSlide();
                        rb.velocity = Vector2.right * (maxHorizontalSpeed + slideSpeedIncrease) * facingDir;
                        state = playerStates.Sliding; break;
                    }
                    if (unsneak && !isCeilinged())
                    {
                        useInput(down);
                        exitSneak();
                        state = playerStates.Grounded; break;
                    }
                    if (LDown
                       && hideableObjects.Count > 0)
                    {
                        useInput(interactL);
                        exitSneak();
                        enterHide(hideableObjects[0]);
                        state = playerStates.Hiding; break;
                    }
                    if (HorizInput   * facingDir < 0)
                    {
                        setFacingDir(Mathf.Sign(HorizInput));
                    }
                    rb.velocity = new Vector2(sneakSpeed * HorizInput, rb.velocity.y);
                    noiseRadius = Mathf.Abs(rb.velocity.x) / sneakSpeed;
                    break;
                }
                case playerStates.Hiding:
                {
                    rb.velocity = Vector2.zero;
                    if (SpaceDown || LDown)
                    {
                        useInput(interactL);
                        useInput(playerJump);
                        exitHide();
                        if (S)
                        {
                            state = playerStates.Sneaking;
                        }
                        else
                        {
                            state = playerStates.Grounded;
                        }
                        break;
                    
                    }
                    break;
                }
                case playerStates.Walking:
                {

                    setFacingDir(walkdir ? -1 : 1);
                    if (walk)
                        rb.velocity = new Vector2(walkdir?-1:1 * walkSpeed, 0);
                    else
                        state = playerStates.Grounded;
                    break;
                }
                default:
                {
                    break;
                }
            }
    }
    void Update()
    {
        //if player dies or goes into hitstun that logic goes here
        //if interrupt
        //ie. state = playerStates.airborne
        
    }

    void FixedUpdate()
    {
        if(!freeze && paused)
        {
            paused = false;
            rb.velocity = storedVelocity;
            rb.gravityScale = 1;

        }
        else if(freeze && !paused)
        {
            paused = true;
            storedVelocity = rb.velocity;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;

        }
        if (!paused)
        {
            readInputsDown();
            int vert = Convert.ToInt32(Input.GetKey(KeyCode.UpArrow)) - Convert.ToInt32(Input.GetKey(KeyCode.DownArrow));
            int horiz = Convert.ToInt32(Input.GetKey(KeyCode.RightArrow)) - Convert.ToInt32(Input.GetKey(KeyCode.LeftArrow));
            if (Mathf.Abs(rb.velocity.x) <= 0.01 && Mathf.Abs(rb.velocity.y) <= 0.01 && (vert != 0 || horiz != 0))
            { 
                if(camTarget)
                    camTarget.setOffset(8 * new Vector2(horiz, vert));
            }
            else
            {
                if(camTarget)
                    camTarget.setOffset(Vector2.zero);
                StateMachine();
            }
            updateAnimationValue();
            resetInputs();
            purgeCorners();
        }
        
        
    }

    public void StartVaultParticlesCoroutine()
    {
        if (vaultCoroutine != null)
            StopCoroutine(vaultCoroutine);

        vaultCoroutine = StartCoroutine(startVaultParticles());
    }

    private IEnumerator startVaultParticles()
    {
        //print("start dash particles");

        foreach (GameObject particles in vaultParticles)
        {
            if(particles != null)
            {
                particles.SetActive(true);
                TrailRenderer lr = particles.GetComponent<TrailRenderer>();
                //Vector2 diff = lr.GetPosition(1) - lr.GetPosition(0);
                float angle = Vector2.Angle(rb.velocity, Vector2.left * facingDir);

                Color col = new Color(angle / 180f, 1 - angle / 180f, 0, 1);
                lr.startColor = col;
                lr.endColor = col;

            }


        }
        float startTime = Time.time;
        yield return new WaitForSeconds(1);

        foreach (GameObject particles in vaultParticles)
        {
            if(particles!=null) particles.SetActive(false);
        }
    }
}
