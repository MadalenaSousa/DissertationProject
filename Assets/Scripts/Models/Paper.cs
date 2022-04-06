using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paper
{
    public int id;
    public string title, date;
    public int year;

    public List<Author> author;
    public PubOutlet publication_outlet;

    public List<Practice> practice;
    public List<Strategy> strategy;
    public List<Use> use;

    public Paper(int id, string title, string date, int year, List<Author> author, PubOutlet puboutlet, List<Practice> practice, List<Strategy> strategy, List<Use> use)
    {
        this.id = id;
        this.title = title;
        this.date = date;
        this.year = year;
        this.author = author;
        this.publication_outlet = puboutlet;
        this.practice = practice;
        this.strategy = strategy;
        this.use = use;
  
    }
}
