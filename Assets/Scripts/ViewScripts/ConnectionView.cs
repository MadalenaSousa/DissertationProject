using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionView : MonoBehaviour
{
    //Instances
    public Connection connection;

    //Other
    public Vector3 paperPos, catPos;

    public void setConnection(Vector3 initNode, Vector3 endNode, int paperId, Category category)
    {
        connection = new Connection(paperId, category);

        gameObject.GetComponent<LineRenderer>().SetPosition(0, initNode);
        gameObject.GetComponent<LineRenderer>().SetPosition(1, endNode);

        paperPos = initNode;
        catPos = endNode;
    }

    public void updateConnection(Vector3 initNode, Vector3 endNode)
    {
        gameObject.GetComponent<LineRenderer>().SetPosition(0, initNode);
        gameObject.GetComponent<LineRenderer>().SetPosition(1, endNode);
    }
}
