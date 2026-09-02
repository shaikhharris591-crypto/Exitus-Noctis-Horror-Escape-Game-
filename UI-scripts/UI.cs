using System;
using TMPro;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GraphicsSettingsManager;

public class UI : NetworkBehaviour
{
    

    public bool canPause=true;

    string savePath;

    public GameObject inventoryCanvas;
    #region pause
    
    public Slider sensSlider;
    public GameObject pauseMenu;
    
    public bool paused = false;
    public static bool st_paused = false;
    public Button resume, mainmenu, graphics, controls, volume, lobbyButton, restartButton;

    #endregion
    #region GameOverUI
    public Button restartFromGameOverButton;
    public Button quitFromGameOverButton;
    #endregion
    #region volume
    [Header("---Volume---")]
    public GameObject volCanvas;
    public AudioMixer audioMixer;
    public Slider entitySlider, sfxSlider, musicSlider, masterSlider;
    #endregion
    #region graphicsMenu
    [Header("---Graphics---")]
    public GameObject graphicsCanvas;
    public TMP_Dropdown fpsDropDown;
    public TMP_Dropdown qualityDropDown;
    public Toggle fpsToggle;
    public TMP_Dropdown rayTracingDropDown;
    public TMP_Dropdown dlssDropDown;
    public Toggle pathTracingToggle;
    public Toggle shadowstoggle;
    public Toggle vsyncToggle;
    #endregion
    #region controls
    [Header("---Controls---")]
    public GameObject contMenu;
    public Text contText;
   
    #endregion
    #region lobbyMenu
    [Header("---Lobby---")] 
    public GameObject lobbyCanv;
    #endregion
    #region backbuttons
    [Header("BackButtons")]
    public Button volBackButton;
    public Button lobbyBack;
    public Button graphicsBackButton;
    public Button backFromControlMenu;
    #endregion


    public static UI Instance;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        #region setup


        Setup();
      
        savePath = GameManager.Instance.settingsSavePath;

        ApplyLoadedSettings();

        if (NetworkManager.Singleton == null)
            lobbyButton.gameObject.SetActive(false);

        if (NetworkManager.Singleton != null && !IsServer)
            restartButton.gameObject.SetActive(false);
        #endregion
        #region main pause menu

        resume.onClick.AddListener(Play);
        controls.onClick.AddListener(ViewControls);

        if (lobbyButton.gameObject.activeSelf)
        {
            var volScript = volume.GetComponent<ButtonHover>();
            if (volScript!=null) volScript.down = lobbyButton;
            var restartScript = restartButton.GetComponent<ButtonHover>();
            if(restartScript!=null)restartScript.up = lobbyButton;
        }
        volume.onClick.AddListener(OnVolumeButton);
        mainmenu.onClick.AddListener(QuitGame);
        lobbyButton.onClick.AddListener(LobbySettings);
        restartButton.onClick.AddListener(RestartGame);
        lobbyBack.onClick.AddListener(OnLobbySettingsBack);
        graphics.onClick.AddListener(OnGraphicsButton);
        restartFromGameOverButton.onClick.AddListener(RestartGame);
        quitFromGameOverButton.onClick.AddListener(QuitGame);

        #endregion

        #region volume
        sensSlider.onValueChanged.AddListener(OnSensitivityChanged);
        entitySlider.onValueChanged.AddListener(OnEntityVolChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolChanged);
        masterSlider.onValueChanged.AddListener(OnMasterVolChanged);
        volBackButton.onClick.AddListener(OnVolumeButtonBack);
        #endregion
      
        #region controls
        backFromControlMenu.onClick.AddListener(OnControlsButtonBack);
        #endregion


        #region graphics
        
        fpsDropDown.onValueChanged.AddListener(OnFrameRateChanged);
        qualityDropDown.onValueChanged.AddListener(OnQualityChanged);
        graphicsBackButton.onClick.AddListener(OnGraphicsBack);
        fpsToggle.onValueChanged.AddListener(ToggleFPSShow);
        pathTracingToggle.onValueChanged.AddListener(EnablePathTracing);
        rayTracingDropDown.onValueChanged.AddListener(EnableRayTracing);
        dlssDropDown.onValueChanged.AddListener(EnableDLSS);
        shadowstoggle.onValueChanged.AddListener(SetShadows);
        vsyncToggle.onValueChanged.AddListener(SetVSync);


