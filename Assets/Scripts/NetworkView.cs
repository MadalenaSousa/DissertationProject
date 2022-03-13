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
    public List<GameObject> papers;
    PaperData data;
    public List<Paper> nodes = new List<Paper>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        data = PaperData.instance;
        
        papers = new List<GameObject>();

        for (int i = 0; i < data.fullpapers.Count; i++)
        {
            papers.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            
            papers[i].AddComponent<Paper>();
            papers[i].GetComponent<Paper>().title = data.fullpapers[i].title;
            
            papers[i].GetComponent<Paper>().x = UnityEngine.Random.Range(-50f, 50f);
            papers[i].GetComponent<Paper>().y = UnityEngine.Random.Range(-50f, 50f);
            papers[i].GetComponent<Paper>().z = UnityEngine.Random.Range(-50f, 50f);
            papers[i].GetComponent<Paper>().radius = UnityEngine.Random.Range(-50f, 50f);           
        }

    }
}


