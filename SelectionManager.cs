using Unity.Netcode;
using UnityEngine;

public class SelectionManager : NetworkBehaviour
{
    public GameObject interactionText;
    public Camera cam;
    public float raydist = 4.3f;
    // bool playerIsLookingAtMannequin = false;
    private DrawerControl drawerControl;
    private DoorControl doorControl;
    private ItemPickup item;
    private NetworkBehaviour currentScript;
    private CodeManager codeManager;
    private FuseBoxController fuseBox;
    private GameObject currentTarget; // track which object is selected

    void Start()
    {
        if (!IsOwner && NetworkManager.Singleton != null)
            enabled = false;

        cam = PlayerMovement.Instance.cameraTransform.GetComponentInChildren<Camera>();
    }

    void Update()
    {
        GameObject hitObj = null;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * raydist, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, raydist))
        {
            if (hit.collider.CompareTag("interactable"))
            {
                hitObj = hit.collider.gameObject;
                interactionText.SetActive(true);
            }
            else
                interactionText.SetActive(false);
           
            
        }

        // Same object -> nothing to do
        if (hitObj == currentTarget)
            return;

        // Disable previous object
        if (currentScript != null)
        {
            if (currentScript == drawerControl && drawerControl != null)
            {
                if (!drawerControl.isAnimating.Value)
                {
                    DisableCurrent();
                }
                else
                {
                    drawerControl.selectedForDisabled = true;
                    ResetAll();
                }
            }
            else if (currentScript == codeManager && codeManager != null)
            {
                if (!codeManager.isCodeActive.Value)
                {
                    DisableCurrent();
                }
                else
                {
                    codeManager.isSelected = true;

                   ResetAll();
                }
            }
            else if (currentScript == item && item != null)
            {
                if (!item.equipped.Value)
                {
                    DisableCurrent();
                }
            }
            else
            {
                DisableCurrent();
            }
        }

        // Enable the new object immediately
        if (hitObj != null)
        {
            
            AssignScript(hitObj);
        }
    }
    void AssignScript(GameObject obj)
    {
        doorControl = obj.GetComponent<DoorControl>();
        drawerControl = obj.GetComponent<DrawerControl>();
        item = obj.GetComponent<ItemPickup>();
        codeManager = obj.GetComponent<CodeManager>();
        fuseBox = obj.GetComponent<FuseBoxController>();

        if (doorControl != null)
            currentScript = doorControl;
        else if (drawerControl != null)
            currentScript = drawerControl;
        else if (item != null)
            currentScript = item;
        else if (codeManager != null)
            currentScript = codeManager;
        else if (fuseBox != null)
            currentScript = fuseBox;

        if (currentScript != null)
        {
            Debug.Log("Enabling: " + currentScript.GetType().Name);
            currentScript.enabled = true;
            currentTarget = obj;
        }
    }

    void DisableCurrent()
    {
        Debug.Log("Disabling: " + currentScript.GetType().Name);
        currentScript.enabled = false;
        currentScript = null;
        currentTarget = null;
        drawerControl = null;
        doorControl = null;
        item = null;
        codeManager = null;
        fuseBox = null;

    }
    void ResetAll() 
    {
        currentScript = null;
        currentTarget = null;
        drawerControl = null;
        doorControl = null;
        item = null;
        codeManager = null;

        fuseBox = null;
    }
}
