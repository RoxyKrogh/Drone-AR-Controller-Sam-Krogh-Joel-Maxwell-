using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneView : MonoBehaviour
{
    private DroneBridge bridge;
    private Transform gimbal;
    private Vector3 targetPosition; // position to interpolate toward (localPosition)
    [Range(0.1f,1.0f)] public float locationInterpolation = 0.5f; // amount to interpolate
    
    private Vector3 positionOrigin = Vector3.zero;
    private DroneBridge.DroneVector gpsOrigin = new DroneBridge.DroneVector(0,0,0);
    private bool isCalibrated = false;

    // Start is called before the first frame update
    void Start()
    {
        bridge = DroneBridge.GetReference(); // get DroneBridge in the scene
        gimbal = GimbalChild; // get Camera gimbal in children
        Debug.Assert(bridge != null, "There must be a DroneBridge component in the scene.");
    }

    // Update is called once per frame
    void Update()
    {
        if (isCalibrated)
        {
            Vector3 relloc = (bridge.DroneLocation - gpsOrigin).Coordinate2Meters(); // relative location of drone to phone
            relloc.y = 0;
            relloc = bridge.MakeRelativeToNorth(relloc); // allign latitude/logitude
            relloc.y = 0;
            relloc.x *= -1;
            relloc.z *= -1;
            targetPosition = positionOrigin + relloc; // position relative to phone object in local space
            targetPosition.y = (float)bridge.DroneLocation.Altitude * 2 + 0.5f; // hover 10 units above the player

            Vector3 moveTo = Vector3.Lerp(transform.localPosition, targetPosition, locationInterpolation);
            transform.localPosition = moveTo;

            transform.localRotation = Quaternion.AngleAxis(bridge.Yaw, Vector3.up);
        }
    }

    public bool IsCalibrated { get { return isCalibrated; } }

    // DroneBridge::OnEnable() sets DroneView::CameraPitch before DroneView::gimbal is initialized in DroneView::Start(). 
    // This throws an null reference exception. The only reason this field exists is so that the gimbal child can be referenced 
    // before DroneView::gimbal is initialized in DroneView::Start().
    private Transform GimbalChild { get { return gimbal ?? transform.Find("DroneGimbal"); } }

    /// <summary>
    /// phoneView position in drone's local space (local position as if phoneView had the same parent as this drone)
    /// </summary>
    public Vector3 PhonePosition
    {
        get { return Position2LocalSpace(bridge.PhoneView.position); }
    }

    public float CameraPitch
    {
        get { return -GimbalChild.localRotation.eulerAngles.x; }
        set
        {
            Vector3 euler = gimbal.localEulerAngles;
            GimbalChild.localEulerAngles = new Vector3(-value, euler.y, euler.z);
        }
    }

    public float CameraYaw
    {
        get { return GimbalChild.localRotation.eulerAngles.y; }
        set
        {
            Vector3 euler = gimbal.localEulerAngles;
            GimbalChild.localEulerAngles = new Vector3(euler.x, value, euler.z);
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

    public void CalibrateGPSCenter()
    {
        try
        {
            positionOrigin = Position2LocalSpace(bridge.PhoneView.localPosition);
            gpsOrigin = bridge.DroneLocation;
            positionOrigin.y = 0f;
            isCalibrated = true;
        } catch(System.NullReferenceException ex)
        {
            Debug.Log("Failed to calibrate GPS (null reference).");
        }
    }
}
