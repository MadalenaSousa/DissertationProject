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
    public GameObject[] papers;
    

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
        papers = new GameObject[totalpapers];

        for(int i = 0; i < totalpapers; i++)
        {
            papers[i] = Instantiate(NodePrefab);
            papers[i].gameObject.transform.position = new Vector3(0, 0, 0);
            papers[i].gameObject.GetComponent<SphereCollider>().radius = 50;
        }

    }
}


