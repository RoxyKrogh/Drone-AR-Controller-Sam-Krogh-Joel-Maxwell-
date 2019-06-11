using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DroneBridge))]
public class DroneControl : MonoBehaviour
{
    public UnityEvent swapVideoEvent;
    public bool controlEnabled = true;

    private DroneBridge bridge;
    private float roll, pitch, yaw, throttle;
    private float maxYaw, maxThrottle, maxPitch, maxRoll;
    private string[] buttons = new string[] { "A", "B", "X", "Y", "R1", "R2", "L1", "L2", "L3", "R3", "START", "BACK" };

    // Start is called before the first frame update
    void Start()
    {
        yaw = 0;
        pitch = 0;
        roll = 0;
        throttle = 0;
        maxYaw = 90f;
        maxThrottle = 2;
        maxPitch = 2;
        maxRoll = 2;
        controlEnabled = true;
    }

    private void OnEnable()
    {
        bridge = GetComponent<DroneBridge>();
    }

    // Update is called once per frame
    void Update()
    {
        ControllerInput();
    }

    void ControllerInput()
    {
        float LJV = Input.GetAxis("LJV");
        float LJH = Input.GetAxis("LJH");
        float RJV = Input.GetAxis("RJV");
        float RJH = Input.GetAxis("RJH");
        float DPV = Input.GetAxis("DPV");
        float DPH = Input.GetAxis("DPH");

        if (controlEnabled)
        {
            if (Mathf.Abs(RJV) >= 0.1f)
            {
                throttle = maxThrottle * -RJV;
                bridge.Throttle = throttle;
            }
            else
            {
                throttle = 0;
                bridge.Throttle = throttle;
            }
            if (Mathf.Abs(RJH) >= 0.1f)
            {
                yaw = maxYaw * RJH;
                bridge.Yaw = yaw;
            }
            else
            {
                yaw = 0;
                bridge.Yaw = yaw;

            }
            if (Mathf.Abs(LJH) >= 0.1f)
            {
                pitch = LJH * maxPitch;
                bridge.Pitch = pitch;
                //droneHolder.transform.Translate(Time.deltaTime * pitch, 0, 0);

            }
            else
            {
                pitch = 0;
                bridge.Pitch = pitch;
            }
            if (Mathf.Abs(LJV) >= 0.1f)
            {
                roll = maxRoll * -LJV;
                bridge.Roll = roll;
                //droneHolder.transform.Translate(0, 0, Time.deltaTime * roll);
            }
            else
            {
                roll = 0;
                bridge.Roll = roll;
            }
            if (Input.GetButtonDown("B"))
            {
                swapVideoEvent.Invoke();
            }
            if (Input.GetButton("L2") && Input.GetButton("R2"))
            {
                if (Input.GetButtonDown("Y"))
                {
                    Debug.Log("take off buttons!");
                    bridge.TakeOff();
                }
                if (Input.GetButtonDown("A"))
                {
                    Debug.Log("Landing buttons!");
                    bridge.Land();
                }
            }

            if (!Input.GetButton("L1") && Input.GetButton("R1"))
            {
                Debug.Log("Gimbal down");
                bridge.DroneCameraPitch = -90f;
            }

            if (Input.GetButton("L1") && !Input.GetButton("R1"))
            {
                Debug.Log("Gimbal up");
                bridge.DroneCameraPitch = 0f;
            }

        }

    }
}
