using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Movement : MonoBehaviour
{
    float panSpeed, rotSpeed;
    public int zoomSpeed;

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit point;
        Physics.Raycast(ray, out point, 1000);
        Vector3 Scrolldirection = ray.GetPoint(5);

        //ZOOM
        if (!PracticesAndStrategies.instance.clusterInfoPanel.activeSelf)
        {
            transform.position = Vector3.MoveTowards(transform.position, Scrolldirection, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);
        }

        //PAN
        if (Input.GetMouseButton(1) && Input.GetKey(KeyCode.Space))
        {
            panSpeed = 50;

        } else if(Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Space))
        {
            panSpeed = 0;
        }
        
        Vector3 pan = new Vector3(-Input.GetAxis("Mouse X") * panSpeed, -Input.GetAxis("Mouse Y") * panSpeed, 0);        
        transform.Translate(pan, Space.Self);

        

        //ROTATE
        if (Input.GetMouseButtonDown(1))
        {
            rotSpeed = 1;
        
        }
        else if (Input.GetMouseButtonUp(1))
        {
            rotSpeed = 0;
        }
        
        Vector3 rot = new Vector3(Input.GetAxis("Mouse Y") * rotSpeed, Input.GetAxis("Mouse X") * rotSpeed, 0);
        
        transform.Rotate(rot, Space.World);
    }

    public void resetPosition()
    {
        transform.position = new Vector3(0, 0, -1000);
        transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
    }


    public float mapValues(float value, float currentMin, float currentMax, float newMin, float newMax)
    {
        return (value - currentMin) * (newMax - newMin) / (currentMax - currentMin) + newMin;
    }

}
