using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;

public class PaperData : MonoBehaviour
{
    public Dictionary<int, string> origin;
    public static PaperData instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start() 
    {
        origin = new Dictionary<int, string>();
        ReadCsv("origin.csv");
    }

    void ReadCsv(string filename) 
    {
        // open the file "Assets/Resources/origin.csv" which is a CSV file with headers
        using (CachedCsvReader csv = new CachedCsvReader(new StreamReader("Assets/Resources/" + filename), true, ';')) 
        {
            while (csv.ReadNextRecord()) 
            {
                string column1 = csv[0];
                string column2 = csv[1];
                origin.Add(int.Parse(column1), column2);
            }
        }
    }
}
