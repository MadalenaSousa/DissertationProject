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
using System.Linq;

public class PracticesView : MonoBehaviour
{
    public static PracticesView instance;

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
        List<int> toDraw = db.getPapersWithPractices();
        totalpapers = toDraw.Count;

        papers = new List<GameObject>();

        for (int i = 0; i < totalpapers; i++)
        {
            radius = 20;

            papers.Add(Instantiate(NodePrefab));

            papers[i].GetComponent<Paper>().setValues(toDraw[i]);
            papers[i].GetComponent<Paper>().setPositionSphere(UnityEngine.Random.insideUnitSphere * radius);
            papers[i].GetComponent<Paper>().setColor(Color.yellow);
        }
    }
}
