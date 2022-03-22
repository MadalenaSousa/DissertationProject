using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.position;
    }
    private void Update()
    {
        transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * 10);
    }
    private void OnMouseDrag()
    {
        transform.Rotate(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0);       
    }

    public void resetPosition()
    {
        transform.position = initialPos;
    }

}
