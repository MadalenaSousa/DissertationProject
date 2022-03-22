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

    public Button resetFilters;

    BrainView bv;

    void Start()
    {
        db = Database.instance;
        authorsFromDB = db.getAllInTable("author");

        bv = BrainView.instance;

        foreach(KeyValuePair<int, string> author in authorsFromDB)
        {
            allAuthors.Add(author.Value);
        }

        authorSearch.SetAvailableOptions(allAuthors);
        options = optionlist.GetComponentsInChildren<Button>();

        for (int i = 0; i < options.Length; i++)
        {
            options[i].onClick.AddListener(filterByAuthor);
        }

        resetFilters.onClick.AddListener(bv.resetPapers);
    }

    void Update()
    {
        
    }

    public void filterByAuthor()
    {
        bv.deactivatePapers(1, authorSearch.Text);
    }
}
