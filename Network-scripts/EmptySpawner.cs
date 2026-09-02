using Unity.Netcode;
using UnityEngine;

public class EmptySpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject emptyPrefab;

    public void SpawnEmpty()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Not server.");
            return;
        }

        NetworkObject obj = Instantiate(
            emptyPrefab,
            Vector3.zero,
            Quaternion.identity);

        obj.Spawn();

        Debug.Log($"Spawned {obj.NetworkObjectId}");
    }

    private void Start()
    {
        SpawnEmpty();
    }
}