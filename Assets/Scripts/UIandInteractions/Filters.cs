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
    PracticesAndStrategies bv;

    //Author Filter UI
    public AutoCompleteComboBox authorSearch;
    public GameObject authorOptionlist;
    Button[] authorOptions;
    List<string> allAuthors = new List<string>();

    //Journal Filter UI
    public AutoCompleteComboBox journalSearch;
    public GameObject journalOptionList;
    Button[] journalOptions;
    List<string> allJournals = new List<string>();
    Dictionary<int, string> journalsFromDB = new Dictionary<int, string>();

    //Institution Filter UI
    public AutoCompleteComboBox instSearch;
    public GameObject instOptionList;
    Button[] instOptions;
    List<string> allInstitutions = new List<string>();

    //Year Interval UI
    public InputField minInput, maxInput;
    public Button triggerFilter;

    //Other
    public Button resetFilters;

    void Start()
    {
        db = Database.instance;
        bv = PracticesAndStrategies.instance;
        
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

        //--
        resetFilters.onClick.AddListener(bv.resetPapers);
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

        bv.deactivatePapersByYear(min, max);
    }

    public void filterByAuthor()
    {
        bv.deactivatePapers(1, authorSearch.Text);
    }

    public void filterByJournal()
    {
        bv.deactivatePapers(2, journalSearch.Text);
    }

    public void filterByInstitution()
    {
        bv.deactivatePapers(3, instSearch.Text);
    }
}
