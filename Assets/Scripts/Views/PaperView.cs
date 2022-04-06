using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;

public class PaperView :  MonoBehaviour
{    
    //Instances
    public Paper paper { get; private set; }

    //UI Elements and Interactions
    public GameObject titleBox;
    public GameObject canvas;

    public bool mousePressed = false;

    //Variables
    public List<Connection> connections;
    //public GameObject connectionPrefab;

    public void bootstrap(int id)
    {
        paper = Database.instance.getPaperById(id); //Set paper data

        gameObject.transform.position = UnityEngine.Random.insideUnitSphere * 80; //Set visual characteristics
        gameObject.GetComponentInChildren<Renderer>().material.color = Color.white;

        titleBox.GetComponent<Text>().text = this.paper.title; //Set UI

        //Set Connections
        connections = new List<Connection>();

        for (int i = 0; i < paper.practice.Count; i++)
        {
            Connection newConnection = new Connection(paper.id, paper.practice[i]);
            connections.Add(newConnection);
            
            //GameObject connection = Instantiate(connectionPrefab);
            //connection.GetComponent<ConnectionView>().setConnection(gameObject.transform.position, new Vector3(0, 0, 0));
        }
        
        for (int i = 0; i < paper.strategy.Count; i++)
        {
            Connection newConnection = new Connection(paper.id, paper.strategy[i]);
            connections.Add(newConnection);
        }
    }

    public int getId()
    {
        return this.paper.id;
    }

    public void setPosition(Vector3 newPosition)
    {
        gameObject.transform.position = newPosition;
    }

    public Vector3 getPosition()
    {
        return gameObject.transform.position;
    }

    public void setColor(Color color)
    {
        gameObject.GetComponentInChildren<Renderer>().material.color = color;
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


    //TEST FUNCTIONS

    public void setPositionByUse(Vector3[] UseCenterPoints) // will be changed
    {        
        Vector3 paperPosition = UseCenterPoints[this.paper.use[0].id - 1];

        //if (this.paper.use.Count > 1)
        //{
        //    List<Vector3> associatedCenters = new List<Vector3>();
        //    
        //    for (int i = 0; i < this.paper.use.Count; i++)
        //    {
        //        associatedCenters.Add(UseCenterPoints[this.paper.use[i].id - 1]);
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

        //this.gameObject.transform.position = paperPosition;
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
}





