using UnityEngine;
using UnityEngine.NVIDIA;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
//using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;



public class GraphicsSettingsManager : MonoBehaviour
{
    public bool settingsApplied;
    //public bool graphicsSettingsApplied;
    //public bool controlSettingsApplied;
    //public bool uiSettingsApplied;

    public enum QualityLevel
    {
        High,
        Medium,
        Low
    }

    public  enum FrameRate
    {
        FPS30 = 30,
        FPS60 = 60,
        FPS90 = 90,
        FPS120 = 120,
        Unlimited = -1
    }


    public  enum RayTracingQuality
    {
        Off,
        Low,
        Medium,
        High,
        Ultra
    }
    public  enum DLSSSetQuality
    {
        Off,
        Low,
        Balanced,
        Medium,
        High,
        Ultra
    }


    public static GraphicsSettingsManager Instance;

    #region for raytracing and pathtracing
    [SerializeField] private Camera playerCamera;
    [Header("Path Tracing")]
    [SerializeField] private Volume globalVolume;
    private PathTracing pathTracing;
    private RayTracingSettings rayTracingSettings;
    private LightCluster lightCluster;
    private RecursiveRendering recursiveRendering;
    private SubSurfaceScattering subSurfaceScattering;
    private HDAdditionalLightData[] lights;

    private ScreenSpaceReflection screenSpaceReflection;
    private GlobalIllumination screenSpaceGI; // this is HDRP's SSGI/RTGI volume component
     private ScreenSpaceAmbientOcclusion ambientOcclusion;
    private ContactShadows contactShadows;


    #endregion

    private HDAdditionalCameraData hdCameraData;

    public FrameRate? currentFrameRate =null;

    public  QualityLevel? currentQuality = null;
    public  QualityLevel? CurrentQuality => currentQuality;
    public  FrameRate? CurrentFrameRate => currentFrameRate;

    public bool shadowsEnabled;
    public bool pathTracingEnabled;
    public bool vsyncEnabled;


    public RayTracingQuality? currentRayTraceSetting = null;
    public DLSSSetQuality? currentDlssSetting = null;
   

    // Cache native resolution once
    private int nativeWidth, nativeHeight;

    private void Awake()
    {
        nativeWidth = Screen.currentResolution.width;
        nativeHeight = Screen.currentResolution.height;

        Instance = this;
      

      //  globalVolume.profile.TryGet(out screenSpaceReflection);
       

    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SetFrameRate(FrameRate.FPS60);
        SetVSync(false);



       
       

    }
    void LoadSettingsFromStart() 
    {
        

        SetShadows(shadowsEnabled);
        EnableDLSS(currentDlssSetting);
        SetRayTracingQuality(currentRayTraceSetting);
        EnablePathTracing(pathTracingEnabled);
        SetQuality(CurrentQuality);
        SetVSync(vsyncEnabled);
        SetFrameRate(currentFrameRate);


       


    }

    void LoadSettings() 
    {
        if (settingsApplied)
            LoadSettingsFromStart();
        else
            DefaultValues();
    }
    void DefaultValues() 
    {
      
          SetShadows(true);
        
    }
    void SetQuality(QualityLevel? quality) 
    {
        if(quality == null) return; 
        if (quality == QualityLevel.Low)
            SetLowGraphics();

        if (quality == QualityLevel.Medium)
            SetMediumGraphics();

        if (quality == QualityLevel.High)
            SetHighGraphics();
    }
    bool settingsAdjusted = false;

