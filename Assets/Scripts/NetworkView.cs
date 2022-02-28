using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;

public class NetworkView : MonoBehaviour
{
    public static NetworkView instance;
    public List<GameObject> papers;
    PaperData data;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }    
    
        data = PaperData.instance;
    }
    
    void Start()
    {   
        papers = new List<GameObject>();
        
        foreach(KeyValuePair<int, string> pair in data.origin)
        {
            papers.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
        }

        for(int i = 0; i < papers.Count; i++)
        {
            papers[i].AddComponent<Paper>();
            papers[i].GetComponent<Paper>().title = data.origin[i + 1];
            papers[i].GetComponent<Paper>().x = UnityEngine.Random.Range(-50f, 50f);
            papers[i].GetComponent<Paper>().y = UnityEngine.Random.Range(-50f, 50f);
            papers[i].GetComponent<Paper>().z = UnityEngine.Random.Range(-50f, 50f);
            papers[i].GetComponent<Paper>().radius = UnityEngine.Random.Range(-50f, 50f);
        }
    }

    void Update()
    {
        
    }
}
