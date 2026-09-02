
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;

public class CodeManager : NetworkBehaviour
{
    public AudioSource codeActivationSound;
    public AudioSource correctCodeSound;
    public AudioSource wrongCodeSound;


    public TMP_Text codeTextOffCanvas;
    public GameObject codeCanvas;
    public NetworkVariable<bool> canCodeNet = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    //  public static bool canCode=false;
    public string correctCode;
    DoorControl targetDoor;
    public NetworkVariable<bool> isCodeActive = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> DoorUnlocked = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public static bool evaluateCode = false;

    public bool isSelected = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public static CodeManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        targetDoor = GameManager.Instance.targetDoor;
        if (enabled)
            enabled = false;

    }
    public void EnableDisplayOnPower() 
    {
        if (NetworkManager.Singleton == null)
            EnableDisplayOnPowerLocal();
        else
            EnableDisplayOnPowerServerRpc();
    }
     void EnableDisplayOnPowerLocal()
    {
        codeTextOffCanvas.gameObject.SetActive(true);
        if(codeActivationSound!=null && !codeActivationSound.isPlaying)codeActivationSound.Play();
    }

    [ServerRpc]
    void EnableDisplayOnPowerServerRpc()
    {
        EnableDisplayOnPowerClientRpc();
    }
    [ClientRpc]
    void EnableDisplayOnPowerClientRpc()
    {
        EnableDisplayOnPowerLocal();
    }
    // Update is called once per frame
    void Update()
    {

        if (DoorUnlocked.Value) { enabled = false; return; }

        if (!canCodeNet.Value || DoorUnlocked.Value) return;
        if (isSelected && !isCodeActive.Value) enabled = false;

        ReportCodeActive();

        if (Input.GetKeyDown(PlayerMovement.InputKeys.interactKey) && !isCodeActive.Value)
        {
            DisplayCodeCanvas(true);

        }

        if (evaluateCode)
        {
            EvaluateCode();
        }



    }
    public void ReportCodeActive() 
    {
        if (NetworkManager.Singleton == null)
            ReportCodeActiveLocal();
        else
            ReportCodeActiveServerRpc();
    }

    void ReportCodeActiveLocal() 
    {
        isCodeActive.Value = codeCanvas.activeSelf;
    }


    [ServerRpc(RequireOwnership =false)]
    void ReportCodeActiveServerRpc()
    {
        isCodeActive.Value = codeCanvas.activeSelf;
    }
    private void DisplayCodeCanvas(bool b)
    {
        codeCanvas.SetActive(b);
        MouseScript.Instance.DisableMouse(b);
    }



    public void UnlockDoor()
    {

        if(NetworkManager.Singleton==null)
            UnlockDoorLocal();
        else
            UnlockDoorServerRpc();
       

    }
    [ServerRpc]
    void UnlockDoorServerRpc()
    {
        UnlockDoorLocal();
    }

    void UnlockDoorLocal()
    {
        DoorUnlocked.Value = true;
       
        targetDoor.UnlockDoor(true);


        SaveData();

        if (correctCodeSound!=null && !correctCodeSound.isPlaying)correctCodeSound.Play();
        enabled = false;


    }

    void SaveData()
    {
        if (NetworkManager.Singleton != null && !IsServer) return;
        if (SaveManager.Instance == null) return;

        SaveManager.Instance.UpdateAndSaveProgress(doorUnlocked: true,playTime:GameManager.Instance.GetTotalPlayTime());
    }
    public void EvaluateCode()
    {

        if (NetworkManager.Singleton == null)
            EvaluteCodeLocal();
        else
            EvaluteCodeServerRpc();
    }

    void EvaluteCodeLocal() 
    {
        if (CodeUI.Instance == null) return;
        string code = CodeUI.Instance.GetCode();
        if (isCodeActive.Value && Input.GetKeyDown(KeyCode.Return))
        {
            if (code == correctCode && !string.IsNullOrWhiteSpace(code))
            {
                UnlockDoor();
            }
        }
    }
    [ServerRpc]
    void EvaluteCodeServerRpc()
    {
        string code = CodeUI.Instance.GetCode();
        if (isCodeActive.Value && Input.GetKeyDown(KeyCode.Return))
        {
            if (code == correctCode && !string.IsNullOrWhiteSpace(code))
            {
                UnlockDoor();
            }
            else
                PlayWrongCodeSound();
        }
    }

    private void PlayWrongCodeSound()
    {
        if (NetworkManager.Singleton == null)
            PlayWrongCodeSoundLocal();
        else
            PlayWrongCodeSoundServerRpc();
    }
    [ServerRpc]
    private void PlayWrongCodeSoundServerRpc()
    {
        PlayWrongCodeSoundClientRpc();
    }
    [ClientRpc]
    private void PlayWrongCodeSoundClientRpc()
    {
        PlayWrongCodeSoundLocal();
    }

    private void PlayWrongCodeSoundLocal()
    {
        if (wrongCodeSound == null || wrongCodeSound.isPlaying) return;
        wrongCodeSound.Play();
    }

    void PlayCodeButtonClickSoundLocal() 
    {
        if(codeActivationSound != null && !codeActivationSound.isPlaying)
               codeActivationSound.Play();
    }
    [ServerRpc]
    void PlayCodeButtonClickSoundServerRpc()
    {
        PlayCodeButtonClickSoundClientRpc();
    }
    [ClientRpc]
    void PlayCodeButtonClickSoundClientRpc()
    {
       PlayCodeButtonClickSoundLocal();
    }


    public void PlayCodeButtonClickSound() 
    {
        if (NetworkManager.Singleton == null) PlayCodeButtonClickSoundLocal();
        else  PlayCodeButtonClickSoundServerRpc();
    }
}
