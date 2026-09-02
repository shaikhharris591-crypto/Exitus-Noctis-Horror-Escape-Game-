using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;


public class DrawerControl : NetworkBehaviour
{
    public bool isSelected=false;
   
    public bool selectedForDisabled = false;
    public float moveSpeed = 2f;
    public float smoothTime = 0.3f;
  
    
    public Vector3 closedPos;
    Vector3 pos;
    float   target;

    public float targetZ = 0.0626f;
   
    public AudioSource openSound, closeSound;

    
    private NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isAnimating = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private void Start()
    {
        closedPos = transform.localPosition;
      
        enabled = false;

      
       var openObj = FindChildByNameContains(gameObject.transform, "open");
       var closeObj = FindChildByNameContains(gameObject.transform,"close");
      
        
        if (closeObj != null)
            closeSound = closeObj.GetComponent<AudioSource>();
    }
    Transform FindChildByNameContains(Transform parent, string keyword)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return child; // found it
            }
        }
        return null; // not found
    }

    void Update()
    {
        // Only owner can trigger input
        
        if (Input.GetKeyDown(PlayerMovement.InputKeys.interactKey) && !isAnimating.Value)
        {





            ToggleDrawer();



        }
         pos = transform.localPosition;

         target = isOpen.Value ? targetZ : closedPos.z;

        pos.z = Mathf.MoveTowards(
            pos.z,
            target,
            moveSpeed * Time.deltaTime);

        transform.localPosition = pos;
        // Smooth movement for everyone

        ReportAnimating();

        if (selectedForDisabled && !isAnimating.Value)
        {
            selectedForDisabled = false;
            enabled = false;
        }
    }

    [ServerRpc]
    void ReportAnimatingServerRpc()
    {
        isAnimating.Value = (pos.z != target);
    }
   
    void ReportAnimatingLocal()
    {
        isAnimating.Value = (pos.z != target);
    }
    public void ReportAnimating() 
    {
    if(NetworkManager.Singleton==null)
            ReportAnimatingLocal();
    else
            ReportAnimatingServerRpc();
    }



    [ServerRpc]
    void ToggleDrawerServerRpc()
    {
        isOpen.Value = !isOpen.Value;
        PlaySound(isOpen.Value ? "open" : "close");
    }

    void ToggleDrawerLocal()
    {
        isOpen.Value = !isOpen.Value;
        PlaySound(isOpen.Value ? "open" : "close");
    }

    public void ToggleDrawer() 
    {
        if (NetworkManager.Singleton == null)
            ToggleDrawerLocal();
        else
            ToggleDrawerServerRpc();
    }

    public void PlaySound(string val) 
    {
        if (NetworkManager.Singleton != null)
            PlaySoundClientRpc(val);
        else
            PlaySoundLocal(val);
        
        
    }

    [ClientRpc]
    void PlaySoundClientRpc(string val) 
    {
        if (val == "close")
        {
            if (closeSound != null)
                closeSound.Play();
        }

        else
        {
            if (openSound != null)
                openSound.Play();
        }
    }
    void PlaySoundLocal(string val) 
    {
        if (val == "close")
        {
            if (closeSound != null)
                closeSound.Play();
        }

        else
        {
            if (openSound != null)
                openSound.Play();
        }
    }
    

    void OnEnable()
    {
        if (isSelected) return; // already selected
        isSelected = true;
    }

    void OnDisable()
    {
        isSelected = false;
    }

}
