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

public class BrainView : MonoBehaviour
{
    public static BrainView instance;
    public GameObject parentObject;

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
            radius = 20;

            papers.Add(Instantiate(NodePrefab));
            papers[i].transform.parent = parentObject.transform;
            papers[i].GetComponent<Paper>().setValues(i);
            papers[i].GetComponent<Paper>().setPositionSphere(UnityEngine.Random.insideUnitSphere * radius);
            papers[i].GetComponent<Paper>().setColor(Color.magenta);
        }

    }

    public void deactivatePapers(int type, string name)
    {
        List<int> results = db.filterByAuthorJournalInstitution(type, name);

        for(int i = 0; i < papers.Count; i++)
        {
            if(results.Contains(papers[i].GetComponent<Paper>().getId()))
            {
                papers[i].SetActive(true);
            } 
            else
            {
                papers[i].SetActive(false);
            }
        }
    }

    public void resetPapers()
    {
        for (int i = 0; i < papers.Count; i++)
        {
            papers[i].SetActive(true);
        }
    }
}