        #endregion


    }


    public void Setup() 
    {
        if (GraphicsSettingsManager.Instance == null) return;
        if (GraphicsSettingsManager.Instance.CurrentQuality != null)
        {
            int qindex = (int)GraphicsSettingsManager.Instance.CurrentQuality;
            qualityDropDown.value = qindex;
        }

            //fps
            int i = GraphicsSettingsManager.Instance.GetFrameRateDropdownIndex();
            if (i != -1)
            {
                fpsDropDown.value = i;
                fpsDropDown.RefreshShownValue();
            }
            //--------


            shadowstoggle.isOn = GraphicsSettingsManager.Instance.shadowsEnabled;
            pathTracingToggle.isOn = GraphicsSettingsManager.Instance.pathTracingEnabled;


            if (GraphicsSettingsManager.Instance.currentRayTraceSetting != null)
            {
                rayTracingDropDown.value = (int)GraphicsSettingsManager.Instance.currentRayTraceSetting;
                rayTracingDropDown.RefreshShownValue();
            }


            if (GraphicsSettingsManager.Instance.currentDlssSetting != null)
            {
                dlssDropDown.value = (int)GraphicsSettingsManager.Instance.currentDlssSetting;
                dlssDropDown.RefreshShownValue();
            }

        

    }

    private void SetVSync(bool arg0)
    {
        GraphicsSettingsManager.Instance.SetVSync(arg0);
    }

    #region graphicsSettingsFunctions
    void OnQualityChanged(int arg0) 
    {
        if(arg0 == 0)
            GraphicsSettingsManager.Instance.SetLowGraphics();

        if (arg0 == 1)
            GraphicsSettingsManager.Instance.SetMediumGraphics();
        if (arg0 == 2)
            GraphicsSettingsManager.Instance.SetHighGraphics();

    }

    private void OnGraphicsButton()
    {
        graphicsCanvas.SetActive(true);
        DisplayMenu(false);
       
        canPause = false;
    }

    private void OnGraphicsBack() 
    {
        graphicsCanvas.SetActive(false);
        DisplayMenu(true);
 
        canPause = true;

    }

   


    public void OnFrameRateChanged(int index)
    {
       if(GraphicsSettingsManager.Instance!=null)
       GraphicsSettingsManager.Instance.OnFrameRateChanged(index);
    }

    public void EnablePathTracing(bool arg) 
    {
        GraphicsSettingsManager.Instance.EnablePathTracing(arg);
    }

    public void EnableRayTracing(int arg) 
    {
        GraphicsSettingsManager.Instance.SetRayTracingQuality((RayTracingQuality)arg);
    }


    public void EnableDLSS (int arg)
    {
        GraphicsSettingsManager.Instance.EnableDLSS((DLSSSetQuality)arg);
    }


    public void SetShadows(bool arg) 
    { 
    GraphicsSettingsManager.Instance.SetShadows(arg);
    }
    #endregion

  

    private void Update()
    {
        //--this is incorrect anyways 1st time pause 2nd time nothing happens 
        //canPause = (pauseMenu.activeSelf && paused) || (!pauseMenu.activeSelf && !paused);  

        if (Input.GetKeyDown(PlayerMovement.InputKeys.pauseKey) && !GameManager.Instance.isGameOver)
            HandlePause();
      //  if (Input.GetKeyDown(PlayerMovement.ControlKey))
        //    ViewControls();
    }
    
    #region pauseLogic
    public void HandlePause()
    {
        if (!canPause) return;
        if (!paused)
            Pause();
        else
            Play();
        paused = !paused;
        st_paused = paused;
        inventoryCanvas.SetActive(!paused);
    }

    public void Pause()
    {
        DisplayMenu(true);

        if (NetworkManager.Singleton == null)
            Time.timeScale = 0;

        MouseScript.Instance.DisableMouse(true);
    }

    public void Play()
    {
        DisplayMenu(false);

        if (NetworkManager.Singleton == null)
            Time.timeScale = 1;

        MouseScript.Instance.DisableMouse(false);
    }

    void DisplayMenu(bool b)
    {  if (pauseMenu == null) return;
        pauseMenu.SetActive(b);
    }

    #endregion

    #region viewControls
    private void OnControlsButtonBack()
    {
        contMenu.SetActive(false);
        DisplayMenu(true);
        canPause = true;
    }

    
    public void ViewControls()
    {
        contMenu.SetActive(true);
        DisplayMenu(false);

        contText.text = "\nSprint        -   " + PlayerMovement.InputKeys.runKey.ToString() +
                        "\nJump          -   " + PlayerMovement.InputKeys.jumpKey.ToString() +
                        "\nCrouch        -   " + PlayerMovement.InputKeys.crouchKey.ToString() +
                        "\nInteraction   -   " + PlayerMovement.InputKeys.interactKey.ToString() +
                        "\nUse Item      -   " + PlayerMovement.InputKeys.functionKey.ToString() +
                        "\nDrop Item     -   " + PlayerMovement.InputKeys.dropKey.ToString();

        canPause = false;
    }
    #endregion

    #region volume menu

    void OnVolumeButton()
    {
        volCanvas.SetActive(true);
        DisplayMenu(false);
        canPause = false;
    }
    void OnVolumeButtonBack()
    {
        volCanvas.SetActive(false);
        DisplayMenu(true);
        canPause = true;
    }
    #endregion

    public void QuitGame()
    {
        Time.timeScale = 1;
        bool islocal = NetworkManager.Singleton == null;
        if (islocal)
            SceneManager.LoadScene("main menu");
        else
        {
            NetworkManager.SceneManager.LoadScene("main menu", LoadSceneMode.Single);
            SessionHandler.Instance.LeaveSession(SessionHandler.Instance.ActiveSession.Id);
        }
    }

    #region settingssave_load


    public void ApplyLoadedSettings()
    {
        if (SaveManager.Instance == null) return; 
        SaveManager.UISettings settings = SaveManager.Instance.LoadUI();

        if (settings == null)
            return;

        masterSlider.value = settings.master;
        musicSlider.value = settings.music;
        sfxSlider.value = settings.sfx;
        entitySlider.value = settings.entity;
        sensSlider.value = settings.sens;

        // Apply to AudioMixer
        audioMixer.SetFloat("Master", Mathf.Log10(settings.master) * 20);
        audioMixer.SetFloat("music", Mathf.Log10(settings.music) * 20);
        audioMixer.SetFloat("sfx", Mathf.Log10(settings.sfx) * 20);
        audioMixer.SetFloat("entity", Mathf.Log10(settings.entity) * 20);

        // Slider value is saved separately from actual mouse sensitivity
        MouseScript.stSens = settings.sens + 200;
        MouseScript.settingsApplied = true;
    }
    #endregion


    #region volume & sensitivity adjustment

    // --- Sensitivity handler ---
    private void OnSensitivityChanged(float value)
    {
        MouseScript.settingsApplied = true;
        MouseScript.stSens = value + 200;

        SaveManager.Instance.SaveUI(CollectSettings());
    }


    private void OnSFXVolChanged(float value)
    {
        audioMixer.SetFloat("sfx", Mathf.Log10(value) * 20);

        SaveManager.Instance.SaveUI(CollectSettings());
    }

    private void OnMusicVolChanged(float value)
    {
        audioMixer.SetFloat("music", Mathf.Log10(value) * 20);

        SaveManager.Instance.SaveUI(CollectSettings());
    }

    private void OnEntityVolChanged(float value)
    {
        audioMixer.SetFloat("entity", Mathf.Log10(value) * 20);

        SaveManager.Instance.SaveUI(CollectSettings());
    }



    private void OnMasterVolChanged(float value)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(value) * 20);

        musicSlider.onValueChanged.RemoveListener(OnMusicVolChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXVolChanged);
        entitySlider.onValueChanged.RemoveListener(OnEntityVolChanged);

        musicSlider.value = value;
        sfxSlider.value = value;
        entitySlider.value = value;

        musicSlider.onValueChanged.AddListener(OnMusicVolChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolChanged);
        entitySlider.onValueChanged.AddListener(OnEntityVolChanged);

        MouseScript.settingsApplied = true;
        MouseScript.stSens = sensSlider.value + 200;

        SaveManager.Instance.SaveUI(CollectSettings());
    }

    private SaveManager.UISettings CollectSettings()
    {
        return new SaveManager.UISettings
        {
            masterSaved = true,
            master = masterSlider.value,

            musicSaved = true,
            music = musicSlider.value,

            sfxSaved = true,
            sfx = sfxSlider.value,

            entitySaved = true,
            entity = entitySlider.value,

            sensSaved = true,
            sens = sensSlider.value
        };
    }
    /*

    private SaveManager.UISettings CollectSettingsVol()
    {
        return new SaveManager.UISettings
        {
            master = masterSlider.value,
            music = musicSlider.value,
            sfx = sfxSlider.value,
            entity = entitySlider.value,

        };
    }
    private SaveManager.UISettings CollectSettingsSensitivity()
    {
        return new SaveManager.UISettings
        {

            sens = sensSlider.value + 200
        };
    }
    */

    #endregion

    #region lobby menu
    public void LobbySettings()
    {
        lobbyCanv.SetActive(true);
        DisplayMenu(false);
        canPause = false;
    }

    public void OnLobbySettingsBack()
    {
        lobbyCanv.SetActive(false);
        DisplayMenu(true);
        canPause = true;
    }
    #endregion
    public void RestartGame()
    {

        Time.timeScale = 1;
        MouseScript.Instance.DisableMouse(false);
        if (NetworkManager.Singleton == null)
            SceneManager.LoadScene("main game");
        else
            NetworkManager.SceneManager.LoadScene("main game", LoadSceneMode.Single);

    }

    void ToggleFPSShow(bool b) 
    {
     DisplayFps.canShowFps = b;
    }

   
}







