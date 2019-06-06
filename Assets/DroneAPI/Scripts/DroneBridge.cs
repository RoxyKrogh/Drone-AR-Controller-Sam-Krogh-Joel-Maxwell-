using System;
using System.Collections;
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

    private bool frameReady, phoneLocReady, phoneHeadReady, droneLocReady, droneAttReady;
    private float phoneHead;
    private DroneVector phoneLoc, droneLoc, droneAtt;
    private Texture2D videoFeedOut;
    private DroneView droneView;
    private Matrix4x4 alignToNorth = Matrix4x4.identity;

    [SerializeField]
    private Camera phoneView; // object that is tracked to the phone's location
    public Material videoFeedDisplay;
    public int minimumGpsLevel = 4; // while DroneGpsLevel is below this, GPS sensor will be ignored

    /// <summary>
    /// A vector of doubles returned by some Drone api properties. 
    /// Usually there are 3 elements in the vector, either (latitude,longitude,altitude) or (pitch,roll,yaw)
    /// </summary>
    public class DroneVector
    {
        public DroneVector(params double[] xyz)
        {
            this.xyz = xyz;
        }
        private double[] xyz;
        public double Latitude { get { return xyz[0]; } set { xyz[0] = value; } }
        public double Longitude { get { return xyz[1]; } set { xyz[1] = value; } }
        public double Altitude { get { return xyz[2]; } set { xyz[2] = value; } } // phone has bearing instead of altitude
        public double Bearing { get { return xyz[2]; } set { xyz[2] = value; } } // phone has bearing instead of altitude

        public double Pitch { get { return xyz[0]; } set { xyz[0] = value; } }
        public double Roll { get { return xyz[1]; } set { xyz[1] = value; } }
        public double Yaw { get { return xyz[2]; } set { xyz[2] = value; } }
        public double this[int i]
        {
            get { return xyz[i]; }
            set { xyz[i] = value; }
        }
        public override string ToString()
        {
            return xyz == null ? "Loc(null)" : "Loc("+string.Join(",",xyz)+")";
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
            float lat = (float)Latitude * latScale;
            float longi = (float)Longitude * longiScale;
            float alti = (float)Altitude;
            return new Vector3(lat,alti,-longi);
        }

        public Quaternion ToRotation()
        {
            return Quaternion.Euler((float)Pitch,(float)Yaw,(float)Roll); 
        }
        
        public static DroneVector operator +(DroneVector loc1, DroneVector loc2)
        {
            return new DroneVector(loc1.Latitude + loc2.Latitude, loc1.Longitude + loc2.Longitude, loc1.Altitude + loc2.Altitude);
        }

        public static DroneVector operator -(DroneVector loc1, DroneVector loc2)
        {
            return new DroneVector(loc1.Latitude - loc2.Latitude, loc1.Longitude - loc2.Longitude, loc1.Altitude - loc2.Altitude);
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
        {
            GetVideoFrame(videoFeedDisplay);
            frameReady = false;
        }
        if (phoneLocReady) // phone location ready
        {
            phoneLoc = new DroneVector(CallDroneFunc<double[]>("getPhoneLocation"));
            phoneLocReady = false;
        }
        if (droneLocReady)
        {
            if (DroneGpsLevel < minimumGpsLevel)
                droneLoc = null;
            else
                droneLoc = new DroneVector(CallDroneFunc<double[]>("getDroneLocation"));
            if (droneView != null && !droneView.IsCalibrated)
                droneView.CalibrateGPSCenter();
            droneLocReady = false;
        }
        if (droneAttReady)
        {
            droneAtt = new DroneVector(CallDroneFunc<double[]>("getDroneAttitude"));
            droneAttReady = false;
        }
        if (phoneHeadReady)
        {
            phoneHead = CallDroneFunc<float>("getPhoneHeading");
            phoneHeadReady = false;
        }
        RefreshConnectionStatus();
        RefreshFlightControllerStatus();
    }

    public Vector3 MakeRelativeToNorth(Vector3 vector)
    {
        return alignToNorth.MultiplyVector(vector);
    }

    public void CalibrateWorldNorth(Transform world)
    {
        Vector3 pfwd = Vector3.ProjectOnPlane(PhoneView.forward, Vector3.up).normalized;
        float pvbear = Mathf.Rad2Deg * Mathf.Atan2(pfwd.z, pfwd.x); // world space (in-game) phone object bearing
        double pbear =  - PhoneHeading; // real-world (sensor) phone bearing (direction / 0deg = north)
        Vector3 wfwd = world.forward; // world forward vector
        float wbear = Mathf.Rad2Deg * Mathf.Atan2(wfwd.z, wfwd.x); // world transform bearing
        float relbear = pvbear - (float)pbear - wbear;
        alignToNorth = Matrix4x4.Rotate(Quaternion.AngleAxis(relbear, Vector3.up));
        ShowToast("Adjusting north by " + relbear + " deg");
    }

    private void OnEnable()
    {
        CallVoidDroneFunc("registerUnityBridge", gameObject.name); // tell Java which Unity object to send function calls to
        StartLocationService(); // start phone gps and orientation sensor listener
        EnableVideo();
        VirtualControlEnabled = controlEnabled;
        DroneCameraPitch = 90f;
    }

    // C# TO JAVA:

    public int DroneGpsLevel { get { return CallDroneFunc<int>("getDroneGpsLevel"); } }

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

    public DroneVector DroneAttitude
    {
        get
        {
            return droneAtt ?? new DroneVector(0, 0, 0);
        }
    }

    public DroneVector DroneLocation
    {
        get
        {
            return (droneLoc ?? (PhoneLocation + new DroneVector(0, 0, 10)));
        }
    }

    public string FlightMode
    {
        get { return CallDroneFunc<string>("getFlightMode"); }
    }

    public string IMUState
    {
        get { return CallDroneFunc<string>("getIMUstate"); }
    }

    public bool IsFlying
    {
        get { return CallDroneFunc<bool>("getIsFlying"); }
    }

    public bool IsMotorOn
    {
        get { return CallDroneFunc<bool>("getMotorsOn"); }
    }

    public DroneVector PhoneLocation
    {
        get
        {
            return (phoneLoc ?? new DroneVector(0, 0, 0)); // use 0 location if not receiving location from java
        }
    }

    public float PhoneHeading
    {
        get { return phoneHead; }
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

    public void SetDroneCameraGimbal(float pitchValue, float yawValue)
    {
        CallVoidDroneFunc("setGimbalRotation", pitchValue, yawValue);
        droneView.CameraPitch = pitchValue;
        droneView.CameraYaw = yawValue;
    }

    public float DroneCameraPitch
    {
        get { return droneView != null ? droneView.CameraPitch : float.NaN; }
        set
        {
            SetDroneCameraGimbal(value, droneView.CameraYaw);
        }
    }

    public float DroneCameraYaw
    {
        get { return droneView != null ? droneView.CameraYaw : float.NaN; }
        set
        {
            SetDroneCameraGimbal(droneView.CameraPitch, value);
        }
    }

    /// <summary>
    /// The yaw of the drone. 
    /// Caution: this property is handled by the Java api, and changes to it's value may not be immediately visible.
    /// </summary>
    public float Yaw
    {
        set
        {
            CallVoidDroneFunc("setYaw", value);
            if (!IsConnected)
                droneView.Yaw = value;
        }
        get { return (float)DroneAttitude.Yaw; }
    }

    /// <summary>
    /// The pitch of the drone. 
    /// Caution: this property is handled by the Java api, and changes to it's value may not be immediately visible.
    /// </summary>
    public float Pitch
    {
        set { CallVoidDroneFunc("setPitch", value); }
        get { return (float)DroneAttitude.Pitch; }
    }

    /// <summary>
    /// The roll of the drone. 
    /// Caution: this property is handled by the Java api, and changes to it's value may not be immediately visible.
    /// </summary>
    public float Roll
    {
        set { CallVoidDroneFunc("setRoll", value); }
        get { return (float)DroneAttitude.Roll; }
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
    public bool CallVoidDroneFunc(string funcName)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                try
                {
#if UNITY_EDITOR
                    Debug.Log("Calling drone function: " + funcName + "()");
                    return false;
#else
                    obj_Activity.Call(funcName);
                    return true;
#endif

                }
                catch (Exception partyPooper) // catch exception from Java
                {
                    Debug.LogException(partyPooper);
                    return false;
                }
            }
        }
    }

    // helper function to reduce code length
    public bool CallVoidDroneFunc(string funcName, params object[] args)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                try
                {
#if UNITY_EDITOR
                    string argsStr = string.Join(", ", args.Select(a => a.ToString() ?? "null").ToArray());
                    Debug.Log("Calling drone function: " + funcName + "(" + argsStr + ")");
                    return false;
#else
                    obj_Activity.Call(funcName, args);
                    return true;
#endif
                }
                catch (Exception partyPooper) // catch exception from Java
                {
                    Debug.LogException(partyPooper);
                    return false;
                }
            }
        }
    }

    public T CallDroneFunc<T>(string funcName)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                try
                {
#if UNITY_EDITOR
                    Debug.Log("Calling drone function: " + funcName + "()");
                    return default(T);
#else
                    return obj_Activity.Call<T>(funcName);
#endif
                }
                catch (Exception partyPooper) // catch exception from Java
                {
                    Debug.LogException(partyPooper);
                    return default(T);
                }
            }
        }
    }

    public T CallDroneFunc<T>(string funcName, params object[] args)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                try
                {
#if UNITY_EDITOR
                string argsStr = string.Join(", ", args.Select(a => a.ToString() ?? "null").ToArray());
                Debug.Log("Calling drone function: " + funcName + "(" + argsStr + ")");
                return default(T);
#else
                return obj_Activity.Call<T>(funcName, args);
#endif
                }
                catch (Exception partyPooper) // catch exception from Java
                {
                    Debug.LogException(partyPooper);
                    return default(T);
                }
            }
        }
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
                case "PHONE_HEAD":
                    phoneHeadReady = true;
                    break;
            }
        }
    }

}
