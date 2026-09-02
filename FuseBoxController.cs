using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FuseBoxController : NetworkBehaviour
{
    // int local_i = 0;
    public bool isFuseInHand = false;
    public GameObject[] fuses;
    public NetworkVariable<int> i = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int limit = 4;
    // Update is called once per frame
    void Update()
    {
        // if(NetworkManager.Singleton==null)
        if (i.Value != limit)
            CheckForFuses();
        else
            RestorePower();

    }

    void CheckForFuses()
    {

        var equipItem = InventoryManager.Instance.GetSelectedItem();
        if (equipItem != null)
            isFuseInHand = equipItem.itemName.Contains("Fuse");
        

        if (isFuseInHand && (Input.GetKeyDown(PlayerMovement.InputKeys.functionKey) ) && fuses[i.Value] != null)
        {





            fuses[i.Value].SetActive(true);
            Debug.Log("Fuse Inserted " + i.Value);

            IncrementFuseIndex(); 

            // local_i++;
            GameManager.Instance.DisableFuseFromPlayerHand();
            isFuseInHand = false;
        }
    }

    public void IncrementFuseIndex() 
    {
    if(NetworkManager.Singleton==null)
            IncrementFuseIndexLocal();
    else
         IncrementFuseIndexServerRpc();
    }

    [ServerRpc(RequireOwnership =false)]
    private void IncrementFuseIndexServerRpc()
    {
        IncrementFuseIndexLocal();
    }
    private void IncrementFuseIndexLocal()
    {
        i.Value++;
    }

    void RestorePower()
    {

        GameManager.Instance.RestorePower();
    }
}
