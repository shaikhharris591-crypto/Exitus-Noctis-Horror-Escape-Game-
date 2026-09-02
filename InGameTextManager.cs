using System;
using TMPro;
using UnityEngine;

public class InGameTextManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float textDisableTime;
    public GameObject inGameTextCanvas;
    public TMP_Text inGameText;

    public static InGameTextManager Instance;
    void Awake()
    {
     Instance = this;

    }

    private void Start()
    {
        
        if (textDisableTime == 0) textDisableTime = 3f;
        Invoke(nameof(DisableText), textDisableTime);
    }

    public void DisableText()
    {
        if (inGameTextCanvas == null) return;
        inGameTextCanvas.SetActive(false);

    }


    public void EnableText()
    {
        if (inGameTextCanvas == null) return;
        inGameTextCanvas.SetActive(true);

    }

    public void DisplayText(string text) 
    {
    inGameText.text = text;
    }
   
    // Update is called once per frame
    void Update()
    {
        
    }
}
