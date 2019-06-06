using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DebugDroneLocation : MonoBehaviour
{
    private Text text;
    private DroneBridge bridge;
    private DroneView drone;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        bridge = DroneBridge.GetReference();
        drone = FindObjectOfType<DroneView>();
    }
    
    private void Reset()
    {
        text = GetComponent<Text>();
        text.text = "Phone: " + " / GPS=" + "\n" +
                    "Heading: " + "\n" +
                    "Drone: " + " / GPS=" + "\n" +
                    "Gimbal: ";
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 p = drone.PhonePosition;
        Vector3 d = drone.transform.localPosition;
        var pGps = bridge.PhoneLocation; // gps location (latitude, longitude, altitude)
        var dGps = bridge.DroneLocation; // gps location (latitude, longitude, altitude)
        text.text = "Phone: " + p + " / GPS=" + pGps + "\n" +
                    "Heading: " + bridge.PhoneHeading + "\n" +
                    "Drone: " + d + " / GPS=" + dGps + "\n" +
                    "Gimbal(p,y): " + drone.CameraPitch + ", " + drone.CameraYaw;
    }
}
