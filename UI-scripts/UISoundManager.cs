using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AudioSource buttonClickSound;
    public AudioSource buttonHoverSound;
    public AudioSource buttonSubmitSound;
    public static UISoundManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
