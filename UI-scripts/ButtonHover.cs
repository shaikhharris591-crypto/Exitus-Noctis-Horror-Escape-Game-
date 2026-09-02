using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Selectable up, down;
    public Image highlight;

    AudioSource clickSound;
    AudioSource hoverSound;
    AudioSource submitSound;


    private Button button;
    private Selectable selectable;
    private bool mouseHovering;

    public bool isConfirmButton;

    private void Awake()
    {
        button = GetComponent<Button>();
        selectable = GetComponent<Selectable>();
        submitSound = UISoundManager.Instance.buttonSubmitSound;
        clickSound = UISoundManager.Instance.buttonClickSound;
        hoverSound = UISoundManager.Instance.buttonHoverSound;
        if (highlight == null)
            highlight = GetComponent<Image>();

       

        // Only Buttons have onClick — Sliders don't, so guard against null.
        if (button != null)
            button.onClick.AddListener(PlayClickSound);

       
    }

    void Start()
    {
        // Only auto-fill up/down if they weren't already assigned in the Inspector.
        if (up != null && down != null)
            return;

        Selectable[] all = FindObjectsOfType<Selectable>();

        float upDist = Mathf.Infinity;
        float downDist = Mathf.Infinity;

        foreach (Selectable s in all)
        {
            if (s == selectable)
                continue;

            float dy = s.transform.position.y - transform.position.y;

            // Above me
            if (up == null && dy > 0 && dy < upDist)
            {
                upDist = dy;
                up = s;
            }

            // Below me
            if (down == null && -dy > 0 && -dy < downDist)
            {
                downDist = -dy;
                down = s;
            }
        }

        // Keep the Slider's built-in navigation in sync if we just auto-filled these.
       
    }

    private void Update()
    {
        // Highlight if mouse is over us OR we're the keyboard-selected item.
        bool selected = UINavigation.CurrentSelected == selectable;

        highlight.enabled = mouseHovering || selected;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseHovering = true;

        // Mouse automatically becomes the selected item.
        UINavigation.CurrentSelected = selectable;

       
        if(hoverSound!=null)
        hoverSound.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseHovering = false;
    }

    public void PlayClickSound()
    {
        /*  if (!isConfirmButton)
          {
              if (clickSound != null)
                  clickSound.Play();
          }
          else 
          {
          if(submitSound!=null)
                  submitSound.Play();
          }
        */

        if(clickSound!=null)clickSound.Play();
    }
}