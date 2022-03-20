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

public class UsesView : MonoBehaviour
{
    public static UsesView instance;

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

        List<int> toDestroy = new List<int>(); // COULD HAVE A BETTER WAY, ONLY INSTANTIATE THE ONES WITH THE RIGHT IDs SOMEHOW
        for (int i = 0; i < totalpapers; i++)
        {
            papers.Add(Instantiate(NodePrefab));

            x = UnityEngine.Random.Range(-20, 20);
            y = UnityEngine.Random.Range(-20, 20);
            z = UnityEngine.Random.Range(-20, 20);

            radius = UnityEngine.Random.Range(0, 10);

            papers[i].GetComponent<Paper>().setValues(i);
            papers[i].GetComponent<Paper>().setPosition(x, y, z);
            papers[i].GetComponent<Paper>().setColor(Color.magenta);

            if (papers[i].GetComponent<Paper>().use.Count < 1)
            {
                toDestroy.Add(i);
            }
        }   

        for(int i = 0; i < toDestroy.Count; i++)
        {
            Destroy(papers[i].gameObject);
        }
    }
}
