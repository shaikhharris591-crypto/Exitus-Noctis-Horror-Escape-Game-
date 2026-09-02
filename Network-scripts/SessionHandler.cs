using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;

using Unity.Services.Multiplayer;
using UnityEngine;


public struct PlayerEntry : INetworkSerializable
{
    
    public ulong ClientId;
    public FixedString64Bytes PlayerId;

    // Correct signature for INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerId);
    }


}


public class SessionHandler : MonoBehaviour
{

    
    //public NetworkList<PlayerEntry> Players = new();
    // private bool respawned=false;
    [SerializeField] private SessionHandlerBridge sessionHandlerBridgePrefab;
    private SessionHandlerBridge bridgeInstance;
    public static SessionHandler Instance { get; set; }
    ISession activesession;


    public async Task<bool> GetAllPlayersConnectedAsync()
    {
        await ActiveSession.RefreshAsync(); // make sure data is current
        return ActiveSession.Players.Count == ActiveSession.MaxPlayers;
    }

    public async Task<bool> GetIsPrivateAsync()
    {
        await ActiveSession.RefreshAsync();
        return ActiveSession.IsPrivate;
    }

    public async Task<bool> GetIsHostAsync()
    {
        await ActiveSession.RefreshAsync();
        return ActiveSession.IsHost;
    }

  
    public ISession ActiveSession
    {
        get => activesession;
        set
        {
            activesession = value;
            Debug.Log("active session: " + activesession.Name);
        }
    }
    const string playerNamePropertyKey = "playerName";


    
    private async void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        




        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            Debug.Log("Initialized");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void SpawnBridge()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (SessionHandlerBridge.Instance != null)
            return;

        bridgeInstance = Instantiate(sessionHandlerBridgePrefab);

        bridgeInstance.NetworkObject.Spawn();

        SessionHandlerBridge.Instance = bridgeInstance;

        DontDestroyOnLoad(bridgeInstance.gameObject);

        Debug.Log("SessionHandlerBridge spawned.");
    }
    public void Update()
    {
        
        


    }
    


    
    public void RegisterSessionEvents()
    {
        ActiveSession.Changed += OnSessionChanged;
    }
    public void UnregisterSessionEvents()
    {
        ActiveSession.Changed -= OnSessionChanged;
    }


    public async void KickPlayer(string id)
    {
        if (!ActiveSession.IsHost)
            return;

        await ActiveSession.AsHost().RemovePlayerAsync(id);

    }

    public async void LeaveSession(string id)
    {
        if (ActiveSession == null)
            return;
        try
        {
            await ActiveSession.LeaveAsync();
        }
        catch { }
        finally { ActiveSession = null; }

    }
    public async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties()
    {
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();

        return new Dictionary<string, PlayerProperty>
    {
        { "PlayerName", new PlayerProperty(playerName, VisibilityPropertyOptions.Member) },
        { "Character", new PlayerProperty(LobbyUIManager.Instance.SelectedCharacter, VisibilityPropertyOptions.Member) }
    };
    }
    public async Task UpdatePlayerProperties(string newCharacterId = null, string newPlayerName = null)
    {
        if (ActiveSession == null)
        {
            Debug.LogWarning("No active session to update properties.");
            return;
        }

        var updatedProps = new Dictionary<string, PlayerProperty>();

        if (!string.IsNullOrEmpty(newPlayerName))
            updatedProps["PlayerName"] = new PlayerProperty(newPlayerName, VisibilityPropertyOptions.Member);

        if (!string.IsNullOrEmpty(newCharacterId))
            updatedProps["Character"] = new PlayerProperty(newCharacterId, VisibilityPropertyOptions.Member);

        try
        {
            // Apply properties to the current player
            ActiveSession.CurrentPlayer.SetProperties(updatedProps);

            // Save changes to the backend
            await ActiveSession.SaveCurrentPlayerDataAsync();

            Debug.Log("Player properties updated in session.");
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to update player properties: {e}");
        }
    }
   
    public  async void StartSessionAsHost(string name, int max, string mode, string slot) 
    {
        var playerProperties = await GetPlayerProperties();
        var sessionProperties = new Dictionary<string, SessionProperty>
    {
        
        { "Slot", new SessionProperty(slot) },
        { "Difficulty", new SessionProperty(mode) }
    };
        var options = new SessionOptions
        {
            Name = name,
            MaxPlayers = max,
            IsLocked = false,
            IsPrivate = false,
            PlayerProperties = playerProperties,
            SessionProperties = sessionProperties
            

        }.WithRelayNetwork();
        await VivoxManager.Instance.EnsureLoggedInAsync();
        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        //if (!NetworkObject.IsSpawned) NetworkObject.Spawn();

        

        
        NetworkManager.Singleton.StartHost();
        if(NetworkManager.Singleton.IsHost)
        SpawnBridge();

      



       
        await VivoxManager.Instance.JoinChannelAsync(ActiveSession.Id);
    }
    private void OnServerStarted()
    {
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;

        
    }

    public async void JoinSessionById(string sessionId)
    {
        await VivoxManager.Instance.EnsureLoggedInAsync();
        var options = new JoinSessionOptions
        {
            PlayerProperties = await GetPlayerProperties()
        };
        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId,options);
        Debug.Log($"Session joined successfully: {ActiveSession.Id}");


       
        NetworkManager.Singleton.StartClient();
      //  StartCoroutine(WaitForConnection());

        await VivoxManager.Instance.JoinChannelAsync(ActiveSession.Id);

    }
    public async void JoinSessionByCode(string sessionCode)
    {
        await VivoxManager.Instance.EnsureLoggedInAsync();
        var options = new JoinSessionOptions
        {
            PlayerProperties = await GetPlayerProperties()
        };
        ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode,options);
        
        NetworkManager.Singleton.StartClient();
      

        Debug.Log($"Session joined successfully: {ActiveSession.Id}");
        await VivoxManager.Instance.JoinChannelAsync(ActiveSession.Id);
    }

    void OnSessionChanged() 
    { 
    
    }
  


}
/*public struct PlayerEntry : INetworkSerializable
{
    public ulong ClientId;
    public FixedString64Bytes PlayerId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IBufferSerializer
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerId);
    }
}*/


