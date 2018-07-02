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
    public Button showToast;
    public Button startDrone;
    public Button takeOffBtn;
    public Button landBtn;
    public Button enableCam;
    public Button enableVirtualControl;
    public Button yawRight;
    public Button yawLeft;
    public Button droneUp;
    public Button droneDown;
    public Button droneAhead;
    public Button droneBack;
    public Button droneLeft;
    public Button droneRight;
    public Button stopControl;

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

    //public Button startYawBtn;
    //public Button stopYawBtn;
    public GameObject left_display;
    public GameObject right_display;
    public GameObject left_pip_display;
    public GameObject right_pip_display;
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

    // Drone Camera
    private Texture2D tex2d;

    // phoneCam Camera
    private WebCamTexture webTex;
    public Texture noVideo;

    // Joystick Buttons
    private string[] buttons = new string[] { "A", "B", "X", "Y", "R1", "R2", "L1", "L2", "L3", "R3", "START", "BACK" };

    // Display positions
    private Vector3 defaultPos;
    private Vector3 defaultScale;
    private Vector3 viewcale;
    private Vector3 leftView;
    private Vector3 rightView;

    // colors
    private ColorBlock on;
    private ColorBlock off;

    // Use this for initialization
    void Start()
    {
        update_display_flag = true;
        update_status_flag = false;
        frame_ready_flag = false;
        drone_camera_flag = false;
        phone_camera_flag = false;
        defaultPos = new Vector3(-236, -283, 0);
        defaultScale = new Vector3(926, 768, 1);
        viewcale = new Vector3(1270, 1050, 1);
        leftView = new Vector3(-640, 0, 0);
        rightView = new Vector3(640, 0, 0);
        startBtn.onClick.AddListener(update_status_toggle);
        Drone_video_btn.onClick.AddListener(toggle_drone_cam);
        Phone_video_btn.onClick.AddListener(toggle_phone_cam);
        startDrone.onClick.AddListener(startDroneFunc);
        showToast.onClick.AddListener(toast);
        takeOffBtn.onClick.AddListener(takeOff);
        landBtn.onClick.AddListener(land);
        enableVirtualControl.onClick.AddListener(enableVirtualSticks);
        //yawRight.onClick.AddListener(StartYaw);
        stopControl.onClick.AddListener(disableVirtualSticks);
        followMeButton.onClick.AddListener(startFollowMe);
        stopFollowButton.onClick.AddListener(stopFollowMe);

        // flight controll values
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
            Drone_video_btn.colors = off;
        }
        else
        {
            drone_camera_flag = true;
            Drone_video_btn.colors = on;
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
        if (message == "true")
        {
            frame_ready_flag = true;
        }
    }

    void toast()
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                obj_Activity.Call("showToast", "Button Clicked in Unity");
            }
        }
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
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                obj_Activity.Call("setupDroneConnection");
            }
        }
    }

    void updateDisplay()
    {
        if (drone_camera_flag )
        {
            if (frame_ready_flag)
            {
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
                            left_display.GetComponent<Renderer>().material.mainTexture = tex2d;
                            right_display.GetComponent<Renderer>().material.mainTexture = tex2d;
                            fps_display.text = "" + Math.Round(1 / (Time.time - lastFrame), 2) + "fps";
                            lastFrame = Time.time;
                        }

                    }
                }
            }
        }
        else
        {
            left_display.GetComponent<Renderer>().material.mainTexture = noVideo;
            right_display.GetComponent<Renderer>().material.mainTexture = noVideo;
        }
        if ( phone_camera_flag )
        {
            if (webTex.isPlaying == false) { webTex.Play(); }
            left_pip_display.GetComponent<Renderer>().material.mainTexture = webTex;
            right_pip_display.GetComponent<Renderer>().material.mainTexture = webTex;
        }
        else
        {
            webTex.Stop();
            left_pip_display.GetComponent<Renderer>().material.mainTexture = noVideo;
            right_pip_display.GetComponent<Renderer>().material.mainTexture = noVideo;
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

    /*void StartYaw()
    {
        callVoidDroneFunc("startSendingControl");
    }

    void stopYaw()
    {
        callVoidDroneFunc("stopYaw");    
    }*/

    void startFollowMe()
    {
        callVoidDroneFunc("followMeStart");
    }

    void stopFollowMe()
    {
        callVoidDroneFunc("followMeStop");
    }



    void enableVirtualSticks()
    {
        callVoidDroneFunc("setVirtualControlActive", new object[] { true });
    }
    void disableVirtualSticks()
    {
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
        yaw += 1;
        yawValue.text = yaw.ToString();
        callVoidDroneFunc("setYaw", new object[] { yaw });
    }
    void yawDown()
    {
        yaw -= 1;
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
        
    }

    void setLocationText(String text)
    {
        locationText.text = text;
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
