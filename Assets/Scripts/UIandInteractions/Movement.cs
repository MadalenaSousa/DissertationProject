using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Movement : MonoBehaviour
{
    Vector3 initialPos;
    Quaternion initialRot;
    private CinemachineFreeLook.Orbit[] originalOrbits;
    private CinemachineFreeLook freelook;

    private void Start()
    {
        freelook = this.transform.GetComponent<CinemachineFreeLook>();

        initialPos = transform.position;
        initialRot = transform.rotation;

        originalOrbits = new CinemachineFreeLook.Orbit[freelook.m_Orbits.Length];
        
        for (int i = 0; i < freelook.m_Orbits.Length; i++)
        {
            originalOrbits[i].m_Height = freelook.m_Orbits[i].m_Height;
            originalOrbits[i].m_Radius = freelook.m_Orbits[i].m_Radius;
        }
    }
    private void Update()
    {
        //transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * 50);

        //UpdateOrbit(Input.GetAxis("Mouse ScrollWheel"));
    }

    public void resetPosition()
    {
        transform.position = initialPos;
        transform.rotation = initialRot;
    }

    public void UpdateOrbit(float zoomPercent)
    {
        for (int i = 0; i < freelook.m_Orbits.Length; i++)
        {
            freelook.m_Orbits[i].m_Height = originalOrbits[i].m_Height * zoomPercent;
            freelook.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius * zoomPercent;
        }
    }

}
