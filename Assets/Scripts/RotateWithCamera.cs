using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithCamera : MonoBehaviour
{

    public GameObject cameraHolder;
    //Rotate on x and z axis in relation to camera
    void Update()
    {
        transform.eulerAngles = new Vector3(0, cameraHolder.transform.eulerAngles.y, cameraHolder.transform.eulerAngles.z);
    }

}
