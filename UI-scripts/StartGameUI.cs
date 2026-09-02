using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameUI : MonoBehaviour
{
    // public KeyCode[] assignedKey;

    public enum GameResolution
    {
        R640x480,     // 4:3
        R800x600,     // 4:3
        R1024x768,    // 4:3
        R1280x720,    // 16:9 (HD)
        R1366x768,    // 16:9 (Laptop common)
        R1600x900,    // 16:9
        R1920x1080,   // 16:9 (Full HD)
        R2560x1440,   // 16:9 (2K)
        R3840x2160    // 16:9 (4K UHD)
    }

    public AudioMixer audioMixer;

    [System.Serializable]
    public class SaveWrapper
    {
        public string dataType;
        public string jsonData;
    }

   [System.Serializable]
public class UISettings
{
    public bool masterSaved;
    public float master;

    public bool musicSaved;
    public float music;

    public bool sfxSaved;
    public float sfx;

    public bool entitySaved;
    public float entity;

    public bool sensSaved;
    public float sens;
}

[System.Serializable]
public class Controls
{
    public bool interactKeySaved;
    public KeyCode interactKey;

    public bool functionKeySaved;
    public KeyCode functionKey;

    public bool dropKeySaved;
    public KeyCode dropKey;

    public bool jumpKeySaved;
    public KeyCode jumpKey;

    public bool crouchKeySaved;
    public KeyCode crouchKey;

    public bool runKeySaved;
    public KeyCode runKey;
}

[System.Serializable]
public class Graphics
{
    public bool qualitySaved;
    public int quality;

    public bool frameRateSaved;
    public int frameRate;

    public bool vsyncSaved;
    public bool vsync;

    public bool rayTracingSaved;
    public int rayTracing;

    public bool pathTracingSaved;
    public bool pathTracing;

    public bool shadowsSaved;
    public bool shadows;

    public bool dlssSaved;
    public int dlss;
}


    #region buttons
    public Button newGameButton;
    public Button loadGameButton;
    public Button multiplayerButton;
    public Button settingsButton;
    public Button quitGameButton;
    public Button settingsBackButton;
    #endregion

    #region settings content

    #region settings main menu
    [Header("---Settings Buttons---")]
    public Button volumeSettingsButton;
    public Button graphicsSettingsButton;
    public Button controlSettingsButton;
    public Button settingsButtonBack;
    #endregion


    //---volume
    #region volume
    [Header("---Volume---")]
    public GameObject volumeCanvas;
    public Slider sfxSlider;
    public Slider musicSlider;
    public Slider masterSlider;
    public Slider entitySlider;
    public Button volumeButtonBack;
  
    [Header("---Volume Changes Confirmation---")]
    public bool canAskForUiChangesConfirmation;
    public GameObject volumeChangesConfirmationMenu;

    public Button volumeChangesConfirmButton;
    public Button volumeChangesDenyButton;
    #endregion
    //---graphics
    #region graphicsMenu
    [Header("---Graphics---")]
    public GameObject graphicsCanvas;
    public Button graphicsButtonBack;
    public TMP_Dropdown fpsDropDown;
    public TMP_Dropdown qualityDropdown;
    public Toggle fpsToggle;
    public TMP_Dropdown rayTracingDropDown;
    public TMP_Dropdown dlssDropDown;
    public Toggle pathTracingToggle;
    public Toggle shadowstoggle;
    public Toggle vsyncToggle;
    public TMP_Dropdown resolutionDropdown;

    [Header("---Graphics Changes Confirmation---")]
    public bool canAskForGraphicsChangesConfirmation;
    public GameObject graphicsChangesConfirmationMenu;

    public Button graphicsChangesConfirmButton;
    public Button graphicsChangesDenyButton;

    #endregion
    //---controls
    #region control settings
    [Header("---Control Settings---")]
    public GameObject controlCanvas;
    public Button controlsButtonBack;
    public Slider sensSlider;
    public Button[] keyAssignmentButtons;
    public KeyCode[] tempKeys;
    public int currentIndexForKeyassignment = -1;
    public int currentIndexForTempKey = 0;
    public bool keyStateConfirmed = false;
    public int assignmentIndex;
    public int[] currentIndexesForKeyAssignment;
    public bool canAskForKeyAssignConfirmation = false;
    public GameObject keyAssignmentConfirmationMenu;
    public Button keyAssignmentConfirmationButton;
    public Button keyAssignmentCancellationButton;
    private HashSet<int> changedKeyIndexes = new HashSet<int>();

    public List<int> buttonIndexes;
    #endregion

    #endregion

    public GameObject settingsCanvas;
    public GameObject mainCanvas;

    public static bool loaded = false;
    public static bool LoadGame = false;

    public string uiSavePath;
    public string controlsSavePath;
    public string graphicsSavePath;




    void PopulateResolutionDropdown()
    { if (resolutionDropdown == null) return;
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(System.Enum.GetNames(typeof(GameResolution)).ToList());
    }

    private void Start()
    {

        SetCurrentResolutionDropdown();
        PopulateResolutionDropdown();


        uiSavePath = Application.persistentDataPath + "/ui.json";
        controlsSavePath = Application.persistentDataPath + "/controls.json";
        graphicsSavePath = Application.persistentDataPath + "/graphics.json";


        //ApplyLoadedSettings();

        newGameButton.onClick.AddListener(OnNewGame);
        loadGameButton.onClick.AddListener(OnLoadGame);
        multiplayerButton.onClick.AddListener(OnMultiplayer);
        settingsButton.onClick.AddListener(OnSettings);

        settingsBackButton.onClick.AddListener(OnSettingsBack);
        quitGameButton.onClick.AddListener(OnQuitGame);

        #region graphicsSettings

        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        fpsDropDown.onValueChanged.AddListener(OnFpsChanged);
        vsyncToggle.onValueChanged.AddListener(OnVsyncChanged);
        pathTracingToggle.onValueChanged.AddListener(OnToggledPathTracing);
        rayTracingDropDown.onValueChanged.AddListener(OnToggledRayTracing);
        shadowstoggle.onValueChanged.AddListener(OnToggledShadows);
        dlssDropDown.onValueChanged.AddListener(OnToggledDLSS);
        graphicsButtonBack.onClick.AddListener(OnGraphicsBack);

        graphicsChangesConfirmButton.onClick.AddListener(OnGraphicsChangesConfirmed);
        graphicsChangesDenyButton.onClick.AddListener(OnGraphicsChangesCancelled);


        //if(resolutionDropdown!=null)resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        #endregion

        #region volumeSettings
        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        entitySlider.onValueChanged.AddListener(OnEntitySliderChanged);
        sensSlider.onValueChanged.AddListener(OnSensitivitySliderChanged);

        volumeButtonBack.onClick.AddListener(OnVolumeBack);

        volumeChangesConfirmButton.onClick.AddListener(OnVolumeChangesConfirmed);
        volumeChangesDenyButton.onClick.AddListener(OnVolumeChangesCancelled);


        #endregion
        #region controlSettings
        for (int i = 0; i < keyAssignmentButtons.Length; i++)
        {
            int index = i; // Keep this button's index

            keyAssignmentButtons[i].onClick.AddListener
            (
                () =>
                {
                    var currButton = keyAssignmentButtons[i];
                    var textCMP = currButton.GetComponent<TMP_Text>();
                    if (textCMP != null)
                        StartCoroutine(WaitForKeyPress(index, textCMP));
                    else
                        Debug.LogError("Text Not Found For " + currButton.name);
                });
        }

        keyAssignmentConfirmationButton.onClick.AddListener(OnKeysChangesConfirmed);
        keyAssignmentCancellationButton.onClick.AddListener(OnKeysChangesCancelled);
        controlsButtonBack.onClick.AddListener(OnControlsBack);
        #endregion

        #region settings listeners
        volumeSettingsButton.onClick.AddListener(OnVolumeSettings);
        graphicsSettingsButton.onClick.AddListener(OnGraphicsSettings);
        controlSettingsButton.onClick.AddListener(OnControlsSettings);
        settingsBackButton.onClick.AddListener(OnSettingsBack);
        #endregion
    }

    private void OnControlsBack()
    {
        if (!canAskForKeyAssignConfirmation)
        {
            DisplayMenu(controlCanvas, false);
            DisplayMenu(settingsCanvas, true);
        }
        else
        AskForKeysConfirmation();
    }





    private void OnGraphicsBack()
    {
        if (!canAskForGraphicsChangesConfirmation)
        {
            DisplayMenu(graphicsCanvas, false);
            DisplayMenu(settingsCanvas, true);
        }
        else
        AskForGraphicChangesConfirmation();
    }

    
    void AskForGraphicsChangesConfirmation()
    {
        if (!canAskForGraphicsChangesConfirmation)
            return;

        DisplayMenu(graphicsChangesConfirmationMenu, true);
    }

    void OnGraphicsChangesConfirmed()
    {
        SaveGraphics();

       

        DisplayMenu(graphicsChangesConfirmationMenu, false);

        DisplayMenu(graphicsCanvas, false);
        DisplayMenu(settingsCanvas, true);

        canAskForGraphicsChangesConfirmation = false;
    }

    void OnGraphicsChangesCancelled()
    {
       

        DisplayMenu(graphicsChangesConfirmationMenu, false);
       

        DisplayMenu(graphicsCanvas, false);
        DisplayMenu(settingsCanvas, true);

        canAskForGraphicsChangesConfirmation = false;
    }


    private void OnVolumeBack()
    {
        if (!canAskForUiChangesConfirmation)
        {
            DisplayMenu(volumeCanvas, false);
            DisplayMenu(settingsCanvas, true);
        }

        else
        AskForVolumeChangesConfirmation();
    }

    void AskForVolumeChangesConfirmation()
    {
        if (!canAskForUiChangesConfirmation)
            return;

        DisplayMenu(volumeChangesConfirmationMenu, true);
    }

    void OnVolumeChangesConfirmed()
    {
        SaveUI();

       

        DisplayMenu(volumeChangesConfirmationMenu, false);
        DisplayMenu(volumeCanvas,false);
        DisplayMenu(settingsCanvas,true);

        canAskForUiChangesConfirmation = false;
    }

    void OnVolumeChangesCancelled()
    {


        DisplayMenu(volumeChangesConfirmationMenu, false);
        DisplayMenu(volumeCanvas, false);
        DisplayMenu(settingsCanvas, true);


        canAskForUiChangesConfirmation = false;

      
    }

    private void OnSettingsBack()
    {
        DisplayMenu(settingsCanvas, false);
        DisplayMenu(mainCanvas, true);

    }

#region controls functions


    IEnumerator WaitForKeyPress(int i, TMP_Text ButtonName)
    {
        yield return new WaitUntil(
            () =>
                Input.anyKeyDown
        );

        // After a key is pressed, figure out which one
        foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
        {
            if
            (
              k == KeyCode.Mouse0 ||
              k == KeyCode.Mouse1 ||
              k == KeyCode.Mouse2 ||
              k == KeyCode.Mouse3 ||
              k == KeyCode.Mouse4 ||
              k == KeyCode.Mouse5 ||
              k == KeyCode.Mouse6
              || PlayerMovement.FindKeyAssignment(k) != null
            )
            {
                continue;
            }

            if (Input.GetKeyDown(k))
            {
                Debug.Log("Pressed: " + k);
                canAskForKeyAssignConfirmation = true;

                currentIndexForKeyassignment = i;

                // Save the key specifically to this button's index
                tempKeys[i] = k;
                ButtonName.text = k.ToString();

                OnKeyChanged(i);

                break;
            }
        }
    }

    private void OnKeyChanged(int i)
    {
        // KeyCode currentKey = tempKeys[i];

        changedKeyIndexes.Add(i);
    }

    public void AssignKeys()
    {


        foreach (int i in changedKeyIndexes)
        {
            KeyCode currentKey = tempKeys[i];

            switch (i)
            {
                case 0:
                    PlayerMovement.InputKeys.interactKey = currentKey;
                    break;

                case 1:
                    PlayerMovement.InputKeys.functionKey = currentKey;
                    break;

                case 2:
                    PlayerMovement.InputKeys.dropKey = currentKey;
                    break;

                case 3:
                    PlayerMovement.InputKeys.jumpKey = currentKey;
                    break;

                case 4:
                    PlayerMovement.InputKeys.crouchKey = currentKey;
                    break;

                case 5:
                    PlayerMovement.InputKeys.runKey = currentKey;
                    break;
            }
        }


        changedKeyIndexes.Clear();


    }
    void AskForKeysConfirmation()
    {
        if (!canAskForKeyAssignConfirmation) return;
        DisplayMenu(keyAssignmentConfirmationMenu, true);

        

    }
    void OnKeysChangesConfirmed()
    {
        AssignKeys();
        SaveControls();
        DisplayMenu(keyAssignmentConfirmationMenu, false);
        DisplayMenu(controlCanvas, false);
        DisplayMenu(settingsCanvas, true);
        canAskForKeyAssignConfirmation = false;
    }
    void OnKeysChangesCancelled()
    {
        DisplayMenu(keyAssignmentConfirmationMenu, false);
        DisplayMenu(controlCanvas, false);
        DisplayMenu(settingsCanvas, true);
        canAskForKeyAssignConfirmation = false;
    }

    #endregion


    
    void OnVolumeSettings()
    {
        DisplayMenu(volumeCanvas, true);
        DisplayMenu(settingsCanvas, false);
    }
    void OnGraphicsSettings()
    {
        DisplayMenu(graphicsCanvas, true);
        DisplayMenu(settingsCanvas, false);
    }

    void OnControlsSettings()
    {
        DisplayMenu(controlCanvas, true);
        DisplayMenu(settingsCanvas, false);

        foreach (var button in keyAssignmentButtons)
        {
            var tmp = button.GetComponent<TMP_Text>();
            int c = 0;


            if (tmp != null)
            {
                if (c == 0)
                    tmp.text = PlayerMovement.InputKeys.interactKey.ToString();
                if (c == 1) tmp.text = PlayerMovement.InputKeys.functionKey.ToString();

                if (c == 2) tmp.text = PlayerMovement.InputKeys.dropKey.ToString();

                if (c == 3) tmp.text = PlayerMovement.InputKeys.jumpKey.ToString();
                if (c == 4) tmp.text = PlayerMovement.InputKeys.crouchKey.ToString();
                if (c == 5) tmp.text = PlayerMovement.InputKeys.runKey.ToString();


                c++;
            }
        }
    }

    void DisplayMenu(GameObject g, bool b) { g.SetActive(b); }


    #region graphcis settings
    void OnFpsChanged(int arg)
    {
        GraphicsSettingsManager.Instance.currentFrameRate = (GraphicsSettingsManager.FrameRate)arg;
        OnGraphicsSettings();

    }
    private void OnQualityChanged(int arg0)
    {
        GraphicsSettingsManager.Instance.currentQuality = (GraphicsSettingsManager.QualityLevel)arg0;
        OnGraphicsSettings();
    }

    void SetCurrentResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        int w = Screen.width;
        int h = Screen.height;

        // Loop through enum values and match
        foreach (GameResolution res in System.Enum.GetValues(typeof(GameResolution)))
        {
            switch (res)
            {
                case GameResolution.R640x480: if (w == 640 && h == 480) resolutionDropdown.value = (int)res; break;
                case GameResolution.R800x600: if (w == 800 && h == 600) resolutionDropdown.value = (int)res; break;
                case GameResolution.R1024x768: if (w == 1024 && h == 768) resolutionDropdown.value = (int)res; break;
                case GameResolution.R1280x720: if (w == 1280 && h == 720) resolutionDropdown.value = (int)res; break;
                case GameResolution.R1366x768: if (w == 1366 && h == 768) resolutionDropdown.value = (int)res; break;
                case GameResolution.R1600x900: if (w == 1600 && h == 900) resolutionDropdown.value = (int)res; break;
                case GameResolution.R1920x1080: if (w == 1920 && h == 1080) resolutionDropdown.value = (int)res; break;
                case GameResolution.R2560x1440: if (w == 2560 && h == 1440) resolutionDropdown.value = (int)res; break;
                case GameResolution.R3840x2160: if (w == 3840 && h == 2160) resolutionDropdown.value = (int)res; break;
            }
        }

        // Refresh dropdown display
        resolutionDropdown.RefreshShownValue();



    } 

    void AskForGraphicChangesConfirmation()
    {
        if (!canAskForGraphicsChangesConfirmation) return;
        DisplayMenu(graphicsChangesConfirmationMenu, true);

    }
    void OnGraphicChangesConfirmed()
    {
       
        SaveGraphics();
        canAskForGraphicsChangesConfirmation = false;

        DisplayMenu(graphicsChangesConfirmationMenu, false);
        DisplayMenu(settingsCanvas, true);
    }
    void OnGraphicChangesCancelled()
    {
        canAskForGraphicsChangesConfirmation = false;

        DisplayMenu(graphicsChangesConfirmationMenu, false);
        DisplayMenu(settingsCanvas, true);
    }