    void Update()
    {
        if( SceneManager.GetActiveScene().name == "main game" && PlayerMovement.Instance!=null && !settingsAdjusted) 
        {
            Debug.Log("===== GRAPHICS STARTUP =====");
            Debug.Log("Settings Applied: " + settingsApplied);
            Debug.Log("Current Quality Variable: " + currentQuality);
            Debug.Log("Unity Quality Index: " + QualitySettings.GetQualityLevel());
            Debug.Log("Unity Quality Name: " +
                      QualitySettings.names[QualitySettings.GetQualityLevel()]);
            Debug.Log("============================");


            lights = FindObjectsByType<HDAdditionalLightData>();
            if (playerCamera == null)
            {

                var camP = PlayerMovement.Instance.cameraTransform;
                playerCamera = camP.GetComponentInChildren<Camera>();
                hdCameraData = playerCamera.GetComponent<HDAdditionalCameraData>();


            }

            if (globalVolume == null)
                globalVolume = GameObject.Find("Global Volume").GetComponent<Volume>();

            globalVolume.profile.TryGet(out pathTracing);
            globalVolume.profile.TryGet(out rayTracingSettings);
            globalVolume.profile.TryGet(out lightCluster);
            globalVolume.profile.TryGet(out recursiveRendering);
            globalVolume.profile.TryGet(out screenSpaceReflection);
            globalVolume.profile.TryGet(out screenSpaceGI);
            globalVolume.profile.TryGet(out ambientOcclusion);

            LoadSettings();
            settingsAdjusted = true;
           
        }
       

    }
  

    public void EnablePathTracing(bool enabled)
    {
        if (pathTracing == null)
            return;

        pathTracing.active = true;
        pathTracing.enable.value = enabled;
        pathTracingEnabled = enabled;
    }

    public void SetShadows(bool enabled)
    {
        shadowsEnabled = enabled;
        if (hdCameraData == null)
            return;

        hdCameraData.customRenderingSettings = true;

        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[
            (uint)FrameSettingsField.ShadowMaps
        ] = true;

        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(
            FrameSettingsField.ShadowMaps,
            enabled
        );

        if (lights != null)
        {
            foreach (HDAdditionalLightData lightData in lights)
            {
                if (lightData == null) continue;

                lightData.EnableShadows(enabled);
                
            }
        }
    }
    #region quality
    private void SetUnityQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    private void SetMipmapLimit(int limit)
    {
        QualitySettings.globalTextureMipmapLimit = limit;
    }
  
    public void SetLowGraphics()
    {
        currentQuality = QualityLevel.Low;

        // Unity Quality Level: Performant
        SetUnityQualityLevel(2);

        
        SetMipmapLimit(2);
    }

    public void SetMediumGraphics()
    {
        currentQuality = QualityLevel.Medium;

        // Unity Quality Level: Balanced
        SetUnityQualityLevel(1);

      
        SetMipmapLimit(1);
    }

    public void SetHighGraphics()
    {
        currentQuality = QualityLevel.High;

        // Unity Quality Level: High Fidelity
        SetUnityQualityLevel(0);

       ;
        SetMipmapLimit(0);
    }

    #endregion

    #region fps
    public void SetFrameRate(FrameRate? frameRate)
    {if (frameRate == null) return;
        currentFrameRate = frameRate;
        Application.targetFrameRate = (int)currentFrameRate;
    }

    public int GetFrameRateDropdownIndex()
    {
        if (currentFrameRate == null) return -1;
        switch (currentFrameRate)
        {
            case FrameRate.FPS30: return 0;
            case FrameRate.FPS60: return 1;
            case FrameRate.FPS90: return 2;
            case FrameRate.FPS120: return 3;
            case FrameRate.Unlimited: return 4;
            default: return 1;
        }
    }

    public void OnFrameRateChanged(int index)
    {

        switch (index)
        {
            case 0:
                GraphicsSettingsManager.Instance.SetFrameRate(FrameRate.FPS30);
                break;
            case 1:
                GraphicsSettingsManager.Instance.SetFrameRate(FrameRate.FPS60);
                break;
            case 2:
                GraphicsSettingsManager.Instance.SetFrameRate(FrameRate.FPS90);
                break;
            case 3:
                GraphicsSettingsManager.Instance.SetFrameRate(FrameRate.FPS120);
                break;
            case 4:
                GraphicsSettingsManager.Instance.SetFrameRate(FrameRate.Unlimited);
                break;
        }
    }
    #endregion

