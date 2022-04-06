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

    //Visual Objects
    public GameObject NodePrefab;
    List<PaperView> papers;

    public GameObject CategoryPrefab;
    List<CategoryView> practices;
    List<CategoryView> strategies;

    public GameObject ConnectionPrefab;
    List<ConnectionView> connections;

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

        //--- BASE DRAWING ---//

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

        //Draw Papers & Connections
        List<int> practicesandstrategies = db.getPapersWithPracticesOrStrategies();
        totalpapers = practicesandstrategies.Count;

        papers = new List<PaperView>();
        connections = new List<ConnectionView>();

        for (int i = 0; i < totalpapers; i++)
        {
            PaperView newPaper = Instantiate(NodePrefab).GetComponent<PaperView>();
            newPaper.bootstrap(practicesandstrategies[i]);
            
            papers.Add(newPaper);

            for(int j = 0; j < newPaper.connections.Count; j++)
            {
                ConnectionView newConnection = Instantiate(ConnectionPrefab).GetComponent<ConnectionView>();
                
                Vector3 paperLocation = newPaper.gameObject.transform.position;
                Vector3 categoryLocation = new Vector3(0, 0, 0);
                
                if(newPaper.connections[j].category is Practice)
                {
                    for(int k = 0; k < practices.Count; k++)
                    {
                        if(newPaper.connections[j].category.id == practices[k].category.id)
                        {
                            categoryLocation = practices[k].gameObject.transform.position;
                        }
                    }
                } 
                else if(newPaper.connections[j].category is Strategy)
                {
                    for (int k = 0; k < strategies.Count; k++)
                    {
                        if (newPaper.connections[j].category.id == strategies[k].category.id)
                        {
                            categoryLocation = strategies[k].gameObject.transform.position;
                        }
                    }
                }

                newConnection.setConnection(paperLocation, categoryLocation, newPaper.getId(), newPaper.connections[j].category);
                connections.Add(newConnection);
            }
        }

        //--- OTHER STUFF ---//

        for(int i = 0; i < totalpract; i++)
        {
            for(int j = 0; i < connections.Count; j++)
            {
                if(connections[j].connection.category == practices[i].category)
                {
                    //practices[i].setRadius(connections);
                }
            }
            
        }

        closePopUpButton.onClick.AddListener(closePopUp);
    }

    private void Update()
    {
        openPopUp();
        clearConnectionsWithPapers();
    }

    public void closePopUp()
    {
        for (int i = 0; i < papers.Count; i++)
        {
            papers[i].mousePressed = false;
        }
        
        popUp.SetActive(false);
    }

    public void openPopUp()
    {
        for (int i = 0; i < papers.Count; i++)
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

    public void deactivatePapers(int type, string name)
    {
        List<int> results = db.filterByAuthorJournalInstitution(type, name);

        for (int i = 0; i < papers.Count; i++)
        {
            if (results.Contains(papers[i].getId()))
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
            if (results.Contains(papers[i].getId()))
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

    public void clearConnectionsWithPapers()
    {
        for(int i = 0; i < totalpapers; i++)
        {
            if(papers[i].gameObject.activeInHierarchy == false)
            {              
                for(int j = 0; j < connections.Count; j++)
                {
                    if(papers[i].getId() == connections[j].connection.paperId)
                    {
                        connections[j].gameObject.SetActive(false);
                    }
                }
            } 
            else
            {
                for (int j = 0; j < connections.Count; j++)
                {
                    if (papers[i].getId() == connections[j].connection.paperId)
                    {
                        connections[j].gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
