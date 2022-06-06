using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Linq;
using UnityEngine.Events;
using System;

public class Filters : MonoBehaviour
{
    //Instances
    Database db;
    PracticesAndStrategies ps;
    public static Filters instance;

    //Author Filter UI
    public AutoCompleteComboBox authorSearch;
    public GameObject authorOptionlist;
    Button[] authorOptions;
    List<string> allAuthors = new List<string>();
    string currentAuthorInFilter;

    //Journal Filter UI
    public AutoCompleteComboBox journalSearch;
    public GameObject journalOptionList;
    Button[] journalOptions;
    List<string> allJournals = new List<string>();
    Dictionary<int, string> journalsFromDB = new Dictionary<int, string>();
    string currentJournalInFilter;

    //Institution Filter UI
    public AutoCompleteComboBox instSearch;
    public GameObject instOptionList;
    Button[] instOptions;
    List<string> allInstitutions = new List<string>();
    string currentInstitutionInFilter;

    //Year Interval UI
    public InputField minInput, maxInput;
    public Button triggerFilter;

    //Other
    public Button resetFilters;
    public Dictionary<string, List<int>> filteredPapers = new Dictionary<string, List<int>>();

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
        ps = PracticesAndStrategies.instance;
        
        //AUTHORS
        allAuthors = db.getPSAuthors();
        authorSearch.SetAvailableOptions(allAuthors);
        authorOptions = authorOptionlist.GetComponentsInChildren<Button>();

        for (int i = 0; i < authorOptions.Length; i++)
        {
            authorOptions[i].onClick.AddListener(filterPapers);
        }

        //PUB OUTLETS
        allJournals = db.getPSJournals();

        journalSearch.SetAvailableOptions(allJournals);   
        journalOptions = journalOptionList.GetComponentsInChildren<Button>();

        for (int i = 0; i < journalOptions.Length; i++)
        {
            journalOptions[i].onClick.AddListener(filterPapers);
        }

        //INSTITUTIONS
        allInstitutions = db.getPSInstitutions();

        instSearch.SetAvailableOptions(allInstitutions);
        instOptions = instOptionList.GetComponentsInChildren<Button>();

        for (int i = 0; i < instOptions.Length; i++)
        {
            instOptions[i].onClick.AddListener(filterPapers);
        }
    }

    public void clearFilter(string filter)
    {
        if(filter == "author")
        {
            authorSearch.clearInputField();
            authorSearch.Text = null;
        } 
        else if(filter == "journal")
        {
            journalSearch.clearInputField();
            journalSearch.Text = null;
        }
        else if (filter == "institution")
        {
            instSearch.clearInputField();
            instSearch.Text = null;
        }
        else if (filter == "year")
        {
            minInput.text = "";
            maxInput.text = "";
        }

        filterPapers();
    }

    public void filterPapers()
    {
        bool isAllPapersActive = true;
        List<int> results = new List<int>();

        int min = -1;
        int max = DateTime.Now.Year + 1;

        if (!string.IsNullOrEmpty(minInput.text))
        {
            min = int.Parse(minInput.text);
        }

        if (!string.IsNullOrEmpty(maxInput.text))
        {
            max = int.Parse(maxInput.text);
        }

        if (authorSearch.Text != null || journalSearch.Text != null || instSearch.Text != null || min > 1500 || max < DateTime.Now.Year)
        {
            results = db.filter(authorSearch.Text, journalSearch.Text, instSearch.Text, min, max);
            isAllPapersActive = false;
        }

        for (int i = 0; i < ps.papers.Count; i++)
        {
            if (isAllPapersActive || results.Contains(ps.papers[i].getId()))
            {
                ps.papers[i].gameObject.SetActive(true);
            }
            else
            {
                ps.papers[i].gameObject.SetActive(false);
            }
        }
    }

    public void applyYearFilter()
    {
        filterPapers();
        PracticesAndStrategies.instance.changeClusterCriteria();
    }
}
