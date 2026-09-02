using System;
using System.Collections.Generic;
using System.IO;

using Unity.Netcode;

using UnityEngine;
using static SaveManager;

public class GameManager : NetworkBehaviour
{
    public float sessionPlayTime;
    public float totalPlayTime;
    #region escape logic
    [Header("Escape Logic")]
    public int keyLimit = 30;
    public NetworkVariable<int> KeysCount = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> lockIndex = new(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public GameObject targetEscapeDoor;
    public List<Rigidbody> targetMainDoorLocks;
    #endregion


    #region monster spawn
    [Header("Monster Spawn Logic")]
    public bool testMode = false;
    public List<GameObject> monsters;
    public GameObject monster;
    public NetworkVariable<bool> isPlayerOnTopFloor = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public GameObject mannequinTrap;

    public string topFloorTag;

    public LayerMask enterTriggerMask;
    public LayerMask buildingEnterTriggerMask;
    #endregion




    public DoorControl targetDoor;

    #region saveLoadLogic
    [Header("save & load logic")]
    public bool isDoorUnlocked = false;
    public bool isPowerRestored = false;
    public string settingsSavePath;
    public string gameSavePath;
    #endregion



    #region gameOverLogic
    [Header("Game Over Logic")]
    public GameObject ambience;
    public AudioSource gameOverSound;
    public float gameOverTime;
    public GameObject gameOverScreen;
    public bool isGameOver = false;
    bool canRestart = false;
    public NetworkList<ulong> DeadPlayers = new(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    #endregion

    #region spawn logic items
    [Header("Item Spawning Logic")]
    bool isNetworked = false;
    public int slot = 1;
    public List<GameObject> ItemList;
    public List<Transform> spawnList;
    public List<GameObject> fusePrefabs;
    public List<GameObject> trackOfFuses;
    public List<Transform> trackOfSpawns;
    public GameObject currPlayer;
    bool fuseSpawned = false;
    #endregion

    #region spawnAI
    public NetworkVariable<bool> playerEntered = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> playerEnteredBuilding= new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool playerEnteredBuildingLocal=false;
    public GameObject mainDoorTrigger, playerTrigger;
    #endregion


    #region powerRestoratin Logic
    public GameObject buildingLights;
    #endregion

    public static GameManager Instance;

    private void Awake()
    {

        Instance = this;

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (!testMode && monster != null && !monster.activeSelf) monster.SetActive(true);
        if (slot == 0) slot = 1;
        isNetworked = NetworkManager.Singleton != null;



        gameSavePath = Application.persistentDataPath + "/" + slot + ".json";
        settingsSavePath = Application.persistentDataPath + "/ui_settings" + ".json";

        targetDoor.UnlockDoor(false);

        var targetEscapeDoorScript = targetEscapeDoor.GetComponent<DoorControl>();

        if (targetEscapeDoor != null && targetEscapeDoorScript != null)
            targetEscapeDoorScript.UnlockDoor(false);


        //load data after settings values up


        LoadData();
        if (gameOverTime == 0) gameOverTime = 4f;

     




        if (!isNetworked)
        {
            if (!currPlayer.activeSelf)
                currPlayer.SetActive(true);
        }
        else
        {
            if (currPlayer.activeSelf)
                currPlayer.SetActive(false);
            // PlayerMovement.Instance.rb.isKinematic = false;
        }

        SpawnItems();
        SpawnRandomFuseItems();




    }

    private void LoadData()
    {
        GameProgress progress = SaveManager.Instance.LoadProgress();
        if (progress == null)
            return; // no save file yet, nothing to load

        isDoorUnlocked = progress.doorUnlocked;
        if (isDoorUnlocked) targetDoor.UnlockDoor(true);

        isPowerRestored = progress.powerRestored;
        if (isPowerRestored) RestorePower();
    }

    private void UnlockEscapeDoor()
    {
        var escapeDoorScript = targetEscapeDoor.GetComponent<DoorControl>();
        
       escapeDoorScript.UnlockDoor(true);
    }

    private void SpawnMonsters()
    {
        if (testMode) return;
        if (monstersSpawned) return;
        foreach (var monster in monsters)
        {
            if (!monster.activeSelf) monster.SetActive(true);
        }
        monstersSpawned = true;
    }
    bool monstersSpawned = false;


    public float GetTotalPlayTime()
    {
        return totalPlayTime + sessionPlayTime;
    }
    // Update is called once per frame
    void Update()
    {
        sessionPlayTime += Time.deltaTime;


        if (CodeManager.Instance.canCodeNet.Value)
            buildingLights.SetActive(true);


        if (KeysCount.Value == keyLimit)
            UnlockEscapeDoor();

        SpawnMannequinTrap();
        if (testMode)
            Debug.Log("test Mode ");

        if (playerEntered.Value)
        {
            playerTrigger.SetActive(false);
            SpawnMonsters();
            CloseMainGate();
        }
        if (SessionHandler.Instance!=null && playerEnteredBuilding.Value==SessionHandler.Instance.ActiveSession.PlayerCount || playerEnteredBuildingLocal)
        { 
            mainDoorTrigger.SetActive(false); 
            targetDoor.UnlockDoor(false); 
        }

        isGameOver = gameOverScreen.activeSelf;
        if (!isNetworked || DeadPlayers.Count == SessionHandler.Instance.ActiveSession.PlayerCount)
            canRestart = true;

        CheckForParentingOfFuses();
    }

    private void SpawnMannequinTrap()
    {
        mannequinTrap.SetActive(isPlayerOnTopFloor.Value);
    }

    void CheckForParentingOfFuses()
    {
        if (fuseSpawned) return;

        if (!isNetworked)
        {
            for (int i = 0; i < trackOfFuses.Count; i++)
            {
                GameObject fuse = trackOfFuses[i];
                Transform spawn = trackOfSpawns[i];

                // Only parent if the NetworkObject is gone
                if (fuse.GetComponent<NetworkObject>() == null)
                {
                    fuse.transform.SetParent(spawn, true); // keep world position/scale
                }

                // Optional: if you want to clear spawn usage
                // trackOfSpawns.RemoveAt(i);
            }

            fuseSpawned = true;
        }
    }


    public void SpawnItems()
    {
        int roomCount = 1;
        for (int i = 0; i < spawnList.Count; i++)
        {
            if (i >= ItemList.Count)
                break; // stop if no more items

            if (ItemList[i] != null && spawnList[i] != null)
            {
                var g = Instantiate(ItemList[i], spawnList[i].position, ItemList[i].transform.rotation, spawnList[i]);
                g.SetActive(true);
                if (ItemList[i].name.ToLower().Contains("roomkey"))
                {
                    SwitchLayer("room" + roomCount, g);
                    roomCount++;
                }

            }
        }

    }


    public List<Transform> fuseSpawnPoints;

    public void SpawnRandomFuseItems()
    {
        List<Transform> availableSpawns = new List<Transform>(fuseSpawnPoints);
        foreach (GameObject fuse in fusePrefabs)
        {
            if (availableSpawns.Count == 0)
                break;

            int randomIndex = UnityEngine.Random.Range(0, availableSpawns.Count);

            Transform spawn = availableSpawns[randomIndex];
            Quaternion originalRot = fuse.transform.localRotation;
            GameObject item = Instantiate(fuse, spawn.position, spawn.rotation);
            if (!isNetworked) { NetworkObject net = item.GetComponent<NetworkObject>(); if (net != null) Destroy(net); }
            trackOfFuses.Add(item);
            trackOfSpawns.Add(spawn);

            if (!item.activeSelf) item.SetActive(true);

            availableSpawns.RemoveAt(randomIndex);
        }


    }

    public void OnGameOver()
    {
        if (ambience != null) ambience.SetActive(false);
        if (gameOverSound != null) gameOverSound.Play();

        if (canRestart)
            Invoke(nameof(EnableGameOverScreen), gameOverTime);
        else
            PlayerMovement.Instance.Spectate();
        if (isNetworked)
        {
            ReportDeath();
        }
        MouseScript.Instance.DisableMouse(true);
        PlayerMovement.Instance.OnPlayerKilled();


    }
    [ServerRpc(RequireOwnership = false)]
    public void ReportDeathServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (!DeadPlayers.Contains(clientId))
        {
            DeadPlayers.Add(clientId);
        }
    }


    public void ReportDeathLocal()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        if (!DeadPlayers.Contains(clientId))
        {
            DeadPlayers.Add(clientId);
            // Optionally broadcast to clients if you want UI updates
        }
    }

