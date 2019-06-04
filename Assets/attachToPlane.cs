using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class attachToPlane : MonoBehaviour {

    /// <summary>
    /// The first-person camera being used to render the passthrough camera image (i.e. AR
    /// background).
    /// </summary>
    public Camera FirstPersonCamera;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject DetectedPlanePrefab;

    /// <summary>
    /// A model to place when a raycast from a user touch hits a plane.
    /// </summary>
    public Transform m_MazeHolder;

    /// <summary>
    /// The rotation in degrees need to apply to model when the Andy model is placed.
    /// </summary>
    private const float k_ModelRotation = 180.0f;

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error,
    /// otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    /// <summary>
    /// The Unity Update() method.
    /// </summary>
    public void Update()
    {
        _UpdateApplicationLifecycle();

        // If the player has not touched the screen, we are done with this update.
        // Also should not handle input if the player is pointing on UI.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began || EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            if (m_MazeHolder.transform.parent == null) // if maze is not yet anchored
                BindToSurface(Screen.width / 2, Screen.height / 2);
        }
        else
        {
            BindToSurface(touch.position.x, touch.position.y);
        }
    }

    void BindToSurface(float screenX, float screenY)
    {

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(screenX, screenY, raycastFilter, out hit))
        {
            _ShowAndroidToastMessage(screenX + " " + screenY);
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) &&
                Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                    hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                // Choose the Andy model for the Trackable that got hit.
                GameObject prefab;
                if (hit.Trackable is DetectedPlane)
                {
                    var plane = (DetectedPlane)hit.Trackable;
                    if (plane.PlaneType != DetectedPlaneType.HorizontalUpwardFacing)
                        return;
                    if (plane.TrackingState != TrackingState.Tracking)
                        return;

                    // Compensate for the hitPose rotation facing away from the raycast (i.e.
                    // camera).
                    //andyObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of
                    // the physical world evolves.
                    
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make maze a child of the anchor.
                    var oldAnchor = m_MazeHolder.transform.parent;
                    m_MazeHolder.transform.parent = anchor.transform;
                    m_MazeHolder.transform.localPosition = Vector3.Scale(m_MazeHolder.transform.localPosition, new Vector3(1, 0, 1));
                    FindObjectOfType<DroneBridge>().CalibrateWorldNorth(m_MazeHolder);
                    if (oldAnchor != null)
                        Destroy(oldAnchor);
                }
            }
        }
    }

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to
        // appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage(
                "ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject =
                    toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
