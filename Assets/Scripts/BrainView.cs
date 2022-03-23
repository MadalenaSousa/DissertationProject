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
    
    public GameObject popUp;
    public Button closePopUpButton;
    public Text title;
    public Text puboutletname;
    public Text puboutleturl;
    public Text puboutletissn;
    public GameObject authorPanel;
    public GameObject authorPrefab;

    int totalpapers;
    Database db;

    public GameObject NodePrefab;
    List<PaperView> papers;

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

        papers = new List<PaperView>();

        for (int i = 0; i < totalpapers; i++)
        {
            radius = 20;

            papers.Add(Instantiate(NodePrefab).GetComponent<PaperView>());
            papers[i].transform.parent = parentObject.transform;
            papers[i].bootstrap(i);
            papers[i].setPositionSphere(UnityEngine.Random.insideUnitSphere * radius);
            papers[i].setColor(Color.magenta);
        }

        closePopUpButton.onClick.AddListener(closePopUp);

    }

    private void Update()
    {
        for(int i = 0; i < papers.Count; i++)
        {
            if (papers[i].mousePressed)
            {
                popUp.SetActive(true);
                title.text = papers[i].paper.title;
                puboutletname.text = papers[i].paper.publication_outlet.name;
                puboutleturl.text = papers[i].paper.publication_outlet.url;
                puboutletissn.text = papers[i].paper.publication_outlet.issn;
            }
        }
        
    }

    public void closePopUp()
    {
        for (int i = 0; i < papers.Count; i++)
        {
            papers[i].mousePressed = false;
        }
        popUp.SetActive(false);
    }

    public void deactivatePapers(int type, string name)
    {
        List<int> results = db.filterByAuthorJournalInstitution(type, name);

        for(int i = 0; i < papers.Count; i++)
        {
            if(results.Contains(papers[i].GetComponent<PaperView>().getId()))
            {
                papers[i].gameObject.SetActive(true);
            } 
            else
            {
                papers[i].gameObject.SetActive(false);
            }
        }
    }

    public void deactivatePapersByYear(int min, int max)
    {
        List<int> results = db.getPapersByYearInterval(min, max);

        for (int i = 0; i < papers.Count; i++)
        {
            if (results.Contains(papers[i].GetComponent<PaperView>().getId()))
            {
                papers[i].gameObject.SetActive(true);
            }
            else
            {
                papers[i].gameObject.SetActive(false);
            }
        }
    }

    public void resetPapers()
    {
        for (int i = 0; i < papers.Count; i++)
        {
            papers[i].gameObject.SetActive(true);
        }
    }
}


