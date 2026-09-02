using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeUI : MonoBehaviour
{
    public TMP_Text codeTextOnCanvas, codeTextOffCanvas;
    public Button[] buttons;
    public Button resetButton, exitButton, submitButton;
    string temp_code, code;

    public GameObject codeCanvas;

    public static CodeUI Instance;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var button in buttons)
        {
            var t = button.GetComponentInChildren<TMP_Text>();
            if (t != null)
                button.onClick.AddListener(
                    () =>
                    {
                        temp_code += t.text;
                        UpdateCodeText();
                        if(CodeManager.Instance != null)
                        CodeManager.Instance.PlayCodeButtonClickSound();
                    });
            else
                Debug.LogWarning("Text for Button  " + button.name + "is NULL");
        }

        resetButton.onClick.AddListener(ResetCode);
        exitButton.onClick.AddListener(OnExitCodeCanvas);
        submitButton.onClick.AddListener(OnCodeSubmitted);
    }

    private void OnCodeSubmitted()
    {
        CodeManager.evaluateCode = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) OnExitCodeCanvas();
        if (Input.GetKeyDown(KeyCode.Return)) OnCodeSubmitted();
    }

    public void OnExitCodeCanvas()
    {

        code = "";
        temp_code = "";
        UpdateCodeText();
        enabled = false;
    }

    public string GetCode() { return code; }
    public void ResetCode()
    {
        code = "";
        UpdateCodeText();
    }
    public void SetCode()
    {
        code = temp_code;

    }

    public void UpdateCodeText()
    {
        codeTextOnCanvas.text = temp_code;
        codeTextOffCanvas.text = temp_code;
    }

    public void ShowCodeCanvas(bool b) { codeCanvas.SetActive(b); }
}
