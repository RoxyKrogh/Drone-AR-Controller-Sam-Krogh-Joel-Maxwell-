using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class setupScreenScript : MonoBehaviour {

    public Button start;
    public Button init;
    public Text connectionStatus;
    private bool pollStatus;

    // flight controll values
    private float maxYaw, maxThrottle, maxPitch, maxRoll;
    private float yaw, roll, pitch, throttle;
    private bool controlEnabled;


    // Use this for initialization
    void Start () {
        // flight controll values
        maxYaw = 90f;
        maxThrottle = 2;
        maxPitch = 2;
        maxRoll = 2;
        yaw = 0;
        pitch = 0;
        roll = 0;
        throttle = 0;


        
        start.gameObject.SetActive(false);
        init.onClick.AddListener(initDroneFunc);
    }



    void initDroneFunc()
    {
        if (Application.isMobilePlatform)
        {
            callVoidDroneFunc("setupDroneConnection");
            pollStatus = true;
        }
        start.gameObject.SetActive(true);
        start.onClick.AddListener(switchScene);
        controlEnabled = true;
    }

    void switchScene()
    {
        SceneManager.LoadScene(2);
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

    void controllerInput()
    {
        float LJV = Input.GetAxis("LJV");
        float LJH = Input.GetAxis("LJH");
        float RJV = Input.GetAxis("RJV");
        float RJH = Input.GetAxis("RJH");
        float DPV = Input.GetAxis("DPV");
        float DPH = Input.GetAxis("DPH");

        if (controlEnabled && Application.isMobilePlatform)
        {
            if (Mathf.Abs(RJV) >= 0.1f)
            {
                throttle = maxThrottle * -RJV;
                callVoidDroneFunc("setThrottle", new object[] { throttle });
            }
            else
            {
                throttle = 0;
                callVoidDroneFunc("setThrottle", new object[] { throttle });
            }
            if (Mathf.Abs(RJH) >= 0.1f)
            {
                yaw = maxYaw * RJH;
                //callVoidDroneFunc("setYaw", new object[] { yaw });
            }
            else
            {
                yaw = 0;
                callVoidDroneFunc("setYaw", new object[] { yaw });
            }
            if (Mathf.Abs(LJH) >= 0.1f)
            {
                pitch = LJH * maxPitch;
                callVoidDroneFunc("setPitch", new object[] { pitch });
            }
            else
            {
                pitch = 0;
                callVoidDroneFunc("setPitch", new object[] { pitch });
            }
            if (Mathf.Abs(LJV) >= 0.1f)
            {
                roll = maxRoll * -LJV;
                callVoidDroneFunc("setRoll", new object[] { roll });
            }
            else
            {
                roll = 0;
                callVoidDroneFunc("setRoll", new object[] { roll });
            }

            if (Input.GetButton("L2") && Input.GetButton("R2"))
            {
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
                callVoidDroneFunc("setGimbalRotation", new object[] { -90f, 0f });
            }

            if (Input.GetButton("L1") && !Input.GetButton("R1"))
            {
                Debug.Log("Gimbal up");
                callVoidDroneFunc("setGimbalRotation", new object[] { 0f, 0f });
            }
            if (Input.GetButton("A") && Input.GetButton("B"))
            {
                switchScene();
            }
            if (Input.GetButtonDown("X"))
            {
                enableVirtualSticks();
            }
        }
    }


    // helper function to reduce code length
    void callVoidDroneFunc(string funcName)
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
    void callVoidDroneFunc(string funcName, object[] args)
    {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                obj_Activity.Call(funcName, args);
            }
        }
    }

    // Update is called once per frame
    void Update () {
        controllerInput();
        if (pollStatus && Application.isMobilePlatform)
        {
            //Debug.Log("Polling status");
            using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    obj_Activity.Call("refreshFlightControllerStatus");
                    obj_Activity.Call("refreshConnectionStatus");
                    connectionStatus.text = "Connection status: " + obj_Activity.Call<string>("getConnectionStatus");
                    connectionStatus.text += "\nIMU Status: " + obj_Activity.Call<string>("getIMUstate");
                }
            }
        }
	}
}
