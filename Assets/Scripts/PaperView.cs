using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;

public class PaperView :  MonoBehaviour
{    
    public Paper paper { get; private set; }

    //UI Elements
    public GameObject titleBox;
    public GameObject canvas;

    public bool mousePressed = false;

    public void bootstrap(int id)
    {
        paper = Database.instance.getPaperById(id);
        titleBox.GetComponent<Text>().text = this.paper.title;
    }

    public int getId()
    {
        return this.paper.id;
    }

    public void setPosition(float x, float y, float z)
    {
        gameObject.transform.position = new Vector3(x, y, z);
    }

    public void setPositionSphere(Vector3 sphere)
    {
        gameObject.transform.position = sphere;
    }

    public void setPositionByUse(Vector3[] UseCenterPoints) // will be changed
    {        
        Vector3 paperPosition = UseCenterPoints[this.paper.use[0].id - 1];

        //if (this.use.Count > 1)
        //{
        //    List<Vector3> associatedCenters = new List<Vector3>();
        //    
        //    for (int i = 0; i < this.use.Count; i++)
        //    {
        //        associatedCenters.Add(UseCenterPoints[this.use[i].id - 1]);
        //    }
        //
        //    float xSum = 0;
        //    float ySum = 0;
        //    float zSum = 0;
        //
        //    for (int i = 0; i < associatedCenters.Count; i++)
        //    {
        //            xSum = xSum + associatedCenters[i].x;
        //            ySum = ySum + associatedCenters[i].y;
        //            zSum = zSum + associatedCenters[i].z;
        //    }
        //
        //    float x = xSum / associatedCenters.Count;
        //    float y = ySum / associatedCenters.Count;
        //    float z = zSum / associatedCenters.Count;
        //
        //    paperPosition = new Vector3(x, y, z);
        //}

        this.gameObject.transform.position = paperPosition + new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5));
    }

    public void setColor(Color color)
    {
        gameObject.GetComponentInChildren<Renderer>().material.color = color;
    }

    public void setColorByUse() // will be changed
    {
        Material sphereToColor = this.gameObject.GetComponentInChildren<Renderer>().material;
        
        switch (this.paper.use[0].id)
        {
            case 1:
                sphereToColor.color = Color.blue;
                break;
            case 2:
                sphereToColor.color = Color.red;
                break;
            case 3:
                sphereToColor.color = Color.yellow;
                break;
            case 4:
                sphereToColor.color = Color.magenta;
                break;
            case 5:
                sphereToColor.color = Color.cyan;
                break;
            case 6:
                sphereToColor.color = Color.green;
                break;
            case 7:
                sphereToColor.color = Color.grey;
                break;
            case 8:
                sphereToColor.color = new Color(1f, 0.5f, 0.2f);
                break;
            case 9:
                sphereToColor.color = new Color(0.5f, 1f, 0.2f);
                break;
            case 10:
                sphereToColor.color = new Color(0.2f, 0.5f, 1f);
                break;
            case 11:
                sphereToColor.color = new Color(1f, 0.2f, 0.5f);
                break;
            case 12:
                sphereToColor.color = new Color(0.2f, 1f, 0.5f);
                break;
            case 13:
                sphereToColor.color = new Color(0.5f, 0.2f, 1f);
                break;
            case 14:
                sphereToColor.color = new Color(0.6f, 0.7f, 0.2f);
                break;
            case 15:
                sphereToColor.color = new Color(0.2f, 0.8f, 0.4f);
                break;
            case 16:
                sphereToColor.color = new Color(0.4f, 1f, 0.2f);
                break;
            case 17:
                sphereToColor.color = new Color(0.8f, 0.2f, 0.9f);
                break;
        }
    }

    private void OnMouseDown()
    {
        this.mousePressed = true;
    }

    private void OnMouseEnter()
    {
        this.canvas.SetActive(true);
    }

    private void OnMouseExit()
    {
        this.canvas.SetActive(false);
    }
}





