using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Cinemachine;

public class PracticesAndStrategies : MonoBehaviour
{
    //Instances
    public static PracticesAndStrategies instance;
    Database db;

    [Header("Visualization Parameters")]
    public int globalSphereRadius;
    public int minCatRadius, maxCatRadius;
    public int totalClusters;

    //UI Elements and Interaction
    [Header("Paper Pop Up UI")]
    public GameObject popUp;
    public Button closePopUpButton;
    public Text title;
    public Text puboutletname;
    public Text puboutleturl;
    public Text puboutletissn;
    public Text paperYear;
    public GameObject authorPanel;
    public GameObject authorPrefab;
    public GameObject practicesPanel;
    public GameObject strategiesPanel;
    public GameObject categoryNamePrefab;

    [Header("Category Pop Up UI")]
    public GameObject catPopUp;
    public Button closeCatPopUpButton;
    public Text catName;
    public Text catConns;
    public Text catInterval;  

    [Header("Cluster Info Pop Up UI")]
    public GameObject clusterInfoPanel;
    public GameObject clusterInfoPrefab;
    public GameObject clusterInfoBackground;

    //Visual Objects
    [Header("Element Prefabs")]
    public GameObject NodePrefab;
    public GameObject CategoryPrefab;
    public GameObject ConnectionPrefab;

    [Header("Other")]
    public GameObject parentObject;
    public Transform cameraTarget;
    public Dropdown switchCriteria;

    [HideInInspector]
    public List<PaperView> papers = new List<PaperView>();   
    
    Dictionary<int, CategoryView> strategiesViews = new Dictionary<int, CategoryView>();
    Dictionary<int, CategoryView> practicesViews = new Dictionary<int, CategoryView>();
       
    
    List<ConnectionView> connections =  new List<ConnectionView>();
    
    int clusterConnInterval, maxCriteriaValue, minCriteriaValue;   
    List<Cluster> clusters = new List<Cluster>();

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

        changeClusterCriteria();

