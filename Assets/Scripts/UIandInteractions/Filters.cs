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

        //YEAR
        triggerFilter.onClick.AddListener(filterByYearInterval);
    }

    public void filterByYearInterval()
    {
        int min = -2;
        int max = DateTime.Now.Year + 1;

        if (!string.IsNullOrEmpty(minInput.text))
        {
            min = int.Parse(minInput.text);
        }

        if (!string.IsNullOrEmpty(maxInput.text))
        {
            max = int.Parse(maxInput.text);
        }

        deactivatePapersByYear(min, max);
    }

    public void clearFilter(string filter)
    {
        if(filter == "author")
        {
            authorSearch.clearInputField();
        } 
        else if(filter == "journal")
        {
            journalSearch.clearInputField();
        }
        else if (filter == "institution")
        {
            instSearch.clearInputField();
        }
        else if (filter == "year")
        {
            //to be done
        }

        filterPapers();
    }

    public void filterPapers()
    {
        bool isAllActive = true;
        List<int> results = new List<int>();

        if (authorSearch.Text != null || journalSearch.Text != null || instSearch.Text != null)
        {
            results = db.filter(authorSearch.Text, journalSearch.Text, instSearch.Text);
            isAllActive = false;
        }

        for (int i = 0; i < ps.papers.Count; i++)
        {
            if (isAllActive || results.Contains(ps.papers[i].getId()))
            {
                ps.papers[i].gameObject.SetActive(true);
            }
            else
            {
                ps.papers[i].gameObject.SetActive(false);
            }
        }
    }

    public void deactivatePapersByYear(int min, int max)
    {
        List<int> results = db.getPapersByYearInterval(min, max);

        for (int i = 0; i < ps.papers.Count; i++)
        {
            if (ps.papers[i].gameObject.activeInHierarchy)
            {
                if (!results.Contains(ps.papers[i].getId()))
                {
                    ps.papers[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void resetPapers(string typeName)
    {
        foreach (KeyValuePair<string, List<int>> filteredPapers in filteredPapers)
        {
            if (filteredPapers.Key == typeName)
            {
                for (int i = 0; i < filteredPapers.Value.Count; i++)
                {
                    for (int j = 0; j < ps.papers.Count; j++)
                    {
                        if (ps.papers[j].getId() == filteredPapers.Value[i])
                        {
                            ps.papers[j].gameObject.SetActive(true);
                        }
                    }
                }
            }
        }

        filteredPapers.Remove(typeName);
    }
}
