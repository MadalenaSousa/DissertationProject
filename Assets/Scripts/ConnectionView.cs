using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionView : MonoBehaviour
{
    public void setConnection(Vector3 initNode, Vector3 endNode)
    {
        gameObject.GetComponent<LineRenderer>().SetPosition(0, initNode);
        gameObject.GetComponent<LineRenderer>().SetPosition(1, endNode);
    }
}
