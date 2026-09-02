using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.Netcode.Components;

using UnityEngine;
using UnityEngine.SceneManagement;



[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Step Climb Logic")]
    public bool includeHandleStepOffsets = false;
    public float stepRadius = 0.2f;
    public float stepOffset = 0.2f;
    public float stepCheckHeight = 0.1f;
    public float stepCheckOffset = 0.6f;


   
    bool setKilled = false;
    public GameObject killCam;
    public bool isKilled = false;
    public bool hasTorch;
    float originalWalkSpeed;
    public static PlayerMovement Instance { get; set; }
    //public MouseScript mouse;

    public Animator localAnimator;
    public NetworkAnimator netAnimator;

    public Transform cameraTransform;
    public Transform handTransform;
    public Transform itemHandTransform;
    public Transform handPos;

    public Vector3 containerPos;
    public Vector3 containerRot;
    public Vector3 handScale;



    bool moving = false;


    #region keys
    public class InputKeys : MonoBehaviour
    {
        // Movement
        public static KeyCode forwardKey = KeyCode.W;
        public static KeyCode backwardKey = KeyCode.S;
        public static KeyCode leftKey = KeyCode.A;
        public static KeyCode rightKey = KeyCode.D;

        // Actions
        
        public static KeyCode jumpKey = KeyCode.Space;
        public static KeyCode runKey = KeyCode.LeftShift;
        public static KeyCode crouchKey = KeyCode.LeftControl;
        //itemFunctions
        public static KeyCode interactKey = KeyCode.F;
        public static KeyCode dropKey = KeyCode.H;
        public static KeyCode functionKey = KeyCode.G;


        //useless for now--------------------------------
        public static KeyCode toggleCrouchKey = KeyCode.C;

        // System
        public static KeyCode pauseKey = KeyCode.Escape;
    }

    #endregion
   


    public List<Camera> cams;
    public enum PlayerState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Crouch,
        UnCrouch,
        CrouchIdle,
        CrouchWalk,
        Died,

        // new item-specific states
        IdleItem,
        WalkItem,
        RunItem,
        CrouchIdleItem,
        CrouchWalkItem
    }


    [Header("Slope")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;

    private bool isSprinting;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpForce = 7f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    private float crouchTime = 0f;
    public float crouchSpeed = 2.4f;

    [Header("References")]
    public Animator animator;
    public AudioSource walkSound, runSound;

    public Rigidbody rb;

    private bool grounded;
    private bool crouching;

    private Vector3 movement;



    public PlayerState State { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log(
       $"Object:{name}\n" +
       $"NetworkObjectId:{NetworkObjectId}\n" +
       $"Owner:{OwnerClientId}\n" +
       $"Local:{NetworkManager.Singleton.LocalClientId}\n" +
       $"IsOwner:{IsOwner}\n" +
       $"IsServer:{IsServer}\n" +
       $"IsClient:{IsClient}"
   );
        CameraManager.Register(cameraTransform.gameObject);
        // Debug.Log("IsOwner" + IsOwner);
        if (!IsOwner)
        {
            MouseScript mouseScript = cameraTransform.GetComponent<MouseScript>();
            mouseScript.enabled = false;

            cameraTransform.gameObject.SetActive(false);

            enabled = false; tag = "Player";
            Instance = null;
        }
        else
            Instance = this;

    }
    void Awake()
    {
        // netAnimator = GetComponent<NetworkAnimator>();
        bool isLocal = NetworkManager.Singleton == null;
        if (isLocal) Instance = this;
        tag = "MainPlayer";

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

    }
    void Start()
    {
        standingHeight = transform.localScale.y;
        originalWalkSpeed = walkSpeed;
        bool isLocal = NetworkManager.Singleton == null;
        animator = isLocal ? localAnimator : netAnimator.Animator;
        if (stepOffset == 0) 
            stepOffset = 0.2f;
       // if (stepCheckHeight == 0)
            stepCheckHeight = 0.41f;
        //if (stepCheckOffset == 0) 
            stepCheckOffset = 0.6f;
        //if (crouchSpeed == 0 || crouchSpeed > wal)
            crouchSpeed = walkSpeed / 2;
    }
    bool assigned = false;
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "main game")
        {
            rb.isKinematic = false;
        }



        if (!isKilled) setKilled = false;
        isKilled = State == PlayerState.Died;
        if (State == PlayerState.Died && !setKilled)
        {
            killCam.SetActive(true);
            SetCrouchedItemLayerWeight(0f);
            SetItemLayerWeight(0f);
            SetKillLayerWeight(1f);
            setKilled = true;

            return;
        }

        GroundCheck();
        CheckForPlayerEntered();
        CheckForPlayerEnteredMainBuilding();
        ReadMovement();
        HandleJump();
        HandleCrouch();
        Move();
        HandleStepOffset();
        HandleState();
        UpdateAnimator();
        CheckForPlayerOnTopFloor();
    }

    private void CheckForPlayerOnTopFloorLocal()
    {
        GameManager.Instance.isPlayerOnTopFloor.Value = Physics
            .OverlapSphere(groundCheck.position, groundDistance)
            .Any(c => c.CompareTag(GameManager.Instance.topFloorTag));
    }
    [ServerRpc]
    private void CheckForPlayerOnTopFloorServerRpc() 
    {
        GameManager.Instance.isPlayerOnTopFloor.Value = Physics
               .OverlapSphere(groundCheck.position, groundDistance)
               .Any(c => c.CompareTag(GameManager.Instance.topFloorTag));
    }

    public void CheckForPlayerOnTopFloor() 
    {
       if(NetworkManager.Singleton==null)
            CheckForPlayerOnTopFloorLocal();
       else
            CheckForPlayerOnTopFloorServerRpc();
    }
    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 1.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle > 0 && angle <= maxSlopeAngle;
        }
        return false;
    }

    Vector3 GetSlopeDirection()
    {
        return Vector3.ProjectOnPlane(movement, slopeHit.normal).normalized;
    }

    void ReadMovement()
    {

        float x = 0;
        float z = 0;

   

        // Left
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            x -= 1f;

        // Right
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            x += 1f;

        // Forward
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            z += 1f;

        // Backward
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            z -= 1f;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        movement = (forward * z + right * x).normalized;
    }

    void Move()
    {
        isSprinting = (Input.GetKey(InputKeys.runKey) &&
                      movement.magnitude > 0 &&
                      grounded &&
                      !crouching);

        float speed = isSprinting ? runSpeed : walkSpeed;

        Vector3 velocity = rb.linearVelocity;

        if (OnSlope())
        {
            Vector3 slopeMove = GetSlopeDirection() * speed;
            velocity.x = slopeMove.x;
            velocity.z = slopeMove.z;
        }
        else
        {
            velocity.x = movement.x * speed;
            velocity.z = movement.z * speed;
        }

        rb.linearVelocity = velocity;
        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        moving = movement.magnitude > 0.1f && grounded;

        if (!moving)
        {
            if (walkSound.isPlaying) walkSound.Stop();
            if (runSound.isPlaying) runSound.Stop();
            return;
        }

        if (isSprinting)
        {
            if (walkSound.isPlaying) walkSound.Stop();
            if (!runSound.isPlaying) runSound.Play();
        }
        else
        {
            if (runSound.isPlaying) runSound.Stop();
            if (!walkSound.isPlaying) walkSound.Play();
        }
    }

    public void HandleJump()
    {


        if (Input.GetKeyDown(InputKeys.jumpKey))
        {
            Jump();
        }
    }

    public void Jump()
    {
        if (!grounded) return;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        SetState(PlayerState.Jump);
    }
    public void Crouch()
    {
        
        walkSpeed = crouchSpeed;
        crouching = true;


        crouchTime += Time.deltaTime;

        Vector3 scale = transform.localScale;
        scale.y = crouchHeight;
        transform.localScale = scale;

        SetState(PlayerState.Crouch);

        // Enter crouch layer
        SetCrouchLayerWeight(1f);
        animator.SetTrigger("crouch");
    }

    public void UnCrouch()
    {
        walkSpeed = originalWalkSpeed;
        crouching = false;
        crouchTime = 0f;

        Vector3 scale = transform.localScale;
        scale.y = standingHeight;
        transform.localScale = scale;

        SetState(PlayerState.UnCrouch);

        // Exit crouch layer
        animator.SetTrigger("uncrouch");
        SetCrouchLayerWeight(0f);
    }
    void HandleCrouch()
    {

        if (Input.GetKey(InputKeys.crouchKey))
        {
            // accumulate crouch time while holding

            Crouch();
        }

        if (Input.GetKeyUp(InputKeys.crouchKey))
        {
            UnCrouch();
        }
    }


    void GroundCheck()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        rb.useGravity = !OnSlope();
    }

    void CheckForPlayerEntered()
    { 
    if(NetworkManager.Singleton==null)
            CheckForPlayerEnteredlocal();
    else
            CheckForPlayerEnteredServerRpc();
    }
        [ServerRpc]
    void CheckForPlayerEnteredServerRpc()
    {
        bool playerEntered = Physics.CheckSphere(groundCheck.position, groundDistance, GameManager.Instance.enterTriggerMask);

        if (playerEntered) GameManager.Instance.playerEntered.Value = true;
    }
    void CheckForPlayerEnteredlocal()
    {
        bool playerEntered = Physics.CheckSphere(groundCheck.position, groundDistance, GameManager.Instance.enterTriggerMask);

        if (playerEntered) GameManager.Instance.playerEntered.Value = true;
    }
    [ServerRpc]
    void CheckForPlayerEnteredMainBuildingServerRpc()
    {
        bool playerEntered = Physics.CheckSphere(groundCheck.position, groundDistance, GameManager.Instance.buildingEnterTriggerMask);

        if (playerEntered) GameManager.Instance.playerEnteredBuilding.Value++;

        
    }

    void CheckForPlayerEnteredMainBuildingLocal()
    {
        GameManager.Instance.playerEnteredBuildingLocal = Physics.CheckSphere(groundCheck.position, groundDistance, GameManager.Instance.buildingEnterTriggerMask);


    }

    public void CheckForPlayerEnteredMainBuilding() 
    {
    if(NetworkManager.Singleton==null)
            CheckForPlayerEnteredMainBuildingLocal();
    else
            CheckForPlayerEnteredMainBuildingServerRpc();
    }

    void HandleState()
    {


        if (!grounded)
        {
            SetState(PlayerState.Jump);
            return;
        }

        if (crouching)
        {
            if (moving)
            {
                SetState(!hasTorch ? PlayerState.CrouchWalk : PlayerState.CrouchWalkItem);
            }
            else
            {
                if (crouchTime > 0.5f)
                    SetState(!hasTorch ? PlayerState.CrouchIdle : PlayerState.CrouchIdleItem);
                else
                    SetState(PlayerState.Crouch);



            }
        }

        if (movement.magnitude < 0.1f)
        {
            SetState(!hasTorch ? PlayerState.Idle : PlayerState.IdleItem);
            return;
        }

        if (isSprinting)
            SetState(!hasTorch ? PlayerState.Run : PlayerState.RunItem);
        else
            SetState(!hasTorch ? PlayerState.Walk : PlayerState.WalkItem);
    }

    void SetState(PlayerState newState)
    {
        if (State == newState) return;
        State = newState;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        SwitchStates();

    }

    public void SwitchStates()
    {
        animator.ResetTrigger("idle");
        animator.ResetTrigger("walk");
        animator.ResetTrigger("run");
        animator.ResetTrigger("jump");

        // default: turn off item layers
        SetItemLayerWeight(0f);
        SetCrouchedItemLayerWeight(0f);

        switch (State)
        {
            case PlayerState.Idle:
            case PlayerState.CrouchIdle:
                animator.SetTrigger("idle");
                break;

            case PlayerState.Walk:
            case PlayerState.CrouchWalk:
                animator.SetTrigger("walk");
                break;

            case PlayerState.Run:
                animator.SetTrigger("run");
                break;

            case PlayerState.Jump:
                animator.SetTrigger("jump");
                break;

            case PlayerState.Crouch:
                // handled in Crouch()
                break;

            case PlayerState.UnCrouch:
                // handled in Crouch()
                break;


            // item logic
            case PlayerState.IdleItem:
                animator.SetTrigger("idle");
                SetItemLayerWeight(1f);
                break;

            case PlayerState.WalkItem:
                animator.SetTrigger("walk");
                SetItemLayerWeight(1f);
                break;

            case PlayerState.RunItem:
                animator.SetTrigger("run");
                SetItemLayerWeight(1f);
                break;

            // crouched item logic
            case PlayerState.CrouchIdleItem:
                animator.SetTrigger("idle");
                SetCrouchedItemLayerWeight(1f);
                break;

            case PlayerState.CrouchWalkItem:
                animator.SetTrigger("walk");
                SetCrouchedItemLayerWeight(1f);
                break;
        }
    }

    void SetCrouchLayerWeight(float weight)
    {
        int crouchLayerIndex = animator.GetLayerIndex("crouchLayer");
        if (crouchLayerIndex != -1)
        {
            animator.SetLayerWeight(crouchLayerIndex, weight);
        }
    }
    void SetItemLayerWeight(float weight)
    {
        int itemLayerIndex = animator.GetLayerIndex("itemLayer");
        if (itemLayerIndex != -1)
        {
            animator.SetLayerWeight(itemLayerIndex, weight);
        }
    }

    void SetKillLayerWeight(float weight)
    {
        int itemLayerIndex = animator.GetLayerIndex("killLayer");
        if (itemLayerIndex != -1)
        {
            animator.SetLayerWeight(itemLayerIndex, weight);
        }
    }
    void SetCrouchedItemLayerWeight(float weight)
    {
        int crouchedItemLayerIndex = animator.GetLayerIndex("crouchedItemLayer");
        if (crouchedItemLayerIndex != -1)
        {
            animator.SetLayerWeight(crouchedItemLayerIndex, weight);
        }
    }


    public void OnPlayerKilled()
    {
        rb.isKinematic = true;
        State = PlayerState.Died;
        SwitchLayer("Default");

        if (InventoryManager.Instance != null)
        {
            var obj = InventoryManager.Instance.GetSelectedGameObject();
            if (obj != null && obj.GetComponent<ItemPickup>() != null)
                obj.GetComponent<ItemPickup>().Drop();
        }
    }
    public void OnRevived()
    {
        rb.isKinematic = false;
        Debug.LogError("Revive system Needs to be built yet");
    }
    public void SwitchLayer(string newLayerName)
    {
        int layerIndex = LayerMask.NameToLayer(newLayerName);

        if (layerIndex == -1)
        {
            Debug.LogError("Layer '" + newLayerName + "' does not exist!");
            return;
        }

        gameObject.layer = layerIndex;
    }

    public void Spectate()
    {
        int lim = CameraManager.activeCameras.Count - 1;
        int randomIndex = Random.Range(0, lim);
        float h = Input.GetAxis("Horizontal");

        if (h != 0f)
        {
            if (h > 0f)
                if (randomIndex != lim) randomIndex++;
                else { if (randomIndex != 0) randomIndex--; }

        }

        HandleSpectateMode(randomIndex);


    }

    void HandleSpectateMode(int i)
    {

        // disable all first so only one is active
        foreach (var camHolder in CameraManager.activeCameras)
        {
            camHolder.SetActive(false);
        }

        cameraTransform.gameObject.SetActive(false);
        CameraManager.activeCameras[i].SetActive(true);
    }
    void HandleStepOffset()
    {
        if (!includeHandleStepOffsets) return;
        Vector3 origin = groundCheck.position + movement.normalized * stepCheckOffset;

        Collider[] overlaps = Physics.OverlapSphere(origin, stepRadius, groundMask);

        foreach (Collider col in overlaps)
        {
            // Example using tag
            if (!col.CompareTag("Step"))
                continue;

            // OR use layer instead:
            // if (col.gameObject.layer != LayerMask.NameToLayer("Step"))
            //     continue;

            RaycastHit hit;

            if (Physics.Raycast(origin + Vector3.up * stepCheckHeight,
                                Vector3.down,
                                out hit,
                                2f,
                                groundMask))
            {
                float targetY = hit.point.y;
                float currentY = groundCheck.position.y;

                RaycastHit groundHit;

                if (Physics.Raycast(groundCheck.position,
                                    Vector3.down,
                                    out groundHit,
                                    2f,
                                    groundMask))
                {
                    if (groundHit.transform != hit.transform &&
                        targetY > currentY &&
                        targetY - currentY <= stepOffset)
                    {
                        Debug.Log("Climbing Step");
                        rb.MovePosition(new Vector3(
                            rb.position.x,
                            targetY + stepOffset,
                            rb.position.z
                        ));
                    }
                }
            }

            break; // only use the first valid step
        }
    }

    private void OnDrawGizmos()
    {
        // Vector3 origin = new(groundCheck.position.x, groundCheck.position.y, groundCheck.position.z + stepCheckOffset);
        Vector3 origin = groundCheck.position + movement.normalized * stepCheckOffset;
        Debug.DrawRay(origin + Vector3.up * stepCheckHeight, Vector3.down, Color.green);
        Debug.DrawRay(groundCheck.position, Vector3.down, Color.yellow);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, stepRadius);

    }
    public static string FindKeyAssignment(KeyCode key)
    {
        foreach (FieldInfo field in typeof(InputKeys).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            KeyCode value = (KeyCode)field.GetValue(null);
            if (value == key)
                return field.Name; // return the field name that has this key
        }
        return null; // not found
    }

}
