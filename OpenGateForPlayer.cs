using System;
using System.Threading;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class OpenGateForPlayer : NetworkBehaviour
{

    public static OpenGateForPlayer Instance;
    

  public  AudioSource openSound;
    public AudioSource closeSound;
  public Animator animatorRef;
    public Animator localAnimator;
    public NetworkAnimator networkAnimator;
    bool isLocal;
   public float openTime=2.5f;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(openTime==0f)
        openTime = 2.5f;
        isLocal = NetworkManager.Singleton == null;
        animatorRef = isLocal ? localAnimator : networkAnimator.Animator;
        Invoke(nameof(OpenDoor),2.5f); 
    }

    public void OpenDoor()
    {
      
        animatorRef.SetTrigger("open");
        PlaySound("open");
    }
    public void CloseDoor()
    {

        animatorRef.SetTrigger("close");
        PlaySound("close");
    }


    private void PlaySound(string state)
    {
        if (isLocal)
           PlaySoundLocal(state);
        else
            PlaySoundServerRpc(state);
            
    }

    [ServerRpc(RequireOwnership =false)]
    private void PlaySoundServerRpc(string state)
    {
     PlaySoundClientRpc(state);  
    }
    [ClientRpc]
    private void PlaySoundClientRpc(string state)
    {
        PlaySoundLocal(state);
    }


    private void PlaySoundLocal(string state) 
    {
        if (state == "open" && openSound!=null && !openSound.isPlaying)
            openSound.Play();
        else
        {
            if (closeSound!=null && !closeSound.isPlaying)
                closeSound.Play();
        }
    }

    
}
