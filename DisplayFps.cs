using TMPro;
using UnityEngine;

public class DisplayFps : MonoBehaviour
{
    public TMP_Text fpsText;
   public static bool canShowFps=false;
    int fps;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canShowFps = UI.Instance.fpsToggle.isOn;
        if (canShowFps) fpsText.gameObject.SetActive(true);
    }

    // Update is called once per frame
   

    float deltaTime;

    void Update()
    {
        if (!canShowFps) return;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        

        fps = GetCurrentFPS();

        ToggleFps();

    }

    public int GetCurrentFPS()
    {
        return Mathf.RoundToInt(1f / deltaTime);
    }

    public void ToggleFps() 
    {

        if (fpsText == null) return;

        if (canShowFps)
        {
            if (!fpsText.gameObject.activeSelf) 
                fpsText.gameObject.SetActive(true);
           
            fpsText.text = fps + " FPS";
        }
        else
        { 
            if (fpsText.gameObject.activeSelf)
                fpsText.gameObject.SetActive(false); 
        }
    }
}
