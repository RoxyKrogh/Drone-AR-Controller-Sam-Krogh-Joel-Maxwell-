using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityStandardAssets.CrossPlatformInput;

public class droneMaze : MonoBehaviour
{
    public DroneBridge controller;

    public Button startBtn;
    public Button Drone_video_btn;
    public Button Phone_video_btn;
    public Button startDrone;
    public Button takeOffBtn;
    public Button landBtn;
    public Button enableVirtualControl;
    public Button stopControl;
    public Button locationButton;
    public Button swapDisplays;
    public RenderTexture droneView;
    public RenderTexture phoneView;

    // flight controll values
    public Text yawValue;

    public Text rollValue;

    public Text pitchValue;

    public Text throttleValue;

    public Text locationText;
    public Text distanceText;


    public GameObject left_display;
    public GameObject right_display;
    public GameObject droneDisplay;
    public Camera droneCamera;

    public GameObject droneHolder;

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
    private DroneBridge.Location baseLoc;
    private DroneBridge.Location phoneLoc;
    private DroneBridge.Location droneLoc;

    // Drone Camera
    private Texture2D tex2d;

    // phoneCam Camera
    private WebCamTexture webTex;
    public Texture noVideo;

    // Joystick Buttons
    private string[] buttons = new string[] { "A", "B", "X", "Y", "R1", "R2", "L1", "L2", "L3", "R3", "START", "BACK" };
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
        takeOffBtn.onClick.AddListener(takeOff);
        landBtn.onClick.AddListener(land);
        controlEnabled = false;
        enableVirtualControl.onClick.AddListener(enableVirtualSticks);
        stopControl.onClick.AddListener(disableVirtualSticks);
        swapDisplays.onClick.AddListener(swap);
        droneRender = false;

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
        drone_camera_flag = true;
        controller.EnableVideo();
        enableVirtualSticks();
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
            controller.DisableVideo();
            Drone_video_btn.colors = on;
        }
        else
        {
            drone_camera_flag = true;
            controller.EnableVideo();
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

    public void swap()
    {
        droneRender = !droneRender;

        if (droneRender)
        {
            droneDisplay.GetComponent<Renderer>().enabled = true;
            left_display.GetComponent<Renderer>().material.mainTexture = droneView;
            right_display.GetComponent<Renderer>().material.mainTexture = droneView;
            //phoneDisplay.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            //phoneDisplay.GetComponent<Renderer>().enabled = true;
            left_display.GetComponent<Renderer>().material.mainTexture = phoneView;
            right_display.GetComponent<Renderer>().material.mainTexture = phoneView;
            droneDisplay.GetComponent<Renderer>().enabled = false;
        }
    }

    void toast()
    {
        controller.ShowToast("Button Clicked in Unity"); 
    }

    void updateText()
    {
        controller.RefreshConnectionStatus();
        controller.RefreshFlightControllerStatus();
        connection_status.text = controller.ConnectionStatus;
        connected_hardware.text = controller.ProductText;
        flight_controller_status.text = controller.State;
    }

    void startDroneFunc()
    {
        controller.SetupDroneConnection();
    }

    void updateDisplay()
    {
        /*if (drone_camera_flag )
        {
            if (frame_ready_flag)
            {
                frame_ready_flag = false;
                if (controller.GetVideoFrame(tex2d)) // get video frame and apply to tex2d
                {
                    droneDisplay.GetComponent<Renderer>().material.mainTexture = tex2d;
                    fps_display.text = "" + Math.Round(1 / (Time.time - lastFrame), 2) + "fps";
                    lastFrame = Time.time;
                }
            }
        }
        else
        {
            droneDisplay.GetComponent<Renderer>().material.mainTexture = noVideo;
        }*/
        if ( phone_camera_flag )
        {
            //if (webTex.isPlaying == false) { webTex.Play(); }
            //phoneDisplay.GetComponent<Renderer>().material.mainTexture = webTex;
            //right_display.GetComponent<Renderer>().material.mainTexture = webTex;
        }
        else
        {
            if(null != webTex)
            {
                //webTex.Stop();
            }
            //phoneDisplay.GetComponent<Renderer>().material.mainTexture = noVideo;
            //right_display.GetComponent<Renderer>().material.mainTexture = noVideo;
        }
    }


    void takeOff()
    {
        controller.TakeOff();
    }

    void land()
    {
        controller.Land();
    }

    void startFollowMe()
    {
        controller.FollowMeStart();
    }

    void stopFollowMe()
    {
        controller.FollowMeStop();
    }

    void startLocation()
    {
        controller.StartLocationService();
    }

    void enableVirtualSticks()
    {
        controlEnabled = true;
        controller.VirtualControlEnabled = true;
    }

    void disableVirtualSticks()
    {
        controlEnabled = false;
        controller.VirtualControlEnabled = false;
    }

    // Haversine formula
    // https://en.wikipedia.org/wiki/Haversine_formula
    // The haversine formula determines the great-circle distance between two points on a sphere given their longitudes and latitudes.
    // Code adapted from examples found here:
    // https://www.movable-type.co.uk/scripts/latlong.html 
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
        var loc = controller.PhoneLocation;
        var dLoc = controller.DroneLocation;
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
    }
}
