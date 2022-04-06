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

public class PracticesAndStrategies : MonoBehaviour
{
    //Instances
    public static PracticesAndStrategies instance;
    Database db;

    //UI Elements and Interaction
    public GameObject popUp;
    public Button closePopUpButton;
    public Text title;
    public Text puboutletname;
    public Text puboutleturl;
    public Text puboutletissn;
    public GameObject authorPanel;
    public GameObject authorPrefab;

    //Variables
    int totalpapers;
    float radius;

    //Visual Objects
    public GameObject NodePrefab;
    List<PaperView> papers;

    public GameObject CategoryPrefab;
    List<CategoryView> practices;
    List<CategoryView> strategies;

    public GameObject ConnectionPrefab;
    List<GameObject> PractConnections, StratConnections;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        radius = 80;

        db = Database.instance;       

        //Draw Practices
        List<int> temppractices = db.getPractices();
        int totalpract = temppractices.Count;
        
        practices = new List<CategoryView>();


        for (int i = 0; i < totalpract; i++)
        {
            CategoryView newPractice = Instantiate(CategoryPrefab).GetComponent<CategoryView>();
            newPractice.bootstrapPractices(temppractices[i]);
            
            practices.Add(newPractice);
        }

        //Draw Strategies
        List<int> tempstrategies = db.getStrategies();
        int totalstrat = tempstrategies.Count;
        strategies = new List<CategoryView>();

        for (int i = 0; i < totalstrat; i++)
        {
            CategoryView newStrategy = Instantiate(CategoryPrefab).GetComponent<CategoryView>();
            newStrategy.bootstrapStrategies(tempstrategies[i]);

            strategies.Add(newStrategy);    
        }

        //Draw Papers
        List<int> practicesandstrategies = db.getPapersWithPracticesOrStrategies();
        totalpapers = practicesandstrategies.Count;

        papers = new List<PaperView>();

        for (int i = 0; i < totalpapers; i++)
        {
            PaperView newPaper = Instantiate(NodePrefab).GetComponent<PaperView>();
            newPaper.bootstrap(practicesandstrategies[i]);
            
            papers.Add(newPaper);
        }

        


        //for (int i = 0; i < totalpapers; i++)
        //{
        //    PaperView paperTemp = papers[i].GetComponent<PaperView>();
        //    Vector3 paperpos = paperTemp.getPaperPosition();
        //
        //    int paperPractices = paperTemp.getPractices().Count;
        //    int paperStrategies = paperTemp.getStrategies().Count;
        //
        //    for (int j = 0; j < paperPractices; j++)
        //    {
        //        for (int k = 0; k < totalpract; k++)
        //        {
        //            CategoryView practiceTemp = practices[k].GetComponent<CategoryView>();
        //            Vector3 catPos = practiceTemp.getCategoryPosition();
        //
        //            if (practiceTemp.getId() == paperTemp.getPractices()[j])
        //            {
        //                GameObject connection = Instantiate(ConnectionPrefab);
        //                PractConnections.Add(connection);
        //                connection.GetComponent<ConnectionView>().updateConnection(paperpos, catPos);
        //                practices[k].GetComponent<CategoryView>().setRadius(0.1f);
        //            }
        //        }
        //    }
        //
        //    for (int j = 0; j < paperStrategies; j++)
        //    {
        //        for (int k = 0; k < totalstrat; k++)
        //        {
        //            CategoryView strategyTemp = strategies[k].GetComponent<CategoryView>();
        //            Vector3 catPos = strategyTemp.getCategoryPosition();
        //
        //            if (strategyTemp.getId() == paperTemp.getStrategies()[j])
        //            {
        //                GameObject connection = Instantiate(ConnectionPrefab);
        //                StratConnections.Add(connection);
        //                connection.GetComponent<ConnectionView>().updateConnection(paperpos, catPos);
        //                strategies[k].GetComponent<CategoryView>().setRadius(0.1f);
        //            }
        //        }
        //    }
        //}

        closePopUpButton.onClick.AddListener(closePopUp);
    }

    private void Update()
    {
        for (int i = 0; i < papers.Count; i++)
        {
            if (papers[i].GetComponent<PaperView>().mousePressed)
            {
                popUp.SetActive(true);
                title.text = papers[i].GetComponent<PaperView>().paper.title;
                puboutletname.text = papers[i].GetComponent<PaperView>().paper.publication_outlet.name;
                puboutleturl.text = papers[i].GetComponent<PaperView>().paper.publication_outlet.url;
                puboutletissn.text = papers[i].GetComponent<PaperView>().paper.publication_outlet.issn;
            }
        }

    }

    public void closePopUp()
    {
        for (int i = 0; i < papers.Count; i++)
        {
            papers[i].GetComponent<PaperView>().mousePressed = false;
        }
        popUp.SetActive(false);
    }

    public void deactivatePapers(int type, string name)
    {
        List<int> results = db.filterByAuthorJournalInstitution(type, name);

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
