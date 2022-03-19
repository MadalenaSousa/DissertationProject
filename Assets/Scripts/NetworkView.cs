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
    List<Paper> papers;
    Database db;
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        //db = Database.instance;
        //db.StartDataSQLite();
        //totalpapers = db.getTotalPapers();
        //
        //for(int i = 0; i < totalpapers; i++)
        //{
        //    papers.Add(new Paper(i));
        //}
    }
}


