using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

public class NetworkView : MonoBehaviour
{
    public static NetworkView instance;

    int totalpapers;
    Database db;

    public GameObject NodePrefab;
    List<GameObject> papers;

    float x, y, z, radius;
    Color color;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        db = Database.instance;
        totalpapers = db.getTotalPapers();

        papers = new List<GameObject>();

        for (int i = 0; i < totalpapers; i++)
        {
            x = UnityEngine.Random.Range(-20, 20);
            y = UnityEngine.Random.Range(-20, 20);
            z = UnityEngine.Random.Range(-20, 20);

            radius = UnityEngine.Random.Range(0, 10);

            papers.Add(Instantiate(NodePrefab));
            papers[i].GetComponent<Paper>().setValues(i);
            papers[i].GetComponent<Paper>().setPosition(x, y, z);
            papers[i].GetComponent<Paper>().setColor(Color.magenta);
        }

    }
}


