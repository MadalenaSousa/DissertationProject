using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Movement : MonoBehaviour
{
    Vector3 initialPos;
    private CinemachineFreeLook.Orbit[] originalOrbits;
    private CinemachineFreeLook freelook;

    private void Start()
    {
        freelook = this.transform.GetComponent<CinemachineFreeLook>();
        initialPos = freelook.transform.position;

        originalOrbits = new CinemachineFreeLook.Orbit[freelook.m_Orbits.Length];
        
        for (int i = 0; i < freelook.m_Orbits.Length; i++)
        {
            originalOrbits[i].m_Radius = freelook.m_Orbits[i].m_Radius;
        }
    }
    private void Update()
    {
        Zoom(Input.GetAxis("Mouse ScrollWheel") * 100);       
    }

    public void resetPosition()
    {
        originalOrbits[0].m_Radius = 400;
        originalOrbits[1].m_Radius = 800;
        originalOrbits[2].m_Radius = 400;

        freelook.transform.position = new Vector3(0, 0, -800);
    }

    public void Zoom(float zoom)
    {
        for (int i = 0; i < freelook.m_Orbits.Length; i++)
        {
            freelook.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius + zoom;
            originalOrbits[i].m_Radius = freelook.m_Orbits[i].m_Radius;
        }
    }

    public float mapValues(float value, float currentMin, float currentMax, float newMin, float newMax)
    {
        return (value - currentMin) * (newMax - newMin) / (currentMax - currentMin) + newMin;
    }

}