    #region ray tracing

    /// <summary>
    /// AAA-style RT quality preset. Drives reflections, GI, AO and shadows
    /// together instead of just toggling ray traced shadows.
    /// Off = fully rasterized (SSR/SSAO fallback).
    /// Low/Medium/High = scaled RT reflections + RTGI + RT shadows, AO ray traced from Medium up.
    /// Ultra = max quality + recursive rendering for transparents/glass.
    /// </summary>
    public void SetRayTracingQuality(RayTracingQuality? quality)
    {
        if (hdCameraData == null || quality == null)
            return;

        currentRayTraceSetting = quality;

        // =========================================================
        // CAMERA - master RT toggle
        // =========================================================
        hdCameraData.customRenderingSettings = true;

        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[
            (uint)FrameSettingsField.RayTracing
        ] = true;

        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(
            FrameSettingsField.RayTracing,
            quality != RayTracingQuality.Off
        );

        // Shadow maps always stay available as a fallback / for non-RT lights.
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[
            (uint)FrameSettingsField.ShadowMaps
        ] = true;

        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(
            FrameSettingsField.ShadowMaps,
            true
        );

        // =========================================================
        // OFF - everything back to rasterized fallbacks
        // =========================================================
        if (quality == RayTracingQuality.Off)
        {
            if (rayTracingSettings != null) rayTracingSettings.active = false;
            if (lightCluster != null) lightCluster.active = false;

            if (recursiveRendering != null)
            {
                recursiveRendering.active = true;
                recursiveRendering.enable.value = false;
            }

            if (screenSpaceReflection != null)
            {
                screenSpaceReflection.tracing.overrideState = true;
                screenSpaceReflection.tracing.value = RayCastingMode.RayMarching;
            }

            if (screenSpaceGI != null)
            {
                screenSpaceGI.tracing.overrideState = true;
                screenSpaceGI.tracing.value = RayCastingMode.RayMarching;
            }

            if (ambientOcclusion != null)
            {
                ambientOcclusion.rayTracing.overrideState = true;
                ambientOcclusion.rayTracing.value = false;
            }

            SetLightRayTracing(false, 0, false);
            return;
        }

        // =========================================================
        // SHARED RT CONFIG (not quality-dependent)
        // =========================================================
        if (rayTracingSettings != null)
        {
            rayTracingSettings.active = true;
            rayTracingSettings.rayBias.value = 0.001f;
            rayTracingSettings.distantRayBias.value = 0.001f;
        }

        // =========================================================
        // LIGHT CLUSTER - bigger range at higher quality = more accurate
        // reflections/GI far from camera, at a perf cost
        // =========================================================
        if (lightCluster != null)
        {
            lightCluster.active = true;

            switch (quality)
            {
                case RayTracingQuality.Low: lightCluster.cameraClusterRange.value = 10f; break;
                case RayTracingQuality.Medium: lightCluster.cameraClusterRange.value = 20f; break;
                case RayTracingQuality.High: lightCluster.cameraClusterRange.value = 30f; break;
                case RayTracingQuality.Ultra: lightCluster.cameraClusterRange.value = 40f; break;
            }
        }

        // =========================================================
        // RAY TRACED REFLECTIONS
        // Only 'tracing' is a real VolumeParameter (needs .overrideState/.value).
        // minSmoothness, rayMaxIterations, denoise, denoiserRadius, rayLength,
        // fullResolution are plain C# properties - direct assignment, no .value.
        // denoiserRadius is an int here (not float). Confirmed against HDRP API.
        //
        // minSmoothness = how rough a surface can be and still get RT reflections
        // (lower = more surfaces qualify = more expensive)
        // =========================================================
        if (screenSpaceReflection != null)
        {
            screenSpaceReflection.tracing.overrideState = true;
            screenSpaceReflection.tracing.value = RayCastingMode.RayTracing;

            switch (quality)
            {
                case RayTracingQuality.Low:
                    screenSpaceReflection.minSmoothness = 0.7f;
                    screenSpaceReflection.rayMaxIterations = 16;
                    screenSpaceReflection.denoise = true;
                    screenSpaceReflection.denoiserRadius = 8;
                    break;

                case RayTracingQuality.Medium:
                    screenSpaceReflection.minSmoothness = 0.4f;
                    screenSpaceReflection.rayMaxIterations = 32;
                    screenSpaceReflection.denoise = true;
                    screenSpaceReflection.denoiserRadius = 12;
                    break;

                case RayTracingQuality.High:
                    screenSpaceReflection.minSmoothness = 0.2f;
                    screenSpaceReflection.rayMaxIterations = 48;
                    screenSpaceReflection.denoise = true;
                    screenSpaceReflection.denoiserRadius = 16;
                    break;

                case RayTracingQuality.Ultra:
                    screenSpaceReflection.minSmoothness = 0.0f;
                    screenSpaceReflection.rayMaxIterations = 64;
                    screenSpaceReflection.denoise = true;
                    screenSpaceReflection.denoiserRadius = 20;
                    break;
            }
        }

        // =========================================================
        // RAY TRACED GLOBAL ILLUMINATION
        // Only 'tracing' is a real VolumeParameter. There is NO 'rayMaxIterations'
        // field on GlobalIllumination - the ray-marching step count property is
        // called 'maxRaySteps'. fullResolution/denoise are plain bool properties.
        // =========================================================
        if (screenSpaceGI != null)
        {
            screenSpaceGI.tracing.overrideState = true;
            // Mixed = ray traced far/expensive parts + screen space for cheap parts,
            // a common AAA compromise below the top preset.
            screenSpaceGI.tracing.value =
                quality == RayTracingQuality.Ultra ? RayCastingMode.RayTracing : RayCastingMode.Mixed;

            screenSpaceGI.denoise = true;

            switch (quality)
            {
                case RayTracingQuality.Low:
                    screenSpaceGI.fullResolution = false;
                    screenSpaceGI.maxRaySteps = 16;
                    break;

                case RayTracingQuality.Medium:
                    screenSpaceGI.fullResolution = false;
                    screenSpaceGI.maxRaySteps = 32;
                    break;

                case RayTracingQuality.High:
                    screenSpaceGI.fullResolution = true;
                    screenSpaceGI.maxRaySteps = 48;
                    break;

                case RayTracingQuality.Ultra:
                    screenSpaceGI.fullResolution = true;
                    screenSpaceGI.maxRaySteps = 64;
                    break;
            }
        }

        // =========================================================
        // RAY TRACED AMBIENT OCCLUSION
        // Skip on Low - RTAO is a relatively expensive add-on, most AAA
        // "Low RT" presets keep SSAO and only add RTAO from Medium up.
        //
        // Note: rayTracing is a BoolParameter (needs .overrideState/.value).
        // denoise, denoiserRadius, rayLength, sampleCount, fullResolution are
        // plain properties on ScreenSpaceAmbientOcclusion - direct assignment,
        // no .value. Confirmed against HDRP 17.5 API.
        // =========================================================
        if (ambientOcclusion != null)
        {
            bool rtao = quality != RayTracingQuality.Low;

            ambientOcclusion.rayTracing.overrideState = true;
            ambientOcclusion.rayTracing.value = rtao;

            if (rtao)
            {
                switch (quality)
                {
                    case RayTracingQuality.Medium:
                        ambientOcclusion.fullResolution = false;
                        ambientOcclusion.sampleCount = 1;
                        ambientOcclusion.rayLength = 3f;
                        ambientOcclusion.denoise = true;
                        ambientOcclusion.denoiserRadius = 0.5f;
                        break;

                    case RayTracingQuality.High:
                        ambientOcclusion.fullResolution = true;
                        ambientOcclusion.sampleCount = 2;
                        ambientOcclusion.rayLength = 5f;
                        ambientOcclusion.denoise = true;
                        ambientOcclusion.denoiserRadius = 0.75f;
                        break;

                    case RayTracingQuality.Ultra:
                        ambientOcclusion.fullResolution = true;
                        ambientOcclusion.sampleCount = 4;
                        ambientOcclusion.rayLength = 8f;
                        ambientOcclusion.denoise = true;
                        ambientOcclusion.denoiserRadius = 1f;
                        break;
                }
            }
        }

        // Note: Contact Shadows has no ray-tracing option in HDRP - it's a
        // screen-space-only effect regardless of RT settings elsewhere.

        // =========================================================
        // RECURSIVE RENDERING
        // Only worth the cost at Ultra - handles glass/transparents that
        // SSR/RTR can't do properly (proper refraction, nested reflections).
        // =========================================================
        if (recursiveRendering != null)
        {
            recursiveRendering.active = true;
            recursiveRendering.enable.value = quality == RayTracingQuality.Ultra;
        }

        // =========================================================
        // RAY-TRACED SHADOWS ON LIGHTS
        // =========================================================
        switch (quality)
        {
            case RayTracingQuality.Low:
                SetLightRayTracing(true, 1, false);
                break;

            case RayTracingQuality.Medium:
                SetLightRayTracing(true, 2, true);
                break;

            case RayTracingQuality.High:
                SetLightRayTracing(true, 4, true);
                break;

            case RayTracingQuality.Ultra:
                SetLightRayTracing(true, 8, true);
                break;
        }
    }

