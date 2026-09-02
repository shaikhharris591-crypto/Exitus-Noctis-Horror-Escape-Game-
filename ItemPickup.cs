using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{

    public bool useOnceOnlyItem = false;
    public bool isFuse = false;
    IItemFunctionality itemScript;

    public bool isTorch = false;

    public bool dropped = false;

    public AudioSource dropSound;
    public Rigidbody rb;
    public BoxCollider coll;
    public Transform player, container, containerPos, fpsCam;

    public float dropForwardForce, dropUpwardForce;

    public NetworkVariable<bool> equipped = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkObject netObj;
    public static bool hasTorch = true;
    //  public bool isKey=false;
    void Awake()
    {
        netObj = GetComponent<NetworkObject>();
    }
    private void Start()
    {


        if (equipped.Value)
        {
            rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;
            coll = GetComponent<BoxCollider>();
            if (coll != null)
                coll.isTrigger = true;


            if (isTorch && PlayerMovement.Instance != null) PlayerMovement.Instance.hasTorch = true;
        }
        else
            enabled = false;
    }
    bool assigned = false;
    private void Update()
    {
        if (PlayerMovement.Instance != null && isTorch && !PlayerMovement.Instance.hasTorch && equipped.Value)
            PlayerMovement.Instance.hasTorch = true;

        if (PlayerMovement.Instance != null && !assigned)
        {

            itemScript = GetComponent<IItemFunctionality>();
            if (rb == null) rb = GetComponent<Rigidbody>();
            coll = GetComponent<BoxCollider>();
            player = PlayerMovement.Instance.gameObject.transform;
            fpsCam = PlayerMovement.Instance.cameraTransform;
            container = (isTorch) ? PlayerMovement.Instance.handTransform : PlayerMovement.Instance.itemHandTransform;
            assigned = true;

        }
        //Check if player is in range and "E" is pressed
        
        if (Input.GetKeyDown(PlayerMovement.InputKeys.interactKey)) PickUp();

        //Drop if equipped and "dropkey" is pressed
        if (equipped.Value && Input.GetKeyDown(PlayerMovement.InputKeys.dropKey)) Drop();


    }
    [ServerRpc]
    private void PickUpServerRpc()
    {
        if (equipped.Value) return;
        InventoryManager.Instance.AddItem(gameObject);

        equipped.Value = true;


        if (itemScript != null)
            itemScript.Activate();


        if (isTorch)
            PlayerMovement.Instance.hasTorch = true;



        //Make weapon a child of the camera and move it to default position
        SetParent(container);
        transform.localPosition = PlayerMovement.Instance.containerPos;
        transform.localRotation = Quaternion.Euler(PlayerMovement.Instance.containerRot);

        //Make Rigidbody kinematic and BoxCollider a trigger
        rb.isKinematic = true;
        coll.isTrigger = true;


    }
    private void PickUpLocal()
    {
        if (equipped.Value) return;
        InventoryManager.Instance.AddItem(gameObject);

        equipped.Value = true;


        if (itemScript != null)
            itemScript.Activate();


        if (isTorch)
            PlayerMovement.Instance.hasTorch = true;



        //Make weapon a child of the camera and move it to default position
        SetParent(container);
        transform.localPosition = PlayerMovement.Instance.containerPos;
        transform.localRotation = Quaternion.Euler(PlayerMovement.Instance.containerRot);

        //Make Rigidbody kinematic and BoxCollider a trigger
        rb.isKinematic = true;
        coll.isTrigger = true;


    }



    public void PickUp() 
    {

        if (NetworkManager.Singleton == null)
            PickUpLocal();
        else
            PickUpServerRpc();
    }





    void SetParent(Transform newParent)
    {
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && NetworkManager.Singleton != null)
        {
            // Safe network re-parent
            netObj.TrySetParent(newParent, true);
        }
        else
        {

            // Local-only re-parent
            transform.SetParent(newParent, true);
        }
    }
    [ServerRpc(RequireOwnership =false)]
    private void DropServerRpc()
    {
        if (useOnceOnlyItem)
        {
            tag = "useless";
            if (isFuse)
            {
                gameObject.SetActive(false);
                
            }
        }


        InventoryManager.Instance.RemoveItem(gameObject);
        if (itemScript != null)
            itemScript.Deactivate();


        if (isTorch)
            PlayerMovement.Instance.hasTorch = false;


        // if(isTorch)hasTorch = false;
        equipped.Value = false;


        //Set parent to null
        SetParent(null);

        //Make Rigidbody not kinematic and BoxCollider normal
        rb.isKinematic = false;
        coll.isTrigger = false;

        //Gun carries momentum of player
        rb.linearVelocity = player.GetComponent<Rigidbody>().linearVelocity;

        //AddForce
        rb.AddForce(fpsCam.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(fpsCam.up * dropUpwardForce, ForceMode.Impulse);
        //Add random rotation
        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random) * 10);

        if (NetworkManager.Singleton != null)
            PlayCurrentSoundServerRpc();
        else
            PlayDropSound();

        enabled = false;


    }

    private void DropLocal()
    {
        if (useOnceOnlyItem)
        {
            tag = "useless";
            if (isFuse)
            {
                gameObject.SetActive(false);
               
            }
          //  GameManager.Instance.SwitchLayer("Default", gameObject);
        }

        if(InventoryManager.Instance.GetSelectedGameObject()==gameObject)InventoryManager.Instance.RemoveItem(gameObject);
        if (itemScript != null)
            itemScript.Deactivate();


        if (isTorch)
            PlayerMovement.Instance.hasTorch = false;


        // if(isTorch)hasTorch = false;
        equipped.Value = false;


        //Set parent to null
        SetParent(null);

        //Make Rigidbody not kinematic and BoxCollider normal
        rb.isKinematic = false;
        coll.isTrigger = false;

        //Gun carries momentum of player
        rb.linearVelocity = player.GetComponent<Rigidbody>().linearVelocity;

        //AddForce
        rb.AddForce(fpsCam.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(fpsCam.up * dropUpwardForce, ForceMode.Impulse);
        //Add random rotation
        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random) * 10);

        if (NetworkManager.Singleton != null)
            PlayCurrentSoundServerRpc();
        else
            PlayDropSound();

        enabled = false;


    }
    public void Drop() 
    {
        if (NetworkManager.Singleton == null) DropLocal();
        else DropServerRpc();
    }


    [ServerRpc]
    private void PlayCurrentSoundServerRpc()
    {
        PlayCurrentSoundClientRpc();
    }
    [ClientRpc]
    private void PlayCurrentSoundClientRpc()
    {
        PlayDropSound();
    }

    private void PlayDropSound()
    {
        if (!dropped) return;
        dropSound.Play();
        dropped = false;
    }



    private void OnCollisionEnter(Collision collision)
    {
        dropped = true;
    }
}
