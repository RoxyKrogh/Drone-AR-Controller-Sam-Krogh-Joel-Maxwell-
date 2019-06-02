using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DebugDroneStatus : MonoBehaviour
{
    private Text text;
    private DroneBridge bridge;
    
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        bridge = DroneBridge.GetReference();
    }

    private void Reset()
    {
        text = GetComponent<Text>();
        text.text = "FlightMode: " + "\n" +
                    "Status: " + "\n" +
                    "IMU: " + "\n" +
                    "Connection: " + "\n" +
                    "IsConnected: ";
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "FlightMode: " + bridge.FlightMode + "\n" +
                    "Status: " + bridge.State + "\n" +
                    "IMU: " + bridge.IMUState + "\n" +
                    "Connection: " + bridge.ConnectionStatus + "\n" +
                    "IsConnected: " + bridge.IsConnected;
    }
}
