using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneControl : MonoBehaviour
{
    // Joystick Buttons
    //private string[] buttons = new string[] { "A", "B", "X", "Y", "R1", "R2", "L1", "L2", "L3", "R3", "START", "BACK" };
    //private float maxYaw, maxThrottle, maxPitch, maxRoll;
    private bool controlEnabled = false;

    private void Start()
    {
    }

    private void OnEnable()
    {
        VirtualControlEnabled = controlEnabled;
    }

    public void TakeOff()
    {
        CallVoidDroneFunc("takeOff");
    }

    public void Land()
    {
        CallVoidDroneFunc("land");
    }

    public void StartFollowMe()
    {
        CallVoidDroneFunc("followMeStart");
    }

    public void StopFollowMe()
    {
        CallVoidDroneFunc("followMeStop");
    }

    public void StartLocation()
    {
        CallVoidDroneFunc("startLocationService");
    }

    public bool VirtualControlEnabled
    {
        get
        {
            return controlEnabled;
        }

        set
        {
            CallVoidDroneFunc("setVirtualControlActive", value);
            controlEnabled = value;
        }
    }

    public void EnableVirtualSticks()
    {
        VirtualControlEnabled = true;
    }

    public void DisableVirtualSticks()
    {
        VirtualControlEnabled = false;
    }

    // helper function to reduce code length
    public void CallVoidDroneFunc(string funcName)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
#if UNITY_EDITOR
                Debug.Log("Calling drone function: " + funcName + "()");
#endif
                obj_Activity.Call(funcName);
            }
        }
    }

    // helper function to reduce code length
    public void CallVoidDroneFunc(string funcName, params object[] args)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
#if UNITY_EDITOR
                string argsStr = string.Join(", ", args.Select(a => a.ToString() ?? "null").ToArray());
                Debug.Log("Calling drone function: " + funcName + "(" + argsStr + ")");
#endif
                obj_Activity.Call(funcName, args);
            }
        }
    }

}
