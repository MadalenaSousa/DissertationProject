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
    public GameObject parentObject;

    public GameObject NodePrefab;
    List<PaperView> papers;

    public GameObject CategoryPrefab;
    List<CategoryView> practices;
    List<CategoryView> strategies;

    public GameObject ConnectionPrefab;
    List<ConnectionView> connections;

    //Still Testing
    List<ClusterThreshold> thresholds = new List<ClusterThreshold>();

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
            CategoryView newPractice = Instantiate(CategoryPrefab, parentObject.transform).GetComponent<CategoryView>();
            newPractice.bootstrapPractices(temppractices[i]);
            
            practices.Add(newPractice);
        }

        //Draw Strategies
        List<int> tempstrategies = db.getStrategies();
        int totalstrat = tempstrategies.Count;
        strategies = new List<CategoryView>();

        for (int i = 0; i < totalstrat; i++)
        {
            CategoryView newStrategy = Instantiate(CategoryPrefab, parentObject.transform).GetComponent<CategoryView>();
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
            PaperView newPaper = Instantiate(NodePrefab, parentObject.transform).GetComponent<PaperView>();
            newPaper.bootstrap(practicesandstrategies[i]);
            
            papers.Add(newPaper);

            for(int j = 0; j < newPaper.connections.Count; j++)
            {
                ConnectionView newConnection = Instantiate(ConnectionPrefab, parentObject.transform).GetComponent<ConnectionView>();
                
                Vector3 paperLocation = newPaper.gameObject.transform.position;
                Vector3 categoryLocation = new Vector3(0, 0, 0);
                
                if(newPaper.connections[j].category is Practice)
                {
                    for(int k = 0; k < practices.Count; k++)
                    {
                        if(newPaper.connections[j].category.id == practices[k].category.id)
                        {
                            categoryLocation = practices[k].gameObject.transform.position;
                            practices[k].totalConnections++;
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
                            strategies[k].totalConnections++;
                        }
                    }
                }

                newConnection.setConnection(paperLocation, categoryLocation, newPaper.getId(), newPaper.connections[j].category);
                connections.Add(newConnection);
            }
        }

        //--- OTHER STUFF ---//
        for (int i = 0; i < totalstrat; i++)
        {
            strategies[i].setRadius(strategies[i].totalConnections/2);
        }

        for (int i = 0; i < totalpract; i++)
        {
            practices[i].setRadius(practices[i].totalConnections/2);
        }
        
        closePopUpButton.onClick.AddListener(closePopUp);

        //setClusters(50);
        //setCategoryPositions();

    }

    private void Update()
    {
        openPopUp();
        clearConnectionsWithPapers();       
    }

    public void setCategoryPositions() // this one i think is right
    {
        for(int i = 0; i < strategies.Count; i++)
        {
            for(int j = 0; j < thresholds.Count; j++)
            {
                if(strategies[i].totalConnections > thresholds[j].minValue && strategies[i].totalConnections <= thresholds[j].maxValue)
                {
                    int avg = (thresholds[j].minValue + thresholds[j].maxValue) / 2;
                    int xOffset = Mathf.Abs(strategies[i].totalConnections - avg) * getRandom(-1, 1);
                    int yOffset = Mathf.Abs(strategies[i].totalConnections - avg) * getRandom(-1, 1);
                    int zOffset = Mathf.Abs(strategies[i].totalConnections - avg) * getRandom(-1, 1);

                    Vector3 offsetV = new Vector3(xOffset, yOffset, zOffset);
                        
                    strategies[i].setPosition((thresholds[j].clusterCenter + offsetV));
                }
            }
        }

        for (int i = 0; i < practices.Count; i++)
        {
            for (int j = 0; j < thresholds.Count; j++)
            {
                if (practices[i].totalConnections > thresholds[j].minValue && practices[i].totalConnections <= thresholds[j].maxValue)
                {
                    int avg = (thresholds[j].minValue + thresholds[j].maxValue) / 2;
                    int xOffset = Mathf.Abs(practices[i].totalConnections - avg) * getRandom(-1, 1);
                    int yOffset = Mathf.Abs(practices[i].totalConnections - avg) * getRandom(-1, 1);
                    int zOffset = Mathf.Abs(practices[i].totalConnections - avg) * getRandom(-1, 1);

                    Vector3 offsetV = new Vector3(xOffset, yOffset, zOffset);

                    practices[i].setPosition((thresholds[j].clusterCenter + offsetV));
                }
            }
        }  
    }

    public void setClusters(int res) //this one i think is wrong
    {
        List<int> allConnections = new List<int>();

        for (int i = 0; i < strategies.Count; i++)
        {
            allConnections.Add(strategies[i].totalConnections);
        }

        for (int i = 0; i < practices.Count; i++)
        {
            allConnections.Add(practices[i].totalConnections);
        }

        int totalClusters = MaxValue(allConnections.ToArray()) / res;

        for(int i = 0; i < allConnections.Count; i = i + totalClusters)
        {
            int min = i;
            int max = i + totalClusters;

            thresholds.Add(new ClusterThreshold(min, max));
        }

        for(int i = 0; i < thresholds.Count; i++)
        {
            int x = i * 5 * getRandom(-1, 1);
            int y = i * 5 * getRandom(-1, 1);
            int z = i * 5 * getRandom(-1, 1);

            thresholds[i].clusterCenter = new Vector3(x, y, z);
        }
    }

    public int getRandom(int first, int second)
    {
        if (UnityEngine.Random.value < 0.5f)
        {
            return first;
        }
        else
        {
            return second;
        }
    }

    public float mapValues(float value, float currentMin, float currentMax, float newMin, float newMax)
    {
        return (value - currentMin) * (newMax - newMin) / (currentMax - currentMin) + newMin;
    }

    public void closePopUp()
    {
        for (int i = 0; i < papers.Count; i++)
        {
            papers[i].mousePressed = false;
            
            foreach (Transform child in authorPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        popUp.SetActive(false);
    }

    public int MaxValue(int[] intArray) 
    {    
        int max = intArray[0];

            for (int i = 1; i<intArray.Length; i++) {

                if (intArray[i] > max) {
                    max = intArray[i];
                }
            }

        return max;
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

                Debug.Log(papers[i].paper.author.Count);

                for(int j = 0; j < papers[i].paper.author.Count; j++)
                {
                    GameObject author = Instantiate(authorPrefab, authorPanel.transform);
                    author.GetComponentsInChildren<Text>()[0].text = papers[i].paper.author[j].name;
                    author.GetComponentsInChildren<Text>()[1].text = papers[i].paper.author[j].institution.name;
                }  

                papers[i].mousePressed = false;
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

    public void updateConnectionPos()
    {
        for (int i = 0; i < papers.Count; i++)
        {
            for (int j = 0; j < connections.Count; j++)
            {
                Vector3 paperLocation = connections[i].paperPos;
                Vector3 categoryLocation = connections[i].catPos;

                if (papers[i].paper.id == connections[i].connection.paperId)
                {
                    paperLocation = papers[i].transform.position;
                }

                if (connections[i].connection.category is Practice)
                {
                    for (int k = 0; k < practices.Count; k++)
                    {
                        if (connections[j].connection.category.id == practices[k].category.id)
                        {
                            categoryLocation = practices[k].gameObject.transform.position;
                        }
                    }
                }
                else if (connections[i].connection.category is Strategy)
                {
                    for (int k = 0; k < strategies.Count; k++)
                    {
                        if (connections[j].connection.category.id == strategies[k].category.id)
                        {
                            categoryLocation = strategies[k].gameObject.transform.position;
                        }
                    }
                }

                connections[j].updateConnection(paperLocation, categoryLocation);
            }
        }
    }
}
