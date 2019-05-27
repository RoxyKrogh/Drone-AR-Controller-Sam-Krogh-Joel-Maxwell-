using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneControl : MonoBehaviour
{
    // Joystick Buttons
    //private string[] buttons = new string[] { "A", "B", "X", "Y", "R1", "R2", "L1", "L2", "L3", "R3", "START", "BACK" };
    //private float maxYaw, maxThrottle, maxPitch, maxRoll;
    private static bool controlEnabled = false;

    private void Start()
    {
    }

    private void OnEnable()
    {
        VirtualControlEnabled = controlEnabled;
    }

    public void RefreshConnectionStatus()
    {
        CallVoidDroneFunc("refreshConnectionStatus");
    }

    public void FollowMeStart()
    {
        CallVoidDroneFunc("followMeStart");
    }

    public void FollowMeStop()
    {
        CallVoidDroneFunc("followMeStop");
    }

    public void ShowToast(string message )
    {
        CallVoidDroneFunc("showToast", message);
    }

    public float Throttle
    {
        set { CallVoidDroneFunc("setThrottle", value); }
    }

    public void DisableVideo()
    {
        CallVoidDroneFunc("disableVideo");
    }

    public void EnableVideo()
    {
        CallVoidDroneFunc("enableVideo");
    }

    public string ConnectionStatus
    {
        get { return CallDroneFunc<string>("getConnectionStatus"); }
    }

    public Vector3 DroneAttitude
    {
        get
        {
            var v = CallDroneFunc<double[]>("getDroneAttitude");
            return new Vector3((float)v[0], (float)v[1], (float)v[2]);
        }
    }

    public Vector3 DroneLocation
    {
        get
        {
            var v = CallDroneFunc<double[]>("getDroneLocation");
            return new Vector3((float)v[0], (float)v[1], (float)v[2]);
        }
    }

    public string FlightMode
    {
        get { return CallDroneFunc<string>("getFlightMode"); }
    }

    public string IMUState
    {
        get { return CallDroneFunc<string>("getIMUState"); }
    }

    public bool IsFlying
    {
        get { return CallDroneFunc<bool>("getIsFlying"); }
    }

    public bool IsMotorOn
    {
        get { return CallDroneFunc<bool>("getMotorsOn"); }
    }

    public Vector3 PhoneLocation
    {
        get
        {
            var v = CallDroneFunc<double[]>("getFlightMode");
            return new Vector3((float)v[0], (float)v[1], (float)v[2]);
        }
    }

    public string ProductText
    {
        get { return CallDroneFunc<string>("getProductText"); }
    }

    public string State
    {
        get { return CallDroneFunc<string>("getState"); }
    }

    public bool GetVideoFrame(Texture2D tex2d)
    {
        byte[] frame = CallDroneFunc<byte[]>("getVideoFrame");
        if (frame != null)
        {
            tex2d.LoadImage(frame);
            tex2d.Apply();
        }
        return frame != null;
    }

    public void Land()
    {
        CallVoidDroneFunc("land");
    }

    public void RefreshFlightControllerStatus()
    {
        CallVoidDroneFunc("refreshFlightControllerStatus");
    }

    public void SetGimbalRotation(float pitchValue, float rollValue)
    {
        CallVoidDroneFunc("setGimbalRotation", pitchValue, rollValue);
    }

    public float Yaw
    {
        set { CallVoidDroneFunc("setYaw", value); }
    }

    public float Pitch
    {
        set { CallVoidDroneFunc("setPitch", value); }
    }

    public float Roll
    {
        set { CallVoidDroneFunc("setRoll", value); }
    }

    public void SetupDroneConnection()
    {
        CallVoidDroneFunc("setupDroneConnection");
    }

    public void StartLocationService()
    {
        CallVoidDroneFunc("startLocationService");
    }

    public void TakeOff()
    {
        CallVoidDroneFunc("takeOff");
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

    public T CallDroneFunc<T>(string funcName)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
#if UNITY_EDITOR
                Debug.Log("Calling drone function: " + funcName + "()");
#endif
                return obj_Activity.Call<T>(funcName);
            }
        }
    }

    public T CallDroneFunc<T>(string funcName, params object[] args)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
#if UNITY_EDITOR
                string argsStr = string.Join(", ", args.Select(a => a.ToString() ?? "null").ToArray());
                Debug.Log("Calling drone function: " + funcName + "(" + argsStr + ")");
#endif
                return obj_Activity.Call<T>(funcName, args);
            }
        }
    }

}
