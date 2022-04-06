using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Linq;
using UnityEngine.Events;
using System;
using System.Linq;

public class Filters : MonoBehaviour
{
    //Instances
    Database db;
    PracticesAndStrategies bv;

    //Author Filter UI
    public AutoCompleteComboBox authorSearch;
    public GameObject optionlist;
    Button[] options;
    List<string> allAuthors = new List<string>();
    Dictionary<int, string> authorsFromDB = new Dictionary<int, string>();

    //Journal Filter UI
    public AutoCompleteComboBox journalSearch;
    public GameObject journalOptionList;
    Button[] journalOptions;
    List<string> allJournals = new List<string>();
    Dictionary<int, string> journalsFromDB = new Dictionary<int, string>();

    //Year Interval UI
    public InputField minInput, maxInput;
    public Button triggerFilter;

    //Other
    public Button resetFilters;

    void Start()
    {
        db = Database.instance;
        bv = PracticesAndStrategies.instance;
        
        authorsFromDB = db.getAllInTable("author");
        foreach (KeyValuePair<int, string> author in authorsFromDB)
        {
            allAuthors.Add(author.Value);
        }

        authorSearch.SetAvailableOptions(allAuthors);
        options = optionlist.GetComponentsInChildren<Button>();

        for (int i = 0; i < options.Length; i++)
        {
            options[i].onClick.AddListener(filterByAuthor);
        }

        journalsFromDB = db.getAllInTable("puboutlet");
        foreach (KeyValuePair<int, string> journal in journalsFromDB)
        {
            string newjournal = journal.Value;
            allJournals.Add(journal.Value);
            Debug.Log(newjournal);
        }

        journalSearch.SetAvailableOptions(allJournals);   
        journalOptions = journalOptionList.GetComponentsInChildren<Button>();

        for (int i = 0; i < journalOptions.Length; i++)
        {
            journalOptions[i].onClick.AddListener(filterByJournal);
        }

        triggerFilter.onClick.AddListener(filterByYearInterval);

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
}
