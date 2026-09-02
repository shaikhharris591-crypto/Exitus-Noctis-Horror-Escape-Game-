using UnityEngine;
using UnityEngine.UI;

public class UINavigation : MonoBehaviour
{
    public static Selectable CurrentSelected;

    public float firstRepeatDelay = 0.35f;
    public float repeatRate = 0.1f;

    float nextMoveTime;
    bool repeating;

    void Update()
    {
        if (CurrentSelected == null)
            return;

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        bool anyInput = Mathf.Abs(v) >= 0.1f || Mathf.Abs(h) >= 0.1f;

        if (!anyInput)
        {
            repeating = false;
            nextMoveTime = 0f;
            return;
        }

        if (Time.time < nextMoveTime)
            return;

        Slider slider = CurrentSelected.GetComponent<Slider>();

        if (slider != null && Mathf.Abs(h) >= 0.1f)
        {
            if (h > 0) slider.value += slider.wholeNumbers ? 1 : 0.01f;
            else if (h < 0) slider.value -= slider.wholeNumbers ? 1 : 0.01f;
        }
        else if (Mathf.Abs(v) >= 0.1f)
        {
            ButtonHover nav = CurrentSelected.GetComponent<ButtonHover>();
            if (nav != null)
            {
                if (v > 0 && nav.up != null)
                    CurrentSelected = nav.up;
                else if (v < 0 && nav.down != null)
                    CurrentSelected = nav.down;

                Debug.Log("Selected: " + CurrentSelected.name);
            }
        }

        if (!repeating)
        {
            nextMoveTime = Time.time + firstRepeatDelay;
            repeating = true;
        }
        else
        {
            nextMoveTime = Time.time + repeatRate;
        }
    }
}