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
    public GameObject node;
    public List<GameObject> papers;
    
    PaperData data;

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
            GameObject temp = Instantiate(node);
           
            temp.GetComponent<Paper>().title = data.fullpapers[i].title;

            if(data.fullpapers[i].date != null)
            {
                temp.GetComponent<Paper>().year = int.Parse(data.fullpapers[i].date);
            } 
            else
            {
                temp.GetComponent<Paper>().year = 0000;
            }

            //temp.GetComponent<Paper>().practice_id = data.fullpapers[i].practice_id;

            switch(temp.GetComponent<Paper>().practice_id)
            {
                case 1:
                    temp.GetComponent<Renderer>().material.color = Color.blue;
                    break;
                case 2:
                    temp.GetComponent<Renderer>().material.color = Color.red;
                    break;
                case 3:
                    temp.GetComponent<Renderer>().material.color = Color.yellow;
                    break;
                case 4:
                    temp.GetComponent<Renderer>().material.color = Color.magenta;
                    break;
                case 5:
                    temp.GetComponent<Renderer>().material.color = Color.cyan;
                    break;
            }

            temp.GetComponent<Paper>().x = UnityEngine.Random.Range(-50f, 50f);
            temp.GetComponent<Paper>().y = UnityEngine.Random.Range(-50f, 50f);
            temp.GetComponent<Paper>().z = UnityEngine.Random.Range(-50f, 50f);
            temp.GetComponent<Paper>().radius = UnityEngine.Random.Range(-50f, 50f);

            papers.Add(temp);
        }

    }
}


