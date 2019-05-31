using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneView : MonoBehaviour
{
    private DroneBridge bridge;
    private Camera gimbal;

    // Start is called before the first frame update
    void Start()
    {
        bridge = FindObjectOfType<DroneBridge>(); // get DroneBridge in the scene
        gimbal = GetComponentInChildren<Camera>(); // get Camera in children
        Debug.Assert(bridge != null, "There must be one DroneBridge component in the scene.");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 relloc = (bridge.DroneLocation - bridge.PhoneLocation).Coordinate2Meters(); // relative location of drone to phone
        transform.localPosition = bridge.phoneView.localPosition + relloc; // position relative to phone object in the scene
        transform.localRotation = bridge.DroneAttitude.ToRotation();
    }
}
