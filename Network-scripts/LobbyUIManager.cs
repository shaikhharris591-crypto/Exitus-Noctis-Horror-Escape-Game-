using System;

using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LobbyUIManager : MonoBehaviour
{

    public static LobbyUIManager Instance { get; set; }
    [Header("Main UI")]
    #region mainUI 
    public Button createButton, backButton, joinButton;
    #endregion
    [Header("Lobby UI")]
    #region lobbyUI 
    public Button lobbyCreateButton, lobbyBackButton;
    public TMP_Dropdown maxPlayers;
    public TMP_Dropdown loadSlots;
    public TMP_Dropdown difficulty;
    public TMP_InputField lobbyNameField;
    public Toggle privToggle;
    #endregion

    [Header("lobby scene")]
    #region lobby scene ui
    public Button startGame, cancelButton, copyButton;
    public TMP_Text codeText;
    public GameObject codeContainer;
    public TMP_Dropdown playerSelector;
    #endregion

    public GameObject lobbyPanel;


    #region gameobjs
    public GameObject lobbyObj, mainUI, lobbyScene;
    #endregion

    #region lobby panel
    public Button refreshButton, backTomainUIButton;
    #endregion

    [Header("Settings")]
    #region lobbySettings
    public int max;
    public string mode;
    public string lobbyName;
    public bool priv = false;
    public string slot = "";
    public string SelectedCharacter = "1";

    #endregion
    private bool _isRefreshing;
    private float _lastRefreshTime;
    [SerializeField] private float refreshCooldown = 2f;

    public async void RefreshLobbies()
    {
        // Already refreshing? Ignore.
        if (_isRefreshing)
            return;

        // Cooldown not finished? Ignore.
        if (Time.time - _lastRefreshTime < refreshCooldown)
            return;

        _isRefreshing = true;
        _lastRefreshTime = Time.time;

        try
        {
            await SessionFetcher.Instance.RefreshSessions();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            _isRefreshing = false;
        }
    }
    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //----startup--------------//


        // startGame.interactable = false;
        //if(SessionHandlerBridge.Instance!=null)SessionHandlerBridge.Instance.OnAllPlayersRegistered += () => startGame.interactable = true;

        mode = difficulty.captionText.text;
        string caption = maxPlayers.captionText.text;


        if (int.TryParse(maxPlayers.captionText.text, out int value))
            max = value;

        else
            Debug.LogWarning("Caption is not a valid integer: " + caption);

        joinButton.onClick.AddListener(JoinLobby);

        createButton.onClick.AddListener(GoToLobby);
        backButton.onClick.AddListener(GoToMainMenu);

        //------Lobby UI----------------//
        lobbyNameField.onDeselect.AddListener(SetName);
        lobbyCreateButton.onClick.AddListener(CreateLobby);
        lobbyBackButton.onClick.AddListener(GoToUI);
        maxPlayers.onValueChanged.AddListener(OnMaxPlayersChanged);
        loadSlots.onValueChanged.AddListener(OnSlotsChanged);
        difficulty.onValueChanged.AddListener(OnDifficultyChanged);
        privToggle.onValueChanged.AddListener(OnStatusChanged);

        //----lobby scene ui------------------------------

        cancelButton.onClick.AddListener(OnCancel);
        startGame.onClick.AddListener(() =>
        {
            // Trigger the scene load
            PlayerSpawnManager.Instance.Spawn();

        });


        //---for load complete trigger if u want to spawn after scene has loaded----------------//

        /* if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += (sceneName, loadMode, clientsCompleted, clientsTimedOut) =>
            {
                if (NetworkManager.Singleton.IsServer && sceneName == "main game")
                {
                    PlayerSpawnManager.Instance.Spawn();
                }
            };
        }
        */




        copyButton.onClick.AddListener(OnCopy);
        playerSelector.onValueChanged.AddListener(OnPlayerSelected);

        //-------------------------------

        backTomainUIButton.onClick.AddListener(BackFromPanel);
        refreshButton.onClick.AddListener(RefreshLobbies);



    }
    void Update()
    {
        if (SessionHandler.Instance == null || SessionHandler.Instance.ActiveSession==null) return;

        bool isPriv = SessionHandler.Instance.ActiveSession.IsPrivate;
        bool isHost = SessionHandler.Instance.ActiveSession.IsHost;
        bool allconnected = SessionHandler.Instance.ActiveSession.MaxPlayers == SessionHandler.Instance.ActiveSession.PlayerCount;
        if (isHost )
        {
            if(isPriv)
            if (codeContainer != null) codeContainer.SetActive(true);

            if (allconnected && startGame != null)
                startGame.gameObject.SetActive(true);
        } 
    }
        
     
    public async void OnPlayerSelected(int arg)
    {
        SelectedCharacter = (arg + 1).ToString();
        await SessionHandler.Instance.UpdatePlayerProperties(SelectedCharacter);
    }
    
    public void InvokeLoadGame(float time)
    {
        Invoke(nameof(LoadMainGame), time);
    }
    public void DirectLoadGame()
    {
        LoadMainGame();
    }

    void LoadMainGame() 
    { 
        NetworkManager.Singleton.SceneManager.LoadScene("main game", LoadSceneMode.Single);
    }

   
    private void OnStatusChanged(bool arg0)
    {
        priv = arg0;
    }

    private void OnDifficultyChanged(int arg0)
    {
        mode = difficulty.captionText.text;
    }



   
  
    

    public void DisableStartForClients() 
    {
        ISession session = SessionHandler.Instance.ActiveSession;
        if (session != null && session.IsHost)
        {
            if (session.IsPrivate)
                codeText.text = session.Code;
            else
            {
                if (codeContainer.activeSelf) codeContainer.SetActive(false);
            }
        }
        else
            startGame.gameObject.SetActive(false);
    }

    void CreateLobby() 
    {
        if (string.IsNullOrWhiteSpace(lobbyName)) return;
        SessionHandler.Instance.StartSessionAsHost(lobbyName,max,mode,slot);
        lobbyScene.SetActive(true);
        lobbyObj.SetActive(false);
    }
   async  void JoinLobby() 
    {
        mainUI.SetActive(false);
        lobbyPanel.SetActive(true);
        await SessionFetcher.Instance.RefreshSessions();

    }

    void OnMaxPlayersChanged(int val)
    {
        
        string caption = maxPlayers.captionText.text;
        if (int.TryParse(maxPlayers.captionText.text, out int value))
            max = value;

        else
            Debug.LogWarning("Caption is not a valid integer: " + caption);
    }

    void OnSlotsChanged(int val)
    {
       slot = loadSlots.captionText.text;
    }

    void SetName(string name) 
    {
        lobbyName = name;
    }

    void GoToUI() 
    {
        lobbyObj.SetActive(false);
        lobbyPanel.SetActive(false);
        mainUI.SetActive(true);
       
    }

    void GoToLobby() 
    {
        lobbyObj.SetActive(true);
        mainUI.SetActive(false);
        lobbyPanel.SetActive(false);
    }

    void GoToMainMenu() 
    {

        SceneManager.LoadScene("main menu");
    }


    async void OnCancel() 
    {
        lobbyScene.SetActive(false);
        SessionHandler.Instance.LeaveSession(SessionHandler.Instance.ActiveSession.Id);
        lobbyPanel.SetActive(true);
        await SessionFetcher.Instance.RefreshSessions();
    }

    void OnCopy() 
    {
    GUIUtility.systemCopyBuffer = codeText.text;

    }


    void BackFromPanel() 
    {
        lobbyPanel.SetActive(false);
        mainUI.SetActive(true);
    }

    public void OnJoin(string id) 
    {
        lobbyPanel.SetActive(false);
        lobbyScene.SetActive(true);
        SessionHandler.Instance.JoinSessionById(id);
    }
  
}
