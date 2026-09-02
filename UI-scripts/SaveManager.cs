using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    // =========================================================
    // SAVE WRAPPER
    // =========================================================

    [Serializable]
    public class SaveWrapper
    {
        public string dataType;
        public string jsonData;
    }
    // =========================================================
    // GAME PROGRESS
    // =========================================================

    [Serializable]
    public class GameProgress
    {
        public bool currentLevelSaved;
        public string currentLevel;

        public bool checkpointIdSaved;
        public string checkpointId;

        public bool playTimeSaved;
        public float playTime;

        public bool healthSaved;
        public float health;

        public bool unlockedLevelsSaved;
        public string[] unlockedLevels;

        public bool collectedItemsSaved;
        public string[] collectedItems;

        public bool completionPercentSaved;
        public float completionPercent;

        public bool lastSaveTimeSaved;
        public string lastSaveTime; // stored as ISO 8601 string

        public bool powerRestoredSaved;
        public bool powerRestored;


        public bool doorUnlockedSaved;
        public bool doorUnlocked;
    }

    // =========================================================
    // UI SETTINGS
    // =========================================================

    [Serializable]
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


    // =========================================================
    // CONTROLS
    // =========================================================

    [Serializable]
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


    // =========================================================
    // GRAPHICS
    // =========================================================

    [Serializable]
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


    // =========================================================
    // PATHS
    // =========================================================
    public string progressSavePath;
    public string uiSavePath;
    public string controlsSavePath;
    public string graphicsSavePath;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        progressSavePath = Application.persistentDataPath + "/progress.json";
        uiSavePath = Application.persistentDataPath + "/ui.json";
        controlsSavePath = Application.persistentDataPath + "/controls.json";
        graphicsSavePath = Application.persistentDataPath + "/graphics.json";
    }


    // =========================================================
    // GENERIC SAVE
    // =========================================================

    public void SaveData<T>(T data, string path)
    {
        if (data == null)
            return;

        if (string.IsNullOrEmpty(path))
            return;

        SaveWrapper wrapper = new SaveWrapper
        {
            dataType = typeof(T).Name,
            jsonData = JsonUtility.ToJson(data)
        };

        string json = JsonUtility.ToJson(wrapper);

        File.WriteAllText(path, json);
    }


    // =========================================================
    // GENERIC LOAD
    // =========================================================

    public T LoadData<T>(string path) where T : class
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (!File.Exists(path))
            return null;

        string json;

        try
        {
            json = File.ReadAllText(path);
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrEmpty(json))
            return null;

        SaveWrapper wrapper;

        try
        {
            wrapper = JsonUtility.FromJson<SaveWrapper>(json);
        }
        catch
        {
            return null;
        }

        if (wrapper == null)
            return null;

        if (wrapper.dataType != typeof(T).Name)
            return null;

        if (string.IsNullOrEmpty(wrapper.jsonData))
            return null;

        try
        {
            return JsonUtility.FromJson<T>(wrapper.jsonData);
        }
        catch
        {
            return null;
        }
    }


    // =========================================================
    // UI SAVE
    // =========================================================

    public void SaveUI(UISettings settings)
    {
        if (settings == null)
            return;

        SaveData(settings, uiSavePath);
    }


    // =========================================================
    // UI LOAD
    // =========================================================

    public UISettings LoadUI()
    {
        UISettings settings = LoadData<UISettings>(uiSavePath);

        if (settings == null)
            return null;

        UISettings validSettings = new UISettings();


        // -------------------------
        // MASTER
        // -------------------------

        if (settings.masterSaved &&
            IsValidFloat(settings.master))
        {
            validSettings.masterSaved = true;
            validSettings.master = settings.master;
        }


        // -------------------------
        // MUSIC
        // -------------------------

        if (settings.musicSaved &&
            IsValidFloat(settings.music))
        {
            validSettings.musicSaved = true;
            validSettings.music = settings.music;
        }


        // -------------------------
        // SFX
        // -------------------------

        if (settings.sfxSaved &&
            IsValidFloat(settings.sfx))
        {
            validSettings.sfxSaved = true;
            validSettings.sfx = settings.sfx;
        }


        // -------------------------
        // ENTITY
        // -------------------------

        if (settings.entitySaved &&
            IsValidFloat(settings.entity))
        {
            validSettings.entitySaved = true;
            validSettings.entity = settings.entity;
        }


        // -------------------------
        // SENSITIVITY
        // -------------------------

        if (settings.sensSaved &&
            IsValidFloat(settings.sens))
        {
            validSettings.sensSaved = true;
            validSettings.sens = settings.sens;
        }


        return validSettings;
    }


    // =========================================================
    // CONTROLS SAVE
    // =========================================================

    public void SaveControls(Controls settings)
    {
        if (settings == null)
            return;

        SaveData(settings, controlsSavePath);
    }


    // =========================================================
    // CONTROLS LOAD
    // =========================================================

    public Controls LoadControls()
    {
        Controls settings = LoadData<Controls>(controlsSavePath);

        if (settings == null)
            return null;

        Controls validSettings = new Controls();


        // -------------------------
        // INTERACT
        // -------------------------

        if (settings.interactKeySaved &&
            IsValidKeyCode(settings.interactKey))
        {
            validSettings.interactKeySaved = true;
            validSettings.interactKey = settings.interactKey;
        }


        // -------------------------
        // FUNCTION
        // -------------------------

        if (settings.functionKeySaved &&
            IsValidKeyCode(settings.functionKey))
        {
            validSettings.functionKeySaved = true;
            validSettings.functionKey = settings.functionKey;
        }


        // -------------------------
        // DROP
        // -------------------------

        if (settings.dropKeySaved &&
            IsValidKeyCode(settings.dropKey))
        {
            validSettings.dropKeySaved = true;
            validSettings.dropKey = settings.dropKey;
        }


        // -------------------------
        // JUMP
        // -------------------------

        if (settings.jumpKeySaved &&
            IsValidKeyCode(settings.jumpKey))
        {
            validSettings.jumpKeySaved = true;
            validSettings.jumpKey = settings.jumpKey;
        }


        // -------------------------
        // CROUCH
        // -------------------------

        if (settings.crouchKeySaved &&
            IsValidKeyCode(settings.crouchKey))
        {
            validSettings.crouchKeySaved = true;
            validSettings.crouchKey = settings.crouchKey;
        }


        // -------------------------
        // RUN
        // -------------------------

        if (settings.runKeySaved &&
            IsValidKeyCode(settings.runKey))
        {
            validSettings.runKeySaved = true;
            validSettings.runKey = settings.runKey;
        }


        return validSettings;
    }


    // =========================================================
    // GRAPHICS SAVE
    // =========================================================

    public void SaveGraphics(Graphics settings)
    {
        if (settings == null)
            return;

        SaveData(settings, graphicsSavePath);
    }


    // =========================================================
    // GRAPHICS LOAD
    // =========================================================

    public Graphics LoadGraphics()
    {
        Graphics settings = LoadData<Graphics>(graphicsSavePath);

        if (settings == null)
            return null;

        Graphics validSettings = new Graphics();


        // =====================================================
        // QUALITY
        // =====================================================

        if (settings.qualitySaved &&
            IsValidQuality(settings.quality))
        {
            validSettings.qualitySaved = true;
            validSettings.quality = settings.quality;
        }


        // =====================================================
        // FRAME RATE
        // =====================================================

        if (settings.frameRateSaved &&
            IsValidFrameRate(settings.frameRate))
        {
            validSettings.frameRateSaved = true;
            validSettings.frameRate = settings.frameRate;
        }


        // =====================================================
        // VSYNC
        // =====================================================

        if (settings.vsyncSaved)
        {
            validSettings.vsyncSaved = true;
            validSettings.vsync = settings.vsync;
        }


        // =====================================================
        // RAY TRACING
        // =====================================================

        if (settings.rayTracingSaved &&
            IsValidRayTracing(settings.rayTracing))
        {
            validSettings.rayTracingSaved = true;
            validSettings.rayTracing = settings.rayTracing;
        }


        // =====================================================
        // PATH TRACING
        // =====================================================

        if (settings.pathTracingSaved)
        {
            validSettings.pathTracingSaved = true;
            validSettings.pathTracing = settings.pathTracing;
        }


        // =====================================================
        // SHADOWS
        // =====================================================

        if (settings.shadowsSaved)
        {
            validSettings.shadowsSaved = true;
            validSettings.shadows = settings.shadows;
        }


        // =====================================================
        // DLSS
        // =====================================================

        if (settings.dlssSaved &&
            IsValidDLSS(settings.dlss))
        {
            validSettings.dlssSaved = true;
            validSettings.dlss = settings.dlss;
        }


        return validSettings;
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private bool IsValidFloat(float value)
    {
        return !float.IsNaN(value) &&
               !float.IsInfinity(value);
    }


    private bool IsValidKeyCode(KeyCode key)
    {
        return Enum.IsDefined(typeof(KeyCode), key);
    }


    private bool IsValidQuality(int value)
    {
        return Enum.IsDefined(
            typeof(GraphicsSettingsManager.QualityLevel),
            value
        );
    }


    private bool IsValidFrameRate(int value)
    {
        return Enum.IsDefined(
            typeof(GraphicsSettingsManager.FrameRate),
            value
        );
    }


    private bool IsValidRayTracing(int value)
    {
        return Enum.IsDefined(
            typeof(GraphicsSettingsManager.RayTracingQuality),
            value
        );
    }


    private bool IsValidDLSS(int value)
    {
        return Enum.IsDefined(
            typeof(GraphicsSettingsManager.DLSSSetQuality),
            value
        );
    }




    // =========================================================
    // GAME PROGRESS SAVE
    // =========================================================

    public void SaveProgress(GameProgress progress)
    {
        if (progress == null)
            return;

        SaveData(progress, progressSavePath);
    }

    // =========================================================
    // GAME PROGRESS LOAD
    // =========================================================

    public GameProgress LoadProgress()
    {
        GameProgress progress = LoadData<GameProgress>(progressSavePath);

        if (progress == null)
            return null;

        GameProgress validProgress = new GameProgress();


        // -------------------------
        // CURRENT LEVEL
        // -------------------------

        /*if (progress.currentLevelSaved &&
            !string.IsNullOrEmpty(progress.currentLevel))
        {
            validProgress.currentLevelSaved = true;
            validProgress.currentLevel = progress.currentLevel;
        }*/


        // -------------------------
        // CHECKPOINT ID
        // -------------------------

        if (progress.checkpointIdSaved &&
            !string.IsNullOrEmpty(progress.checkpointId))
        {
            validProgress.checkpointIdSaved = true;
            validProgress.checkpointId = progress.checkpointId;
        }


        // -------------------------
        // PLAY TIME
        // -------------------------

        if (progress.playTimeSaved &&
            IsValidFloat(progress.playTime) &&
            progress.playTime >= 0f)
        {
            validProgress.playTimeSaved = true;
            validProgress.playTime = progress.playTime;
        }


        // -------------------------
        // HEALTH
        // -------------------------
        /*
        if (progress.healthSaved &&
            IsValidFloat(progress.health) &&
            progress.health >= 0f)
        {
            validProgress.healthSaved = true;
            validProgress.health = progress.health;
        }
        */

        // -------------------------
        // UNLOCKED LEVELS
        // -------------------------

        if (progress.unlockedLevelsSaved &&
            progress.unlockedLevels != null)
        {
            validProgress.unlockedLevelsSaved = true;
            validProgress.unlockedLevels = progress.unlockedLevels;
        }


        // -------------------------
        // COLLECTED ITEMS
        // -------------------------

        if (progress.collectedItemsSaved &&
            progress.collectedItems != null)
        {
            validProgress.collectedItemsSaved = true;
            validProgress.collectedItems = progress.collectedItems;
        }


        // -------------------------
        // COMPLETION PERCENT
        // -------------------------

        if (progress.completionPercentSaved &&
            IsValidFloat(progress.completionPercent) &&
            progress.completionPercent >= 0f &&
            progress.completionPercent <= 100f)
        {
            validProgress.completionPercentSaved = true;
            validProgress.completionPercent = progress.completionPercent;
        }


        // -------------------------
        // LAST SAVE TIME
        // -------------------------

        if (progress.lastSaveTimeSaved &&
            !string.IsNullOrEmpty(progress.lastSaveTime) &&
            DateTime.TryParse(progress.lastSaveTime, out _))
        {
            validProgress.lastSaveTimeSaved = true;
            validProgress.lastSaveTime = progress.lastSaveTime;
        }

        if (progress.powerRestoredSaved) 
        {
            validProgress.powerRestoredSaved = true;
            validProgress.powerRestored = progress.powerRestored;
        }

        if (progress.doorUnlockedSaved)
        {
            validProgress.doorUnlockedSaved = true;
            validProgress.doorUnlocked= progress.doorUnlocked;
        }



        return validProgress;
    }





    // =========================================================
    // GAME PROGRESS - UPDATE & SAVE (partial update, safe)
    // =========================================================

    public void UpdateAndSaveProgress(
        string currentLevel = null,
        string checkpointId = null,
        float? playTime = null,
        float? health = null,
        string[] unlockedLevels = null,
        string[] collectedItems = null,
        float? completionPercent = null,
        bool? doorUnlocked = null,
        bool? powerRestored=null)
    {
        // Load existing progress (or start fresh if none exists yet)
        GameProgress progress = LoadProgress();
        if (progress == null)
            progress = new GameProgress();


        // -------------------------
        // CURRENT LEVEL
        // -------------------------

        if (currentLevel != null)
        {
            progress.currentLevelSaved = true;
            progress.currentLevel = currentLevel;
        }


        // -------------------------
        // CHECKPOINT ID
        // -------------------------

        if (checkpointId != null)
        {
            progress.checkpointIdSaved = true;
            progress.checkpointId = checkpointId;
        }


        // -------------------------
        // PLAY TIME
        // -------------------------

        if (playTime.HasValue)
        {
            progress.playTimeSaved = true;
            progress.playTime = playTime.Value;
        }


        // -------------------------
        // HEALTH
        // -------------------------

        if (health.HasValue)
        {
            progress.healthSaved = true;
            progress.health = health.Value;
        }


        // -------------------------
        // UNLOCKED LEVELS
        // -------------------------

        if (unlockedLevels != null)
        {
            progress.unlockedLevelsSaved = true;
            progress.unlockedLevels = unlockedLevels;
        }


        // -------------------------
        // COLLECTED ITEMS
        // -------------------------

        if (collectedItems != null)
        {
            progress.collectedItemsSaved = true;
            progress.collectedItems = collectedItems;
        }


        // -------------------------
        // COMPLETION PERCENT
        // -------------------------

        if (completionPercent.HasValue)
        {
            progress.completionPercentSaved = true;
            progress.completionPercent = completionPercent.Value;
        }


        // -------------------------
        // DOOR UNLOCKED
        // -------------------------

        if (doorUnlocked.HasValue)
        {
            progress.doorUnlockedSaved = true;
            progress.doorUnlocked = doorUnlocked.Value;
        }

        if(powerRestored.HasValue)
        {  progress.powerRestoredSaved = true;
           progress.powerRestored = powerRestored.Value;
        }


        // -------------------------
        // LAST SAVE TIME (always updated)
        // -------------------------

        progress.lastSaveTimeSaved = true;
        progress.lastSaveTime = DateTime.UtcNow.ToString("o");


        SaveProgress(progress);
    }
}