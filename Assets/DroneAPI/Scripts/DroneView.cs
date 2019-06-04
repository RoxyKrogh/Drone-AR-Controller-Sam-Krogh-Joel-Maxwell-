using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneView : MonoBehaviour
{
    private DroneBridge bridge;
    private Transform gimbal;

    // Start is called before the first frame update
    void Start()
    {
        bridge = FindObjectOfType<DroneBridge>(); // get DroneBridge in the scene
        gimbal = transform.Find("DroneGimbal"); // get Camera gimbal in children
        Debug.Assert(bridge != null, "There must be a DroneBridge component in the scene.");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 relloc = (bridge.DroneLocation - bridge.PhoneLocation).Coordinate2Meters(); // relative location of drone to phone
        relloc.y = 10;
        transform.localPosition = PhonePosition + relloc; // position relative to phone object in local space
        transform.localRotation = bridge.DroneAttitude.ToRotation();
    }

    /// <summary>
    /// phoneView position in drone's local space (local position as if phoneView had the same parent as this drone)
    /// </summary>
    public Vector3 PhonePosition
    {
        get { return Position2LocalSpace(bridge.PhoneView.position); }
    }

    public float CameraPitch
    {
        get { return gimbal.localRotation.eulerAngles.x; }
        set
        {
            Vector3 euler = gimbal.localEulerAngles;
            gimbal.localEulerAngles = new Vector3(value, euler.y, euler.z);
        }
    }

    public float CameraYaw
    {
        get { return gimbal.localRotation.eulerAngles.y; }
        set
        {
            Vector3 euler = gimbal.localEulerAngles;
            gimbal.localEulerAngles = new Vector3(euler.x, value, euler.z);
        }
    }

    public float Yaw
    {
        get { return transform.localRotation.eulerAngles.y; }
        set
        {
            Vector3 euler = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(euler.x, value, euler.z);
        }
    }

    /// <summary>
    /// Convert a world space position to a local position in the same space as the drone.
    /// </summary>
    /// <param name="globalPosition">A world-space position vector.</param>
    /// <returns>local position in the same space as the drone</returns>
    public Vector3 Position2LocalSpace(Vector3 globalPosition)
    {
        Matrix4x4 global2parent = transform.parent != null ? transform.parent.worldToLocalMatrix : Matrix4x4.identity;
        return global2parent.MultiplyPoint(globalPosition);
    }
    /// <summary>
    /// Convert a world space vector to a local vector in the same space as the drone.
    /// </summary>
    /// <param name="globalPosition">A world-space vector.</param>
    /// <returns>local vector in the same space as the drone</returns>
    public Vector3 Vector2LocalSpace(Vector3 globalVector)
    {
        Matrix4x4 global2parent = transform.parent != null ? transform.parent.worldToLocalMatrix : Matrix4x4.identity;
        return global2parent.MultiplyVector(globalVector);
    }
}
