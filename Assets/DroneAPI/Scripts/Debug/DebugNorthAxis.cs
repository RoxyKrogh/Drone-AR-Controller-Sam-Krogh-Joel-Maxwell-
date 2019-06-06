using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugNorthAxis : MonoBehaviour
{
    DroneBridge bridge;
    public Transform followTarget;

    // Start is called before the first frame update
    void Start()
    {
        bridge = DroneBridge.GetReference();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.LookRotation(bridge.MakeRelativeToNorth(Vector3.forward),Vector3.up); // point towards real north
        transform.position = followTarget.transform.position + (Vector3.up * -1); // position below phone
    }
}
