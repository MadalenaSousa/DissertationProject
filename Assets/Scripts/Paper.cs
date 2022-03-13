using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;

public class Paper : MonoBehaviour
{
    public string title, institution, author, journal;
    public int id, year, use_id, practice_id, strategy_id;
    public float x, y, z, radius;
    public Paper instance;
    
    public Paper() 
    {   
        x = 0;
        y = 0;
        z = 0;
        radius = 100;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        gameObject.transform.position = new Vector3(x, y, z);
        gameObject.GetComponent<SphereCollider>().radius = radius;
    }

    void Update()
    {
        
    }
}
