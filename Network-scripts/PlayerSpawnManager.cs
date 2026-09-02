using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    public string characterId = "1";
    public float loadTime=5f;
    public static PlayerSpawnManager Instance;

    [System.Serializable]
    public class CharacterPrefab
    {
        public string CharacterId;
        public NetworkObject Prefab;
    }

    [SerializeField] private List<CharacterPrefab> playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private readonly Dictionary<string, NetworkObject> prefabLookup = new();
    private readonly List<ulong> connectedClients = new();

    private int nextSpawn = 0;

    // How long to wait for every session player to register their clientId
    // (via RegisterPlayerRpc) before we give up and spawn whoever we can resolve.
    private const int MaxWaitAttempts = 50; // 50 * 100ms = 5s
    private const int WaitDelayMs = 100;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var p in playerPrefabs)
        {
            prefabLookup[p.CharacterId] = p.Prefab;

            // Ensure prefab is registered with Netcode
           
        }
    }

   
    private void Start()
    {
       
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!connectedClients.Contains(clientId))
        {
            connectedClients.Add(clientId);
            Debug.Log($"Client connected: {clientId}");
        }
    }

    private bool hasSpawned;

    public async void Spawn()
    {
        if (!NetworkManager.Singleton.IsServer || hasSpawned)
            return;
        hasSpawned = true;

        var session = SessionHandler.Instance.ActiveSession;

        // Don't race RegisterPlayerRpc. Wait until every session player has
        // an entry in ClientIdToPlayerId, or until we time out.
        for (int attempt = 0; attempt < MaxWaitAttempts; attempt++)
        {
            bool allResolved = session.Players.All(p =>
                SessionHandlerBridge.Instance.TryResolveClientId(p.Id, out _));

            if (allResolved)
                break;

            await Task.Delay(WaitDelayMs);
        }

        int spawnedCount = 0;

        // Loop through all players in the session
        foreach (var player in session.Players)
        {
            characterId =
                player.Properties.TryGetValue("Character", out var prop)
                    ? prop.Value
                    : "1";

            if (!SessionHandlerBridge.Instance.TryResolveClientId(player.Id, out ulong clientId))
            {
                Debug.LogWarning($"Giving up on player {player.Id}: no clientId registered in time.");
                continue;
            }

            if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId))
            {
                Debug.LogWarning($"Skipping player {player.Id}: clientId {clientId} isn't an active connection.");
                continue;
            }

            if (SpawnPlayer(clientId, characterId))
                spawnedCount++;
        }

        Debug.Log($"Spawn complete: {spawnedCount}/{session.Players.Count} players spawned.");

        // Fire once, after everyone who could spawn has spawned - not per player.
        LobbyUIManager.Instance.InvokeLoadGame(loadTime);
    }

    private bool SpawnPlayer(ulong clientId, string characterId)
    {
        Debug.Log("ENTER SpawnPlayer");

        if (!prefabLookup.TryGetValue(characterId, out NetworkObject prefab))
        {
            Debug.LogWarning($"Unknown character {characterId}, falling back to default.");

            if (playerPrefabs.Count == 0)
            {
                Debug.LogError("No player prefabs configured; cannot spawn.");
                return false;
            }

            prefab = playerPrefabs[0].Prefab;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points configured; cannot spawn.");
            return false;
        }

        Transform spawn = spawnPoints[nextSpawn % spawnPoints.Length];
        nextSpawn++;

      

        NetworkObject player = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);
        player.SpawnWithOwnership(clientId);

        Debug.Log($"Spawned character {characterId} for client {clientId}");
        return true;
    }
}