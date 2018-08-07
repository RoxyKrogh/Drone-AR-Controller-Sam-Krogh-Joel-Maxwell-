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





        
        

        //int phoneViewStandoff = 300;
        //float frustumHeightP = 2.0f * (phoneCamera.transform.position.z + phoneViewStandoff ) * Mathf.Tan(phoneCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        //float frustumWidthP = frustumHeightP * phoneCamera.aspect;


        //phoneViewPlane.transform.localPosition = new Vector3(0,0, phoneCamera.transform.position.z + phoneViewStandoff);


        //phoneViewPlane.transform.localScale = new Vector3(frustumWidthP / 10, 1, frustumHeightP / 10);
        if (Application.isMobilePlatform)
        {
            double[] loc;
            using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    loc = obj_Activity.Call<double[]>("getDroneLocation");
                }
            }
            if(loc[2] != 0)
            {
                holder.transform.position = new Vector3(holder.transform.position.x, (float)loc[2], holder.transform.position.z);
            }
        }
    }
}