    /// <summary>
    /// Matches the OnFrameRateChanged pattern - hook this to a dropdown's
    /// OnValueChanged the same way.
    /// </summary>
    public void OnRayTracingQualityChanged(int index)
    {
        switch (index)
        {
            case 0: SetRayTracingQuality(RayTracingQuality.Off); break;
            case 1: SetRayTracingQuality(RayTracingQuality.Low); break;
            case 2: SetRayTracingQuality(RayTracingQuality.Medium); break;
            case 3: SetRayTracingQuality(RayTracingQuality.High); break;
            case 4: SetRayTracingQuality(RayTracingQuality.Ultra); break;
        }
    }

    private void SetLightRayTracing(bool enabled, int samples, bool filtering)
    {
        if (lights == null)
            return;

        foreach (HDAdditionalLightData lightData in lights)
        {
            if (lightData == null)
                continue;

            lightData.useRayTracedShadows = enabled;

            if (enabled)
            {
                lightData.numRayTracingSamples = samples;
                lightData.filterTracedShadow = filtering;
            }
        }
    }

    #endregion





    #region dlss
    public void EnableDLSS(DLSSSetQuality? dLSSSet) 
    {
        if (dLSSSet == null) return;
        currentDlssSetting = dLSSSet;
        if (hdCameraData == null) return;
        if (dLSSSet == DLSSSetQuality.Off)
        {
            hdCameraData.allowDeepLearningSuperSampling = false;
        }
        else
        {
            hdCameraData.customRenderingSettings = true;
            hdCameraData.allowDeepLearningSuperSampling = true;

            if (dLSSSet == DLSSSetQuality.Low)
                hdCameraData.deepLearningSuperSamplingQuality = (uint)DLSSQuality.UltraPerformance;

            if (dLSSSet == DLSSSetQuality.Balanced)
                hdCameraData.deepLearningSuperSamplingQuality = (uint)DLSSQuality.MaximumPerformance;

            if (dLSSSet == DLSSSetQuality.Medium)
                hdCameraData.deepLearningSuperSamplingQuality = (uint)DLSSQuality.Balanced;

            if (dLSSSet == DLSSSetQuality.High)
                hdCameraData.deepLearningSuperSamplingQuality = (uint)DLSSQuality.MaximumQuality;

            if (dLSSSet == DLSSSetQuality.Ultra)
                hdCameraData.deepLearningSuperSamplingQuality = (uint)DLSSQuality.DLAA;








        }
    }
    #endregion


    public void SetVSync(bool b) 
    {
    QualitySettings.vSyncCount = b ? 1 : 0;
    
    }
}
