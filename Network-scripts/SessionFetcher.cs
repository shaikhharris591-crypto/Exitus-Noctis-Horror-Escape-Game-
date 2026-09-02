using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using Unity.Services.Core;

public class SessionFetcher : MonoBehaviour
{
    public GameObject lobbyPanel;
    public static SessionFetcher Instance { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private LobbyBox lobbyBoxPrefab;

    // SessionID -> LobbyBox
    private readonly Dictionary<string, LobbyBox> lobbyBoxes = new();
   
    public async Task RefreshSessions()
    {
        if (!AuthenticationService.Instance.IsSignedIn || UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.LogWarning("Player is not signed in yet.");
            return;
        }
        if (!lobbyPanel.activeSelf) return;
        try
        {
            var options = new QuerySessionsOptions();

            var result = await MultiplayerService.Instance.QuerySessionsAsync(options);

            HashSet<string> currentSessions = new();

            foreach (var session in result.Sessions)
            {
                currentSessions.Add(session.Id);

                if (lobbyBoxes.TryGetValue(session.Id, out LobbyBox box))
                {
                    box.Initialize(session);
                }
                else
                {
                    LobbyBox newBox = Instantiate(lobbyBoxPrefab, contentParent);
                    newBox.gameObject.SetActive(true);
                    newBox.Initialize(session);

                    lobbyBoxes.Add(session.Id, newBox);
                }
            }

            // Remove lobbies that no longer exist
            List<string> removeList = new();

            foreach (var pair in lobbyBoxes)
            {
                if (!currentSessions.Contains(pair.Key))
                    removeList.Add(pair.Key);
            }

            foreach (string id in removeList)
            {
                Destroy(lobbyBoxes[id].gameObject);
                lobbyBoxes.Remove(id);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }


  
    private async void Start()
    {

        if (!AuthenticationService.Instance.IsSignedIn || UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.LogWarning("Player is not signed in yet.");
            return;
        }
        while (true)
        {
            await RefreshSessions();
            await Task.Delay(3000);
        }
    }
}

