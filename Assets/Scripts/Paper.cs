using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;

public class Paper :  MonoBehaviour
{
    Database db;

    // DB based variables
    public string title, date;
    public int year;

    public List<Author> author = new List<Author>();
    public PubOutlet publication_outlet;
    
    public List<Practice> practice = new List<Practice>();
    public List<Strategy> strategy = new List<Strategy>();
    public List<Use> use = new List<Use>();

    // externaly defined variables
    public int id;

    //UI Elements
    public GameObject titleBox;

    public void setValues(int id)
    {
        this.id = id;
        this.db = Database.instance;
        db.StartDataSQLite();

        this.title = db.getTitleById(id);
        this.date = db.getDateById(id);
        this.year = db.getYearById(id);

        this.author = db.getAuthorAndInstitutionByPaperId(id);
        this.publication_outlet = db.getPubOutletByPaperId(id);

        this.practice = db.getPracticeByPaperId(id);
        this.strategy = db.getStrategyByPaperId(id);
        this.use = db.getUseByPaperId(id);

        titleBox.GetComponent<Text>().text = this.title;
    }

    public void setPosition(float x, float y, float z)
    {
        gameObject.transform.position = new Vector3(x, y, z);
    }

    public void setColor(Color color)
    {
        gameObject.GetComponentInChildren<Renderer>().material.color = color;
    }
}

public class Author
{
    public int id;
    public string name;
    public string openalexid;
    public Institution institution;

    public Author(int id, string name, string authoropenalexid, int instid, string instname, string countrycode, string instopenalexid)
    {
        this.id = id;
        this.name = name;
        this.openalexid = authoropenalexid;
        this.institution = new Institution(instid, instname, countrycode, instopenalexid);
    }
}

public class Institution
{
    public int id;
    public string name;
    public string countrycode;
    public string openalexid;

    public Institution(int id, string name, string countrycode, string openalexid)
    {
        this.id = id;
        this.name = name;
        this.countrycode = countrycode;
        this.openalexid = openalexid;
    }
}

public class PubOutlet
{
    public int id;
    public string name;
    public string url;
    public string openalexid;
    public string issn;

    public PubOutlet(int id, string name, string url, string openalexid, string issn)
    {
        this.id = id;
        this.name = name;
        this.url = url;
        this.openalexid = openalexid;
        this.issn = issn;
    }
}

public class Practice : Category
{
    public Practice(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

public class Strategy : Category
{
    public Strategy(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

public class Use : Category
{
    public Use(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

public class Category
{
    public int id;
    public string name;
}

