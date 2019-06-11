using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event2Transform : MonoBehaviour
{

    public void SetLocalScaleX(float value)
    {
        Vector3 v = transform.localScale;
        v.x = value;
        transform.localScale = v;
    }

    public void SetLocalScaleY(float value)
    {
        Vector3 v = transform.localScale;
        v.y = value;
        transform.localScale = v;
    }

    public void SetLocalScaleZ(float value)
    {
        Vector3 v = transform.localScale;
        v.z = value;
        transform.localScale = v;
    }
    public void SetLocalPositionX(float value)
    {
        Vector3 v = transform.localPosition;
        v.x = value;
        transform.localPosition = v;
    }

    public void SetLocalPositionY(float value)
    {
        Vector3 v = transform.localPosition;
        v.y = value;
        transform.localPosition = v;
    }

    public void SetLocalPositionZ(float value)
    {
        Vector3 v = transform.localPosition;
        v.z = value;
        transform.localPosition = v;
    }
    public void SetlocalEulerAnglesX(float value)
    {
        Vector3 v = transform.localEulerAngles;
        v.x = value;
        transform.localEulerAngles = v;
    }

    public void SetlocalEulerAnglesY(float value)
    {
        Vector3 v = transform.localEulerAngles;
        v.y = value;
        transform.localEulerAngles = v;
    }

    public void SetlocalEulerAnglesZ(float value)
    {
        Vector3 v = transform.localEulerAngles;
        v.z = value;
        transform.localEulerAngles = v;
    }
}
