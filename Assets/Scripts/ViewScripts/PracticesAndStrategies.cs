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

    //UI Elements and Interaction
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

    public GameObject catPopUp;
    public Button closeCatPopUpButton;
    public Text catName;
    public Text catConns;
    public Text catInterval;


    //Visual Objects
    public GameObject parentObject;

    public GameObject NodePrefab;
    public List<PaperView> papers = new List<PaperView>();

    public GameObject CategoryPrefab;

    Dictionary<int, CategoryView> strategiesViews = new Dictionary<int, CategoryView>();
    Dictionary<int, CategoryView> practicesViews = new Dictionary<int, CategoryView>();

    public GameObject ConnectionPrefab;
    List<ConnectionView> connections =  new List<ConnectionView>();

    int clusterConnInterval, totalClusters;
    List<Cluster> clusters = new List<Cluster>();

    public Transform cameraTarget;

    //Other
    public int minNodeRadius, masNodeRadius;
    public int globalSphereRadius;
    public bool isClicking = false;
    public CinemachineFreeLook cineCam;

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
        minNodeRadius = 10;
        masNodeRadius = 80;
        globalSphereRadius = 800;

        //CLUSTERS
        int maxConnections = db.getMaxConnPS();
        int minConnections = db.getMinConnPS();
        totalClusters = 50;
        clusterConnInterval = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(maxConnections) / Convert.ToDouble(totalClusters)));

        for (int i = 0; i <= totalClusters; i++)
        {
            int min = clusterConnInterval * i;
            int max = clusterConnInterval * (i + 1);

            clusters.Add(new Cluster(min, max));
        }

        //CATEGORIES
        List<int> PracticesId = db.getPractices();
        int totalPractices = PracticesId.Count;

        List<int> StrategiesId = db.getStrategies();
        int totalStrategies = StrategiesId.Count;

        for (int i = 0; i < totalPractices; i++)
        {
            CategoryView newPractice = Instantiate(CategoryPrefab, parentObject.transform).GetComponent<CategoryView>();
            newPractice.bootstrapPractices(PracticesId[i]);
            newPractice.setRadius(mapValues(newPractice.clusterCriteria, minConnections, maxConnections, 10, 80));

            for (int j = 0; j < clusters.Count; j++)
            {
                if (newPractice.clusterCriteria > clusters[j].min && newPractice.clusterCriteria <= clusters[j].max)
                {
                    clusters[j].categories.Add(newPractice.category);
                    newPractice.setPosition(clusters[j].center + clusters[j].getOffsetVector(newPractice));
                    break;
                }
            }

            practicesViews.Add(PracticesId[i], newPractice);
        }

        for (int i = 0; i < totalStrategies; i++)
        {
            CategoryView newStrategy = Instantiate(CategoryPrefab, parentObject.transform).GetComponent<CategoryView>();
            newStrategy.bootstrapStrategies(StrategiesId[i]);
            newStrategy.setRadius(mapValues(newStrategy.clusterCriteria, minConnections, maxConnections, 10, 80));

            for (int j = 0; j < clusters.Count; j++)
            {
                if (newStrategy.clusterCriteria > clusters[j].min && newStrategy.clusterCriteria <= clusters[j].max)
                {
                    clusters[j].categories.Add(newStrategy.category);
                    newStrategy.setPosition(clusters[j].center + clusters[j].getOffsetVector(newStrategy));
                    break;
                }
            }

            strategiesViews.Add(StrategiesId[i], newStrategy);
        }

        // Remove Empty Clusters
        List<Cluster> nonEmptyClusters = new List<Cluster>();
        foreach(Cluster cluster in clusters)
        {
            if (cluster.categories.Count > 0)
            {
                nonEmptyClusters.Add(cluster);
            }
        }
        clusters = nonEmptyClusters;

        //PAPERS
        List<int> PSPapersId = db.getPapersWithPracticesOrStrategies();
        int totalpapers = PSPapersId.Count;

        for (int i = 0; i < totalpapers; i++)
        {
            PaperView newPaper = Instantiate(NodePrefab, parentObject.transform).GetComponent<PaperView>();
            newPaper.bootstrap(PSPapersId[i]);
            newPaper.setRadius(newPaper.connections.Count);

            papers.Add(newPaper);

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
                connections.Add(newConnection);
                newPaper.conns.Add(newConnection);

                xSum += practicesViews[paperPractice.id].transform.position.x;
                ySum += practicesViews[paperPractice.id].transform.position.y;
                zSum += practicesViews[paperPractice.id].transform.position.z;
            }

            int totalPaperCategories = newPaper.conns.Count;
            Vector3 paperPos = new Vector3(xSum / totalPaperCategories, ySum / totalPaperCategories, zSum / totalPaperCategories);
            
            if(newPaper.conns.Count <= 1)
            {
                Vector3 altPos = paperPos + (UnityEngine.Random.insideUnitSphere * 80);
                newPaper.setPosition(altPos);             
            } 
            else
            {
                int offset = 5;
                for(int j = 0; j < papers.Count; j++) 
                { 
                    if(papers[i].transform.position == paperPos)
                    {
                        newPaper.setPosition(paperPos + new Vector3(getRandom(-offset, offset), getRandom(-offset, offset), getRandom(-offset, offset)));
                    } 
                    else
                    {
                        newPaper.setPosition(paperPos);
                    }
                }                
            }           
        }

        //OTHER
        closePopUpButton.onClick.AddListener(closePopUp);
        closeCatPopUpButton.onClick.AddListener(closeCatPopUp);

    }

    private void Update()
    {
        openPopUp();
        openCatPopUp();
        updateConnections();
        lookAtCamera();

        if (Input.GetMouseButtonDown(0))
        {
            cineCam.m_YAxis.m_InputAxisName = "Mouse Y";
            cineCam.m_XAxis.m_InputAxisName = "Mouse X";
        } 
        else if(Input.GetMouseButtonUp(0))
        {
            cineCam.m_YAxis.m_InputAxisName = "";
            cineCam.m_XAxis.m_InputAxisName = "";
            cineCam.m_YAxis.m_InputAxisValue = 0;
            cineCam.m_XAxis.m_InputAxisValue = 0;
        }
    }

    private void lookAtCamera()
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
                catConns.text = strategy.Value.clusterCriteria.ToString();

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
                catConns.text = practice.Value.clusterCriteria.ToString();

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
}
