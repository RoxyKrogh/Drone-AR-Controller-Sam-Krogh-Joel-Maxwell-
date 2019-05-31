using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class droneCameraPlaneControl : MonoBehaviour {

    //public GameObject mazeHolder;
    public GameObject droneViewPlane;
    public GameObject holder;
    //public GameObject phoneViewPlane;
    public Button droneProjection;
    public Camera droneCamera;
    //public Camera phoneCamera;
    //public GameObject phoneParent;
    //public int phoneViewStandoff;

    // Use this for initialization
    void Start () {
        /*if (Application.isMobilePlatform)
        {
            //GameObject cameraParent = new GameObject("camParent");
            //cameraParent.transform.position = phoneCamera.transform.position;
            //phoneCamera.transform.parent = cameraParent.transform;
            phoneParent.transform.Rotate(Vector3.right, 90);
        }
        Input.gyro.enabled = true;*/
        droneProjection.onClick.AddListener(setProjection);
        //phoneViewStandoff = 200;
    }

    void setProjection()
    {
        if(droneCamera.orthographic == true)
        {
            droneCamera.orthographic = false;
        }
        else
        {
            droneCamera.orthographic = true;
        }
    }


    // Update is called once per frame
    void Update () {
        //Quaternion cameraRotation = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);
        //phoneCamera.transform.localRotation = cameraRotation;

        
        if (droneCamera.orthographic == true)
        {
            float frustumHeightD = 2.0f * (holder.transform.position.y) * Mathf.Tan(94 * 0.5f * Mathf.Deg2Rad);
            droneCamera.orthographicSize = frustumHeightD / 2;
            float frustumWidthD = frustumHeightD * droneCamera.aspect;
            droneViewPlane.transform.localScale = new Vector3(frustumWidthD / 10,1,frustumHeightD / 10);
            droneViewPlane.transform.localPosition = new Vector3(0, 0, droneCamera.transform.position.y+2);
        }
        else
        {
            float frustumHeightD = 2.0f * (droneCamera.transform.position.y + 6) * Mathf.Tan(droneCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidthD = frustumHeightD * droneCamera.aspect;
            droneViewPlane.transform.localPosition = new Vector3(0, 0, droneCamera.transform.position.y + 6);
            droneViewPlane.transform.localScale = new Vector3(frustumWidthD / 10, 1, frustumHeightD / 10);
        }
    }
}