#endregion

    void OnNewGame()
    {
        SceneManager.LoadScene("main game");
        LoadGame = false;

    }

    void OnLoadGame()
    {
        SceneManager.LoadScene("main game");
        LoadGame = true;
    }

    void OnSettings()
    {
        settingsCanvas.SetActive(true);
        mainCanvas.SetActive(false);
    }

    

    void OnMultiplayer()
    {
        SceneManager.LoadScene("lobby");
    }

    void OnQuitGame()
    {
        Time.timeScale = 1;
        Application.Quit();
    }


    public void ApplyResolution(GameResolution res, bool fullscreen)
    {
        switch (res)
        {
            case GameResolution.R640x480: Screen.SetResolution(640, 480, fullscreen); break;
            case GameResolution.R800x600: Screen.SetResolution(800, 600, fullscreen); break;
            case GameResolution.R1024x768: Screen.SetResolution(1024, 768, fullscreen); break;
            case GameResolution.R1280x720: Screen.SetResolution(1280, 720, fullscreen); break;
            case GameResolution.R1366x768: Screen.SetResolution(1366, 768, fullscreen); break;
            case GameResolution.R1600x900: Screen.SetResolution(1600, 900, fullscreen); break;
            case GameResolution.R1920x1080: Screen.SetResolution(1920, 1080, fullscreen); break;
            case GameResolution.R2560x1440: Screen.SetResolution(2560, 1440, fullscreen); break;
            case GameResolution.R3840x2160: Screen.SetResolution(3840, 2160, fullscreen); break;
        }
    }

    public void ChangeResolution()
    {
        // Get the selected index from the dropdown
        int selectedIndex = resolutionDropdown.value;

        // Map index to your enum
        GameResolution selectedRes = (GameResolution)selectedIndex;

        // Apply resolution (fullscreen true for now, you can toggle later)
        ApplyResolution(selectedRes, true);

        OnSettingsAppliedGraphics();

        
    }
    


    void OnVsyncChanged(bool arg) 
    {
    
    GraphicsSettingsManager.Instance.vsyncEnabled = arg;
       OnSettingsAppliedGraphics();
    }


    void OnToggledRayTracing(int arg)
    {
        GraphicsSettingsManager.Instance.currentRayTraceSetting = (GraphicsSettingsManager.RayTracingQuality)arg;
        OnSettingsAppliedGraphics();
    }

    void OnToggledPathTracing(bool arg)
    {

        GraphicsSettingsManager.Instance.pathTracingEnabled = arg;
        OnSettingsAppliedGraphics();

    }


    void OnToggledDLSS(int arg)
    {
        GraphicsSettingsManager.Instance.currentDlssSetting = (GraphicsSettingsManager.DLSSSetQuality)arg;
        OnSettingsAppliedGraphics();
    }

    void OnToggledShadows(bool b)
    {
        GraphicsSettingsManager.Instance.SetShadows(b);
        OnSettingsAppliedGraphics();

    }

    
    

    void OnSettingsAppliedGraphics() 
    {
        GraphicsSettingsManager.Instance.settingsApplied = true;
        canAskForGraphicsChangesConfirmation = true;
  
    }


    void OnMasterSliderChanged(float value)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(value) * 20f);

        OnSettingsAppliedUI();
    }

    void OnMusicSliderChanged(float value)
    {
        audioMixer.SetFloat("music", Mathf.Log10(value) * 20f);

        OnSettingsAppliedUI();
    }

    void OnSfxSliderChanged(float value)
    {
        audioMixer.SetFloat("sfx", Mathf.Log10(value) * 20f);

        OnSettingsAppliedUI();
    }

    void OnEntitySliderChanged(float value)
    {
        audioMixer.SetFloat("entity", Mathf.Log10(value) * 20f);

        OnSettingsAppliedUI();
    }

    void OnSensitivitySliderChanged(float value)
    {
        MouseScript.stSens = value;

        OnSettingsAppliedUI();
    }
    void OnSettingsAppliedUI()
    {
        GraphicsSettingsManager.Instance.settingsApplied = true;
        canAskForUiChangesConfirmation = true;

    }



    public void SaveUI()
    {
        SaveManager.UISettings ui = new SaveManager.UISettings
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

        SaveManager.Instance.SaveUI(ui);
    }



    public void SaveGraphics()
    {
        SaveManager.Graphics graphics =
            new SaveManager.Graphics
            {
                qualitySaved = true,
                quality = qualityDropdown.value,

                frameRateSaved = true,
                frameRate = fpsDropDown.value,

                vsyncSaved = true,
                vsync = vsyncToggle.isOn,

                rayTracingSaved = true,
                rayTracing = rayTracingDropDown.value,

                pathTracingSaved = true,
                pathTracing = pathTracingToggle.isOn,

                shadowsSaved = true,
                shadows = shadowstoggle.isOn,

                dlssSaved = true,
                dlss = dlssDropDown.value
            };

        SaveManager.Instance.SaveGraphics(graphics);
    }

    public void SaveControls()
    {
        SaveManager.Controls controls =
            new SaveManager.Controls
            {
                interactKeySaved = true,
                interactKey = PlayerMovement.InputKeys.interactKey,

                functionKeySaved = true,
                functionKey = PlayerMovement.InputKeys.functionKey,

                dropKeySaved = true,
                dropKey = PlayerMovement.InputKeys.dropKey,

                jumpKeySaved = true,
                jumpKey = PlayerMovement.InputKeys.jumpKey,

                crouchKeySaved = true,
                crouchKey = PlayerMovement.InputKeys.crouchKey,

                runKeySaved = true,
                runKey = PlayerMovement.InputKeys.runKey
            };

          SaveManager.Instance.SaveControls(controls);
    }




}
