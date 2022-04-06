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

    Vector3[] UseCenterPoints;
    int totalUses;

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

        totalUses = 17;

        UseCenterPoints = new Vector3[totalUses]; // will be changed

        //for(int i = 0; i < totalUses; i ++)
        //{
        //    UseCenterPoints[i] = new Vector3(UnityEngine.Random.Range(-500, 500), UnityEngine.Random.Range(-500, 500), UnityEngine.Random.Range(-500, 500));
        //}

        UseCenterPoints[0] = new Vector3(40, 40, 40);
        UseCenterPoints[1] = new Vector3(-40, -40, -40);
        UseCenterPoints[2] = new Vector3(40, 40, -40);
        UseCenterPoints[3] = new Vector3(40, -40, 40);
        UseCenterPoints[4] = new Vector3(-40, 40, 40);
        UseCenterPoints[5] = new Vector3(40, -40, -40);
        UseCenterPoints[6] = new Vector3(-40, 40, -40);
        UseCenterPoints[7] = new Vector3(-40, -40, 40);
        UseCenterPoints[8] = new Vector3(0, 0, 0);
        UseCenterPoints[9] = new Vector3(20, 20, 20);
        UseCenterPoints[10] = new Vector3(-20, -20, -20);
        UseCenterPoints[11] = new Vector3(-20, 20, 20);
        UseCenterPoints[12] = new Vector3(20, -20, 20);
        UseCenterPoints[13] = new Vector3(20, 20, -20);
        UseCenterPoints[14] = new Vector3(-20, -20, 20);
        UseCenterPoints[15] = new Vector3(20, -20, -20);
        UseCenterPoints[16] = new Vector3(-20, 20, -20);

        List<int> toDraw = db.getPapersWithUses();
        totalpapers = toDraw.Count;

        papers = new List<GameObject>();

        for (int i = 0; i < totalpapers; i++)
        {
            papers.Add(Instantiate(NodePrefab));

        }

        for(int i = 0; i < papers.Count; i++) 
        { 
            x = UnityEngine.Random.Range(-20, 20);
            y = UnityEngine.Random.Range(-20, 20);
            z = UnityEngine.Random.Range(-20, 20);

            radius = 10;

            papers[i].GetComponent<PaperView>().bootstrap(toDraw[i]);
            papers[i].GetComponent<PaperView>().setPositionByUse(UseCenterPoints);
            papers[i].GetComponent<PaperView>().setColorByUse();
        }
        
    }

    
}
