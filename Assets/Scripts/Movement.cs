using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    Vector3 initialPos;
    Quaternion initialRot;

    private void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
    }
    private void Update()
    {
        transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * 15);
    }
    private void OnMouseDrag()
    {
        transform.Rotate(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0);
    }

    public void resetPosition()
    {
        transform.position = initialPos;
        transform.rotation = initialRot;
    }

}