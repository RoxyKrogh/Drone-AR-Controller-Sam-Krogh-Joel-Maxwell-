using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityStandardAssets.CrossPlatformInput;


public class cameraPIP : MonoBehaviour
{
    public Button startBtn;
    public Button Drone_video_btn;
    public Button Phone_video_btn;
    //public Button showToast;
    public Button startDrone;
    public Button takeOffBtn;
    public Button landBtn;
    //public Button enableCam;
    public Button enableVirtualControl;
    //public Button yawRight;
    //public Button yawLeft;
    //public Button droneUp;
    //public Button droneDown;
    //public Button droneAhead;
    //public Button droneBack;
    //public Button droneLeft;
    //public Button droneRight;
    public Button stopControl;
    public Button locationButton;
    public Button swapDisplays;
    public RenderTexture droneView;
    public RenderTexture phoneView;

    // flight controll values
    public Button yawPlus;
    public Button yawMinus;
    public Text yawValue;
    public float yaw;

    public Button rollPlus;
    public Button rollMinus;
    public Text rollValue;
    public float roll;

    public Button pitchPlus;
    public Button pitchMinus;
    public Text pitchValue;
    public float pitch;

    public Button throttlePlus;
    public Button throttleMinus;
    public Text throttleValue;
    public float throttle;

    public Button followMeButton;
    public Button stopFollowButton;
    public Text locationText;
    public Text distanceText;

    

    //public Button startYawBtn;
    //public Button stopYawBtn;
    public GameObject left_display;
    public GameObject right_display;
    public GameObject droneDisplay;
    public GameObject phoneDisplay;
    public GameObject phoneHolder;
    public GameObject droneHolder;
    //public GameObject left_pip_display;
    //public GameObject right_pip_display;
    public Text connection_status;
    public Text connected_hardware;
    public Text flight_controller_status;
    public Text fps_display;
    public Text buttonText;
    private float lastFrame;
    private bool update_status_flag;
    private bool update_display_flag;
    private bool frame_ready_flag;
    private bool drone_camera_flag;
    private bool phone_camera_flag;
    private bool calc_distance_flag;
    private bool droneRender;
    private Double[] baseLoc;
    private Double[] phoneLoc;
    private Double[] droneLoc;

    // Drone Camera
    private Texture2D tex2d;

    // phoneCam Camera
    private WebCamTexture webTex;
    public Texture noVideo;

    // Joystick Buttons
    private string[] buttons = new string[] { "A", "B", "X", "Y", "R1", "R2", "L1", "L2", "L3", "R3", "START", "BACK" };
    private float maxYaw, maxThrottle, maxPitch, maxRoll;
    private bool controlEnabled;

    // Display positions
    private Vector3 defaultPos;
    private Vector3 defaultScale;
    private Vector3 viewcale;
    private Vector3 leftView;
    private Vector3 rightView;

    // colors
    private ColorBlock on;
    private ColorBlock off;

    
    public Button testButton;
    // Use this for initialization
    void Start()
    {


        testButton.onClick.AddListener(setStartPos);
        update_display_flag = true;
        update_status_flag = false;
        frame_ready_flag = false;
        drone_camera_flag = false;
        phone_camera_flag = false;
        calc_distance_flag = false;
        defaultPos = new Vector3(-236, -283, 0);
        defaultScale = new Vector3(926, 768, 1);
        viewcale = new Vector3(1270, 1050, 1);
        leftView = new Vector3(-640, 0, 0);
        rightView = new Vector3(640, 0, 0);
        startBtn.onClick.AddListener(update_status_toggle);
        Drone_video_btn.onClick.AddListener(toggle_drone_cam);
        Phone_video_btn.onClick.AddListener(toggle_phone_cam);
        startDrone.onClick.AddListener(startDroneFunc);
        //showToast.onClick.AddListener(toast);
        takeOffBtn.onClick.AddListener(takeOff);
        landBtn.onClick.AddListener(land);
        controlEnabled = false;
        enableVirtualControl.onClick.AddListener(enableVirtualSticks);
        //yawRight.onClick.AddListener(StartYaw);
        stopControl.onClick.AddListener(disableVirtualSticks);
        followMeButton.onClick.AddListener(startFollowMe);
        stopFollowButton.onClick.AddListener(stopFollowMe);

        swapDisplays.onClick.AddListener(swap);
        droneRender = false;
        // flight controll values
        maxYaw = 90f;
        maxThrottle = 2;
        maxPitch = 2;
        maxRoll = 2;
        yaw = 0;
        pitch = 0;
        roll = 0;
        throttle = 0;
        yawPlus.onClick.AddListener(yawUp);
        yawMinus.onClick.AddListener(yawDown);
        rollPlus.onClick.AddListener(rollUp);
        rollMinus.onClick.AddListener(rollDown);
        pitchPlus.onClick.AddListener(pitchUp);
        pitchMinus.onClick.AddListener(pitchDown);
        throttlePlus.onClick.AddListener(throttleUp);
        throttleMinus.onClick.AddListener(throttleDown);
        locationButton.onClick.AddListener(startLocation);
        //#######################
        lastFrame = Time.time;
        on = Drone_video_btn.colors;
        off = startBtn.colors;
        off.normalColor = new Color(0.8f, 0f, 0f, 1f);
        off.highlightedColor = new Color(0.8f, 0f, 0f, 1f);
        off.pressedColor = new Color(0.8f, 0f, 0f, 1f);
        tex2d = new Texture2D(960, 720);
        webTex = new WebCamTexture(500, 500, 5);
        webTex.Stop(); // just to be safe
    }

