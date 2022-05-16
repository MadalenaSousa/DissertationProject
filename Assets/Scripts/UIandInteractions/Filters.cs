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
            authorOptions[i].onClick.AddListener(filterByAuthor);
        }

        //PUB OUTLETS
        allJournals = db.getPSJournals();

        journalSearch.SetAvailableOptions(allJournals);   
        journalOptions = journalOptionList.GetComponentsInChildren<Button>();

        for (int i = 0; i < journalOptions.Length; i++)
        {
            journalOptions[i].onClick.AddListener(filterByJournal);
        }

        //INSTITUTIONS
        allInstitutions = db.getPSInstitutions();

        instSearch.SetAvailableOptions(allInstitutions);
        instOptions = instOptionList.GetComponentsInChildren<Button>();

        for (int i = 0; i < instOptions.Length; i++)
        {
            instOptions[i].onClick.AddListener(filterByInstitution);
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

    public void filterByAuthor()
    {
        deactivatePapers(1, authorSearch.Text);
        currentAuthorInFilter = authorSearch.Text;
    }

    public void filterByJournal()
    {
        deactivatePapers(2, journalSearch.Text);
        currentJournalInFilter = journalSearch.Text;
    }

    public void filterByInstitution()
    {
        deactivatePapers(3, instSearch.Text);
        currentInstitutionInFilter = instSearch.Text;
    }

    public void deactivatePapers(int type, string name)
    {
        List<int> results = db.filterByAuthorJournalInstitution(1, authorSearch.Text);
        List<int> resultsJ = db.filterByAuthorJournalInstitution(2, journalSearch.Text);
        List<int> resultsI = db.filterByAuthorJournalInstitution(3, instSearch.Text);
        string typeName = "";
        List<int> deactivatedPapers = new List<int>();

        if(type == 1)
        {
            typeName = "author";
        } 
        else if(type == 2)
        {
            typeName = "journal";
        } 
        else if(type == 3)
        {
            typeName = "institution";
        } 

       //for (int i = 0; i < ps.papers.Count; i++)
       //{
       //    if (ps.papers[i].gameObject.activeInHierarchy) //qd se usa o mesmo filtro duas vezes, também filtra conjunto
       //    {
       //        if (!results.Contains(ps.papers[i].getId()))
       //        {
       //            ps.papers[i].gameObject.SetActive(false);
       //            deactivatedPapers.Add(ps.papers[i].paper.id);
       //        }
       //    }
       //}

        for (int i = 0; i < ps.papers.Count; i++)
        {
            if (results.Count > 0)
            {
                if (!results.Contains(ps.papers[i].getId()))
                {
                    ps.papers[i].gameObject.SetActive(false);
                    deactivatedPapers.Add(ps.papers[i].paper.id);
                }
            }

            if (resultsJ.Count > 0)
            {
                if (!resultsJ.Contains(ps.papers[i].getId()))
                {
                    ps.papers[i].gameObject.SetActive(false);
                    deactivatedPapers.Add(ps.papers[i].paper.id);
                }
            }
        }

        //if typeName already exists
        //go through non active

        filteredPapers.Remove(typeName);
        filteredPapers.Add(typeName, deactivatedPapers);
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
