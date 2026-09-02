using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static List<GameObject> activeCameras = new List<GameObject>();

    public static void Register(GameObject cam)
    {
        if (!activeCameras.Contains(cam))
        {
            activeCameras.Add(cam);
        }
    }

    public static void Unregister(GameObject cam)
    {
        activeCameras.Remove(cam);
    }

 
}
