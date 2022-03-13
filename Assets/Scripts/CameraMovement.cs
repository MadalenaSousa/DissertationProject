using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    float mouse;

    void Start()
    {
        mouse = Input.GetAxis("Mouse X");
    }
    void Update()
    {
        mouse = Input.GetAxis("Mouse X");
        transform.Rotate(new Vector3(0, mouse, 0));
    }
   
}