    public void ReportDeath() 
    {
        if (NetworkManager.Singleton == null)
            ReportDeathLocal();
        else
            ReportDeathServerRpc();
    }


    public void EnableGameOverScreen()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);
    }
    public void DisableGameOverScreen()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
    }

    public void RestorePower() 
    {
    if(NetworkManager.Singleton==null)
            RestorePowerLocal();
    else
            RestorePowerServerRpc();


    }
    [ServerRpc(RequireOwnership =false)]
    private void RestorePowerServerRpc()
    {
        if (IsServer)
        {
            
            SaveManager.Instance.UpdateAndSaveProgress(powerRestored: true, playTime: GetTotalPlayTime());
        }
        CodeManager.Instance.EnableDisplayOnPower();
        CodeManager.Instance.canCodeNet.Value = true;
      
        buildingLights.SetActive(true);

    }
    private void RestorePowerLocal()
    {
        CodeManager.Instance.EnableDisplayOnPower();
        CodeManager.Instance.canCodeNet.Value = true;

        buildingLights.SetActive(true);

        SaveManager.Instance.UpdateAndSaveProgress(powerRestored: true, playTime: GetTotalPlayTime());

    }
  
    void SetParent(GameObject g, Transform newParent)
    {
        var netObj = g.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned && NetworkManager.Singleton != null)
        {
            // Safe network re-parent
            netObj.TrySetParent(newParent, true);
        }
        else
        {

            // Local-only re-parent
            g.transform.SetParent(newParent, true);
        }



    }

   

    public void DisableFuseFromPlayerHand()
    {
        var equippedFuse = InventoryManager.Instance.GetSelectedGameObject();
        if (equippedFuse == null)
            return;

        ItemPickup itemPickup = equippedFuse.GetComponent<ItemPickup>();



        if (itemPickup != null)
        {


            itemPickup.Drop();
            
            equippedFuse = null;


        }



    }

    public void SwitchLayer(string newLayerName, GameObject g)
    {
        int layerIndex = LayerMask.NameToLayer(newLayerName);

        if (layerIndex == -1)
        {
            Debug.LogError("Layer '" + newLayerName + "' does not exist!");
            return;
        }

        g.layer = layerIndex;
    }



    [ServerRpc(RequireOwnership =false)]
    private void OnKeyCollectedServerRpc()
    {
        KeysCount.Value++;
        targetMainDoorLocks[lockIndex.Value].isKinematic = false;
        targetMainDoorLocks.RemoveAt(lockIndex.Value);
        lockIndex.Value++;
    }

   
    private void OnKeyCollectedLocal()
    {
        KeysCount.Value++;
        targetMainDoorLocks[lockIndex.Value].isKinematic = false;
        targetMainDoorLocks.RemoveAt(lockIndex.Value);
        lockIndex.Value++;
    }

    public void OnKeyCollected() 
    {

        if (NetworkManager.Singleton == null)
            OnKeyCollectedLocal();
        else
            OnKeyCollectedServerRpc();
    }

   public void CloseMainGate() 
    {
        OpenGateForPlayer.Instance.CloseDoor();
    }


}
