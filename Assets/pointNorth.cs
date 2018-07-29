using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointNorth : MonoBehaviour {

	// Use this for initialization
	void Start () {
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                obj_Activity.Call("startLocationService");
                Debug.Log("LOCATION START FROM UNITY");
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        // Orient an object to point to magnetic north.
        /*using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                double[] heading = obj_Activity.Call<double[]>("getPhoneLocation");
                transform.rotation = Quaternion.Euler(0, -(float)heading[2], 0);
                Debug.Log("Heading: " + heading[2].ToString());
            }
        }*/

        

    }
}
