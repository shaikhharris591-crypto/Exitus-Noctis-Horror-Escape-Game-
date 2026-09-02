using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool local = NetworkManager.Singleton == null;


        NetworkObject netObj = GetComponent<NetworkObject>();
        if (local) { Destroy(netObj); return; }
        else if (!NetworkManager.Singleton.IsServer)
            return;
       
       
        if (netObj != null && !netObj.IsSpawned)
            netObj.Spawn();
    }

   
}