    void setStartPos()
    {
        phoneLoc = null;
    }

    void update_status_toggle()
    {
        if (update_status_flag)
        {
            update_status_flag = false;
            startBtn.colors = on;
            startBtn.GetComponentInChildren<Text>().text = "START";
        }
        else
        {
            update_status_flag = true;
            startBtn.colors = off;
            startBtn.GetComponentInChildren<Text>().text = "STOP";
        }
    }

    void toggle_drone_cam()
    {
        if (drone_camera_flag)
        {
            drone_camera_flag = false;
            Drone_video_btn.colors = on;
        }
        else
        {
            drone_camera_flag = true;
            Drone_video_btn.colors = off;
        }
    }

    void toggle_phone_cam()
    {
        if (phone_camera_flag)
        {
            phone_camera_flag = false;
            Phone_video_btn.colors = on;
        }
        else
        {
            phone_camera_flag = true;
            Phone_video_btn.colors = off;
        }
    }

    void set_frame_ready(string message)
    {
        Debug.Log("FRAME READY");
        if (message == "true")
        {
            frame_ready_flag = true;
        }
    }

    void swap()
    {
        droneRender = !droneRender;

        if (droneRender)
        {
            droneDisplay.GetComponent<Renderer>().enabled = true;
            left_display.GetComponent<Renderer>().material.mainTexture = droneView;
            right_display.GetComponent<Renderer>().material.mainTexture = droneView;
            phoneDisplay.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            phoneDisplay.GetComponent<Renderer>().enabled = true;
            left_display.GetComponent<Renderer>().material.mainTexture = phoneView;
            right_display.GetComponent<Renderer>().material.mainTexture = phoneView;
            droneDisplay.GetComponent<Renderer>().enabled = false;
        }
    }

    void toast()
    {
        callVoidDroneFunc("showToast", new object[] { "Button Clicked in Unity" }); 
    }

