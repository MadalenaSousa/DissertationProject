using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;

public class CategoryView : MonoBehaviour
{
    //Instances
    public Category category;

    //UI Elements and Interactions
    public GameObject titleBox;

    //Other Variables
    public int totalConnections = 0;

    public void bootstrapPractices(int id)
    {
        category = Database.instance.getPracticeById(id); //Set category data

        gameObject.transform.position = UnityEngine.Random.insideUnitSphere * 250; //Set visual
        gameObject.GetComponentInChildren<Renderer>().material.color = Color.cyan;
        gameObject.GetComponentInChildren<Transform>().localScale = gameObject.GetComponentInChildren<Transform>().localScale * 3;

        titleBox.GetComponent<Text>().text = this.category.name; //Set UI
    }

    public void bootstrapStrategies(int id)
    {
        category = Database.instance.getStrategyById(id); //Set category data

        gameObject.transform.position = UnityEngine.Random.insideUnitSphere * 250; //Set visual
        gameObject.GetComponentInChildren<Renderer>().material.color = Color.yellow;
        gameObject.GetComponentInChildren<Transform>().localScale = gameObject.GetComponentInChildren<Transform>().localScale * 3;

        titleBox.GetComponent<Text>().text = this.category.name; //Set UI
    }

    public void setPosition(Vector3 newPosition)
    {
        gameObject.transform.position = newPosition;   
    }

    public void setColor(Color color)
    {
        gameObject.GetComponentInChildren<Renderer>().material.color = color;
    }

    public Vector3 getPosition()
    {
        return gameObject.transform.position;
    }

    public int getId()
    {
        return category.id;
    }

    public void setRadius(float radius)
    {
        gameObject.GetComponentInChildren<Transform>().localScale = gameObject.GetComponentInChildren<Transform>().localScale + new Vector3(radius, radius, radius);
    }
}
 