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
    Database db;

    public AutoCompleteComboBox authorSearch;
    public GameObject optionlist;
    Button[] options;
    List<string> allAuthors = new List<string>();
    Dictionary<int, string> authorsFromDB = new Dictionary<int, string>();

    public AutoCompleteComboBox journalSearch;
    public GameObject journalOptionList;
    Button[] journalOptions;
    List<string> allJournals = new List<string>();
    Dictionary<int, string> journalsFromDB = new Dictionary<int, string>();

    public Button resetFilters;

    BrainView bv;

    void Start()
    {
        db = Database.instance;
        bv = BrainView.instance;
        
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
            allJournals.Add(journal.Value);
        }

        journalSearch.SetAvailableOptions(allJournals);
        journalOptions = journalOptionList.GetComponentsInChildren<Button>();

        for (int i = 0; i < journalOptions.Length; i++)
        {
            journalOptions[i].onClick.AddListener(filterByJournal);
        }



        resetFilters.onClick.AddListener(bv.resetPapers);
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
