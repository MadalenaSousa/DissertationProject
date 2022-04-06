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

public class StrategiesView : MonoBehaviour
{
    public static StrategiesView instance;

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
        List<int> toDraw = db.getPapersWithStrategies();
        totalpapers = toDraw.Count;

        papers = new List<GameObject>();

        for (int i = 0; i < totalpapers; i++)
        {
            radius = 20;
            
            papers.Add(Instantiate(NodePrefab));
            papers[i].GetComponent<PaperView>().bootstrap(toDraw[i]);
        }
    }
}