        //OTHER
        closePopUpButton.onClick.AddListener(closePopUp);
        closeCatPopUpButton.onClick.AddListener(closeCatPopUp);

    }

    private void Update()
    {
        openPopUp();
        openCatPopUp();
        updateConnections();
        categoriesLookAtCamera();
    }

    public void changeClusterCriteria()
    {
        cleanAllGameObjects();

        if (switchCriteria.value == 1)
        {
            maxCriteriaValue = (int)mapValues(db.getMaxCitPS(), db.getMinCitPS(), db.getMaxCitPS(), 1, 500);
            //maxCriteriaValue = db.getMaxCitPS();
            minCriteriaValue = db.getMinCitPS();
        }
        else
        {
            maxCriteriaValue = (int)mapValues(db.getMaxConnPS(), db.getMinConnPS(), db.getMaxConnPS(), 1, 500);
            //maxCriteriaValue = db.getMaxConnPS();
            minCriteriaValue = db.getMinConnPS();
        }

        //CLUSTERS
        setClusters(maxCriteriaValue, minCriteriaValue);

        //CATEGORIES
        drawCategories(minCatRadius, maxCatRadius, switchCriteria);

        //PAPERS
        drawPapers();

        //Remove Empty Clusters
        List<Cluster> nonEmptyClusters = new List<Cluster>();
        foreach (Cluster cluster in clusters)
        {
            if (cluster.categories.Count > 0)
            {
                nonEmptyClusters.Add(cluster);
            }
        }
        clusters = nonEmptyClusters;

        //INFO PANEL
        updateClusterInfoPanel();

        //FILTERS
        Filters.instance.filterPapers(); //Maintain filters

        //Filters.instance.clearFilter("author"); //Clear filters
        //Filters.instance.clearFilter("journal");
        //Filters.instance.clearFilter("institution");
        //Filters.instance.clearFilter("year");
    }

    private void cleanAllGameObjects()
    {
        foreach (Transform child in parentObject.transform)
        {
            Destroy(child.gameObject);
            papers = new List<PaperView>();
            connections = new List<ConnectionView>();
            strategiesViews = new Dictionary<int, CategoryView>();
            practicesViews = new Dictionary<int, CategoryView>();
            clusters = new List<Cluster>();
        }
    }

    public int getMaxConnYear()
    {
        int maxConn = 0;

        for(int i = 0; i < papers.Count; i++)
        {
            if(papers[i].gameObject.activeSelf)
            {
                if (papers[i].connections.Count > maxConn)
                {
                    maxConn = papers[i].connections.Count;
                }
            }           
        }

        //Debug.Log(maxConn);
        return maxConn;
    }

    public int getMinConnYear()
    {
        int minConn = db.getMaxConnPS();

        for (int i = 0; i < papers.Count; i++)
        {
            if (papers[i].gameObject.activeSelf)
            {
                if (papers[i].connections.Count < minConn)
                {
                    minConn = papers[i].connections.Count;
                }
            }
        }

        //Debug.Log(minConn);
        return minConn;
    }

    public void setClusters(int maxCriteriaValue, int minCriteriaValue)
    {
        //Debug.Log(maxCriteriaValue);
        clusterConnInterval = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(maxCriteriaValue) / Convert.ToDouble(totalClusters)));

        for (int i = 0; i <= totalClusters; i++)
        {
            int min = clusterConnInterval * i;
            int max = clusterConnInterval * (i + 1);

            clusters.Add(new Cluster(min, max)); //Save in general variable
        }
    }

    public void drawCategories(int minRadius, int maxRadius, Dropdown criteria)
    {
        List<int> PracticesId = db.getPractices();
        int totalPractices = PracticesId.Count;

        List<int> StrategiesId = db.getStrategies();
        int totalStrategies = StrategiesId.Count;        

        for (int i = 0; i < totalPractices; i++)
        {
            CategoryView newPractice = Instantiate(CategoryPrefab, parentObject.transform).GetComponent<CategoryView>();
            newPractice.bootstrapPractices(PracticesId[i]);
            newPractice.setCriteria(criteria);
            newPractice.setRadius(mapValues(newPractice.clusterCriteria, minCriteriaValue, maxCriteriaValue, minRadius, maxRadius));

            for (int j = 0; j < clusters.Count; j++)
            {
                if (newPractice.clusterCriteria > clusters[j].min && newPractice.clusterCriteria <= clusters[j].max)
                {
                    clusters[j].categories.Add(newPractice.category);
                    newPractice.setPosition(clusters[j].center + clusters[j].getOffsetVector(newPractice));
                    break;
                }
            }

            practicesViews.Add(PracticesId[i], newPractice); //Save in general variable
        }

        for (int i = 0; i < totalStrategies; i++)
        {
            CategoryView newStrategy = Instantiate(CategoryPrefab, parentObject.transform).GetComponent<CategoryView>();
            newStrategy.bootstrapStrategies(StrategiesId[i]);
            newStrategy.setCriteria(criteria);
            newStrategy.setRadius(mapValues(newStrategy.clusterCriteria, minCriteriaValue, maxCriteriaValue, minRadius, maxRadius));         

            for (int j = 0; j < clusters.Count; j++)
            {
                if (newStrategy.clusterCriteria > clusters[j].min && newStrategy.clusterCriteria <= clusters[j].max)
                {
                    clusters[j].categories.Add(newStrategy.category);
                    newStrategy.setPosition(clusters[j].center + clusters[j].getOffsetVector(newStrategy));
                    break;
                }
            }

            strategiesViews.Add(StrategiesId[i], newStrategy); //Save in general variable
        }
    }

    public void drawPapers()
    {
        List<int> PSPapersId = db.getPapersWithPracticesOrStrategies();
        int totalpapers = PSPapersId.Count;
        
        int singleConnOffsetRadius = 80;
        int paperPosOffset = 5;

        for (int i = 0; i < totalpapers; i++)
        {
            PaperView newPaper = Instantiate(NodePrefab, parentObject.transform).GetComponent<PaperView>();
            newPaper.bootstrap(PSPapersId[i]);
            newPaper.setRadius(newPaper.connections.Count);

            papers.Add(newPaper); //Save in general variable

            float xSum = 0;
            float ySum = 0;
            float zSum = 0;

            foreach (Strategy paperStrategy in newPaper.paper.strategy)
            {
                ConnectionView newConnection = Instantiate(ConnectionPrefab, parentObject.transform).GetComponent<ConnectionView>();
                newConnection.setConnection(newPaper, strategiesViews[paperStrategy.id]);
                connections.Add(newConnection);
                newPaper.conns.Add(newConnection);

                xSum += strategiesViews[paperStrategy.id].transform.position.x;
                ySum += strategiesViews[paperStrategy.id].transform.position.y;
                zSum += strategiesViews[paperStrategy.id].transform.position.z;
            }

            foreach (Practice paperPractice in newPaper.paper.practice)
            {
                ConnectionView newConnection = Instantiate(ConnectionPrefab, parentObject.transform).GetComponent<ConnectionView>();
                newConnection.setConnection(newPaper, practicesViews[paperPractice.id]);
                
                connections.Add(newConnection); //Save in general variable

                newPaper.conns.Add(newConnection);

                xSum += practicesViews[paperPractice.id].transform.position.x;
                ySum += practicesViews[paperPractice.id].transform.position.y;
                zSum += practicesViews[paperPractice.id].transform.position.z;
            }

            int totalPaperCategories = newPaper.conns.Count;
            Vector3 paperPos = new Vector3(xSum / totalPaperCategories, ySum / totalPaperCategories, zSum / totalPaperCategories);

            if (newPaper.conns.Count <= 1)
            {
                Vector3 altPos = paperPos + (UnityEngine.Random.insideUnitSphere * singleConnOffsetRadius);
                newPaper.setPosition(altPos);
            }
            else
            {
                for (int j = 0; j < papers.Count; j++)
                {
                    paperPosOffset = UnityEngine.Random.Range(0, 8);
                    Vector3 offsetVector = new Vector3(getRandom(-paperPosOffset, paperPosOffset), getRandom(-paperPosOffset, paperPosOffset), getRandom(-paperPosOffset, paperPosOffset));                   
                    
                    if (papers[i].transform.position == paperPos)
                    {
                        newPaper.setPosition(paperPos + offsetVector);
                    } 
                    else if(papers[i].transform.position == (paperPos + offsetVector))
                    {
                        newPaper.setPosition(paperPos + new Vector3(getRandom(-paperPosOffset, paperPosOffset), getRandom(-paperPosOffset, paperPosOffset), getRandom(-paperPosOffset, paperPosOffset)));
                    }
                    else
                    {
                        newPaper.setPosition(paperPos);
                    }
                }
            }
        }
    }

    private void categoriesLookAtCamera()
    {
        foreach (KeyValuePair<int, CategoryView> practice in practicesViews)
        {
            practice.Value.transform.rotation = cameraTarget.rotation;
        }

        foreach (KeyValuePair<int, CategoryView> strategy in strategiesViews)
        {
            strategy.Value.transform.rotation = cameraTarget.rotation;
        }

        foreach(PaperView papers in papers)
        {
            papers.transform.rotation = cameraTarget.rotation;
        }
    }

    private void updateConnections()
    {
        foreach (ConnectionView connection in connections)
        {
            connection.updateConnection();
        }
    }

    //UI FUNTIONS
    public void updateClusterInfoPanel()
    {
        foreach (Transform child in clusterInfoPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < clusters.Count; i++)
        {
            GameObject newClusterInfo = Instantiate(clusterInfoPrefab, clusterInfoPanel.transform);
            newClusterInfo.GetComponentsInChildren<Text>()[0].text = "CLUSTER " + i.ToString();
            //newClusterInfo.GetComponentsInChildren<Text>()[1].text = "[" + clusters[i].min + ", " + clusters[i].max + "]";
            newClusterInfo.GetComponentsInChildren<Text>()[1].text = "";

            string practiceList = "";
            string strategyList = "";

            for(int j = 0; j < clusters[i].categories.Count; j++)
            {
                if(clusters[i].categories[j] is Practice)
                {
                    practiceList = practiceList + clusters[i].categories[j].name + ", ";
                } 
                else
                {
                    strategyList = strategyList + clusters[i].categories[j].name + ", ";
                }
            }

            if(practiceList == "") { practiceList = "None. "; }
            if (strategyList == "") { strategyList = "None. "; }

            newClusterInfo.GetComponentsInChildren<Text>()[3].text = practiceList.Substring(0, practiceList.Length - 2);
            newClusterInfo.GetComponentsInChildren<Text>()[5].text = strategyList.Substring(0, strategyList.Length - 2);
        }
    }
    
    public void openClusterInfoPanel()
    {
        bool state = clusterInfoBackground.activeSelf;
        
        clusterInfoBackground.SetActive(!state);
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

            foreach (Transform child in practicesPanel.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in strategiesPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }

        popUp.SetActive(false);
    }

    public void closeCatPopUp()
    {
        foreach (KeyValuePair<int, CategoryView> strategy in strategiesViews)
        {
            strategy.Value.mousePressed = false;
        }

        foreach (KeyValuePair<int, CategoryView> practice in practicesViews)
        {
            practice.Value.mousePressed = false;
        }

        catPopUp.SetActive(false);
    }

    public void openPopUp()
    {
        for (int i = 0; i < papers.Count; i++)
        {
            PaperView paperView = papers[i];
            if (paperView.mousePressed)
            {
                popUp.SetActive(true);
                title.text = paperView.paper.title;
                puboutletname.text = paperView.paper.publication_outlet.name;
                puboutleturl.text = paperView.paper.publication_outlet.url;
                puboutletissn.text = paperView.paper.publication_outlet.issn;
                paperYear.text = paperView.paper.date;

                for (int j = 0; j < paperView.paper.author.Count; j++)
                {
                    Author author = paperView.paper.author[j];
                    GameObject authorView = Instantiate(authorPrefab, authorPanel.transform);
                    authorView.GetComponentsInChildren<Text>()[0].text = author.name;
                    authorView.GetComponentsInChildren<Text>()[1].text = author.institution.name;
                }

                for (int j = 0; j < paperView.paper.practice.Count; j++)
                {
                    Practice practice = paperView.paper.practice[j];
                    GameObject practiceView = Instantiate(categoryNamePrefab, practicesPanel.transform);
                    practiceView.GetComponentsInChildren<Text>()[0].text = practice.name;
                }

                for (int j = 0; j < paperView.paper.strategy.Count; j++)
                {
                    Strategy strategy = paperView.paper.strategy[j];
                    GameObject strategyView = Instantiate(categoryNamePrefab, strategiesPanel.transform);
                    strategyView.GetComponentsInChildren<Text>()[0].text = strategy.name;
                }

                paperView.mousePressed = false;

                break;
            }
        }
    }

    public void openCatPopUp()
    {
        foreach (KeyValuePair<int, CategoryView> strategy in strategiesViews)
        {
            if (strategy.Value.mousePressed)
            {
                catPopUp.SetActive(true);

                catName.text = strategy.Value.category.name;
                
                int criteriaValue = strategy.Value.getRawCriteria(switchCriteria);
                catConns.text = criteriaValue.ToString();

                int min = 0;
                int max = 0;

                for(int i = 0; i < clusters.Count; i++)
                {
                    if(clusters[i].categories.Contains(strategy.Value.category))
                    {
                        min = clusters[i].min;
                        max = clusters[i].max;
                    }
                }

                catInterval.text = "[ " + min + ", " + max + " ]";

                strategy.Value.mousePressed = false;

                break;
            }
        }

        foreach (KeyValuePair<int, CategoryView> practice in practicesViews)
        {
            if (practice.Value.mousePressed)
            {
                catPopUp.SetActive(true);

                catName.text = practice.Value.category.name;

                int criteriaValue = practice.Value.getRawCriteria(switchCriteria);
                catConns.text = criteriaValue.ToString();

                int min = 0;
                int max = 0;

                for (int i = 0; i < clusters.Count; i++)
                {
                    if (clusters[i].categories.Contains(practice.Value.category))
                    {
                        min = clusters[i].min;
                        max = clusters[i].max;
                    }
                }

                catInterval.text = "[ " + min + ", " + max + " ]";

                practice.Value.mousePressed = false;

                break;
            }
        }
    }

    //UTILITY FUNCTIONS
    public int MaxValue(int[] intArray)
    {
        int max = intArray[0];

        for (int i = 1; i < intArray.Length; i++)
        {

            if (intArray[i] > max)
            {
                max = intArray[i];
            }
        }

        return max;
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
}
