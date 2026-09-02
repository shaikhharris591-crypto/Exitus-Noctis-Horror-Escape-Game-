using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.UI;

public class DoorControl : NetworkBehaviour
{

    public Rigidbody associatedLockRb;
    public AudioSource lockSound, openSound, closeSound;
    NetworkVariable<bool> locked =new( false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    NetworkVariable<bool> isOpen = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Animator doorAnimator, animator;
    public NetworkAnimator netAnimator;
   

  

    private void Awake()
    {
        

        animator = NetworkManager.Singleton == null
            ? GetComponent<Animator>()
            : GetComponent<NetworkAnimator>().Animator;

        enabled = false;
    }

    private void Update()
    {


        if (Input.GetKeyDown(PlayerMovement.InputKeys.interactKey))
        {
            ToggleDoor();
        }
    }
    public void ToggleDoor() 
    {
        if (NetworkManager.Singleton != null)
            ToggleDoorServerRpc();
        else
            ToggleDoorLocal();
    }

    [ServerRpc]
    private void ToggleDoorServerRpc()
    {
        if (locked.Value)
        {
            PlaySoundClientRpc("lock");
            return;
        }

        if (isOpen.Value)
        {
            animator.SetTrigger("close");
            PlaySoundClientRpc("close");
        }
        else
        {
            animator.SetTrigger("open");
            PlaySoundClientRpc("open");
        }

        
            isOpen.Value = !isOpen.Value;
        

    }

    [ClientRpc]
    private void PlaySoundClientRpc(string clipName)
    {
        switch (clipName)
        {
            case "lock":
                if (lockSound != null) lockSound.Play();
                break;
            case "open":
                if (openSound != null) openSound.Play();
                break;
            case "close":
                if (closeSound != null) closeSound.Play();
                break;
        }
    }

    private void PlaySoundLocal(string clipName)
    {
        switch (clipName)
        {
            case "lock":
                if (lockSound != null) lockSound.Play();
                break;
            case "open":
                if (openSound != null) openSound.Play();
                break;
            case "close":
                if (closeSound != null) closeSound.Play();
                break;
        }
    }

    private  void ToggleDoorLocal() 
    {
        if (locked.Value)
        {
            PlaySoundLocal("lock");
            return;
        }

        if (isOpen.Value)
        {
            animator.SetTrigger("close");
            PlaySoundLocal("close");
        }
        else
        {
            animator.SetTrigger("open");
            PlaySoundLocal("open");
        }

        isOpen.Value = !isOpen.Value;
    }

    [ServerRpc]
    private void UnlockDoorServerRpc(bool unlockDoor)
    {
      
        locked.Value = !unlockDoor;
        if(associatedLockRb!=null)
        associatedLockRb.isKinematic = false;

    }

    
    private void UnlockDoorLocal(bool unlockDoor)
    {
       
        locked.Value = !unlockDoor;
        if (associatedLockRb != null)
            associatedLockRb.isKinematic = false;

    }

    public void UnlockDoor(bool unlockDoor) 
    {
        if (NetworkManager.Singleton == null)
            UnlockDoorLocal(unlockDoor);
        else
            UnlockDoorServerRpc(unlockDoor);
    
    }
}
