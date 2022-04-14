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

    public void bootstrap(int id)
    {
        paper = Database.instance.getPaperById(id); //Set paper data
        
        gameObject.transform.position = UnityEngine.Random.insideUnitSphere * 250; //Set visual characteristics
        gameObject.GetComponentInChildren<Renderer>().material.color = Color.white;

        titleBox.GetComponent<Text>().text = this.paper.title; //Set UI

        //Set Connections
        connections = new List<Connection>();

        for (int i = 0; i < paper.practice.Count; i++)
        {
            Connection newConnection = new Connection(paper.id, paper.practice[i]);
            connections.Add(newConnection);
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
}





