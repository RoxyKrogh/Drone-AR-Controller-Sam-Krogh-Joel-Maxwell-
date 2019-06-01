﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class DroneBridge : MonoBehaviour
{
    // Joystick Buttons
    //private string[] buttons = new string[] { "A", "B", "X", "Y", "R1", "R2", "L1", "L2", "L3", "R3", "START", "BACK" };
    //private float maxYaw, maxThrottle, maxPitch, maxRoll;
    private static bool controlEnabled = false;

    private bool frameReady, phoneLocReady, droneLocReady, droneAttReady;
    private Location phoneLoc, droneLoc, droneAtt;
    private Texture2D videoFeedOut;
    private DroneView droneView;

    [SerializeField]
    private Camera phoneView; // object that is tracked to the phone's location
    public Material videoFeedDisplay;

    public class Location
    {
        public Location(params double[] xyz)
        {
            this.xyz = xyz;
        }
        private double[] xyz;
        public double X { get { return xyz[0]; } set { xyz[0] = value; } }
        public double Y { get { return xyz[1]; } set { xyz[1] = value; } }
        public double Z { get { return xyz[2]; } set { xyz[2] = value; } }
        public double this[int i]
        {
            get { return xyz[i]; }
            set { xyz[i] = value; }
        }
        public override string ToString()
        {
            return xyz == null ? "Loc(null)" : "Loc("+X+","+Y+","+Z+")";
        }

        /// <summary>
        /// Convert latitude/longitude to meters
        /// </summary>
        /// <returns>Vector3(X,Z,Y)</returns>
        public Vector3 Coordinate2Meters()
        {
            float r = 40007860f; // circumference of Earth in meters
            float latScale = (r / 2) / 180;  // latitude from 0 (North pole) to 180 (South pole)
            float longiScale = r / 360; // longitude from 0 to 360
            float lat =(float) X * latScale;
            float longi = (float)Y * longiScale;
            return new Vector3(lat,(float)Z,longi);
        }

        public Quaternion ToRotation()
        {
            return Quaternion.Euler((float)X,(float)Y,(float)Z);
        }
        
        public static Location operator +(Location loc1, Location loc2)
        {
            return new Location(loc1.X + loc2.X, loc1.Y + loc2.Y, loc1.Z + loc2.Z);
        }

        public static Location operator -(Location loc1, Location loc2)
        {
            return new Location(loc1.X - loc2.X, loc1.Y - loc2.Y, loc1.Z - loc2.Z);
        }
    }

    /// <summary>
    /// Get the instance of DroneBridge running in the active scene.
    /// </summary>
    /// <returns>A reference to a DroneBridge.</returns>
    public static DroneBridge GetReference()
    {
        return FindObjectOfType<DroneBridge>();
    }

    private static bool TooManyBridges { get { return FindObjectsOfType<DroneBridge>().Length > 1; } } // no more than 1 DroneBridge per scene
    private static DroneBridge WorstBridge
    {
        get
        {
            DroneBridge[] bridges = FindObjectsOfType<DroneBridge>();
            int ping = bridges.Length;
            foreach (DroneBridge b in bridges)
            {
                if (b.videoFeedDisplay == null || b.phoneView == null || ping == 0) 
                    return b; // return least defined DroneBridge, or last DroneBridge
                ping--;
            }
            return null; // no bridges
        }
    }

    // UNITY FUNCTIONS:
    
    void OnValidate()
    {
        Debug.Assert(!TooManyBridges, "Scene may only contain 1 instance of DroneBridge",this);
    }

    private void Start()
    {
        videoFeedOut = new Texture2D(960, 720);
        droneView = FindObjectOfType<DroneView>();
    }

    private void Update()
    {
        if (frameReady && videoFeedOut != null)
            GetVideoFrame(videoFeedDisplay);
        if (phoneLocReady) // phone location ready
            phoneLoc = new Location(CallDroneFunc<double[]>("getPhoneLocation"));
        if (droneLocReady)
            droneLoc = new Location(CallDroneFunc<double[]>("getDroneLocation"));
        if (droneAttReady)
            droneAtt = new Location(CallDroneFunc<double[]>("getDroneAttitude"));
    }

    private void OnEnable()
    {
        VirtualControlEnabled = controlEnabled;
        CallVoidDroneFunc("registerUnityBridge", gameObject.name);
    }

    // C# TO JAVA:

    public bool IsConnected
    {
        get { return droneLoc != null; } // TODO: replace with actual connection status query
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

    public Location DroneAttitude
    {
        get
        {
            return droneAtt ?? new Location(0, 0, 0);
        }
    }

    public Location DroneLocation
    {
        get
        {
            return droneLoc ?? (PhoneLocation + new Location(0, 0, 10));
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

    public Location PhoneLocation
    {
        get
        {
            return (phoneLoc ?? new Location(0, 0, 0)); // use 0 location if not receiving location from java
        }
    }

    public Transform PhoneView
    {
        get { return phoneView.transform; }
    }

    public string ProductText
    {
        get { return CallDroneFunc<string>("getProductText"); }
    }

    public string State
    {
        get { return CallDroneFunc<string>("getState"); }
    }

    public bool GetVideoFrame(Material surface)
    {
        byte[] frame = CallDroneFunc<byte[]>("getVideoFrame");
        if (frame != null)
        {
            videoFeedOut.LoadImage(frame);
            videoFeedOut.Apply();
            if (surface != null)
            {
                surface.mainTexture = videoFeedOut;
            }
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
        droneView.CameraPitch = pitchValue;
    }

    public float Yaw
    {
        set
        {
            CallVoidDroneFunc("setYaw", value);
            if (!IsConnected)
                droneView.Yaw = value;
        }
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
#else
                obj_Activity.Call(funcName);
#endif
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
#else
                obj_Activity.Call(funcName, args);
#endif
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
                return default(T);
#else
                return obj_Activity.Call<T>(funcName);
#endif
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
                return default(T);
#else
                return obj_Activity.Call<T>(funcName, args);
#endif
            }
        }
    }

    AndroidJavaObject getAppContext()
    {
        AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        return obj_Activity;
    }

    // FROM JAVA TO C#:

    public void SetReady(string tags)
    {
        foreach (string tag in tags.Split(' '))
        {
            switch (tag)
            {
                case "VIDEO":
                    // frame ready
                    frameReady = true;
                    break;
                case "PHONE_LOC":
                    // phone location ready
                    phoneLocReady = true; 
                    break;
                case "DRONE_LOC":
                    // drone location ready
                    droneLocReady = true;
                    break;
                case "DRONE_ATT":
                    // drone attitude ready
                    droneAttReady = true;
                    break;
            }
        }
    }

}
