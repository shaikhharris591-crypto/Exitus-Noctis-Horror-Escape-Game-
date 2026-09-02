using UnityEngine;
public interface IItemFunctionality
{
    void Activate();
    void Deactivate();
}
public class FlashLightController : MonoBehaviour, IItemFunctionality
{

    public GameObject torchLight;
    bool b;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(PlayerMovement.InputKeys.functionKey))
        {
            ToggleFlashLight(b);
            b = !b;
        }
    }


    public void Activate() 
    {
        enabled = true;
    }
    public void Deactivate() 
    {
        enabled = false;
    }

    void ToggleFlashLight(bool b)
    {
        torchLight.SetActive(b);
    }
   
}
