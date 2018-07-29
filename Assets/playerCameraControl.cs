using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerCameraControl : MonoBehaviour {

    public GameObject mazeHolder;
    public GameObject droneViewPlane;
    public GameObject phoneViewPlane;
    public Button setMazeDirection;
    public Camera droneCamera;
    public Camera phoneCamera;
    public GameObject phoneParent;
    public int phoneViewStandoff;

    // Use this for initialization
    void Start () {
        if (Application.isMobilePlatform)
        {
            //GameObject cameraParent = new GameObject("camParent");
            //cameraParent.transform.position = phoneCamera.transform.position;
            //phoneCamera.transform.parent = cameraParent.transform;
            phoneParent.transform.Rotate(Vector3.right, 90);
        }
        Input.gyro.enabled = true;
        setMazeDirection.onClick.AddListener(setMaze);
        phoneViewStandoff = 200;
    }
	
    void setMaze()
    {
        Quaternion rot = new Quaternion();
        float yRot = phoneCamera.transform.rotation.eulerAngles.y;
        rot.eulerAngles = new Vector3(0, yRot, 0);
        mazeHolder.transform.rotation = rot;
    }


	// Update is called once per frame
	void Update () {
        Quaternion cameraRotation = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);
        phoneCamera.transform.localRotation = cameraRotation;

        float frustumHeightD = 2.0f * (droneCamera.transform.position.y + 6 ) * Mathf.Tan(droneCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidthD = frustumHeightD * droneCamera.aspect;
        droneViewPlane.transform.localPosition = new Vector3(0, 0, droneCamera.transform.position.y + 6);
        droneViewPlane.transform.localScale = new Vector3(frustumWidthD/10, 1, frustumHeightD/10);

        int phoneViewStandoff = 300;
        float frustumHeightP = 2.0f * (phoneCamera.transform.position.z + phoneViewStandoff ) * Mathf.Tan(phoneCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidthP = frustumHeightP * phoneCamera.aspect;


        phoneViewPlane.transform.localPosition = new Vector3(0,0, phoneCamera.transform.position.z + phoneViewStandoff);


        phoneViewPlane.transform.localScale = new Vector3(frustumWidthP / 10, 1, frustumHeightP / 10);



    }
}