    void updateText()
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                obj_Activity.Call("refreshSDKRelativeUI");
                obj_Activity.Call("flightControllerStatus");
                connection_status.text = obj_Activity.Call<string>("getConnectionStatus");
                connected_hardware.text = obj_Activity.Call<string>("getProductText");
                flight_controller_status.text = obj_Activity.Call<string>("getState");
            }
        }
    }

    void startDroneFunc()
    {
         callVoidDroneFunc("setupDroneConnection");
    }

    void updateDisplay()
    {
        if (drone_camera_flag )
        {
            Debug.Log("Drone Camera");
            if (frame_ready_flag)
            {
                Debug.Log("Frame Ready");
                frame_ready_flag = false;
                using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        byte[] t = obj_Activity.Call<byte[]>("getVid");
                        if (null != t)
                        {
                            tex2d.LoadImage(t);
                            tex2d.Apply();
                            droneDisplay.GetComponent<Renderer>().material.mainTexture = tex2d;
                            //right_display.GetComponent<Renderer>().material.mainTexture = tex2d;
                            fps_display.text = "" + Math.Round(1 / (Time.time - lastFrame), 2) + "fps";
                            lastFrame = Time.time;
                        }

                    }
                }
            }
        }
        else
        {
            droneDisplay.GetComponent<Renderer>().material.mainTexture = noVideo;
        }
        if ( phone_camera_flag )
        {
            if (webTex.isPlaying == false) { webTex.Play(); }
            phoneDisplay.GetComponent<Renderer>().material.mainTexture = webTex;
            //right_display.GetComponent<Renderer>().material.mainTexture = webTex;
        }
        else
        {
            if(null != webTex)
            {
                webTex.Stop();
            }
            phoneDisplay.GetComponent<Renderer>().material.mainTexture = noVideo;
            //right_display.GetComponent<Renderer>().material.mainTexture = noVideo;
        }
    }


    void takeOff()
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                obj_Activity.Call("takeOff");
            }
        }
    }

    void land()
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                obj_Activity.Call("land");
            }
        }
    }

    void startFollowMe()
    {
        callVoidDroneFunc("followMeStart");
    }

    void stopFollowMe()
    {
        callVoidDroneFunc("followMeStop");
    }

    void startLocation()
    {
        callVoidDroneFunc("startLocationService");
    }

    void enableVirtualSticks()
    {
        controlEnabled = true;
        callVoidDroneFunc("setVirtualControlActive", new object[] { true });
    }
    void disableVirtualSticks()
    {
        controlEnabled = false;
        callVoidDroneFunc("setVirtualControlActive", new object[] { false });
    }

    // helper function to reduce code length
    void callVoidDroneFunc(String funcName)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                obj_Activity.Call(funcName);
            }
        }
    }

    // helper function to reduce code length
    void callVoidDroneFunc(String funcName, object[] args)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                obj_Activity.Call(funcName, args);
            }
        }
    }

    void yawUp()
    {
        yaw += 10;
        yawValue.text = yaw.ToString();
        callVoidDroneFunc("setYaw", new object[] { yaw });
    }
    void yawDown()
    {
        yaw -= 10;
        yawValue.text = yaw.ToString();
        callVoidDroneFunc("setYaw", new object[] { yaw });
    }


    void rollUp()
    {
        roll += 1;
        rollValue.text = roll.ToString();
        callVoidDroneFunc("setRoll", new object[] { roll });
    }
    void rollDown()
    {
        roll -= 1;
        rollValue.text = roll.ToString();
        callVoidDroneFunc("setRoll", new object[] { roll });
    }

    void pitchUp()
    {
        pitch += 1;
        pitchValue.text = pitch.ToString();
        callVoidDroneFunc("setPitch", new object[] { pitch });
    }
    void pitchDown()
    {
        pitch -= 1;
        pitchValue.text = pitch.ToString();
        callVoidDroneFunc("setPitch", new object[] { pitch });
    }

    void throttleUp()
    {
        throttle += 1;
        throttleValue.text = throttle.ToString();
        callVoidDroneFunc("setThrottle", new object[] { throttle });
    }
    void throttleDown()
    {
        throttle -= 1;
        throttleValue.text = throttle.ToString();
        callVoidDroneFunc("setThrottle", new object[] { throttle });
    }


    void controllerInput()
    {
        buttonText.text = "";
        foreach (string key in buttons)
        {
            if (Input.GetButton(key))
            {
                buttonText.text += key + " ";
            }

        }
        float LJV = Input.GetAxis("LJV");
        float LJH = Input.GetAxis("LJH");
        float RJV = Input.GetAxis("RJV");
        float RJH = Input.GetAxis("RJH");
        float DPV = Input.GetAxis("DPV");
        float DPH = Input.GetAxis("DPH");
        buttonText.text += Mathf.Abs(LJV) >= 0.1f ? "LJV: "+LJV.ToString() : "";
        buttonText.text += Mathf.Abs(LJH) >= 0.1f ? "LJH: " + LJH.ToString() : "";
        buttonText.text += Mathf.Abs(RJV) >= 0.1f ? "RJV: " + RJV.ToString() : "";
        buttonText.text += Mathf.Abs(RJH) >= 0.1f ? "RJH: " + RJH.ToString() : "";
        buttonText.text += Mathf.Abs(DPH) >= 0.1f ? "DPH: " + DPH.ToString() : "";
        buttonText.text += Mathf.Abs(DPV) >= 0.1f ? "DPV: " + DPV.ToString() : "";

        if( controlEnabled)
        {
            if (Mathf.Abs(RJV) >= 0.1f)
            {
                throttle = maxThrottle * -RJV;
                throttleValue.text = throttle.ToString();
                callVoidDroneFunc("setThrottle", new object[] { throttle });
            }
            else
            {
                throttle = 0;
                throttleValue.text = throttle.ToString();
                callVoidDroneFunc("setThrottle", new object[] { throttle });
            }
            if (Mathf.Abs(RJH) >= 0.1f)
            {
                yaw = maxYaw * RJH;
                yawValue.text = yaw.ToString();
                callVoidDroneFunc("setYaw", new object[] { yaw });
            }
            else
            {
                yaw = 0;
                yawValue.text = yaw.ToString();
                callVoidDroneFunc("setYaw", new object[] { yaw });
            }
            if (Mathf.Abs(LJV) >= 0.1f)
            {
                pitch = LJV * maxPitch;
                pitchValue.text = pitch.ToString(); 
                callVoidDroneFunc("setPitch", new object[] { pitch });
            }
            else
            {
                pitch = 0;
                pitchValue.text = pitch.ToString();
                callVoidDroneFunc("setPitch", new object[] { pitch });
            }
            if (Mathf.Abs(LJH) >= 0.1f)
            {
                roll = maxRoll * -LJH;
                rollValue.text = roll.ToString();
                callVoidDroneFunc("setRoll", new object[] { roll });
            }
            else
            {
                roll = 0;
                rollValue.text = roll.ToString();
                callVoidDroneFunc("setRoll", new object[] { roll });
            }
            if(Input.GetButton("L2") && Input.GetButton("R2")){
                if (Input.GetButtonDown("Y"))
                {
                    Debug.Log("take off buttons!");
                    callVoidDroneFunc("takeOff");
                }
                if (Input.GetButtonDown("A"))
                {
                    Debug.Log("Landing buttons!");
                    callVoidDroneFunc("land");
                }
            }

            if (!Input.GetButton("L1") && Input.GetButton("R1"))
            {
                Debug.Log("Gimbal down");
                callVoidDroneFunc("adjustGimbalRotation", new object[] {-90f, 0f});
            }

            if (Input.GetButton("L1") && !Input.GetButton("R1"))
            {
                Debug.Log("Gimbal up");
                callVoidDroneFunc("adjustGimbalRotation", new object[] {0f, 0f});
            }

        }
        
    }

    // Haversine formula
    // https://en.wikipedia.org/wiki/Haversine_formula
    // The haversine formula determines the great-circle distance between two points on a sphere given their longitudes and latitudes.
    double calculate_distance(double lat1, double lon1, double lat2, double lon2)
    {  // generally used geo measurement function
        double R = 6378.137f; // Radius of earth in KM
        double dLat = lat2 * Math.PI / 180 - lat1 * Math.PI / 180;
        double dLon = lon2 * Math.PI / 180 - lon1 * Math.PI / 180;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double d = R * c;
        return d * 1000; // meters
    }



    public static double DegreeBearing(double lat1, double lon1, double lat2, double lon2)
    {
        Double dLon = ToRadians(lon2 - lon1);
        Double dPhi = Math.Log(
            Math.Tan(ToRadians(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRadians(lat1) / 2 + Math.PI / 4));
        if (Math.Abs(dLon) > Math.PI)
            dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
        return ToBearing(Math.Atan2(dLon, dPhi));
    }

    public static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    public static double ToDegrees(double radians)
    {
        return radians * 180 / Math.PI;
    }

    public static double ToBearing(double radians)
    {
        // convert radians to degrees (as bearing: 0...360)
        return (ToDegrees(radians) + 360) % 360;
    }


    // parameter data is not used.  UnityPlayer injection funtion requires a string parmeter
    void locationUpdate(String data)
    {
        Debug.Log("LOCATION");
        Double[] loc;
        Double[] dLoc;
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                loc = obj_Activity.Call<Double[]>("getPhoneLocation");
                dLoc = obj_Activity.Call<Double[]>("getDroneLocation");
                
            }
        }
        if(null == baseLoc)
        {
            baseLoc = loc;
        }
        if( null == phoneLoc)
        {
            phoneLoc = loc;
        }
        if (null == droneLoc)
        {
            droneLoc = dLoc;
        }
        


        if (null != loc && null != dLoc)
        {
            baseLoc = loc;
            //locationText.text = "Location: "+ loc[0].ToString() + " " + loc[1].ToString() + "|| " + dLoc[0].ToString() + " " + dLoc[1].ToString() + " " + dLoc[2].ToString();
            locationText.text = "Distance: lat:" + ((phoneLoc[0] - loc[0]) * 1000000).ToString() + " \nlon: " + ((phoneLoc[1] - loc[1]) * 1000000).ToString() + "\nStartLoc: " + phoneLoc[0] + " " + phoneLoc[1];
            //phoneHolder.transform.position = new Vector3(0, 0, 0);
            Vector3 translate = new Vector3((float)((phoneLoc[0] - loc[0]) * 1000000), 0, (float)((phoneLoc[1] - loc[1]) * 1000000));
            distanceText.text = "Vector: " + translate.ToString("G6");
            phoneHolder.transform.position = translate;
            distanceText.text += "\nTransform: " + phoneHolder.transform.position.ToString("G6");
        }      
    }

    // Update is called once per frame
    void Update()
    {
        if (update_status_flag)
        {
            updateText();
        }
        if (update_display_flag)
        {
            updateDisplay();
        }
        controllerInput();
    }
}
