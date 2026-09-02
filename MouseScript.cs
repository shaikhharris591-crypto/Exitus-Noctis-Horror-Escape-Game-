
using Unity.Netcode;

using UnityEngine;

public class MouseScript : NetworkBehaviour
{
    public Transform spine_rotation_point;
    public bool testModeOn = false;
    public bool moveBodyXrotation=false;
    public static MouseScript Instance;
    public static bool cantUse = false;
    public static bool settingsApplied = false;
    [Header("References")]
    public Transform cameraHolder;
    public Transform bodyTransform;
    // Empty object that holds the camera

    [Header("Settings")]
    public float mouseSensitivity = 200f;
    public static float stSens;
    public static float stTouchSens;
    public float minLookAngle = -90f;
    public float maxLookAngle = 90f;

    float xRotation = 0f; // Pitch (up/down)
    float yRotation = 0f; // Yaw (left/right)


    [Header("Camera Clip")]
    Camera cam;
    public LayerMask wallMask;
    public float clipRadius = 0.05f;
    public float clipDistance = 0.1f;

    void Start()
    {
        if (testModeOn) return;

        cam = GetComponentInChildren<Camera>();
        CameraManager.Register(gameObject);





        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {

        //if (!IsOwner && NetworkManager.Singleton != null) { Instance = null; enabled = false; }
        if ((PlayerMovement.Instance != null && PlayerMovement.Instance.isKilled) || cantUse) return;

       
            MouseLookRotation();

        PreventCameraClip();

        ApplySettings();
    }
    private int cameraFingerId = -1;
    private Vector2 lastTouchPos;

    [Header("Mobile")]
    public float touchSensitivity = 0.12f;
    public float maxDeltaPerFrame = 25f;

    private void LateUpdate()
    {
        if (!moveBodyXrotation) return;
          Quaternion animRotation = spine_rotation_point.localRotation;

            float angle = xRotation;

            spine_rotation_point.localRotation = animRotation * Quaternion.Euler(angle, 0, 0);
        
    }
    void HandleCameraTouch()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                if (cameraFingerId == -1 && touch.position.x > Screen.width * 0.5f)
                {
                    cameraFingerId = touch.fingerId;
                    lastTouchPos = touch.position;
                }
            }

            if (touch.fingerId != cameraFingerId)
                continue;

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - lastTouchPos;
                lastTouchPos = touch.position;

                // Ignore insane spikes
                delta = Vector2.ClampMagnitude(delta, maxDeltaPerFrame);

                xRotation -= delta.y * touchSensitivity;
                xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

                yRotation += delta.x * touchSensitivity;

                cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                bodyTransform.rotation = Quaternion.Euler(0f, yRotation, 0f);
                //if(moveBodyXrotation)spine_rotation_point.rotation = Quaternion.Euler(xRotation, 0f,0f);
            }

            if (touch.phase == TouchPhase.Ended ||
                touch.phase == TouchPhase.Canceled)
            {
                cameraFingerId = -1;
            }

            break;
        }
    }
    void MouseLookRotation()
    {


        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical rotation (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        // Horizontal rotation (yaw)
        yRotation += mouseX;

        // Apply both rotations to the camera holder
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        bodyTransform.Rotate(Vector3.up * mouseX);
    }

    public void ApplySettings()
    {
        if (settingsApplied)
        {
            mouseSensitivity = stSens;
            touchSensitivity = stTouchSens;
            settingsApplied = false;

        }
    }
    public void DisableMouse(bool b)
    {
        Cursor.lockState = b ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = b;
        cantUse = b;
    }
   
    

    void PreventCameraClip()
    {
        if (cam == null) return;

        if (Physics.SphereCast(
            cam.transform.position,
            clipRadius,
            cam.transform.forward,
            out RaycastHit hit,
            clipDistance,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            cameraHolder.position -= cam.transform.forward * (clipDistance - hit.distance);
        }
    }
}
