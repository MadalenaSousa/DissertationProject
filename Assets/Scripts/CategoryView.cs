using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;

public class CategoryView : MonoBehaviour
{
    public Category category;

    //UI Elements
    public GameObject titleBox;

    public void bootstrap(int id, int type)
    {
        if (type == 0)
        {
            category = Database.instance.getPracticeById(id); 
        } 
        else if (type == 1)
        {
            category = Database.instance.getStrategyById(id);
        }

        titleBox.GetComponent<Text>().text = this.category.name;
    }

    public void setPositionSphere(Vector3 sphere)
    {
        gameObject.transform.position = sphere;
    }

    public void setColor(Color color)
    {
        gameObject.GetComponentInChildren<Renderer>().material.color = color;
    }

    public Vector3 getCategoryPosition()
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
 